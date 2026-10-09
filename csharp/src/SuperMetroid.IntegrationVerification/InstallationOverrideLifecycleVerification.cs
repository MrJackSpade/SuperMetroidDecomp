using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

/// <summary>
/// Installer acceptance for the source-inventoried presentation domains. Import is
/// the only cartridge consumer; these checks never advance gameplay to find reads.
/// All files belong to a fresh test-owned directory, not the player's installation.
/// </summary>
internal static partial class InstallationOverrideLifecycleVerification
{
    /// <summary>Exercises override persistence across fresh startup, stock repair, and extraction-format upgrade.</summary>
    /// <param name="sourceRom">ROM image copied into the isolated test installation for extraction.</param>
    /// <returns>Zero after all lifecycle assertions pass.</returns>
    internal static int Run(string sourceRom)
    {
        string temporary = Directory.CreateTempSubdirectory("SuperMetroid-override-lifecycle-").FullName;
        try
        {
            string input = Path.Combine(temporary, "import-input.smc");
            File.Copy(sourceRom, input);
            GameInstallation installation = GameAssetInstaller.Install(input, Path.Combine(temporary, "installation"));
            Dictionary<string, string> stockFiles = FileSnapshot(installation.ContentDirectory);
            Dictionary<string, string> baseline = CatalogSnapshot(installation);
            string[] domains = CopyEveryOverrideDomain(installation);
            AssertSnapshot(baseline, CatalogSnapshot(installation), "stock-equivalent override selection");
            OverrideExpectations edits = EditPresentation(installation);
            Dictionary<string, string> selected = CatalogSnapshot(installation);
            AssertOnlyEditedCatalogsChanged(baseline, selected);
            Dictionary<string, string> overrideFiles = FileSnapshot(Path.Combine(installation.Root, "overrides"));
            AssertSnapshot(stockFiles, FileSnapshot(installation.ContentDirectory), "editing overrides leaves stock untouched");

            string heldRom = Path.Combine(temporary, "held-import-copy.smc");
            File.Move(installation.RomPath, heldRom);
            File.Delete(input);
            Assert(GameAssetInstaller.OpenOrRepair(installation.Root) is not null,
                "complete overridden installation opens with source and installed ROM unavailable");
            VerifySelected(installation, edits, selected, overrideFiles, "ROM-unavailable startup");
            using (var session = new SuperMetroid.Android.AndroidSessionData(installation.Root))
            {
                Assert(session.ContentIdentity.MapContentSha256 == selected[nameof(GameInstallation.LoadMaps)] &&
                    session.ContentIdentity.AudioContentSha256 == selected[nameof(GameInstallation.LoadAudio)],
                    "fresh portable Android session binds selected map/font and audio identities");
                Assert(session.Bus.GetType().GetProperty("Rom") is null,
                    "fresh portable session retains no cartridge payload");
            }
            File.Move(heldRom, installation.RomPath);

            // Exercise the real publication transaction, not manual replacement of
            // one asset directory. Repair must preserve every override file/domain.
            File.WriteAllText(Path.Combine(installation.StandardObjectDirectory,
                StandardObjectArtworkFormat.FileName), "broken stock PNG");
            Assert(GameAssetInstaller.TryOpenExtractedContent(installation.Root) is null,
                "damaged stock is detected even with a complete valid override");
            Assert(GameAssetInstaller.EnsureInstalled(installation.Root) is not null, "stock repair succeeds");
            AssertSnapshot(stockFiles, FileSnapshot(installation.ContentDirectory), "repair regenerates exact stock");
            VerifySelected(installation, edits, selected, overrideFiles, "stock repair");

            // A previous installation format uses the same import/update path as a
            // shipped extraction upgrade. Preserve both authored edits and extras.
            string receiptPath = Path.Combine(installation.ContentDirectory, GameInstallationLayout.ReceiptFileName);
            JsonObject receipt = JsonNode.Parse(File.ReadAllText(receiptPath))?.AsObject()
                ?? throw new InvalidDataException("Installation receipt is null.");
            receipt["FormatVersion"] = GameInstallationLayout.FormatVersion - 1;
            File.WriteAllText(receiptPath, receipt.ToJsonString());
            Assert(GameAssetInstaller.EnsureInstalled(installation.Root) is not null, "outdated extraction format is rebuilt");
            AssertSnapshot(stockFiles, FileSnapshot(installation.ContentDirectory), "upgrade regenerates exact stock");
            VerifySelected(installation, edits, selected, overrideFiles, "extraction upgrade");

            VerifyInvalidOverrideIsRetained(installation, overrideFiles);
            Console.WriteLine($"PASS override lifecycle: {domains.Length} installed directories, {selected.Count} catalog loaders, " +
                $"{overrideFiles.Count} override files; PNG/font/palette/WAV/authored-SFX/instrument edits survive " +
                "ROM-unavailable startup, fresh host binding, stock repair and extraction upgrade. No gameplay probes.");
            return 0;
        }
        finally
        {
            Directory.Delete(temporary, recursive: true);
        }
    }

