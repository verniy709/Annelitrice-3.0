using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Annelitrice.DdsCleaner
{
    // This lives in a separate 1.7 assembly; the original Annelitrice assembly is unchanged.
    public sealed class DdsCleanerMod : Mod
    {
        public DdsCleanerMod(ModContentPack content) : base(content) { }

        public override string SettingsCategory() { return UiText.Get("Anneli.Dds.Title"); }

        public override void DoSettingsWindowContents(Rect rect)
        {
            string description = UiText.Get("Anneli.Dds.Settings");
            float height = Text.CalcHeight(description, rect.width);
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, height), description);
            Rect button = new Rect(rect.x, rect.y + height + 16f, 280f, 36f);
            if (Widgets.ButtonText(button, UiText.Get("Anneli.Dds.Scan")))
            {
                if (DdsCleanerController.AtMainMenu)
                    DdsCleanerController.RequestScan(true);
                else
                    Find.WindowStack.Add(new DdsNotice(UiText.Get("Anneli.Dds.MainMenuOnly"), false));
            }
        }
    }

    [StaticConstructorOnStartup]
    internal static class DdsCleanerStartup
    {
        static DdsCleanerStartup()
        {
            try
            {
                new Harmony("Verniy709.Annelitrice3.0.DdsCleaner").Patch(
                    AccessTools.Method(typeof(MainMenuDrawer), "MainMenuOnGUI"),
                    postfix: new HarmonyMethod(typeof(DdsCleanerController), nameof(DdsCleanerController.MenuTick)));
            }
            catch (Exception exception)
            {
                Log.Error("[Annelitrice DDS cleaner] Could not register the main-menu check: " + exception);
            }
        }
    }

    internal static class UiText
    {
        internal static string Get(string key, params object[] arguments)
        {
            string translated = key.Translate().ToString();
            return arguments.Length == 0 ? translated : string.Format(CultureInfo.CurrentCulture, translated, arguments);
        }
    }

    internal static class DdsCleanerController
    {
        private static bool automaticCheckStarted;
        private static bool busy;
        internal static bool AtMainMenu { get { return Current.ProgramState == ProgramState.Entry && Find.UIRoot is UIRoot_Entry; } }

        // Called only when RimWorld actually draws its main menu, after loading and startup dialogs.
        public static void MenuTick()
        {
            if (automaticCheckStarted || busy || !AtMainMenu || LongEventHandler.AnyEventNowOrWaiting)
                return;
            automaticCheckStarted = true;
            RequestScan(false);
        }

        internal static void RequestScan(bool manual)
        {
            if (busy || !AtMainMenu)
                return;
            busy = true;
            // Take the registry snapshot on the UI thread. Do not filter Active, and do not deduplicate package IDs.
            List<InstalledMod> installed;
            string localModsDirectory;
            try
            {
                localModsDirectory = GenFilePaths.ModsFolderPath;
                installed = ModLister.AllInstalledMods
                    .Where(mod => mod.RootDir != null)
                    .Select(mod => new InstalledMod
                    {
                        PackageId = mod.PackageIdNonUnique,
                        Name = mod.Name,
                        RootDirectory = mod.RootDir.FullName
                    }).ToList();
            }
            catch (Exception exception)
            {
                ScanFailed(exception);
                return;
            }
            LongEventHandler.QueueLongEvent(delegate
            {
                // The registry omits extra duplicates from the same source, so also inspect its physical containers.
                DdsScan scan = DdsDiscovery.ScanAllInstalledCopies(installed, localModsDirectory);
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    busy = false;
                    foreach (string warning in scan.Warnings)
                        Log.Warning("[Annelitrice DDS cleaner] " + warning);
                    if (!AtMainMenu)
                    {
                        // Never interrupt a loaded colony or silently perform a cleanup.
                        automaticCheckStarted = false;
                        return;
                    }
                    if (scan.FileCount > 0)
                        Find.WindowStack.Add(new DdsCleanupPrompt(scan));
                    else if (manual || scan.Warnings.Count > 0)
                        Find.WindowStack.Add(new DdsNotice(UiText.Get("Anneli.Dds.NoFiles") + WarningText(scan), false));
                });
            }, "Anneli.Dds.Scanning", true, ScanFailed);
        }

        private static void ScanFailed(Exception exception)
        {
            Log.Error("[Annelitrice DDS cleaner] Scan failed; no files were deleted: " + exception);
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                busy = false;
                if (Find.WindowStack != null)
                    Find.WindowStack.Add(new DdsNotice(UiText.Get("Anneli.Dds.ScanFailed"), false));
            });
        }

        internal static string WarningText(DdsScan scan)
        {
            if (scan.Warnings.Count == 0)
                return string.Empty;
            return "\n\n" + UiText.Get("Anneli.Dds.ScanWarnings", scan.Warnings.Count) + "\n" +
                string.Join("\n", scan.Warnings.Take(20));
        }

        // The only UI call path to Clean is the explicit delete button on the preview window.
        internal static void ConfirmCleanup(DdsScan scan)
        {
            if (busy || !AtMainMenu)
                return;
            busy = true;
            LongEventHandler.QueueLongEvent(delegate
            {
                DdsCleanupResult result = DdsCleanup.Clean(scan);
                Log.Message(string.Format(CultureInfo.InvariantCulture,
                    "[Annelitrice DDS cleaner] Player-approved cleanup: deleted={0}, already absent={1}, skipped={2}, failed={3}.",
                    result.Deleted, result.Missing, result.Skipped, result.Failed));
                foreach (string message in result.Messages)
                    Log.Message("[Annelitrice DDS cleaner] " + message);
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    busy = false;
                    string text = UiText.Get("Anneli.Dds.Result", result.Deleted, result.Missing, result.Skipped, result.Failed);
                    if (result.RestartNeeded)
                        text += "\n\n" + UiText.Get("Anneli.Dds.RestartExplanation");
                    if (result.Skipped > 0 || result.Failed > 0)
                        text += "\n\n" + UiText.Get("Anneli.Dds.PartialFailure");
                    if (result.Messages.Count > 0)
                    {
                        text += "\n\n" + UiText.Get("Anneli.Dds.DetailsInLog") + "\n" + string.Join("\n", result.Messages.Take(40));
                        if (result.Messages.Count > 40)
                            text += "\n" + UiText.Get("Anneli.Dds.MorePaths", result.Messages.Count - 40);
                    }
                    Find.WindowStack.Add(new DdsNotice(text, result.RestartNeeded));
                });
            }, "Anneli.Dds.Cleaning", true, delegate(Exception exception)
            {
                Log.Error("[Annelitrice DDS cleaner] Cleanup interrupted: " + exception);
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    busy = false;
                    // Some candidates might have been removed before an unexpected interruption.
                    Find.WindowStack.Add(new DdsNotice(UiText.Get("Anneli.Dds.CleanupFailed"), true));
                });
            });
        }
    }

    internal abstract class DdsWindow : Window
    {
        private Vector2 scrollPosition;
        private string previousBody;
        private float previousWidth;
        private float bodyHeight;
        protected DdsWindow()
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            closeOnAccept = false; // Enter must never approve file deletion or a restart.
            closeOnCancel = true;
            doCloseX = true;
            doCloseButton = false;
        }
        public override Vector2 InitialSize { get { return new Vector2(820f, 660f); } }

        protected void DrawBody(Rect rect, string body)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, rect.width - 32f, 36f), UiText.Get("Anneli.Dds.Title"));
            Text.Font = GameFont.Small;
            Rect scrollRect = new Rect(0f, 44f, rect.width, Math.Max(40f, rect.height - 102f));
            float width = scrollRect.width - 20f;
            if (body != previousBody || width != previousWidth)
            {
                previousBody = body;
                previousWidth = width;
                bodyHeight = Text.CalcHeight(body, width) + 12f;
            }
            Rect viewRect = new Rect(0f, 0f, width, Math.Max(bodyHeight, scrollRect.height));
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            Widgets.Label(new Rect(0f, 0f, width, bodyHeight), body);
            Widgets.EndScrollView();
        }

        protected Rect ButtonRect(Rect rect, int index, int total)
        {
            const float gap = 12f;
            float width = (rect.width - gap * (total - 1)) / total;
            return new Rect(index * (width + gap), rect.height - 42f, width, 38f);
        }
    }

    internal sealed class DdsCleanupPrompt : DdsWindow
    {
        private readonly DdsScan scan;
        private readonly string summary;
        private readonly string detailed;
        private bool showFiles;

        internal DdsCleanupPrompt(DdsScan scan)
        {
            this.scan = scan;
            StringBuilder text = new StringBuilder();
            text.AppendLine(UiText.Get("Anneli.Dds.Introduction", scan.FileCount)).AppendLine();
            text.AppendLine(UiText.Get("Anneli.Dds.Scope")).AppendLine();
            foreach (ModScan mod in scan.Mods.Where(mod => mod.Files.Count > 0))
            {
                text.AppendLine(UiText.Get("Anneli.Dds.ModCount", mod.Mod.Name, mod.Files.Count));
                text.AppendLine(mod.Mod.RootDirectory).AppendLine();
            }
            text.AppendLine(UiText.Get("Anneli.Dds.Consent"));
            text.Append(DdsCleanerController.WarningText(scan));
            summary = text.ToString();
            StringBuilder details = new StringBuilder(summary);
            details.AppendLine().AppendLine().AppendLine(UiText.Get("Anneli.Dds.FilePreview"));
            var paths = scan.Mods.SelectMany(mod => mod.Files).Select(file => file.Path).Take(200);
            foreach (string path in paths) details.AppendLine(path);
            if (scan.FileCount > 200) details.AppendLine(UiText.Get("Anneli.Dds.MorePaths", scan.FileCount - 200));
            detailed = details.ToString();
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawBody(inRect, showFiles ? detailed : summary);
            if (Widgets.ButtonText(ButtonRect(inRect, 0, 3), UiText.Get("Anneli.Dds.NotNow")))
                Close();
            if (Widgets.ButtonText(ButtonRect(inRect, 1, 3), UiText.Get(showFiles ? "Anneli.Dds.HideFiles" : "Anneli.Dds.ShowFiles")))
                showFiles = !showFiles;
            if (Widgets.ButtonText(ButtonRect(inRect, 2, 3), UiText.Get("Anneli.Dds.Delete")))
            {
                Close();
                DdsCleanerController.ConfirmCleanup(scan);
            }
        }
    }

    internal sealed class DdsNotice : DdsWindow
    {
        private readonly string text;
        private readonly bool restart;
        internal DdsNotice(string text, bool restart) { this.text = text; this.restart = restart; }

        public override void DoWindowContents(Rect inRect)
        {
            DrawBody(inRect, text);
            if (!restart)
            {
                if (Widgets.ButtonText(ButtonRect(inRect, 0, 1), UiText.Get("Anneli.Dds.Close"))) Close();
                return;
            }
            if (Widgets.ButtonText(ButtonRect(inRect, 0, 2), UiText.Get("Anneli.Dds.Later"))) Close();
            if (Widgets.ButtonText(ButtonRect(inRect, 1, 2), UiText.Get("Anneli.Dds.Restart")))
            {
                if (!DdsCleanerController.AtMainMenu) return;
                Close();
                GenCommandLine.Restart();
            }
        }
    }
}
