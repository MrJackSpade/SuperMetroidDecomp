using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Audio.AudioInstrumentsVerification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks editable SPC music definitions against uploaded APU bytes and rendered title audio.</summary>
internal static class AudioMusicProgramsVerification
{
    /// <summary>Matches compiled stock banks to APU RAM, then checks malformed sizes and references fail.</summary>
    internal static void VerifyMusicPrograms()
    {
        string sourceDirectory = RepositoryInstallation.Installation.AudioImportDirectory();
        ExtractedAudioAssetCatalog stock = InstalledFixtures.Audio;
        int trackCount = 0;
        int phraseCount = 0;
        int programCount = 0;
        int instructionCount = 0;
        foreach (AudioUploadAssetDefinition definition in AudioAssetCatalogDefinitions.All)
        {
            AudioBankMetadata bank = stock.GetMusicBank(definition.AudioSet);
            trackCount += bank.MusicTracks.Count;
            phraseCount += bank.MusicPhrases.Count;
            programCount += bank.MusicPrograms.Count;
            instructionCount += bank.MusicPrograms.Sum(program => program.Instructions.Count);
            var raw = new ManagedSpcPlayer();
            raw.Uploads.Upload(stock.GetUpload(AudioBank.SpcEngine).Span);
            if (definition.AudioSet != AudioBank.SpcEngine)
                raw.Uploads.Upload(stock.GetUpload(definition.AudioSet).Span);
            foreach ((int address, byte value) in SpcMusicDefinitionCodec.CompileBank(bank))
            {
                AssertEqual(
                    value,
                    raw.ReadApuByteForVerification(address),
                    $"{bank.Name} compiled music byte ${address:X4}");
            }
            for (int track = 0; track < bank.MusicTracks.Count; track++)
            {
                int pointer = SpcDriverDefinitions.Ram.DefaultMusicPointer + track * 2;
                ushort actual = unchecked(
                    LittleEndianBytes.Word(raw.ReadApuByteForVerification(pointer), raw.ReadApuByteForVerification(pointer + 1)));
                AssertEqual(bank.MusicTracks[track].Address, actual, $"{bank.Name} track {track} route");
            }
        }

        using var fixtureScratch = new TestScratchDirectory("audio-music-programs");
        string fixtureDirectory = fixtureScratch.Root;
        CopyAudioDirectory(sourceDirectory, fixtureDirectory);
        try
        {
            string manifestPath = Path.Combine(
                fixtureDirectory,
                ExtractedAudioAssetCatalog.ManifestFileName);
            AudioAssetManifest manifest = ReadAudioManifest(manifestPath);
            AudioBankMetadata title = manifest.Banks.Single(
                bank => bank.SnesAddress == (int)AudioUploadSourceDefinitions.TitleSequence);

            AudioMusicProgramMetadata invalidProgram = title.MusicPrograms[0] with
            {
                ByteCapacity = title.MusicPrograms[0].ByteCapacity + 1,
            };
            WriteAudioManifest(manifestPath, manifest with
            {
                Banks = manifest.Banks.Select(bank => bank.SnesAddress == title.SnesAddress
                    ? title with
                    {
                        MusicPrograms = title.MusicPrograms.Select(program =>
                            program.Id == invalidProgram.Id ? invalidProgram : program).ToArray(),
                    }
                    : bank).ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(fixtureDirectory),
                "music manifest rejects encoded-size mismatch");

            AudioMusicPhraseMetadata phrase = title.MusicPhrases.First(
                candidate => candidate.ChannelPrograms.Any(id => id is not null));
            string?[] missingReference = [.. phrase.ChannelPrograms];
            missingReference[Array.FindIndex(missingReference, id => id is not null)] = "missing-program";
            WriteAudioManifest(manifestPath, manifest with
            {
                Banks = manifest.Banks.Select(bank => bank.SnesAddress == title.SnesAddress
                    ? title with
                    {
                        MusicPhrases = title.MusicPhrases.Select(candidate => candidate.Id == phrase.Id
                            ? candidate with { ChannelPrograms = missingReference }
                            : candidate).ToArray(),
                    }
                    : bank).ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(fixtureDirectory),
                "music manifest rejects missing channel program");
        }
        finally
        {
            if (Directory.Exists(fixtureDirectory))
                Directory.Delete(fixtureDirectory, recursive: true);
        }

        Console.WriteLine(
            $"Music: {trackCount} tracks/{phraseCount} phrases/{programCount} programs/" +
            $"{instructionCount} instructions, exact stock bytes and strict failures pass.");
    }

    /// <summary>Runs the extracted music-program byte and manifest validation verification.</summary>
    [VerificationCommand("--audio-music-programs")]
    internal static int AudioMusicProgramsCommand()
    {
        Suite(nameof(VerifyMusicPrograms), () => VerifyMusicPrograms());
        return 0;
    }
}
