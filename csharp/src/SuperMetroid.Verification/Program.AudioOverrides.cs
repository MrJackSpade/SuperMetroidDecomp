using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyPersistentAudioOverrides()
    {
        string source = Path.GetFullPath("standalone-assets/audio");
        string root = Path.Combine(Path.GetTempPath(), $"sm-audio-override-{Guid.NewGuid():N}");
        var installation = new GameInstallation(root);
        CopyAudioDirectory(source, installation.AudioDirectory);
        try
        {
            ExtractedAudioAssetCatalog stock = installation.LoadAudio();
            AssertEqual(
                stock.ContentIdentity,
                installation.LoadAudio().ContentIdentity,
                "stock audio identity is stable across reload");
            string selectedDirectory = AudioAssetOverrideInstaller.Initialize(installation);
            AssertEqual(
                Path.GetFullPath(installation.AudioOverrideDirectory),
                Path.GetFullPath(selectedDirectory),
                "audio override initializes outside replaceable game content");
            AssertThrows<IOException>(
                () => AudioAssetOverrideInstaller.Initialize(installation),
                "audio override initialization refuses to overwrite user content");

            string overrideManifestPath = Path.Combine(
                installation.AudioOverrideDirectory,
                ExtractedAudioAssetCatalog.ManifestFileName);
            AudioAssetManifest manifest = ReadAudioManifest(overrideManifestPath);
            AudioBankMetadata title = manifest.Banks.Single(
                bank => bank.SnesAddress == AudioUploadAddresses.TitleSequence);
            AudioInstrumentMetadata editedInstrument = title.Instruments[0] with
            {
                PitchBase = unchecked((ushort)(title.Instruments[0].PitchBase + 0x0100)),
            };
            AudioAssetManifest edited = manifest with
            {
                Banks = manifest.Banks.Select(bank => bank.SnesAddress == title.SnesAddress
                    ? bank with
                    {
                        Instruments = bank.Instruments.Select(instrument => instrument.Instrument == 0
                            ? editedInstrument
                            : instrument).ToArray(),
                    }
                    : bank).ToArray(),
            };
            WriteAudioManifest(overrideManifestPath, edited);

            ExtractedAudioAssetCatalog selected = installation.LoadAudio();
            AssertTrue(
                selected.ContentIdentity != stock.ContentIdentity,
                "instrument-only JSON edit changes selected audio identity");
            AssertEqual(
                editedInstrument.PitchBase,
                selected.GetInstrumentBank(AudioUploadAddresses.TitleSequence)[0].PitchBase,
                "installed session selects persistent instrument edit");
            AssertTrue(
                selected.GetInstrumentBank(AudioUploadAddresses.TitleSequence)[0].PitchBase !=
                stock.GetInstrumentBank(AudioUploadAddresses.TitleSequence)[0].PitchBase,
                "selected audio differs from validated stock");

            // A stock repair replaces only game/audio. The persistent override remains selected.
            Directory.Delete(installation.AudioDirectory, recursive: true);
            CopyAudioDirectory(source, installation.AudioDirectory);
            AssertEqual(
                editedInstrument.PitchBase,
                installation.LoadAudio().GetInstrumentBank(AudioUploadAddresses.TitleSequence)[0].PitchBase,
                "stock repair preserves persistent audio edit");
            AssertEqual(
                selected.ContentIdentity,
                installation.LoadAudio().ContentIdentity,
                "stock repair preserves selected audio identity");

            AudioAssetManifest incompatible = edited with
            {
                Uploads = edited.Uploads.Select((upload, index) => index == 0
                    ? upload with { ByteLength = upload.ByteLength + 1 }
                    : upload).ToArray(),
            };
            WriteAudioManifest(overrideManifestPath, incompatible);
            AssertThrows<InvalidDataException>(
                () => installation.LoadAudio(),
                "override cannot replace opaque upload mechanics");
            WriteAudioManifest(overrideManifestPath, edited);

            string stockManifestPath = Path.Combine(
                installation.AudioDirectory,
                ExtractedAudioAssetCatalog.ManifestFileName);
            File.AppendAllText(stockManifestPath, "corrupt");
            AssertThrows<System.Text.Json.JsonException>(
                () => installation.LoadAudio(),
                "valid override cannot conceal corrupt stock");
            File.Copy(
                Path.Combine(source, ExtractedAudioAssetCatalog.ManifestFileName),
                stockManifestPath,
                overwrite: true);

            AudioCanonicalSampleMetadata firstSample = edited.CanonicalSamples[0];
            string missingWave = Path.Combine(
                installation.AudioOverrideDirectory,
                firstSample.WavFile.Replace('/', Path.DirectorySeparatorChar));
            File.Delete(missingWave);
            AssertThrows<FileNotFoundException>(
                () => installation.LoadAudio(),
                "invalid override never silently falls back to stock");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        VerifyGameContentIdentityComposition();

        Console.WriteLine(
            "Persistent audio overrides: isolated initialization, selection, stock repair, " +
            "stable installation identity, compatibility guards and loud corruption failures pass.");
    }

    private static void VerifyGameContentIdentityComposition()
    {
        string audio = new('A', 64);
        string maps = new('B', 64);
        string projectiles = new('C', 64);
        Guid definitions = Guid.Parse("89abcdef-0123-4567-89ab-cdef01234567");
        GameContentIdentity baseline = GameContentIdentity.Create(audio, maps, projectiles, definitions);
        AssertEqual(
            baseline,
            GameContentIdentity.Create(audio, maps, projectiles, definitions),
            "aggregate installation identity is deterministic");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(new string('D', 64), maps, projectiles, definitions).CompositeSha256,
            "audio identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, new string('D', 64), projectiles, definitions).CompositeSha256,
            "map identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, maps, new string('D', 64), definitions).CompositeSha256,
            "projectile identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, maps, projectiles, Guid.Empty).CompositeSha256,
            "compiled-definition build invalidates aggregate installation identity");
        AssertThrows<ArgumentException>(
            () => GameContentIdentity.Create("not-a-sha", maps, projectiles, definitions),
            "aggregate installation identity rejects malformed component digest");

        ControllerRecordingContentIdentity recorded = baseline.ToControllerRecordingIdentity();
        AssertEqual(0, baseline.GetRecordingCompatibilityWarnings(recorded).Count,
            "matching recording identity produces no compatibility warning");
        AssertTrue(
            baseline.GetRecordingCompatibilityWarnings(null).Single().Contains("Legacy", StringComparison.Ordinal),
            "legacy recording explains unavailable installed-content comparison");
        AssertTrue(
            baseline.GetRecordingCompatibilityWarnings(recorded with
            {
                AudioContentSha256 = Convert.FromHexString(new string('D', 64)),
            }).Single().Contains("audio", StringComparison.Ordinal),
            "audio drift receives a component-specific warning");
        AssertTrue(
            baseline.GetRecordingCompatibilityWarnings(recorded with
            {
                CompiledDefinitionsBuildId = Guid.Empty,
            }).Single().Contains("Compiled gameplay definitions", StringComparison.Ordinal),
            "compiled-definition drift receives a specific warning");
        AssertTrue(
            baseline.GetRecordingCompatibilityWarnings(recorded with
            {
                CompositeSha256 = Convert.FromHexString(new string('D', 64)),
            }).Single().Contains("Aggregate", StringComparison.Ordinal),
            "unexplained aggregate drift cannot pass component comparison");
    }
}
