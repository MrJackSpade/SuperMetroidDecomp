using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using System.Text.Json;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that editable instrument metadata controls uploaded SPC records and audible output.</summary>
internal static class AudioInstrumentsVerification
{
    /// <summary>
    /// Checks that the stock instrument definitions match every uploaded SPC record and that the manifest
    /// rejects a missing record and an inconsistent noise identity.
    /// </summary>
    internal static void VerifyAudioInstruments()
    {
        string sourceDirectory = RepositoryInstallation.Installation.AudioImportDirectory();
        ExtractedAudioAssetCatalog stock = InstalledFixtures.Audio;
        foreach (AudioUploadAssetDefinition bank in AudioAssetCatalogDefinitions.All)
        {
            var rawPlayer = new ManagedSpcPlayer();
            rawPlayer.Uploads.Upload(stock.GetUpload(AudioBank.SpcEngine).Span);
            if (bank.AudioSet != AudioBank.SpcEngine)
                rawPlayer.Uploads.Upload(stock.GetUpload(bank.AudioSet).Span);

            IReadOnlyList<AudioInstrumentMetadata> definitions =
                stock.GetInstrumentBank(bank.AudioSet);
            AssertEqual(
                SpcDriverDefinitions.Ram.InstrumentCount,
                definitions.Count,
                $"{bank.Name} instrument count");
            foreach (AudioInstrumentMetadata instrument in definitions)
            {
                int address = SpcDriverDefinitions.Ram.InstrumentTable +
                    instrument.Instrument * SpcDriverDefinitions.Ram.InstrumentRecordSize;
                AssertEqual(instrument.SourceOrNoiseRate,
                    rawPlayer.ReadApuByteForVerification(address),
                    $"{bank.Name} instrument {instrument.Instrument} source/noise");
                AssertEqual(instrument.Adsr1,
                    rawPlayer.ReadApuByteForVerification(address + 1),
                    $"{bank.Name} instrument {instrument.Instrument} ADSR1");
                AssertEqual(instrument.Adsr2,
                    rawPlayer.ReadApuByteForVerification(address + 2),
                    $"{bank.Name} instrument {instrument.Instrument} ADSR2");
                AssertEqual(instrument.Gain,
                    rawPlayer.ReadApuByteForVerification(address + 3),
                    $"{bank.Name} instrument {instrument.Instrument} gain");
                AssertEqual(LittleEndianBytes.HighByte(checked((ushort)instrument.PitchBase)),
                    rawPlayer.ReadApuByteForVerification(address + 4),
                    $"{bank.Name} instrument {instrument.Instrument} pitch high");
                AssertEqual(unchecked((byte)instrument.PitchBase),
                    rawPlayer.ReadApuByteForVerification(address + 5),
                    $"{bank.Name} instrument {instrument.Instrument} pitch low");
            }
        }

        using var fixtureScratch = new TestScratchDirectory("audio-instruments");
        string fixtureDirectory = fixtureScratch.Root;
        CopyAudioDirectory(sourceDirectory, fixtureDirectory);
        try
        {
            string manifestPath = Path.Combine(
                fixtureDirectory, ExtractedAudioAssetCatalog.ManifestFileName);
            AudioAssetManifest original = ReadAudioManifest(manifestPath);
            AudioBankMetadata title = original.Banks.Single(
                bank => bank.SnesAddress == (int)AudioUploadSourceDefinitions.TitleSequence);

            WriteAudioManifest(manifestPath, original with
            {
                Banks = original.Banks
                    .Select(bank => bank.SnesAddress == title.SnesAddress
                        ? bank with { Instruments = bank.Instruments.Take(
                            SpcDriverDefinitions.Ram.InstrumentCount - 1).ToArray() }
                        : bank)
                    .ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(fixtureDirectory),
                "instrument manifest rejects missing record");

            AudioInstrumentMetadata inconsistent = title.Instruments[0] with
            {
                UsesNoise = !title.Instruments[0].UsesNoise,
            };
            WriteAudioManifest(manifestPath, original with
            {
                Banks = original.Banks
                    .Select(bank => bank.SnesAddress == title.SnesAddress
                        ? bank with
                        {
                            Instruments = bank.Instruments
                                .Select(instrument => instrument.Instrument == 0
                                    ? inconsistent
                                    : instrument)
                                .ToArray(),
                        }
                        : bank)
                    .ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(fixtureDirectory),
                "instrument manifest rejects inconsistent noise identity");
        }
        finally
        {
            if (Directory.Exists(fixtureDirectory))
                Directory.Delete(fixtureDirectory, recursive: true);
        }

        Console.WriteLine(
            "Instruments: 1050 stock six-byte records and malformed-bank rejection pass.");
    }

    /// <summary>Reads a fixture manifest using the production audio JSON options.</summary>
    internal static AudioAssetManifest ReadAudioManifest(string path)
    {
        using var input = File.OpenRead(path);
        return JsonSerializer.Deserialize<AudioAssetManifest>(input, AudioAssetJson.Options)
            ?? throw new InvalidDataException($"Audio manifest '{path}' deserialized to null.");
    }

    /// <summary>Writes a malformed manifest for the isolated audio fixture.</summary>
    internal static void WriteAudioManifest(string path, AudioAssetManifest manifest)
    {
        using var output = File.Create(path);
        JsonSerializer.Serialize(output, manifest, AudioAssetJson.Options);
    }

    /// <summary>
    /// Copies the audio's import data, the audio manifest and the streams and samples it names (#1693), into a private
    /// tree before mutating its definitions.
    /// </summary>
    internal static void CopyAudioDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string directory in Directory.EnumerateDirectories(
            source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(
                destination, Path.GetRelativePath(source, directory)));
        }
        foreach (string file in Directory.EnumerateFiles(
            source, "*", SearchOption.AllDirectories))
        {
            File.Copy(
                file,
                Path.Combine(destination, Path.GetRelativePath(source, file)));
        }
    }

    /// <summary>Runs the instrument APU record and manifest validation command.</summary>
    [VerificationCommand("--audio-instruments")]
    internal static int AudioInstrumentsCommand()
    {
        Suite(nameof(VerifyAudioInstruments), () => VerifyAudioInstruments());
        return 0;
    }
}
