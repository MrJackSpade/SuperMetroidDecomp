using SuperMetroid.Tooling;
using SuperMetroid.Core;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Audio;
namespace SuperMetroid.IntegrationVerification.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Installer acceptance for the source-inventoried presentation domains. Import is
/// the only cartridge consumer; these checks never advance gameplay to find reads.
/// All files belong to a fresh test-owned directory, not the player's installation.
/// </summary>
internal static class InstallationOverrideLifecycleVerification
{
    /// <summary>Installs into scratch storage and checks override survival across ROM-free startup, stock repair, and format upgrade.</summary>
    internal static int Run(string sourceRom)
    {
        using var temporaryScratch = new TestScratchDirectory("override-lifecycle");
        string temporary = temporaryScratch.Root;
        Directory.CreateDirectory(temporary);
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
            using (var session = new SuperMetroid.Diagnostics.Android.AndroidSessionData(installation.Root))
            {
                Assert(session.ContentIdentity.MapContentSha256 == selected[nameof(GameInstallation.LoadMaps)] &&
                    session.ContentIdentity.AudioContentSha256 == selected[nameof(GameInstallation.LoadAudioClips)],
                    "fresh portable Android session binds selected map/font and audio identities");
                Assert(!MetadataShape.DeclaresProperty(session.Bus.GetType(), "Rom"),
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
                $"{overrideFiles.Count} override files; PNG/font/palette/replacement-clip edits survive " +
                "ROM-unavailable startup, fresh host binding, stock repair and extraction upgrade. No gameplay probes.");
            return 0;
        }
        finally
        {
            Directory.Delete(temporary, recursive: true);
        }
    }

    /// <summary>
    /// Installed domains that hold gameplay data rather than presentation, so they have no
    /// persistent override: room level streams are the block and collision data rooms run on (#1332).
    /// </summary>
    private static readonly string[] StockOnlyGameplayDomains = [nameof(GameInstallationLayout.RoomLevelStreamDirectoryName)];