    /// <summary>Copies stock files into every declared override directory and adds an unrecognized user file.</summary>
    /// <param name="installation">The installed content whose domain files seed the override tree.</param>
    /// <returns>Names of the override directories copied from installed content.</returns>
    private static string[] CopyEveryOverrideDomain(GameInstallation installation)
    {
        string[] directories = typeof(GameInstallation).GetProperties()
            .Where(property => property.PropertyType == typeof(string) &&
                property.Name.EndsWith("OverrideDirectory", StringComparison.Ordinal))
            .Select(property => Path.GetFileName((string)property.GetValue(installation)!))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        string[] declared = typeof(GameInstallationLayout).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string) && field.Name.EndsWith("DirectoryName", StringComparison.Ordinal) &&
                field.Name != nameof(GameInstallationLayout.ContentDirectoryName))
            .Select(field => (string)field.GetValue(null)!).Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal).ToArray();
        Assert(directories.SequenceEqual(declared), "every declared installed domain exposes a persistent override directory");
        foreach (string directory in directories)
        foreach (string file in Directory.EnumerateFiles(Path.Combine(installation.ContentDirectory, directory), "*", SearchOption.AllDirectories))
        {
            string target = Path.Combine(installation.Root, "overrides", directory,
                Path.GetRelativePath(Path.Combine(installation.ContentDirectory, directory), file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
        File.WriteAllText(Path.Combine(installation.Root, "overrides", "artist-note.txt"), "preserve unrecognized user files");
        return directories;
    }

    /// <summary>Loads each parameterless catalog API and records the content identity it selected.</summary>
    /// <param name="installation">Installation used as the receiver for catalog loaders.</param>
    /// <returns>A map from loader name to the selected catalog's content identity.</returns>
    private static Dictionary<string, string> CatalogSnapshot(GameInstallation installation)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        // This reflection is verification-only: enumerate declared loading APIs so
        // a future catalog cannot silently escape the persistence acceptance gate.
        foreach (MethodInfo loader in typeof(GameInstallation).GetMethods()
                     .Where(method => method.Name.StartsWith("Load", StringComparison.Ordinal) &&
                         method.GetParameters().Length == 0).OrderBy(method => method.Name, StringComparer.Ordinal))
        {
            object catalog;
            try { catalog = loader.Invoke(installation, null)!; }
            catch (TargetInvocationException error) when (error.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
            string identity = catalog is InstalledProjectilePresentation projectile
                ? projectile.SelectedSha256
                : catalog.GetType().GetProperty("ContentIdentity")?.GetValue(catalog) as string
                    ?? throw new InvalidDataException($"Catalog {loader.Name} lacks selected-content identity.");
            result.Add(loader.Name, identity);
        }
        return result;
    }

    /// <summary>Hashes every file beneath a directory, keyed by its path relative to that directory.</summary>
    /// <param name="root">Directory whose recursive file contents are captured.</param>
    /// <returns>Relative file paths paired with uppercase SHA-256 digests.</returns>
    private static Dictionary<string, string> FileSnapshot(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(file => Path.GetRelativePath(root, file),
                file => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))), StringComparer.Ordinal);

    /// <summary>Requires two file or catalog snapshots to contain identical keys and values.</summary>
    /// <param name="expected">Reference snapshot.</param>
    /// <param name="actual">Snapshot produced by the operation under verification.</param>
    /// <param name="context">Scenario label included in assertion failures.</param>
    private static void AssertSnapshot(Dictionary<string, string> expected, Dictionary<string, string> actual, string context)
    {
        Assert(expected.Count == actual.Count, context + ": file/catalog count");
        foreach ((string key, string digest) in expected)
            Assert(actual.TryGetValue(key, out string? selected) && selected == digest, context + ": " + key);
    }

    /// <summary>Checks that presentation edits affect only the catalog loaders expected to consume them.</summary>
    /// <param name="baseline">Catalog identities before override edits.</param>
    /// <param name="selected">Catalog identities after the edits are selected.</param>
    private static void AssertOnlyEditedCatalogsChanged(Dictionary<string, string> baseline, Dictionary<string, string> selected)
    {
        string[] changed = [nameof(GameInstallation.LoadMaps), nameof(GameInstallation.LoadAudio),
            nameof(GameInstallation.LoadStandardObjects), nameof(GameInstallation.LoadGameplayBasePalettes)];
        foreach ((string loader, string digest) in baseline)
            Assert((selected[loader] != digest) == changed.Contains(loader), "isolated presentation edits: " + loader);
    }

    /// <summary>Confirms malformed user content is reported and retained instead of being replaced by stock repair.</summary>
    /// <param name="installation">Installation containing the override under examination.</param>
    /// <param name="expected">Snapshot of all override files before the malformed-file probe.</param>
    private static void VerifyInvalidOverrideIsRetained(GameInstallation installation, Dictionary<string, string> expected)
    {
        string path = Path.Combine(installation.StandardObjectOverrideDirectory, StandardObjectArtworkFormat.FileName);
        byte[] valid = File.ReadAllBytes(path);
        File.WriteAllText(path, "invalid user PNG");
        try
        {
            Assert(GameAssetInstaller.OpenOrRepair(installation.Root) is not null,
                "an invalid override does not trigger destructive stock repair");
            try
            {
                _ = installation.LoadStandardObjects();
                throw new InvalidOperationException("Invalid override silently fell back to stock.");
            }
            catch (InvalidDataException error)
            {
                Assert(error.Message.Contains(path, StringComparison.Ordinal), "invalid override reports its exact path");
            }
            Assert(File.ReadAllText(path) == "invalid user PNG", "invalid override retained for author repair");
        }
        finally { File.WriteAllBytes(path, valid); }
        AssertSnapshot(expected, FileSnapshot(Path.Combine(installation.Root, "overrides")), "author repair restores override files");
    }

    /// <summary>Raises a verification failure with the supplied explanation when a condition is false.</summary>
    /// <param name="condition">Predicate that must hold for the lifecycle check to pass.</param>
    /// <param name="message">Failure context reported if the predicate is false.</param>
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
