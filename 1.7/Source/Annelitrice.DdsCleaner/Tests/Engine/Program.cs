using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Annelitrice.DdsCleaner;

internal static class Program
{
    private const string MainId = "Verniy709.Annelitrice3.0";
    private static readonly string FixtureParent = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../fixtures"));
    private static int passed;
    private static int conditional;

    private static int Main()
    {
        // This executable deliberately receives no real mod roots: every file comes from a new fixture.
        Directory.CreateDirectory(FixtureParent);
        try
        {
            Run("scan is read-only; cancel changes nothing", ReadOnlyScan);
            Run("all eight exact IDs and inactive duplicate copies", ExactIdsAndCopies);
            Run("near-match and mismatched metadata IDs are refused", RefusedIds);
            Run("root/version/DLC texture scopes and excluded folders", TextureScopes);
            Run("DDS cleanup preserves PNG, bundles and unrelated files", SelectiveDeletion);
            Run("changed and newly created files are preserved", ChangedAndNewFiles);
            Run("missing candidates count as already absent", MissingFiles);
            Run("metadata tampering blocks approved deletion", MetadataTampering);
            Run("out-of-bounds and extension-tampered candidates are refused", TamperedCandidates);
            Run("duplicates are deleted once", DuplicateCandidates);
            Run("locked candidate reports failure and is retained", LockedFile);
            Run("read-only candidate reports failure and is retained", ReadOnlyFile);
            Run("file symlink is never followed", FileSymlink);
            Run("directory junction and linked mod root are never followed", DirectoryJunction);
            Run("folder replaced with a junction after preview is refused", ReplacedDirectory);
            Run("linked About.xml metadata is refused", LinkedMetadata);
            Run("mod roots beneath a linked ancestor are refused", LinkedAncestor);
            Run("malformed and external-entity metadata is refused", InvalidMetadata);
            Run("nested mod folders are preserved", NestedModFolders);
            Run("nested mod created after preview is preserved", NestedModAfterPreview);
            Run("discovery includes same-origin copies omitted from registry", DiscoverMissingCopies);
            Run("discovery infers containers from unrelated registered mods", DiscoverViaOtherMod);
            Run("explicit local Mods directory works with an empty registry", DiscoverExplicitLocal);
            Run("discovery only examines direct children and exact IDs", DiscoveryBoundaries);
            Run("known approved metadata failures and container errors stay visible", DiscoveryWarnings);
            Run("discovery never follows linked containers or mod children", DiscoveryLinks);
            Console.WriteLine("PASS: " + passed + " regression cases; " + conditional + " unavailable link/read-only checks skipped.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("FAIL: " + exception);
            return 1;
        }
    }

    private static void Run(string name, Action<Fixture> test)
    {
        using (Fixture fixture = new Fixture()) test(fixture);
        passed++;
        Console.WriteLine("PASS " + name);
    }

    private static void NestedModFolders(Fixture f)
    {
        InstalledMod outer = f.Mod("outer", MainId);
        string own = f.File(outer, "Textures/own.dds");
        InstalledMod inner = f.Mod("outer/NestedOther", "OtherAuthor.UnrelatedMod");
        string other = f.File(inner, "Textures/other.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { outer });
        Equal(1, scan.FileCount);
        Equal(1, DdsCleanup.Clean(scan).Deleted);
        True(!File.Exists(own) && File.Exists(other));
    }

    private static void NestedModAfterPreview(Fixture f)
    {
        InstalledMod outer = f.Mod("outer", MainId);
        string dds = f.File(outer, "Textures/Nested/own.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { outer });
        f.File(outer, "Textures/Nested/About/About.xml", "<ModMetaData><packageId>OtherAuthor.Other</packageId></ModMetaData>");
        DdsCleanupResult result = DdsCleanup.Clean(scan);
        Equal(0, result.Deleted);
        Equal(1, result.Skipped);
        True(File.Exists(dds));
    }

    private static void ReadOnlyScan(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string dds = f.File(mod, "Textures/a.dds", "content");
        byte[] before = File.ReadAllBytes(dds);
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        Equal(1, scan.FileCount);
        True(File.ReadAllBytes(dds).SequenceEqual(before));
        // Cancellation consists of simply not calling Clean; no setting can enable automatic deletion.
        True(File.Exists(dds));
        Equal(0, DdsCleanup.Scan(Array.Empty<InstalledMod>()).FileCount);
        True(!DdsCleanup.Clean(new DdsScan()).RestartNeeded);
    }

    private static void ExactIdsAndCopies(Fixture f)
    {
        string[] ids =
        {
            MainId, MainId + ".Ahmislan", MainId + ".Gakuen", MainId + ".Jurnizia",
            MainId + ".NeoYokohama", MainId + ".Oubliee", MainId + ".YoRHa",
            "Verniy709.FacialAnimationForAnnelitrice3.0Remake"
        };
        List<InstalledMod> mods = new List<InstalledMod>();
        for (int i = 0; i < ids.Length; i++)
        {
            InstalledMod mod = f.Mod("copy" + i, ids[i].ToUpperInvariant());
            f.File(mod, "Textures/a.DDS");
            mods.Add(mod);
        }
        InstalledMod inactive = f.Mod("inactive-second-copy", MainId);
        f.File(inactive, "Textures/a.dds");
        mods.Add(inactive);
        mods.Add(mods[0]);
        DdsScan scan = DdsCleanup.Scan(mods);
        Equal(9, scan.Mods.Count);
        Equal(9, scan.FileCount);
        DdsCleanupResult result = DdsCleanup.Clean(scan);
        Equal(9, result.Deleted);
        True(result.RestartNeeded);
    }

    private static void RefusedIds(Fixture f)
    {
        List<InstalledMod> mods = new List<InstalledMod>();
        foreach (string id in new[] { MainId + ".NotOurs", MainId + "_steam", "X" + MainId, MainId + " " })
        {
            InstalledMod mod = f.Mod("bad" + mods.Count, id);
            f.File(mod, "Textures/a.dds");
            mods.Add(mod);
        }
        InstalledMod mismatch = f.Mod("metadata-mismatch", "Other.Author");
        mismatch.PackageId = MainId;
        f.File(mismatch, "Textures/a.dds");
        mods.Add(mismatch);
        DdsScan scan = DdsCleanup.Scan(mods);
        Equal(0, scan.FileCount);
        True(scan.Warnings.Count > 0);
        foreach (InstalledMod mod in mods) True(File.Exists(Path.Combine(mod.RootDirectory, "Textures/a.dds")));
    }

    private static void TextureScopes(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        foreach (string path in new[] { "Textures/a.dds", "1.7/Textures/b.DdS", "LoadFolders/1.7/Anomaly/Textures/Parts/c.dds", "1.6/Textures/d.dds" })
            f.File(mod, path);
        foreach (string path in new[] { "Source/Textures/a.dds", "AssetBundles/Textures/b.dds", "Assemblies/Textures/c.dds", ".git/Textures/d.dds", "About/Textures/e.dds", "Textures/Source/f.dds", "Other/no.dds", "TexturesLookalike/g.dds" })
            f.File(mod, path);
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        Equal(4, scan.FileCount);
        Equal(4, DdsCleanup.Clean(scan).Deleted);
        True(File.Exists(Path.Combine(mod.RootDirectory, "Textures/Source/f.dds")));
        True(File.Exists(Path.Combine(mod.RootDirectory, "Other/no.dds")));
    }

    private static void SelectiveDeletion(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string dds = f.File(mod, "Textures/a.dds");
        string[] preserved = { f.File(mod, "Textures/a.png"), f.File(mod, "Textures/a.dds.backup"), f.File(mod, "Textures/assets.bundle"), f.File(mod, "AssetBundles/Windows/a.dds") };
        InstalledMod other = f.Mod("unrelated", "Other.Mod");
        string otherDds = f.File(other, "Textures/a.dds");
        DdsCleanupResult result = DdsCleanup.Clean(DdsCleanup.Scan(new[] { mod, other }));
        Equal(1, result.Deleted);
        True(!File.Exists(dds));
        foreach (string path in preserved) True(File.Exists(path));
        True(File.Exists(otherDds));
        True(Directory.Exists(Path.Combine(mod.RootDirectory, "Textures")));
    }

    private static void ChangedAndNewFiles(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string sizeChanged = f.File(mod, "Textures/size.dds", "old");
        string timeChanged = f.File(mod, "Textures/time.dds", "old");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        File.WriteAllText(sizeChanged, "new content longer");
        File.SetLastWriteTimeUtc(timeChanged, File.GetLastWriteTimeUtc(timeChanged).AddMinutes(1));
        string added = f.File(mod, "Textures/added.dds");
        DdsCleanupResult result = DdsCleanup.Clean(scan);
        Equal(0, result.Deleted);
        Equal(2, result.Skipped);
        True(File.Exists(sizeChanged) && File.Exists(timeChanged) && File.Exists(added));
    }

    private static void MissingFiles(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string file = f.File(mod, "Textures/a.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        File.Delete(file);
        DdsCleanupResult result = DdsCleanup.Clean(scan);
        Equal(1, result.Missing);
        Equal(0, result.Deleted);
        True(!result.RestartNeeded);
    }

    private static void MetadataTampering(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string file = f.File(mod, "Textures/a.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        f.Metadata(mod, MainId + ".Gakuen");
        DdsCleanupResult result = DdsCleanup.Clean(scan);
        Equal(1, result.Skipped);
        True(File.Exists(file));
        f.Metadata(mod, MainId);
        scan = DdsCleanup.Scan(new[] { mod });
        scan.Mods[0].Mod.RootDirectory = f.Mod("different", MainId).RootDirectory;
        Equal(1, DdsCleanup.Clean(scan).Skipped);
        True(File.Exists(file));
    }

    private static void TamperedCandidates(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string original = f.File(mod, "Textures/a.dds");
        string outside = f.File(f.Mod("base-near-prefix", MainId), "Textures/outside.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        scan.Mods[0].Files[0].Path = outside;
        scan.Mods[0].Files[0].TextureRoot = Path.GetDirectoryName(outside);
        Equal(1, DdsCleanup.Clean(scan).Skipped);
        True(File.Exists(outside) && File.Exists(original));
        string png = f.File(mod, "Textures/a.png");
        scan = DdsCleanup.Scan(new[] { mod });
        scan.Mods[0].Files[0].Path = png;
        Equal(1, DdsCleanup.Clean(scan).Skipped);
        True(File.Exists(png) && File.Exists(original));
        string source = f.File(mod, "Source/Textures/a.dds");
        scan = DdsCleanup.Scan(new[] { mod });
        scan.Mods[0].Files[0].Path = source;
        scan.Mods[0].Files[0].TextureRoot = Path.GetDirectoryName(source);
        Equal(1, DdsCleanup.Clean(scan).Skipped);
        True(File.Exists(source));
    }

    private static void DuplicateCandidates(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        f.File(mod, "Textures/a.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        scan.Mods[0].Files.Add(scan.Mods[0].Files[0]);
        DdsCleanupResult result = DdsCleanup.Clean(scan);
        Equal(1, result.Deleted);
        Equal(1, result.Skipped);
    }

    private static void LockedFile(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string file = f.File(mod, "Textures/a.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        using (FileStream locked = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            DdsCleanupResult result = DdsCleanup.Clean(scan);
            if (Path.DirectorySeparatorChar == '\\') Equal(1, result.Failed);
            else { conditional++; return; }
            Equal(0, result.Deleted);
            True(File.Exists(file));
        }
    }

    private static void ReadOnlyFile(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string file = f.File(mod, "Textures/a.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        File.SetAttributes(file, File.GetAttributes(file) | FileAttributes.ReadOnly);
        try
        {
            DdsCleanupResult result = DdsCleanup.Clean(scan);
            if (Path.DirectorySeparatorChar != '\\') { conditional++; return; }
            Equal(1, result.Failed);
            True(File.Exists(file));
        }
        finally { if (File.Exists(file)) File.SetAttributes(file, FileAttributes.Normal); }
    }

    private static void FileSymlink(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string target = f.File(f.Mod("outside", "Other.Mod"), "Textures/a.dds");
        string link = Path.Combine(mod.RootDirectory, "Textures/link.dds");
        Directory.CreateDirectory(Path.GetDirectoryName(link));
        if (!TryFileLink(link, target)) return;
        Equal(0, DdsCleanup.Scan(new[] { mod }).FileCount);
        True(File.Exists(target));
        File.Delete(link);
        string original = f.File(mod, "Textures/link.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        File.Delete(original);
        True(TryFileLink(link, target));
        Equal(1, DdsCleanup.Clean(scan).Skipped);
        True(File.Exists(target));
    }

    private static void DirectoryJunction(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        InstalledMod target = f.Mod("outside", MainId);
        string targetFile = f.File(target, "Textures/a.dds");
        string link = Path.Combine(mod.RootDirectory, "Textures");
        if (!TryDirectoryLink(link, Path.GetDirectoryName(targetFile))) return;
        Equal(0, DdsCleanup.Scan(new[] { mod }).FileCount);
        True(File.Exists(targetFile));
        string linkedRoot = Path.Combine(f.Root, "linked-mod");
        True(TryDirectoryLink(linkedRoot, target.RootDirectory));
        Equal(0, DdsCleanup.Scan(new[] { new InstalledMod { Name = "Linked", PackageId = MainId, RootDirectory = linkedRoot } }).FileCount);
    }

    private static void ReplacedDirectory(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string original = f.File(mod, "Textures/Parts/a.dds");
        DdsScan scan = DdsCleanup.Scan(new[] { mod });
        string outside = f.File(f.Mod("outside", "Other.Mod"), "Textures/a.dds");
        File.Delete(original);
        Directory.Delete(Path.GetDirectoryName(original));
        if (!TryDirectoryLink(Path.GetDirectoryName(original), Path.GetDirectoryName(outside))) return;
        Equal(1, DdsCleanup.Clean(scan).Skipped);
        True(File.Exists(outside));
    }

    private static void LinkedMetadata(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        f.File(mod, "Textures/a.dds");
        string target = Path.Combine(f.Mod("outside", MainId).RootDirectory, "About/About.xml");
        string link = Path.Combine(mod.RootDirectory, "About/About.xml");
        File.Delete(link);
        if (!TryFileLink(link, target)) return;
        Equal(0, DdsCleanup.Scan(new[] { mod }).FileCount);
        True(File.Exists(target));
    }

    private static void LinkedAncestor(Fixture f)
    {
        InstalledMod target = f.Mod("real-parent/base", MainId);
        string targetFile = f.File(target, "Textures/a.dds");
        string link = Path.Combine(f.Root, "linked-parent");
        if (!TryDirectoryLink(link, Path.Combine(f.Root, "real-parent"))) return;
        InstalledMod linked = new InstalledMod { Name = "Linked ancestor", PackageId = MainId, RootDirectory = Path.Combine(link, "base") };
        Equal(0, DdsCleanup.Scan(new[] { linked }).FileCount);
        True(File.Exists(targetFile));
    }

    private static void InvalidMetadata(Fixture f)
    {
        InstalledMod mod = f.Mod("base", MainId);
        string file = f.File(mod, "Textures/a.dds");
        string metadata = Path.Combine(mod.RootDirectory, "About/About.xml");
        File.WriteAllText(metadata, "<ModMetaData><packageId>");
        Equal(0, DdsCleanup.Scan(new[] { mod }).FileCount);
        File.WriteAllText(metadata, "<!DOCTYPE ModMetaData [<!ENTITY external SYSTEM 'file:///not-read'>]><ModMetaData><packageId>&external;</packageId></ModMetaData>");
        Equal(0, DdsCleanup.Scan(new[] { mod }).FileCount);
        True(File.Exists(file));
    }

    private static void DiscoverMissingCopies(Fixture f)
    {
        InstalledMod registered = f.Mod("local/registered", MainId);
        InstalledMod duplicate = f.Mod("local/unregistered-copy", MainId);
        InstalledMod workshopRegistered = f.Mod("workshop/111", MainId + ".Gakuen");
        InstalledMod workshopDuplicate = f.Mod("workshop/222", MainId + ".Gakuen");
        foreach (InstalledMod mod in new[] { registered, duplicate, workshopRegistered, workshopDuplicate }) f.File(mod, "Textures/a.dds");
        DdsScan scan = DdsDiscovery.ScanAllInstalledCopies(new[] { registered, workshopRegistered, registered }, Path.Combine(f.Root, "local"));
        Equal(4, scan.Mods.Count);
        Equal(4, scan.FileCount);
        True(File.Exists(Path.Combine(duplicate.RootDirectory, "Textures/a.dds")));
    }

    private static void DiscoverViaOtherMod(Fixture f)
    {
        InstalledMod other = f.Mod("workshop/other", "Other.Author");
        InstalledMod ours = f.Mod("workshop/ours-inactive", MainId + ".YoRHa");
        string oursFile = f.File(ours, "1.7/Textures/a.dds");
        string otherFile = f.File(other, "Textures/a.dds");
        DdsScan scan = DdsDiscovery.ScanAllInstalledCopies(new[] { other }, null);
        Equal(1, scan.FileCount);
        Equal(1, DdsCleanup.Clean(scan).Deleted);
        True(!File.Exists(oursFile) && File.Exists(otherFile));
    }

    private static void DiscoverExplicitLocal(Fixture f)
    {
        InstalledMod ours = f.Mod("local/ours-inactive", MainId + ".Oubliee");
        f.File(ours, "Textures/a.dds");
        Equal(1, DdsDiscovery.ScanAllInstalledCopies(Array.Empty<InstalledMod>(), Path.Combine(f.Root, "local")).FileCount);
    }

    private static void DiscoveryBoundaries(Fixture f)
    {
        InstalledMod near = f.Mod("local/near", MainId + ".NotOurs");
        InstalledMod nested = f.Mod("local/unrelated-container/nested", MainId);
        InstalledMod direct = f.Mod("local/direct", MainId);
        string nearFile = f.File(near, "Textures/a.dds");
        string nestedFile = f.File(nested, "Textures/a.dds");
        string directFile = f.File(direct, "Textures/a.dds");
        DdsScan scan = DdsDiscovery.ScanAllInstalledCopies(Array.Empty<InstalledMod>(), Path.Combine(f.Root, "local"));
        Equal(1, scan.FileCount);
        Equal(1, DdsCleanup.Clean(scan).Deleted);
        True(File.Exists(nearFile) && File.Exists(nestedFile) && !File.Exists(directFile));
    }

    private static void DiscoveryWarnings(Fixture f)
    {
        InstalledMod known = f.Mod("local/known", MainId);
        f.File(known, "Textures/a.dds");
        File.WriteAllText(Path.Combine(known.RootDirectory, "About/About.xml"), "broken");
        DdsScan scan = DdsDiscovery.ScanAllInstalledCopies(new[] { known }, Path.Combine(f.Root, "does-not-exist"));
        Equal(0, scan.FileCount);
        True(scan.Warnings.Count >= 2);
        True(scan.Warnings.Any(message => message.Contains("known")));
        True(scan.Warnings.Any(message => message.Contains("does-not-exist")));
    }

    private static void DiscoveryLinks(Fixture f)
    {
        InstalledMod target = f.Mod("real-container/ours", MainId);
        string file = f.File(target, "Textures/a.dds");
        string linkedContainer = Path.Combine(f.Root, "linked-container");
        if (!TryDirectoryLink(linkedContainer, Path.Combine(f.Root, "real-container"))) return;
        Equal(0, DdsDiscovery.ScanAllInstalledCopies(Array.Empty<InstalledMod>(), linkedContainer).FileCount);
        Directory.CreateDirectory(Path.Combine(f.Root, "local"));
        True(TryDirectoryLink(Path.Combine(f.Root, "local/linked-mod"), target.RootDirectory));
        Equal(0, DdsDiscovery.ScanAllInstalledCopies(Array.Empty<InstalledMod>(), Path.Combine(f.Root, "local")).FileCount);
        True(File.Exists(file));
    }

    private static bool TryFileLink(string link, string target)
    {
        try { File.CreateSymbolicLink(link, target); return true; }
        catch (Exception exception) when (exception is UnauthorizedAccessException || exception is IOException || exception is PlatformNotSupportedException)
        { conditional++; Console.WriteLine("  CONDITIONAL: file symlink unavailable: " + exception.Message); return false; }
    }

    private static bool TryDirectoryLink(string link, string target)
    {
        link = Path.GetFullPath(link);
        target = Path.GetFullPath(target);
        try
        {
            Directory.CreateSymbolicLink(link, target);
            return true;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException || exception is IOException || exception is PlatformNotSupportedException)
        {
            if (Path.DirectorySeparatorChar != '\\') { conditional++; return false; }
            // A Windows junction does not require the symlink privilege. Both arguments are fixture-only paths.
            ProcessStartInfo info = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            info.ArgumentList.Add("/d"); info.ArgumentList.Add("/c"); info.ArgumentList.Add("mklink"); info.ArgumentList.Add("/J"); info.ArgumentList.Add(link); info.ArgumentList.Add(target);
            using (Process process = Process.Start(info))
            {
                process.StandardOutput.ReadToEnd(); process.StandardError.ReadToEnd(); process.WaitForExit();
                if (process.ExitCode == 0) return true;
            }
            conditional++; Console.WriteLine("  CONDITIONAL: directory link unavailable."); return false;
        }
    }

    private static void True(bool value) { if (!value) throw new Exception("Assertion failed."); }
    private static void Equal(int expected, int actual) { if (expected != actual) throw new Exception("Expected " + expected + ", received " + actual + "."); }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } = Path.Combine(FixtureParent, Guid.NewGuid().ToString("N"));
        public Fixture() { Directory.CreateDirectory(Root); }
        public InstalledMod Mod(string directory, string id)
        {
            InstalledMod mod = new InstalledMod { Name = directory, PackageId = id, RootDirectory = Path.Combine(Root, directory) };
            Directory.CreateDirectory(mod.RootDirectory);
            Metadata(mod, id);
            return mod;
        }
        public void Metadata(InstalledMod mod, string id)
        {
            Directory.CreateDirectory(Path.Combine(mod.RootDirectory, "About"));
            System.IO.File.WriteAllText(Path.Combine(mod.RootDirectory, "About/About.xml"), "<ModMetaData><name>Fixture</name><packageId>" + id + "</packageId></ModMetaData>");
        }
        public string File(InstalledMod mod, string relative, string content = "fixture-dds")
        {
            string path = Path.Combine(mod.RootDirectory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, content);
            return path;
        }
        public void Dispose()
        {
            string resolved = Path.GetFullPath(Root);
            string prefix = Path.GetFullPath(FixtureParent).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!resolved.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || resolved == FixtureParent)
                throw new Exception("Refused fixture teardown outside the fixture parent: " + resolved);
            DeleteFixtureDirectory(resolved);
        }
        private static void DeleteFixtureDirectory(string path)
        {
            // Remove links themselves, never recurse into their targets.
            if ((System.IO.File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) { Directory.Delete(path); return; }
            foreach (string entry in Directory.GetFileSystemEntries(path))
            {
                FileAttributes attributes = System.IO.File.GetAttributes(entry);
                if ((attributes & FileAttributes.Directory) != 0) DeleteFixtureDirectory(entry);
                else { System.IO.File.SetAttributes(entry, FileAttributes.Normal); System.IO.File.Delete(entry); }
            }
            Directory.Delete(path);
        }
    }
}
