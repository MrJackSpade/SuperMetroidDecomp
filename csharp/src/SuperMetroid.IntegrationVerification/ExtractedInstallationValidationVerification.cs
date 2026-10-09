using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

/// <summary>
/// Reproduces known extraction-preflight failures without running gameplay or
/// importing a ROM. Strict diagnostics and startup repair admission share one
/// validation inventory but must retain their different error contracts.
/// </summary>
internal static class ExtractedInstallationValidationVerification
{
    /// <summary>Exercises strict extracted-content diagnostics and ROM-free repair-admission behavior on a temporary fixture.</summary>
    /// <param name="sourceRoot">Installed source tree whose stock non-ROM files seed the isolated fixture.</param>
    /// <returns>Zero after every known validation contract is confirmed.</returns>
    internal static int Run(string sourceRoot)
    {
        GameInstallation source = GameAssetInstallerTooling.ValidateExtractedContent(sourceRoot);
        string temporary = Directory.CreateTempSubdirectory("SuperMetroid-extracted-validation-").FullName;
        try
        {
            string missing = Path.Combine(temporary, "absent-installation");
            Reject<DirectoryNotFoundException>(missing, Path.Combine(missing, "game"));
            Assert(!Directory.Exists(missing), "missing-root diagnostics must not create an installation");

            var fixture = new GameInstallation(Path.Combine(temporary, "installation"));
            foreach (string file in Directory.GetFiles(source.ContentDirectory, "*", SearchOption.AllDirectories))
            {
                if (Path.GetFullPath(file) == Path.GetFullPath(source.RomPath)) continue;
                string destination = Path.Combine(fixture.ContentDirectory, Path.GetRelativePath(source.ContentDirectory, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(file, destination);
            }
            Dictionary<string, string> baseline = Snapshot(fixture.ContentDirectory);
            Assert(GameAssetInstallerTooling.ValidateExtractedContent(fixture.Root).Root == fixture.Root,
                "valid extracted-only content must pass the strict contract");
            Assert(GameAssetInstaller.OpenOrRepair(fixture.Root) is not null,
                "normal startup must still accept valid content without a ROM");
            AssertUnchanged(baseline, fixture, "successful validation");

            string receipt = Path.Combine(fixture.ContentDirectory, GameInstallationLayout.ReceiptFileName);
            WithEditedFile(receipt, () =>
            {
                JsonObject json = ReadObject(receipt);
                json["FormatVersion"] = GameInstallationLayout.FormatVersion - 1;
                File.WriteAllText(receipt, json.ToJsonString());
            }, () => VerifyRejection<InvalidDataException>(fixture, receipt, "outdated installation receipt"));

            WithEditedFile(receipt, () =>
            {
                JsonObject json = ReadObject(receipt);
                json["RomSha256"] = "unsupported-source-provenance";
                File.WriteAllText(receipt, json.ToJsonString());
            }, () => VerifyRejection<InvalidDataException>(fixture, receipt, "wrong source provenance"));

            // This is the stale fixture defect observed by the Android gate: the
            // top-level receipt is current but a required nested manifest is not.
            string enemyManifest = Path.Combine(fixture.EnemyTileDirectory, "enemy-tiles.json");
            WithEditedFile(enemyManifest, () =>
            {
                JsonObject json = ReadObject(enemyManifest);
                json["version"] = json["version"]!.GetValue<int>() - 1;
                File.WriteAllText(enemyManifest, json.ToJsonString());
            }, () => VerifyRejection<InvalidDataException>(fixture, enemyManifest, "outdated nested enemy manifest"));

            string heldDomain = Path.Combine(temporary, "held-required-domain");
            Directory.Move(fixture.RoomPlmSamusEaterVisualDirectory, heldDomain);
            try
            {
                VerifyRejection<IOException>(fixture, fixture.RoomPlmSamusEaterVisualDirectory, "missing required PLM domain");
            }
            finally { Directory.Move(heldDomain, fixture.RoomPlmSamusEaterVisualDirectory); }

            AssertUnchanged(baseline, fixture, "restored fixture after all rejection cases");
            Assert(GameAssetInstallerTooling.ValidateExtractedContent(fixture.Root).Root == fixture.Root,
                "validation must succeed again after the known fixture defects are restored");
            Console.WriteLine($"PASS extracted validation contract: {baseline.Count} stock files; valid ROM-free startup, " +
                "missing root, old receipt, wrong provenance, index hash, stale nested manifest and missing PLM domain; " +
                "strict path-bearing exceptions, repair-admission nulls and no asset repair/import.");
            return 0;
        }
        finally { Directory.Delete(temporary, recursive: true); }
    }

    /// <summary>Confirms a damaged installation produces the expected strict error and is not silently repaired.</summary>
    /// <typeparam name="T">Exception type required from strict extracted-content validation.</typeparam>
    /// <param name="fixture">Temporary installation whose content was deliberately damaged.</param>
    /// <param name="failingPath">Path that the strict diagnostic must identify.</param>
    /// <param name="scenario">Label used in assertion messages for this rejection case.</param>
    private static void VerifyRejection<T>(GameInstallation fixture, string failingPath, string scenario) where T : Exception
    {
        Dictionary<string, string> damaged = Snapshot(fixture.ContentDirectory);
        Reject<T>(fixture.Root, failingPath);
        Assert(GameAssetInstaller.TryOpenExtractedContent(fixture.Root) is null, scenario + ": repair admission rejects");
        Assert(GameAssetInstaller.OpenOrRepair(fixture.Root) is null, scenario + ": missing ROM cannot repair");
        AssertUnchanged(damaged, fixture, scenario);
    }

    /// <summary>Requires strict validation to throw the expected exception and name the exact failing path.</summary>
    /// <typeparam name="T">Exception type expected from the invalid installation.</typeparam>
    /// <param name="root">Installation root passed to strict extracted-content validation.</param>
    /// <param name="failingPath">Path that must appear in the diagnostic.</param>
    private static void Reject<T>(string root, string failingPath) where T : Exception
    {
        try { GameAssetInstallerTooling.ValidateExtractedContent(root); }
        catch (T error)
        {
            Assert(error.ToString().Contains(Path.GetFullPath(failingPath), StringComparison.Ordinal),
                "strict diagnostic must identify the exact failing path");
            return;
        }
        throw new InvalidDataException($"Strict validation accepted defective content; expected {typeof(T).Name} for {failingPath}.");
    }

    /// <summary>Applies a temporary file mutation, runs its verification, and restores the original bytes even on failure.</summary>
    /// <param name="path">Fixture file whose original contents must be restored.</param>
    /// <param name="edit">Mutation that creates the invalid fixture state.</param>
    /// <param name="verify">Checks performed while the mutation is present.</param>
    private static void WithEditedFile(string path, Action edit, Action verify)
    {
        byte[] original = File.ReadAllBytes(path);
        try { edit(); verify(); }
        finally { File.WriteAllBytes(path, original); }
    }

    /// <summary>Parses a fixture file as a JSON object.</summary>
    /// <param name="path">JSON file to load.</param>
    /// <returns>The parsed object.</returns>
    /// <exception cref="InvalidDataException">The file does not contain a JSON object.</exception>
    private static JsonObject ReadObject(string path) => JsonNode.Parse(File.ReadAllText(path))?.AsObject()
        ?? throw new InvalidDataException($"Expected JSON object at {path}.");

    /// <summary>Hashes every file under a fixture root, keyed by its path relative to that root.</summary>
    /// <param name="root">Directory tree whose file contents form the snapshot.</param>
    /// <returns>Relative paths and SHA-256 digests used to detect fixture mutation.</returns>
    private static Dictionary<string, string> Snapshot(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        .ToDictionary(path => Path.GetRelativePath(root, path), path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))));

    /// <summary>Checks that validation left all fixture files byte-identical and did not create a ROM file.</summary>
    /// <param name="expected">Pre-validation relative-path hashes.</param>
    /// <param name="fixture">Installation whose content and ROM path are checked.</param>
    /// <param name="scenario">Label added to assertion failures.</param>
    private static void AssertUnchanged(Dictionary<string, string> expected, GameInstallation fixture, string scenario)
    {
        Dictionary<string, string> actual = Snapshot(fixture.ContentDirectory);
        Assert(actual.Count == expected.Count && expected.All(pair => actual.TryGetValue(pair.Key, out string? hash) && hash == pair.Value),
            scenario + ": validation must not change, repair or import stock files");
        Assert(!File.Exists(fixture.RomPath), scenario + ": no cartridge was created");
    }

    /// <summary>Throws the verification failure when a required condition is false.</summary>
    /// <param name="condition">Condition that must hold for the fixture contract to pass.</param>
    /// <param name="message">Failure detail surfaced when the condition is false.</param>
    /// <exception cref="InvalidDataException">The condition is false.</exception>
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
