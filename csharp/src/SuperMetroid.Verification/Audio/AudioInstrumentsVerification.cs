using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
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
    /// Proves that the readable manifest is the live instrument owner rather than a report
    /// generated beside still-authoritative opaque upload bytes. Stock definitions must first
    /// match every uploaded SPC record; an edited copy must then alter both APU RAM and PCM.
    /// </summary>
    [SpcMemoryBoundary("exact APU memory test: checks uploaded APU RAM bytes")]
    internal static void VerifyEditableAudioInstruments()
    {
        string sourceDirectory = RepositoryInstallation.Installation.AudioDirectory;
        ExtractedAudioAssetCatalog stock = ExtractedAudioAssetCatalog.Load(sourceDirectory);
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
                AssertEqual(SnesLittleEndian.HighByte(checked((ushort)instrument.PitchBase)),
                    rawPlayer.ReadApuByteForVerification(address + 4),
                    $"{bank.Name} instrument {instrument.Instrument} pitch high");
                AssertEqual(unchecked((byte)instrument.PitchBase),
                    rawPlayer.ReadApuByteForVerification(address + 5),
                    $"{bank.Name} instrument {instrument.Instrument} pitch low");
            }
        }

        using var editedDirectoryScratch = new TestScratchDirectory("audio-instruments");
        string editedDirectory = editedDirectoryScratch.Root;
        CopyAudioDirectory(sourceDirectory, editedDirectory);
        try
        {
            string manifestPath = Path.Combine(
                editedDirectory, ExtractedAudioAssetCatalog.ManifestFileName);
            AudioAssetManifest original = ReadAudioManifest(manifestPath);
            AudioBankMetadata title = original.Banks.Single(
                bank => bank.SnesAddress == AudioUploadAddresses.TitleSequence);
            AudioInstrumentMetadata[] shifted = title.Instruments
                .Select(instrument => instrument with
                {
                    PitchBase = unchecked((ushort)(instrument.PitchBase + 0x1000)),
                })
                .ToArray();
            AudioBankMetadata editedTitle = title with { Instruments = shifted };
            AudioAssetManifest editedManifest = original with
            {
                Banks = original.Banks
                    .Select(bank => bank.SnesAddress == editedTitle.SnesAddress
                        ? editedTitle
                        : bank)
                    .ToArray(),
            };
            WriteAudioManifest(manifestPath, editedManifest);

            ExtractedAudioAssetCatalog edited =
                ExtractedAudioAssetCatalog.Load(editedDirectory);
            var stockRenderer = CartridgeAudioSessionAccess.CreateSynthesizing(stock);
            var editedRenderer = CartridgeAudioSessionAccess.CreateSynthesizing(edited);
            AudioRequest common =
                new AudioRequest.ChangeMusicDataSet(AudioBank.SpcEngine);
            AudioRequest titleUpload =
                new AudioRequest.ChangeMusicDataSet(AudioBank.TitleSequence);
            AudioRequest titleTrack = AudioRequest.ForTrack(MusicTrack.NumberedOrNone(AudioRomDataMusicTracks.Title));
            stockRenderer.RenderFrame([common]);
            editedRenderer.RenderFrame([common]);
            stockRenderer.RenderFrame([titleUpload]);
            editedRenderer.RenderFrame([titleUpload]);
            stockRenderer.RenderFrame([titleTrack]);
            editedRenderer.RenderFrame([titleTrack]);

            IReadOnlyList<AudioInstrumentMetadata> installed =
                edited.GetInstrumentBank(AudioBank.TitleSequence);
            for (int index = 0; index < installed.Count; index++)
            {
                int address = SpcDriverDefinitions.Ram.InstrumentTable +
                    index * SpcDriverDefinitions.Ram.InstrumentRecordSize;
                AssertEqual(SnesLittleEndian.HighByte(checked((ushort)installed[index].PitchBase)),
                    editedRenderer.PlayerForVerification.ReadApuByteForVerification(address + 4),
                    $"edited title instrument {index} pitch high");
                AssertEqual(unchecked((byte)installed[index].PitchBase),
                    editedRenderer.PlayerForVerification.ReadApuByteForVerification(address + 5),
                    $"edited title instrument {index} pitch low");
            }

            bool stockAudible = false;
            bool editedAudible = false;
            bool pcmDiffers = false;
            for (int frame = 0; frame < 600; frame++)
            {
                short[] stockPcm = stockRenderer.RenderFrame([]);
                short[] editedPcm = editedRenderer.RenderFrame([]);
                stockAudible |= stockPcm.Any(sample => sample != 0);
                editedAudible |= editedPcm.Any(sample => sample != 0);
                pcmDiffers |= !stockPcm.SequenceEqual(editedPcm);
            }
            AssertTrue(stockAudible, "stock title instrument fixture is audible");
            AssertTrue(editedAudible, "edited title instrument fixture is audible");
            AssertTrue(pcmDiffers, "manifest pitch-base edits change rendered PCM");

            WriteAudioManifest(manifestPath, editedManifest with
            {
                Banks = editedManifest.Banks
                    .Select(bank => bank.SnesAddress == editedTitle.SnesAddress
                        ? bank with { Instruments = bank.Instruments.Take(
                            SpcDriverDefinitions.Ram.InstrumentCount - 1).ToArray() }
                        : bank)
                    .ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(editedDirectory),
                "instrument manifest rejects missing record");

            AudioInstrumentMetadata inconsistent = shifted[0] with
            {
                UsesNoise = !shifted[0].UsesNoise,
            };
            WriteAudioManifest(manifestPath, editedManifest with
            {
                Banks = editedManifest.Banks
                    .Select(bank => bank.SnesAddress == editedTitle.SnesAddress
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
                () => ExtractedAudioAssetCatalog.Load(editedDirectory),
                "instrument manifest rejects inconsistent noise identity");
        }
        finally
        {
            if (Directory.Exists(editedDirectory))
                Directory.Delete(editedDirectory, recursive: true);
        }

        Console.WriteLine(
            "Editable instruments: 1050 stock six-byte records, live manifest pitch edits, " +
            "audible PCM change and malformed-bank rejection pass.");
    }

    /// <summary>Reads a fixture manifest using the production audio JSON options.</summary>
    internal static AudioAssetManifest ReadAudioManifest(string path)
    {
        using var input = File.OpenRead(path);
        return JsonSerializer.Deserialize<AudioAssetManifest>(input, AudioAssetJson.Options)
            ?? throw new InvalidDataException($"Audio manifest '{path}' deserialized to null.");
    }

    /// <summary>Writes an edited instrument manifest for the isolated audio fixture.</summary>
    internal static void WriteAudioManifest(string path, AudioAssetManifest manifest)
    {
        using var output = File.Create(path);
        JsonSerializer.Serialize(output, manifest, AudioAssetJson.Options);
    }

    /// <summary>Copies extracted audio assets into a private tree before mutating instrument definitions.</summary>
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

    /// <summary>Runs the editable-instrument APU and PCM verification command.</summary>
    [VerificationCommand("--audio-instruments")]
    internal static int AudioInstrumentsCommand()
    {
        Suite(nameof(VerifyEditableAudioInstruments), () => VerifyEditableAudioInstruments());
        return 0;
    }
}
