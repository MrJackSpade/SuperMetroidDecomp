using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

internal static partial class InstallationOverrideLifecycleVerification
{
    /// <summary>
    /// Edits representative installed presentation and audio assets, then returns the values
    /// that the installation loaders should expose from those edits.
    /// </summary>
    /// <param name="installation">Installation whose override files and audio manifest are edited in place.</param>
    private static OverrideExpectations EditPresentation(GameInstallation installation)
    {
        byte[] standard = EditPng(Path.Combine(installation.StandardObjectOverrideDirectory,
            StandardObjectArtworkFormat.FileName), RoomCharacterAtlasFormat.TileColumns * 8,
            (RoomCharacterAtlasFormat.ValidateTileCount(StandardObjectArtworkFormat.TransferByteCount) +
                RoomCharacterAtlasFormat.TileColumns - 1) / RoomCharacterAtlasFormat.TileColumns * 8, 4)
            .AsSpan(0, StandardObjectArtworkFormat.TransferByteCount).ToArray();
        byte[] font = EditPng(Path.Combine(installation.MapOverrideDirectory, IntroFontAtlasFormat.FileName),
            IntroFontAtlasFormat.Width, IntroFontAtlasFormat.Height, IntroFontAtlasFormat.BitsPerPixel);
        string palettePath = Path.Combine(installation.GameplayBasePaletteOverrideDirectory, GameplayBasePaletteFormat.ArtworkFileName);
        GameplayBasePaletteDocument palette = JsonSerializer.Deserialize<GameplayBasePaletteDocument>(
            File.ReadAllBytes(palettePath), GameplayBasePaletteFormat.JsonOptions)!;
        PaletteRgb5 color = palette.Initial[0];
        palette.Initial[0] = color with { Red = (color.Red + 1) % 32 };
        File.WriteAllBytes(palettePath, GameplayBasePaletteCatalog.Write(palette));
        ushort selectedColor = (ushort)(palette.Initial[0].Red | palette.Initial[0].Green << 5 | palette.Initial[0].Blue << 10);

        string audioPath = Path.Combine(installation.AudioOverrideDirectory, ExtractedAudioAssetCatalog.ManifestFileName);
        AudioAssetManifest manifest = JsonSerializer.Deserialize<AudioAssetManifest>(File.ReadAllText(audioPath), AudioAssetJson.Options)!;
        AudioCanonicalSampleMetadata sample = manifest.CanonicalSamples[0];
        string wavePath = Path.Combine(installation.AudioOverrideDirectory, sample.WavFile.Replace('/', Path.DirectorySeparatorChar));
        (int rate, short[] pcm) = PcmWaveFile.ReadMonoPcm16(File.ReadAllBytes(wavePath), wavePath);
        pcm[0] = unchecked((short)(pcm[0] ^ 1));
        PcmWaveFile.WriteMonoPcm16(wavePath, rate, pcm);
        AudioBankMetadata bank = manifest.Banks.First(candidate => candidate.Samples.Any(value => value.SampleId == sample.Id));
        AudioSampleMetadata source = bank.Samples.First(value => value.SampleId == sample.Id);
        AudioInstrumentMetadata instrument = bank.Instruments[0] with { PitchBase = (ushort)(bank.Instruments[0].PitchBase ^ 1) };
        AudioSoundProgramMetadata program = manifest.SoundPrograms.First(value =>
            value.Instructions.Any(instruction => instruction.Operation == AudioSoundInstructionOperations.PlayNote));
        int instructionIndex = program.Instructions.ToList().FindIndex(value => value.Operation == AudioSoundInstructionOperations.PlayNote);
        byte[] arguments = program.Instructions[instructionIndex].Arguments.ToArray();
        // PlayNote's last byte is duration. Preserve identity, instrument/routing,
        // encoded capacity and flow while authoring a different audible duration.
        arguments[^1] = (byte)(arguments[^1] == byte.MaxValue ? byte.MaxValue - 1 : arguments[^1] + 1);
        AudioSoundInstructionMetadata instruction = program.Instructions[instructionIndex] with { Arguments = arguments };
        AudioAssetManifest edited = manifest with
        {
            CanonicalSamples = manifest.CanonicalSamples.Select(value => value.Id == sample.Id
                ? value with { Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(wavePath))) } : value).ToArray(),
            Banks = manifest.Banks.Select(value => value.SnesAddress == bank.SnesAddress
                ? value with { Instruments = value.Instruments.Select(item => item.Instrument == instrument.Instrument ? instrument : item).ToArray() }
                : value).ToArray(),
            SoundPrograms = manifest.SoundPrograms.Select(value => value.Id == program.Id
                ? value with { Instructions = value.Instructions.Select((item, index) => index == instructionIndex ? instruction : item).ToArray() }
                : value).ToArray(),
        };
        File.WriteAllText(audioPath, JsonSerializer.Serialize(edited, AudioAssetJson.Options));
        return new(standard, font, selectedColor, bank.SnesAddress, source.Source, pcm[0],
            instrument, program.Id, instructionIndex, arguments);
    }

    /// <summary>
    /// Changes the first indexed pixel in a PNG override and returns its SNES planar tile encoding.
    /// </summary>
    /// <param name="path">PNG override file to read and replace.</param>
    /// <param name="width">Expected image width in pixels.</param>
    /// <param name="height">Expected image height in pixels.</param>
    /// <param name="bitsPerPixel">Indexed pixel depth used to wrap the changed palette index and encode tiles.</param>
    private static byte[] EditPng(string path, int width, int height, int bitsPerPixel)
    {
        IndexedPngImage image;
        using (Stream input = File.OpenRead(path)) image = IndexedPng.Read(input, width, height);
        image.Pixels[0] = (byte)((image.Pixels[0] + 1) % (1 << bitsPerPixel));
        using (Stream output = File.Create(path)) IndexedPng.Write(output, width, height, image.Pixels, image.Palette);
        return SnesPlanarTileEncoder.Encode(image.Pixels, width, height, bitsPerPixel);
    }

    /// <summary>
    /// Confirms that selection preserves catalog identities and edited override bytes, and that loaders
    /// return the corresponding graphical, palette, sample, instrument, and sound-program values.
    /// </summary>
    /// <param name="installation">Installation whose selected overrides are loaded for comparison.</param>
    /// <param name="expected">Values captured when the representative overrides were authored.</param>
    /// <param name="identities">Catalog identity snapshot taken before selection.</param>
    /// <param name="files">Override-file snapshot taken before selection.</param>
    /// <param name="phase">Operation label included in assertion messages to identify the lifecycle phase.</param>
    private static void VerifySelected(GameInstallation installation, OverrideExpectations expected,
        Dictionary<string, string> identities, Dictionary<string, string> files, string phase)
    {
        AssertSnapshot(identities, CatalogSnapshot(installation), phase + ": selected catalog identity");
        AssertSnapshot(files, FileSnapshot(Path.Combine(installation.Root, "overrides")), phase + ": override bytes preserved");
        Assert(installation.LoadStandardObjects().Transfer.Span.SequenceEqual(expected.StandardObjects), phase + ": actual PNG transfer");
        Assert(installation.LoadMaps().IntroFont.Transfer.Span.SequenceEqual(expected.Font), phase + ": actual font transfer");
        Assert(installation.LoadGameplayBasePalettes().Initial[0] == expected.Color, phase + ": actual palette word");
        ExtractedAudioAssetCatalog audio = installation.LoadAudio();
        Assert(audio.GetSampleBank(expected.Bank).Resolve(expected.Source).Samples.Span[0] == expected.PcmFirstSample,
            phase + ": actual replaced WAV sample");
        Assert(audio.GetInstrumentBank(expected.Bank)[expected.Instrument.Instrument] == expected.Instrument,
            phase + ": actual authored instrument");
        Assert(audio.SoundPrograms.Single(value => value.Id == expected.SoundProgram).Instructions[expected.SoundInstruction]
            .Arguments.SequenceEqual(expected.SoundArguments), phase + ": actual authored SFX operation");
    }

    /// <summary>
    /// Captures the encoded and decoded values expected after editing representative installed overrides.
    /// </summary>
    /// <param name="StandardObjects">SNES transfer bytes produced from the edited standard-object artwork.</param>
    /// <param name="Font">SNES transfer bytes produced from the edited intro font artwork.</param>
    /// <param name="Color">BGR555 word for the changed first entry of the initial gameplay palette.</param>
    /// <param name="Bank">SNES bank address containing the edited sample and instrument.</param>
    /// <param name="Source">Source identifier of the sample whose first PCM value was changed.</param>
    /// <param name="PcmFirstSample">Expected signed first PCM sample after loading the edited WAV file.</param>
    /// <param name="Instrument">Instrument metadata with the changed pitch base expected from the audio loader.</param>
    /// <param name="SoundProgram">Identifier of the authored sound program containing the edited PlayNote operation.</param>
    /// <param name="SoundInstruction">Zero-based instruction index of that PlayNote operation.</param>
    /// <param name="SoundArguments">Expected argument bytes for the edited PlayNote operation, including its changed duration.</param>
    private sealed record OverrideExpectations(byte[] StandardObjects, byte[] Font, ushort Color,
        int Bank, byte Source, short PcmFirstSample, AudioInstrumentMetadata Instrument,
        string SoundProgram, int SoundInstruction, byte[] SoundArguments);
}
