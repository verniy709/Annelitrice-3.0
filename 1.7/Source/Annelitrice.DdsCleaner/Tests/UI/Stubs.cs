using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UnityEngine
{
    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
    }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }
}

namespace HarmonyLib
{
    public sealed class Harmony
    {
        public Harmony(string id) { }
        public void Patch(MethodInfo original, HarmonyMethod postfix = null) { }
    }
    public sealed class HarmonyMethod { public HarmonyMethod(Type type, string method) { } }
    public static class AccessTools { public static MethodInfo Method(Type type, string name) { return type.GetMethod(name); } }
}

namespace RimWorld { public static class MainMenuDrawer { public static void MainMenuOnGUI() { } } }

namespace Verse
{
    public enum ProgramState { Entry, Playing }
    public enum GameFont { Small, Medium }
    public static class Current { public static ProgramState ProgramState; }
    public class UIRoot { }
    public class UIRoot_Entry : UIRoot { }
    public class UIRoot_Play : UIRoot { }
    public static class Find { public static UIRoot UIRoot; public static WindowStack WindowStack = new WindowStack(); }
    public class ModContentPack { }
    public class Mod
    {
        public Mod(ModContentPack content) { }
        public virtual string SettingsCategory() { return string.Empty; }
        public virtual void DoSettingsWindowContents(UnityEngine.Rect rect) { }
    }
    [AttributeUsage(AttributeTargets.Class)] public sealed class StaticConstructorOnStartupAttribute : Attribute { }
    public sealed class ModMetaData
    {
        public DirectoryInfo RootDir;
        public string PackageIdNonUnique;
        public string Name;
        public bool Active;
    }
    public static class ModLister { public static IEnumerable<ModMetaData> AllInstalledMods = new List<ModMetaData>(); }
    public static class GenFilePaths { public static string ModsFolderPath; }
    public abstract class Window
    {
        public bool forcePause, absorbInputAroundWindow, closeOnClickedOutside, closeOnAccept = true, closeOnCancel = true, doCloseX, doCloseButton;
        public virtual UnityEngine.Vector2 InitialSize { get { return new UnityEngine.Vector2(500, 500); } }
        public abstract void DoWindowContents(UnityEngine.Rect rect);
        public void Close() { Find.WindowStack.Windows.Remove(this); }
    }
    public sealed class WindowStack
    {
        public readonly List<Window> Windows = new List<Window>();
        public void Add(Window window) { Windows.Add(window); }
    }
    public static class Text
    {
        public static GameFont Font;
        public static float CalcHeight(string text, float width) { return Math.Max(20, text.Length / Math.Max(1, width / 10) * 16); }
    }
    public static class Widgets
    {
        public static readonly HashSet<string> PressedLabels = new HashSet<string>();
        public static readonly List<string> Labels = new List<string>();
        public static void Label(UnityEngine.Rect rect, string text) { Labels.Add(text); }
        public static bool ButtonText(UnityEngine.Rect rect, string label) { return PressedLabels.Remove(label); }
        public static void BeginScrollView(UnityEngine.Rect rect, ref UnityEngine.Vector2 position, UnityEngine.Rect view) { }
        public static void EndScrollView() { }
    }
    public static class Translator { public static string Translate(this string key) { return key; } }
    public static class GenCommandLine { public static int RestartCount; public static void Restart() { RestartCount++; } }
    public static class Log
    {
        public static readonly List<string> Messages = new List<string>();
        public static void Message(string text) { Messages.Add(text); }
        public static void Warning(string text) { Messages.Add(text); }
        public static void Error(string text) { Messages.Add(text); }
    }
    public static class LongEventHandler
    {
        private static readonly Queue<Action> Events = new Queue<Action>();
        private static readonly Queue<Action> Finished = new Queue<Action>();
        private static bool running;
        public static bool AnyEventNowOrWaiting { get { return running || Events.Count > 0; } }
        public static int PendingCount { get { return Events.Count; } }
        public static void QueueLongEvent(Action action, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler,
            bool showExtraUIInfo = true, bool forceHideUI = false, Action callback = null)
        {
            Events.Enqueue(delegate {
                try { action(); } catch (Exception exception) { if (exceptionHandler == null) throw; exceptionHandler(exception); }
                if (callback != null) Finished.Enqueue(callback);
            });
        }
        public static void ExecuteWhenFinished(Action action) { Finished.Enqueue(action); }
        public static void RunPending()
        {
            while (Events.Count > 0)
            {
                running = true;
                Events.Dequeue()();
                running = false;
                while (Finished.Count > 0) Finished.Dequeue()();
            }
        }
        public static void Reset() { Events.Clear(); Finished.Clear(); running = false; }
    }
}