    /// <summary>Copies every declared installed domain into persistent overrides and adds an unrecognized user note.</summary>
    [ReflectionAudit("public coverage discovery: every override directory the installation exposes must match a declared installed domain")]
    private static string[] CopyEveryOverrideDomain(GameInstallation installation)
    {
        string[] directories = typeof(GameInstallation).GetProperties()
            .Where(property => property.PropertyType == typeof(string) &&
                property.Name.EndsWith("OverrideDirectory", StringComparison.Ordinal))
            .Select(property => Path.GetFileName((string)(property.GetValue(installation) ?? throw new InvalidOperationException("Verification requires property.GetValue(installation)."))))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        string[] declared = MetadataShape.Fields(typeof(GameInstallationLayout), BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string) && field.Name.EndsWith("DirectoryName", StringComparison.Ordinal) &&
                field.Name != nameof(GameInstallationLayout.ContentDirectoryName) &&
                !StockOnlyGameplayDomains.Contains(field.Name))
            .Select(field => (string)(field.GetValue(null) ?? throw new InvalidOperationException("Verification requires field.GetValue(null)."))).Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal).ToArray();
        Assert(directories.SequenceEqual(declared), "every declared installed domain exposes a persistent override directory");
        foreach (string directory in directories.Where(directory => directory != GameInstallationLayout.AudioDirectoryName))
        foreach (string file in Directory.EnumerateFiles(Path.Combine(installation.ContentDirectory, directory), "*", SearchOption.AllDirectories))
        {
            string target = Path.Combine(installation.Root, "overrides", directory,
                Path.GetRelativePath(Path.Combine(installation.ContentDirectory, directory), file));
            Directory.CreateDirectory((Path.GetDirectoryName(target) ?? throw new InvalidOperationException("Verification requires Path.GetDirectoryName(target).")));
            File.Copy(file, target);
        }
        // The audio override holds replacement clips, not a copy of the stock catalog (#1305 Q3); an empty one selects stock.
        _ = AudioAssetOverrideInstaller.Initialize(installation);
        File.WriteAllText(Path.Combine(installation.Root, "overrides", "artist-note.txt"), "preserve unrecognized user files");
        return directories;
    }

    /// <summary>Reads selected content identities from every declared zero-argument installation loader.</summary>
    [ReflectionAudit("public catalog discovery: every parameterless public Load* method of the installation must persist overrides")]
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
            try { catalog = (loader.Invoke(installation, null) ?? throw new InvalidOperationException("Verification requires loader.Invoke(installation, null).")); }
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

    /// <summary>Hashes each file under a directory by its relative path for byte-preservation checks.</summary>
    private static Dictionary<string, string> FileSnapshot(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(file => Path.GetRelativePath(root, file),
                file => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))), StringComparer.Ordinal);

    /// <summary>Checks that a file or catalog identity snapshot retains exactly the expected keys and digests.</summary>
    private static void AssertSnapshot(Dictionary<string, string> expected, Dictionary<string, string> actual, string context)
    {
        Assert(expected.Count == actual.Count, context + ": file/catalog count");
        foreach ((string key, string digest) in expected)
            Assert(actual.TryGetValue(key, out string? selected) && selected == digest, context + ": " + key);
    }

    /// <summary>Requires only map, audio, standard-object, and gameplay-palette identities to change after the selected edits.</summary>
    private static void AssertOnlyEditedCatalogsChanged(Dictionary<string, string> baseline, Dictionary<string, string> selected)
    {
        string[] changed = [nameof(GameInstallation.LoadMaps), nameof(GameInstallation.LoadAudioClips),
            nameof(GameInstallation.LoadStandardObjects), nameof(GameInstallation.LoadGameplayBasePalettes)];
        foreach ((string loader, string digest) in baseline)
            Assert((selected[loader] != digest) == changed.Contains(loader), "isolated presentation edits: " + loader);
    }

    /// <summary>Checks that a malformed user PNG remains on disk and reports its path instead of falling back to stock.</summary>
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

    /// <summary>Raises a verification failure with the failed lifecycle condition.</summary>
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }

    /// <summary>Authors isolated PNG, font, RGB5 and replacement-clip changes and records their expected values.</summary>
    private static OverrideExpectations EditPresentation(GameInstallation installation)
    {
        byte[] standard = EditPng(Path.Combine(installation.StandardObjectOverrideDirectory,
            StandardObjectArtworkFormat.FileName), new(checked(RoomCharacterAtlasFormat.TileColumns * 8),
            checked((ushort)((RoomCharacterAtlasFormat.ValidateTileCount(StandardObjectArtworkFormat.ByteCount) +
                RoomCharacterAtlasFormat.TileColumns - 1) / RoomCharacterAtlasFormat.TileColumns * 8))), 4)
            .AsSpan(0, StandardObjectArtworkFormat.ByteCount).ToArray();
        byte[] font = EditPng(Path.Combine(installation.MapOverrideDirectory, IntroFontAtlasFormat.FileName),
            new(checked(IntroFontAtlasFormat.Width), checked(IntroFontAtlasFormat.Height)),
            IntroFontAtlasFormat.BitsPerPixel);
        string palettePath = Path.Combine(installation.GameplayBasePaletteOverrideDirectory, GameplayBasePaletteFormat.ArtworkFileName);
        GameplayBasePaletteDocument palette = RequiredJson.Deserialize<GameplayBasePaletteDocument>(
            File.ReadAllBytes(palettePath), GameplayBasePaletteFormat.JsonOptions);
        PaletteRgb5 color = palette.Initial[0];
        palette.Initial[0] = color with { Red = (color.Red + 1) % 32 };
        File.WriteAllBytes(palettePath, GameplayBasePaletteCatalog.Write(palette));
        ushort selectedColor = (ushort)(palette.Initial[0].Red | palette.Initial[0].Green << 5 | palette.Initial[0].Blue << 10);

        ClipAudioCatalog clips = installation.LoadAudioClips();
        MusicClipEntry song = clips.Manifest.Music[0];
        short[] samples = clips.ReadClip(song.Clip);
        samples[0] = unchecked((short)(samples[0] ^ 1));
        string replacement = Path.Combine(installation.Root, "replacement-song.wav");
        PcmWaveFile.WriteStereoPcm16(replacement, clips.Manifest.SampleRate, samples);
        _ = AudioAssetOverrideInstaller.Replace(installation, ClipAudioOverrides.MusicId(song.AudioSet, song.Track), replacement, song.LoopStart);
        File.Delete(replacement);
        return new(standard, font, selectedColor, song.AudioSet, song.Track, samples[0]);
    }

    /// <summary>Changes the first indexed pixel of an override PNG and returns its encoded SNES tile bytes.</summary>
    private static byte[] EditPng(string path, Extent16<ImagePixels> size, int bitsPerPixel)
    {
        IndexedPngImage image;
        using (Stream input = File.OpenRead(path)) image = IndexedPng.Read(input, size);
        image.Pixels[0] = (byte)((image.Pixels[0] + 1) % (1 << bitsPerPixel));
        using (Stream output = File.Create(path)) IndexedPng.Write(output, size, image.Pixels, image.Colours);
        return SnesPlanarTileEncoder.Encode(image.Pixels, size, bitsPerPixel);
    }

    /// <summary>Checks that selected catalogs still expose the authored bytes and unchanged override files at one lifecycle phase.</summary>
    private static void VerifySelected(GameInstallation installation, OverrideExpectations expected,
        Dictionary<string, string> identities, Dictionary<string, string> files, string phase)
    {
        AssertSnapshot(identities, CatalogSnapshot(installation), phase + ": selected catalog identity");
        AssertSnapshot(files, FileSnapshot(Path.Combine(installation.Root, "overrides")), phase + ": override bytes preserved");
        Assert(installation.LoadStandardObjects().Characters.Span.SequenceEqual(expected.StandardObjects), phase + ": actual PNG transfer");
        Assert(installation.LoadMaps().IntroFont.Characters.Span.SequenceEqual(expected.Font), phase + ": actual font transfer");
        Assert(installation.LoadGameplayBasePalettes().Initial[0] == Bgr555.FromWord(expected.Color), phase + ": actual palette word");
        ClipAudioCatalog audio = installation.LoadAudioClips();
        Assert(audio.ReadClip(audio.GetMusic(expected.MusicBank, expected.MusicTrack).Clip)[0] == expected.ClipFirstSample,
            phase + ": actual replacement clip sample");
    }

    /// <summary>Edited transfer, palette, and audio values expected to survive each installation transition.</summary>
    private sealed record OverrideExpectations(byte[] StandardObjects, byte[] Font, ushort Color,
        AudioBank MusicBank, int MusicTrack, short ClipFirstSample);
}
