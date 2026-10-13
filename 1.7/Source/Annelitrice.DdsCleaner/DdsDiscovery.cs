using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Annelitrice.DdsCleaner
{
    public static class DdsDiscovery
    {
        private static readonly StringComparer PathComparer = Path.DirectorySeparatorChar == '\\'
            ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        private static readonly StringComparison PathComparison = Path.DirectorySeparatorChar == '\\'
            ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        // Call on the background scan thread. No filesystem mutation occurs here.
        public static DdsScan ScanAllInstalledCopies(IEnumerable<InstalledMod> registry, string localModsDirectory)
        {
            List<InstalledMod> approved = new List<InstalledMod>();
            HashSet<string> roots = new HashSet<string>(PathComparer);
            HashSet<string> containers = new HashSet<string>(PathComparer);
            List<string> warnings = new List<string>();

            if (registry != null)
            {
                foreach (InstalledMod installed in registry)
                {
                    if (installed == null) continue;
                    // Keep known approved records even when their About.xml cannot be read.
                    // DdsCleanup.Scan will then report its normal validation warning.
                    bool isApproved = DdsCleanup.IsApprovedPackageId(installed.PackageId);
                    try
                    {
                        string root = FullPath(installed.RootDirectory);
                        if (isApproved && roots.Add(root))
                            approved.Add(new InstalledMod { PackageId = installed.PackageId, Name = installed.Name, RootDirectory = root });
                        DirectoryInfo parent = new DirectoryInfo(root).Parent;
                        if (parent != null) containers.Add(FullPath(parent.FullName));
                    }
                    catch (Exception exception) when (IsDiscoveryError(exception))
                    {
                        if (isApproved) approved.Add(installed);
                        warnings.Add("Could not inspect a registered mod location: " + exception.Message);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(localModsDirectory))
            {
                try { containers.Add(FullPath(localModsDirectory)); }
                catch (Exception exception) when (IsDiscoveryError(exception))
                { warnings.Add("Could not inspect the local Mods directory: " + exception.Message); }
            }

            List<string> orderedContainers = new List<string>(containers);
            orderedContainers.Sort(PathComparer);
            foreach (string container in orderedContainers)
            {
                string[] children;
                try
                {
                    VerifyOrdinaryDirectoryPath(container);
                    children = Directory.GetDirectories(container);
                    Array.Sort(children, PathComparer);
                }
                catch (Exception exception) when (IsDiscoveryError(exception))
                {
                    warnings.Add("Could not scan installed mod copies in " + container + ": " + exception.Message);
                    continue;
                }

                foreach (string child in children)
                {
                    try
                    {
                        string root = FullPath(child);
                        DirectoryInfo parent = new DirectoryInfo(root).Parent;
                        if (parent == null || !string.Equals(FullPath(parent.FullName), container, PathComparison)) continue;
                        if (roots.Contains(root)) continue;
                        VerifyOrdinaryDirectoryPath(root);
                        InstalledMod discovered = ReadApprovedMetadata(root);
                        if (discovered != null && roots.Add(root)) approved.Add(discovered);
                    }
                    catch (Exception exception) when (IsDiscoveryError(exception))
                    {
                        // Most children belong to unrelated mods or are not mod folders.
                        // Known approved roots were retained above and receive Scan warnings.
                    }
                }
            }

            // Apply the same package, texture, metadata, and reparse validation to every copy.
            DdsScan scan = DdsCleanup.Scan(approved);
            scan.Warnings.InsertRange(0, warnings);
            return scan;
        }

        private static InstalledMod ReadApprovedMetadata(string root)
        {
            string about = Path.Combine(root, "About");
            VerifyOrdinaryDirectoryPath(about);
            string metadataPath = Path.Combine(about, "About.xml");
            FileAttributes attributes = File.GetAttributes(metadataPath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                throw new IOException("The mod metadata is a link or directory.");
            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 1024 * 1024
            };
            XmlDocument document = new XmlDocument { XmlResolver = null };
            using (XmlReader reader = XmlReader.Create(metadataPath, settings)) document.Load(reader);
            string packageId = document.DocumentElement?.SelectSingleNode("packageId")?.InnerText.Trim();
            if (!DdsCleanup.IsApprovedPackageId(packageId)) return null;
            string name = document.DocumentElement?.SelectSingleNode("name")?.InnerText.Trim();
            return new InstalledMod { PackageId = packageId, Name = string.IsNullOrEmpty(name) ? packageId : name, RootDirectory = root };
        }

        private static void VerifyOrdinaryDirectoryPath(string directory)
        {
            string current = FullPath(directory);
            while (current != null)
            {
                FileAttributes attributes = File.GetAttributes(current);
                if ((attributes & FileAttributes.Directory) == 0 || (attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("The directory path contains a link or non-directory: " + current);
                current = Path.GetDirectoryName(current);
            }
        }

        private static string FullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path)) throw new IOException("An absolute directory path is required.");
            string full = Path.GetFullPath(path);
            if (string.Equals(full, Path.GetPathRoot(full), PathComparison)) return full;
            return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static bool IsDiscoveryError(Exception exception)
        {
            return exception is IOException || exception is UnauthorizedAccessException
                || exception is System.Security.SecurityException || exception is ArgumentException
                || exception is NotSupportedException || exception is XmlException;
        }
    }
}
