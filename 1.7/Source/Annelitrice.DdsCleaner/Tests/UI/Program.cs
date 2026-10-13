using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Annelitrice.DdsCleaner;
using UnityEngine;
using Verse;

internal static class Program
{
    private const string Package = "Verniy709.Annelitrice3.0";
    private static readonly string FixtureRoot = Path.Combine(AppContext.BaseDirectory, "fixtures", Guid.NewGuid().ToString("N"));
    private static int passed;
    private static int fixtureId;
    private static string caseRoot;
    private static readonly Rect WindowRect = new Rect(0, 0, 780, 620);

    private static int Main()
    {
        Directory.CreateDirectory(FixtureRoot);
        try
        {
            Run("Automatic menu scan is read-only and includes disabled duplicate copies", ReadOnlyScanAndCopies);
            Run("Cancel and Enter do not approve deletion", CancelAndEnter);
            Run("Delete and restart are separate explicit clicks", DeleteThenRestart);
            Run("Playing and long-event states block automatic cleanup", PlayStateAndBusyEvent);
            Run("Cleanup and restart buttons remain gated by main-menu state", StateChangesWhilePromptOpen);
            Run("Automatic check runs once per session", OneAutomaticCheck);
            Run("No DDS files is quiet automatically and visible manually", EmptyScan);
            Run("Settings scan obeys the main-menu gate", SettingsGate);
            Run("Scan completion cannot interrupt a loaded colony", StateChangesDuringScan);
            Console.WriteLine("PASS: " + passed + " UI/lifecycle regression cases. Fixtures: " + FixtureRoot);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("FAIL after " + passed + " cases: " + exception);
            return 1;
        }
    }

    private static void Run(string name, Action test)
    {
        Reset();
        test();
        passed++;
        Console.WriteLine("PASS: " + name);
    }

