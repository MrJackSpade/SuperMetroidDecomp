using System.Text.Json;
using SuperMetroid.Core.Audio;

internal static partial class Program
{
    /// <summary>
    /// Proves that the readable manifest is the live instrument owner rather than a report
    /// generated beside still-authoritative opaque upload bytes. Stock definitions must first
    /// match every uploaded SPC record; an edited copy must then alter both APU RAM and PCM.
    /// </summary>
    private static void VerifyEditableAudioInstruments()
    {
        string sourceDirectory = Path.GetFullPath("standalone-assets/audio");
        ExtractedAudioAssetCatalog stock = ExtractedAudioAssetCatalog.Load(sourceDirectory);
        foreach (AudioUploadAssetDefinition bank in AudioAssetCatalogData.All)
        {
            var rawPlayer = new ManagedSpcPlayer();
            rawPlayer.Upload(stock.GetUpload(AudioUploadAddresses.SpcEngine).Span);
            if (bank.SnesAddress != AudioUploadAddresses.SpcEngine)
                rawPlayer.Upload(stock.GetUpload(bank.SnesAddress).Span);

            IReadOnlyList<AudioInstrumentMetadata> definitions =
                stock.GetInstrumentBank(bank.SnesAddress);
            AssertEqual(
                SpcDriverData.Ram.InstrumentCount,
                definitions.Count,
                $"{bank.Name} instrument count");
            foreach (AudioInstrumentMetadata instrument in definitions)
            {
                int address = SpcDriverData.Ram.InstrumentTable +
                    instrument.Instrument * SpcDriverData.Ram.InstrumentRecordSize;
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
                AssertEqual(unchecked((byte)(instrument.PitchBase >> 8)),
                    rawPlayer.ReadApuByteForVerification(address + 4),
                    $"{bank.Name} instrument {instrument.Instrument} pitch high");
                AssertEqual(unchecked((byte)instrument.PitchBase),
                    rawPlayer.ReadApuByteForVerification(address + 5),
                    $"{bank.Name} instrument {instrument.Instrument} pitch low");
            }
        }

        string editedDirectory = Path.Combine(
            Path.GetTempPath(), $"sm-audio-instruments-{Guid.NewGuid():N}");
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
            var stockRenderer = new CartridgeAudioRenderer(stock);
            var editedRenderer = new CartridgeAudioRenderer(edited);
            CartridgeAudioCommand common =
                CartridgeAudioCommand.Upload(AudioUploadAddresses.SpcEngine);
            CartridgeAudioCommand titleUpload =
                CartridgeAudioCommand.Upload(AudioUploadAddresses.TitleSequence);
            CartridgeAudioCommand titleTrack = CartridgeAudioCommand.WritePort(
                AudioRomData.Apu.MusicPort, AudioRomData.MusicTracks.Title);
            stockRenderer.RenderFrame([common]);
            editedRenderer.RenderFrame([common]);
            stockRenderer.RenderFrame([titleUpload]);
            editedRenderer.RenderFrame([titleUpload]);
            stockRenderer.RenderFrame([titleTrack]);
            editedRenderer.RenderFrame([titleTrack]);

            IReadOnlyList<AudioInstrumentMetadata> installed =
                edited.GetInstrumentBank(AudioUploadAddresses.TitleSequence);
            for (int index = 0; index < installed.Count; index++)
            {
                int address = SpcDriverData.Ram.InstrumentTable +
                    index * SpcDriverData.Ram.InstrumentRecordSize;
                AssertEqual(unchecked((byte)(installed[index].PitchBase >> 8)),
                    editedRenderer.Player.ReadApuByteForVerification(address + 4),
                    $"edited title instrument {index} pitch high");
                AssertEqual(unchecked((byte)installed[index].PitchBase),
                    editedRenderer.Player.ReadApuByteForVerification(address + 5),
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
                pcmDiffers |= !stockPcm.AsSpan().SequenceEqual(editedPcm);
            }
            AssertTrue(stockAudible, "stock title instrument fixture is audible");
            AssertTrue(editedAudible, "edited title instrument fixture is audible");
            AssertTrue(pcmDiffers, "manifest pitch-base edits change rendered PCM");

            WriteAudioManifest(manifestPath, editedManifest with
            {
                Banks = editedManifest.Banks
                    .Select(bank => bank.SnesAddress == editedTitle.SnesAddress
                        ? bank with { Instruments = bank.Instruments.Take(
                            SpcDriverData.Ram.InstrumentCount - 1).ToArray() }
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

    private static AudioAssetManifest ReadAudioManifest(string path) =>
        JsonSerializer.Deserialize<AudioAssetManifest>(
            File.ReadAllText(path), AudioAssetJson.Options)
        ?? throw new InvalidDataException($"Audio manifest '{path}' deserialized to null.");

    private static void WriteAudioManifest(string path, AudioAssetManifest manifest) =>
        File.WriteAllText(path, JsonSerializer.Serialize(manifest, AudioAssetJson.Options));

    private static void CopyAudioDirectory(string source, string destination)
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
}
