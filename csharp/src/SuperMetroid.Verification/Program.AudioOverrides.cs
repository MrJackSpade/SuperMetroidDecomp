using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;

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

        Console.WriteLine(
            "Persistent audio overrides: isolated initialization, selection, stock repair, " +
            "compatibility guards and loud corruption failures pass.");
    }
}