    private static void Reset()
    {
        caseRoot = Path.Combine(FixtureRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(caseRoot);
        GenFilePaths.ModsFolderPath = caseRoot;
        SetControllerField("automaticCheckStarted", false);
        SetControllerField("busy", false);
        Current.ProgramState = ProgramState.Entry;
        Find.UIRoot = new UIRoot_Entry();
        Find.WindowStack = new WindowStack();
        ModLister.AllInstalledMods = new List<ModMetaData>();
        LongEventHandler.Reset();
        Widgets.PressedLabels.Clear();
        Widgets.Labels.Clear();
        GenCommandLine.RestartCount = 0;
        Log.Messages.Clear();
    }

    private static void SetControllerField(string field, bool value)
    {
        typeof(DdsCleanerController).GetField(field, BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
    }

    private static ModMetaData ModFixture(bool dds = true, bool active = true)
    {
        string root = Path.Combine(caseRoot, (++fixtureId).ToString());
        Directory.CreateDirectory(Path.Combine(root, "About"));
        Directory.CreateDirectory(Path.Combine(root, "1.7", "Textures", "Nested"));
        File.WriteAllText(Path.Combine(root, "About", "About.xml"), "<ModMetaData><packageId>" + Package + "</packageId></ModMetaData>");
        File.WriteAllText(Path.Combine(root, "1.7", "Textures", "Nested", "sample.png"), "unchanged png");
        if (dds) File.WriteAllText(DdsPath(root), "fixture dds");
        return new ModMetaData { PackageIdNonUnique = Package, Name = "Fixture " + fixtureId, RootDir = new DirectoryInfo(root), Active = active };
    }

    private static string DdsPath(string root) { return Path.Combine(root, "1.7", "Textures", "Nested", "sample.dds"); }
    private static string DdsPath(ModMetaData mod) { return DdsPath(mod.RootDir.FullName); }
    private static void Registry(params ModMetaData[] mods) { ModLister.AllInstalledMods = mods; }
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Click(Window window, string key)
    {
        Widgets.PressedLabels.Add(key);
        window.DoWindowContents(WindowRect);
        Assert(!Widgets.PressedLabels.Contains(key), "The requested button was not rendered: " + key);
    }
    private static T OnlyWindow<T>() where T : Window
    {
        Assert(Find.WindowStack.Windows.Count == 1, "Expected exactly one window, got " + Find.WindowStack.Windows.Count);
        Assert(Find.WindowStack.Windows[0] is T, "Unexpected window type: " + Find.WindowStack.Windows[0].GetType());
        return (T)Find.WindowStack.Windows[0];
    }
    private static DdsCleanupPrompt OpenPrompt()
    {
        DdsCleanerController.MenuTick();
        LongEventHandler.RunPending();
        return OnlyWindow<DdsCleanupPrompt>();
    }

    private static void ReadOnlyScanAndCopies()
    {
        ModMetaData first = ModFixture();
        ModMetaData disabledCopy = ModFixture(active: false);
        // RimWorld can omit a same-origin duplicate entirely; physical discovery must still find it.
        Registry(first, first);
        DdsCleanerController.MenuTick();
        Assert(LongEventHandler.PendingCount == 1, "Menu should enqueue one scan.");
        Assert(Find.WindowStack.Windows.Count == 0, "Prompt appears only after the scan completes.");
        Assert(File.Exists(DdsPath(first)) && File.Exists(DdsPath(disabledCopy)), "Enqueueing scan mutated DDS files.");
        LongEventHandler.RunPending();
        DdsCleanupPrompt prompt = OnlyWindow<DdsCleanupPrompt>();
        DdsScan scan = (DdsScan)typeof(DdsCleanupPrompt).GetField("scan", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(prompt);
        Assert(scan.FileCount == 2 && scan.Mods.Count == 2, "Disabled duplicate copy must be scanned; repeated same root must be deduplicated.");
        Assert(File.Exists(DdsPath(first)) && File.Exists(DdsPath(disabledCopy)), "Read-only scan deleted a DDS file.");
        Assert(GenCommandLine.RestartCount == 0, "Scanning restarted the game.");
    }

    private static void CancelAndEnter()
    {
        ModMetaData mod = ModFixture(); Registry(mod);
        DdsCleanupPrompt prompt = OpenPrompt();
        Assert(!prompt.closeOnAccept, "Enter could accept the deletion prompt.");
        Assert(prompt.closeOnCancel && prompt.doCloseX && !prompt.closeOnClickedOutside, "Cancel/close behavior is unexpected.");
        prompt.DoWindowContents(WindowRect);
        Click(prompt, "Anneli.Dds.ShowFiles");
        prompt.DoWindowContents(WindowRect);
        Assert(Widgets.Labels.Any(label => label.Contains(DdsPath(mod))), "File preview does not show the physical candidate.");
        Assert(File.Exists(DdsPath(mod)), "Drawing or showing files changed DDS files.");
        Click(prompt, "Anneli.Dds.NotNow");
        Assert(Find.WindowStack.Windows.Count == 0 && File.Exists(DdsPath(mod)), "Cancel deleted a file or left the prompt open.");
        Assert(LongEventHandler.PendingCount == 0 && GenCommandLine.RestartCount == 0, "Cancel queued cleanup/restart.");
    }

    private static void DeleteThenRestart()
    {
        ModMetaData mod = ModFixture(); Registry(mod);
        DdsCleanupPrompt prompt = OpenPrompt();
        Click(prompt, "Anneli.Dds.Delete");
        Assert(File.Exists(DdsPath(mod)), "Deletion ran before its queued operation.");
        Assert(GenCommandLine.RestartCount == 0, "Delete click restarted before results.");
        LongEventHandler.RunPending();
        Assert(!File.Exists(DdsPath(mod)), "Approved cleanup did not remove DDS.");
        Assert(File.ReadAllText(Path.ChangeExtension(DdsPath(mod), ".png")) == "unchanged png", "Cleanup changed PNG content.");
        DdsNotice notice = OnlyWindow<DdsNotice>();
        Assert(!notice.closeOnAccept && GenCommandLine.RestartCount == 0, "Result notice automatically accepted/restarted.");
        notice.DoWindowContents(WindowRect);
        Assert(GenCommandLine.RestartCount == 0, "Drawing the result restarted the game.");
        Click(notice, "Anneli.Dds.Restart");
        Assert(GenCommandLine.RestartCount == 1 && Find.WindowStack.Windows.Count == 0, "Explicit restart button did not restart once.");
        Assert(Log.Messages.Any(message => message.Contains("Player-approved cleanup: deleted=1")), "Approved cleanup was not logged.");
    }

    private static void PlayStateAndBusyEvent()
    {
        ModMetaData mod = ModFixture(); Registry(mod);
        Current.ProgramState = ProgramState.Playing; Find.UIRoot = new UIRoot_Play();
        DdsCleanerController.MenuTick();
        DdsCleanerController.RequestScan(true);
        Assert(LongEventHandler.PendingCount == 0 && Find.WindowStack.Windows.Count == 0, "Playing state allowed scan/UI.");
        Assert(File.Exists(DdsPath(mod)) && GenCommandLine.RestartCount == 0, "Playing state caused cleanup/restart.");
        Current.ProgramState = ProgramState.Entry; Find.UIRoot = new UIRoot_Entry();
        LongEventHandler.QueueLongEvent(delegate { }, null, false, null);
        DdsCleanerController.MenuTick();
        Assert(LongEventHandler.PendingCount == 1, "Automatic check ignored an existing long event.");
        LongEventHandler.RunPending();
        Assert(Find.WindowStack.Windows.Count == 0, "Blocked menu check still showed a prompt.");
        OpenPrompt();
    }

    private static void StateChangesWhilePromptOpen()
    {
        ModMetaData mod = ModFixture(); Registry(mod);
        DdsCleanupPrompt prompt = OpenPrompt();
        Current.ProgramState = ProgramState.Playing; Find.UIRoot = new UIRoot_Play();
        Click(prompt, "Anneli.Dds.Delete");
        Assert(File.Exists(DdsPath(mod)) && LongEventHandler.PendingCount == 0, "Stale prompt deleted files in Playing state.");
        DdsNotice result = new DdsNotice("Fixture result", true); Find.WindowStack.Add(result);
        Click(result, "Anneli.Dds.Restart");
        Assert(GenCommandLine.RestartCount == 0, "Stale result restarted a loaded colony.");
    }

    private static void OneAutomaticCheck()
    {
        ModMetaData mod = ModFixture(); Registry(mod);
        Click(OpenPrompt(), "Anneli.Dds.NotNow");
        for (int i = 0; i < 10; i++) DdsCleanerController.MenuTick();
        Assert(LongEventHandler.PendingCount == 0 && Find.WindowStack.Windows.Count == 0, "Automatic prompt repeated after cancellation.");
        Assert(File.Exists(DdsPath(mod)), "Automatic retry silently deleted files.");
        DdsCleanerController.RequestScan(true); LongEventHandler.RunPending(); OnlyWindow<DdsCleanupPrompt>();
    }

    private static void EmptyScan()
    {
        Registry(ModFixture(dds: false));
        DdsCleanerController.MenuTick(); LongEventHandler.RunPending();
        Assert(Find.WindowStack.Windows.Count == 0, "Empty automatic scan showed an unnecessary notice.");
        DdsCleanerController.RequestScan(true); LongEventHandler.RunPending();
        DdsNotice notice = OnlyWindow<DdsNotice>();
        notice.DoWindowContents(WindowRect);
        Assert(Widgets.Labels.Any(label => label.Contains("Anneli.Dds.NoFiles")), "Manual empty scan had no result.");
        Click(notice, "Anneli.Dds.Close");
        Assert(GenCommandLine.RestartCount == 0, "Empty scan restarted.");
    }

    private static void SettingsGate()
    {
        ModMetaData mod = ModFixture(); Registry(mod);
        DdsCleanerMod helper = new DdsCleanerMod(new ModContentPack());
        Current.ProgramState = ProgramState.Playing; Find.UIRoot = new UIRoot_Play();
        Widgets.PressedLabels.Add("Anneli.Dds.Scan"); helper.DoSettingsWindowContents(WindowRect);
        DdsNotice notice = OnlyWindow<DdsNotice>(); notice.DoWindowContents(WindowRect);
        Assert(Widgets.Labels.Contains("Anneli.Dds.MainMenuOnly"), "Settings did not explain the main-menu restriction.");
        Assert(LongEventHandler.PendingCount == 0 && File.Exists(DdsPath(mod)), "Settings scan ran in a colony.");
    }

    private static void StateChangesDuringScan()
    {
        ModMetaData mod = ModFixture(); Registry(mod);
        DdsCleanerController.MenuTick();
        Current.ProgramState = ProgramState.Playing; Find.UIRoot = new UIRoot_Play();
        LongEventHandler.RunPending();
        Assert(Find.WindowStack.Windows.Count == 0 && File.Exists(DdsPath(mod)), "Scan completion interrupted/mutated a loaded colony.");
        Current.ProgramState = ProgramState.Entry; Find.UIRoot = new UIRoot_Entry();
        OpenPrompt();
    }
}
