using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Audio.AudioInstrumentsVerification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks extracted SPC sound programs, channel routing, and editable note playback.</summary>
internal static class AudioSoundProgramsVerification
{
    /// <summary>Compares stock program bytes and routes with APU memory, then checks malformed programs fail.</summary>
    internal static void VerifySoundEffectPrograms()
    {
        string sourceDirectory = RepositoryInstallation.Installation.AudioImportDirectory();
        ExtractedAudioAssetCatalog stock = InstalledFixtures.Audio;
        var rawPlayer = new ManagedSpcPlayer();
        rawPlayer.Uploads.Upload(stock.GetUpload(AudioBank.SpcEngine).Span);

        int instructionCount = 0;
        foreach (AudioSoundProgramMetadata program in stock.SoundPrograms)
        {
            byte[] encoded = SpcSoundEffectProgramCodec.Encode(program);
            instructionCount += program.Instructions.Count;
            for (int offset = 0; offset < encoded.Length; offset++)
            {
                AssertEqual(
                    encoded[offset],
                    rawPlayer.ReadApuByteForVerification(program.Address + offset),
                    $"stock SFX program {program.Id} byte {offset}");
            }
        }

        Dictionary<string, AudioSoundProgramMetadata> programs = stock.SoundPrograms
            .ToDictionary(program => program.Id, StringComparer.Ordinal);
        foreach (AudioSoundLibraryMetadata library in stock.SoundLibraries)
        {
            foreach (AudioSoundEffectMetadata effect in library.Effects)
            {
                for (int channel = 0; channel < effect.ChannelPrograms.Count; channel++)
                {
                    AudioSoundProgramMetadata program = programs[effect.ChannelPrograms[channel]];
                    int pointer = effect.StreamPointer + channel * 2;
                    ushort address = unchecked(
                        LittleEndianBytes.Word(rawPlayer.ReadApuByteForVerification(pointer), rawPlayer.ReadApuByteForVerification(pointer + 1)));
                    AssertEqual(program.Address, address, $"{effect.Id} channel {channel} routing");
                }
            }
        }

        using var fixtureScratch = new TestScratchDirectory("audio-sfx-programs");
        string fixtureDirectory = fixtureScratch.Root;
        CopyAudioDirectory(sourceDirectory, fixtureDirectory);
        try
        {
            string manifestPath = Path.Combine(
                fixtureDirectory,
                ExtractedAudioAssetCatalog.ManifestFileName);
            AudioAssetManifest manifest = ReadAudioManifest(manifestPath);
            AudioSoundEffectMetadata effect = manifest.SoundLibraries[0].Effects[0];
            string programId = effect.ChannelPrograms[0];
            AudioSoundProgramMetadata originalProgram = manifest.SoundPrograms.Single(
                program => program.Id == programId);
            int playIndex = originalProgram.Instructions.ToList().FindIndex(
                instruction => instruction.Operation == AudioSoundInstructionOperations.PlayNote);
            AssertTrue(playIndex >= 0, $"fixture program {programId} contains a playable note");
            AudioSoundInstructionMetadata originalPlay = originalProgram.Instructions[playIndex];

            AudioSoundInstructionMetadata badOperation = originalPlay with { Operation = "unknownOpcode" };
            AudioSoundInstructionMetadata[] invalidInstructions = [.. originalProgram.Instructions];
            invalidInstructions[playIndex] = badOperation;
            WriteAudioManifest(manifestPath, manifest with
            {
                SoundPrograms = manifest.SoundPrograms.Select(program => program.Id == programId
                    ? program with { Instructions = invalidInstructions }
                    : program).ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(fixtureDirectory),
                "SFX manifest rejects unknown operation");

            WriteAudioManifest(manifestPath, manifest with
            {
                SoundPrograms = manifest.SoundPrograms.Select(program => program.Id == programId
                    ? program with { ByteCapacity = program.ByteCapacity + 1 }
                    : program).ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(fixtureDirectory),
                "SFX manifest rejects encoded-size mismatch");

            WriteAudioManifest(manifestPath, manifest with
            {
                SoundPrograms = manifest.SoundPrograms.Where(program => program.Id != programId).ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(fixtureDirectory),
                "SFX manifest rejects missing referenced program");
        }
        finally
        {
            if (Directory.Exists(fixtureDirectory))
                Directory.Delete(fixtureDirectory, recursive: true);
        }

        Console.WriteLine(
            $"SFX programs: {stock.SoundPrograms.Count} programs/{instructionCount} named instructions, " +
            "exact stock bytes and strict failures pass.");
    }

    /// <summary>Runs the SPC sound program contract checks.</summary>
    [VerificationCommand("--audio-sfx-programs")]
    internal static int AudioSfxProgramsCommand()
    {
        Suite(nameof(VerifySoundEffectPrograms), () => VerifySoundEffectPrograms());
        return 0;
    }
}
