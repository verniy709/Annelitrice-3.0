using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Annelitrice.DdsCleaner
{
    public sealed class InstalledMod
    {
        public string PackageId { get; set; }
        public string Name { get; set; }
        public string RootDirectory { get; set; }
    }

    public sealed class DdsScan
    {
        public List<ModScan> Mods { get; } = new List<ModScan>();
        public List<string> Warnings { get; } = new List<string>();
        public int FileCount
        {
            get
            {
                int count = 0;
                foreach (ModScan mod in Mods)
                    if (mod != null) count += mod.Files.Count;
                return count;
            }
        }
    }

    public sealed class ModScan
    {
        public InstalledMod Mod { get; internal set; }
        public List<DdsCandidate> Files { get; } = new List<DdsCandidate>();
        internal string ApprovedRoot { get; set; }
        internal string ApprovedPackageId { get; set; }
    }

    public sealed class DdsCandidate
    {
        public string Path { get; internal set; }
        public string TextureRoot { get; internal set; }
        public long Length { get; internal set; }
        public long LastWriteTimeUtcTicks { get; internal set; }
    }

    public sealed class DdsCleanupResult
    {
        public int Deleted { get; internal set; }
        public int Missing { get; internal set; }
        public int Skipped { get; internal set; }
        public int Failed { get; internal set; }
        public List<string> Messages { get; } = new List<string>();
        public bool RestartNeeded => Deleted > 0;
    }

    // Scan is read-only. Call Clean only after the player approves this specific scan.
    public static class DdsCleanup
    {
        private static readonly HashSet<string> PackageIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Verniy709.Annelitrice3.0",
            "Verniy709.Annelitrice3.0.Ahmislan",
            "Verniy709.Annelitrice3.0.Gakuen",
            "Verniy709.Annelitrice3.0.Jurnizia",
            "Verniy709.Annelitrice3.0.NeoYokohama",
            "Verniy709.Annelitrice3.0.Oubliee",
            "Verniy709.Annelitrice3.0.YoRHa",
            "Verniy709.FacialAnimationForAnnelitrice3.0Remake"
        };

        private static readonly HashSet<string> ExcludedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git", ".svn", "Source", "Assemblies", "About", "AssetBundles"
        };

        private static readonly StringComparison PathComparison =
            System.IO.Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        private static readonly StringComparer PathComparer =
            System.IO.Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        public static bool IsApprovedPackageId(string packageId)
        {
            return packageId != null && PackageIds.Contains(packageId);
        }

        public static DdsScan Scan(IEnumerable<InstalledMod> installedMods)
        {
            DdsScan scan = new DdsScan();
            HashSet<string> roots = new HashSet<string>(PathComparer);
            HashSet<string> files = new HashSet<string>(PathComparer);
            if (installedMods == null) return scan;

            foreach (InstalledMod installed in installedMods)
            {
                if (installed == null || !IsApprovedPackageId(installed.PackageId)) continue;
                string root;
                try
                {
                    root = FullPath(installed.RootDirectory);
                    if (roots.Contains(root)) continue;
                    VerifyPackage(root, installed.PackageId);
                    roots.Add(root);
                }
                catch (Exception exception) when (IsFileSystemError(exception))
                {
                    scan.Warnings.Add("Skipped mod " + (installed.Name ?? installed.PackageId) + ": " + exception.Message);
                    continue;
                }

                ModScan mod = new ModScan
                {
                    Mod = new InstalledMod { PackageId = installed.PackageId, Name = installed.Name ?? installed.PackageId, RootDirectory = root },
                    ApprovedRoot = root,
                    ApprovedPackageId = installed.PackageId
                };
                VisitDirectory(root, root, null, mod, scan, files);
                if (mod.Files.Count > 0) scan.Mods.Add(mod);
            }
            return scan;
        }

        public static DdsCleanupResult Clean(DdsScan scan)
        {
            DdsCleanupResult result = new DdsCleanupResult();
            HashSet<string> processed = new HashSet<string>(PathComparer);
            if (scan == null) return result;

            foreach (ModScan mod in scan.Mods)
            {
                if (mod == null) continue;
                try
                {
                    if (mod.Mod == null || !SamePath(FullPath(mod.Mod.RootDirectory), mod.ApprovedRoot)
                        || !string.Equals(mod.Mod.PackageId, mod.ApprovedPackageId, StringComparison.OrdinalIgnoreCase))
                        throw new IOException("The mod location or package ID changed after the scan.");
                    VerifyPackage(mod.ApprovedRoot, mod.ApprovedPackageId);
                }
                catch (Exception exception) when (IsFileSystemError(exception))
                {
                    result.Skipped += mod.Files.Count;
                    result.Messages.Add("Skipped mod " + (mod.Mod?.Name ?? "(unknown)") + ": " + exception.Message);
                    continue;
                }

                foreach (DdsCandidate candidate in mod.Files)
                {
                    string path = candidate?.Path ?? "(invalid candidate)";
                    try
                    {
                        if (candidate == null) throw new IOException("Invalid file candidate.");
                        path = FullPath(candidate.Path);
                        if (!processed.Add(path))
                        {
                            result.Skipped++;
                            result.Messages.Add("Skipped duplicate file: " + path);
                            continue;
                        }
                        string textureRoot = FullPath(candidate.TextureRoot);
                        if (!IsDescendant(textureRoot, mod.ApprovedRoot)
                            || !string.Equals(new DirectoryInfo(textureRoot).Name, "Textures", StringComparison.OrdinalIgnoreCase)
                            || !IsDescendant(path, textureRoot)
                            || !string.Equals(System.IO.Path.GetExtension(path), ".dds", StringComparison.OrdinalIgnoreCase)
                            || HasExcludedDirectory(path, mod.ApprovedRoot)
                            || HasNestedModRoot(System.IO.Path.GetDirectoryName(path), mod.ApprovedRoot))
                            throw new IOException("The file is outside the approved texture scope.");

                        // Check every directory again: a folder may have become a junction since the notice.
                        VerifyDirectoryChain(System.IO.Path.GetDirectoryName(path), mod.ApprovedRoot);
                        FileAttributes attributes;
                        try
                        {
                            attributes = File.GetAttributes(path);
                        }
                        catch (FileNotFoundException)
                        {
                            result.Missing++;
                            result.Messages.Add("Already absent: " + path);
                            continue;
                        }
                        if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0)
                            throw new IOException("The candidate became a link or directory.");
                        FileInfo info = new FileInfo(path);
                        if (info.Length != candidate.Length || info.LastWriteTimeUtc.Ticks != candidate.LastWriteTimeUtcTicks)
                        {
                            result.Skipped++;
                            result.Messages.Add("Skipped file changed after the scan: " + path);
                            continue;
                        }
                        try
                        {
                            File.Delete(path);
                            result.Deleted++;
                            result.Messages.Add("Deleted: " + path);
                        }
                        catch (Exception exception) when (IsFileSystemError(exception))
                        {
                            result.Failed++;
                            result.Messages.Add("Could not delete " + path + ": " + exception.Message);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        result.Missing++;
                        result.Messages.Add("Already absent: " + path);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        result.Missing++;
                        result.Messages.Add("Already absent: " + path);
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        result.Failed++;
                        result.Messages.Add("Could not delete " + path + ": " + exception.Message);
                    }
                    catch (Exception exception) when (IsFileSystemError(exception))
                    {
                        result.Skipped++;
                        result.Messages.Add("Skipped " + path + ": " + exception.Message);
                    }
                }
            }
            return result;
        }

        private static void VisitDirectory(string directory, string modRoot, string textureRoot,
            ModScan mod, DdsScan scan, HashSet<string> seenFiles)
        {
            string[] entries;
            try
            {
                VerifyDirectoryChain(directory, modRoot);
                if (!SamePath(directory, modRoot) && File.Exists(System.IO.Path.Combine(directory, "About", "About.xml")))
                {
                    scan.Warnings.Add("Skipped nested mod folder: " + directory);
                    return;
                }
                if (textureRoot == null && !SamePath(directory, modRoot)
                    && string.Equals(new DirectoryInfo(directory).Name, "Textures", StringComparison.OrdinalIgnoreCase))
                    textureRoot = directory;
                entries = Directory.GetFileSystemEntries(directory);
                Array.Sort(entries, PathComparer);
            }
            catch (Exception exception) when (IsFileSystemError(exception))
            {
                scan.Warnings.Add("Skipped folder " + directory + ": " + exception.Message);
                return;
            }

            foreach (string entry in entries)
            {
                try
                {
                    FileAttributes attributes = File.GetAttributes(entry);
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        scan.Warnings.Add("Skipped link: " + entry);
                        continue;
                    }
                    if ((attributes & FileAttributes.Directory) != 0)
                    {
                        if (!ExcludedDirectories.Contains(System.IO.Path.GetFileName(entry)))
                            VisitDirectory(entry, modRoot, textureRoot, mod, scan, seenFiles);
                        continue;
                    }
                    if (textureRoot == null || !string.Equals(System.IO.Path.GetExtension(entry), ".dds", StringComparison.OrdinalIgnoreCase)) continue;
                    string path = FullPath(entry);
                    if (!IsDescendant(path, textureRoot) || !seenFiles.Add(path)) continue;
                    VerifyDirectoryChain(System.IO.Path.GetDirectoryName(path), modRoot);
                    FileInfo info = new FileInfo(path);
                    if ((info.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                    mod.Files.Add(new DdsCandidate
                    {
                        Path = path,
                        TextureRoot = textureRoot,
                        Length = info.Length,
                        LastWriteTimeUtcTicks = info.LastWriteTimeUtc.Ticks
                    });
                }
                catch (Exception exception) when (IsFileSystemError(exception))
                {
                    scan.Warnings.Add("Skipped path " + entry + ": " + exception.Message);
                }
            }
        }

        private static void VerifyPackage(string root, string expectedId)
        {
            if (!IsApprovedPackageId(expectedId)) throw new IOException("The package ID is not approved for cleanup.");
            VerifyRootAncestors(root);
            VerifyDirectoryChain(root, root);
            string aboutDirectory = System.IO.Path.Combine(root, "About");
            VerifyDirectoryChain(aboutDirectory, root);
            string aboutFile = System.IO.Path.Combine(aboutDirectory, "About.xml");
            FileAttributes attributes = File.GetAttributes(aboutFile);
            if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0)
                throw new IOException("The mod metadata is a link or directory.");
            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 1024 * 1024
            };
            XmlDocument document = new XmlDocument { XmlResolver = null };
            using (XmlReader reader = XmlReader.Create(aboutFile, settings)) document.Load(reader);
            string actualId = document.DocumentElement?.SelectSingleNode("packageId")?.InnerText.Trim();
            if (!string.Equals(actualId, expectedId, StringComparison.OrdinalIgnoreCase) || !IsApprovedPackageId(actualId))
                throw new IOException("The package ID in About/About.xml does not match the approved mod.");
        }

        private static void VerifyDirectoryChain(string directory, string root)
        {
            string current = FullPath(directory);
            if (!SamePath(current, root) && !IsDescendant(current, root))
                throw new IOException("The folder is outside the approved mod.");
            while (true)
            {
                FileAttributes attributes = File.GetAttributes(current);
                if ((attributes & FileAttributes.Directory) == 0 || (attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("The folder is not an ordinary directory: " + current);
                if (SamePath(current, root)) return;
                current = System.IO.Path.GetDirectoryName(current);
                if (current == null) throw new IOException("The mod boundary could not be verified.");
            }
        }

        private static void VerifyRootAncestors(string root)
        {
            // Checking only the final mod directory would miss a junction in its parent path.
            string current = FullPath(root);
            while (current != null)
            {
                FileAttributes attributes = File.GetAttributes(current);
                if ((attributes & FileAttributes.Directory) == 0 || (attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("The mod path contains a link or non-directory: " + current);
                current = System.IO.Path.GetDirectoryName(current);
            }
        }

        private static bool HasExcludedDirectory(string path, string root)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            while (!SamePath(directory, root))
            {
                if (directory == null || !IsDescendant(directory, root)) return true;
                if (ExcludedDirectories.Contains(System.IO.Path.GetFileName(directory))) return true;
                directory = System.IO.Path.GetDirectoryName(directory);
            }
            return false;
        }

        private static bool HasNestedModRoot(string directory, string root)
        {
            while (!SamePath(directory, root))
            {
                if (directory == null || !IsDescendant(directory, root)) return true;
                if (File.Exists(System.IO.Path.Combine(directory, "About", "About.xml"))) return true;
                directory = System.IO.Path.GetDirectoryName(directory);
            }
            return false;
        }

        private static string FullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.Path.IsPathRooted(path))
                throw new IOException("An absolute path is required.");
            string full = System.IO.Path.GetFullPath(path);
            string pathRoot = System.IO.Path.GetPathRoot(full);
            if (SamePath(full, pathRoot)) return full;
            return full.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        }

        private static bool SamePath(string first, string second)
        {
            return first != null && second != null && string.Equals(first, second, PathComparison);
        }

        private static bool IsDescendant(string path, string root)
        {
            if (path == null || root == null) return false;
            string prefix = root.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? root : root + System.IO.Path.DirectorySeparatorChar;
            return path.StartsWith(prefix, PathComparison);
        }

        private static bool IsFileSystemError(Exception exception)
        {
            return exception is IOException || exception is UnauthorizedAccessException
                || exception is System.Security.SecurityException || exception is ArgumentException
                || exception is NotSupportedException || exception is XmlException;
        }
    }
}
