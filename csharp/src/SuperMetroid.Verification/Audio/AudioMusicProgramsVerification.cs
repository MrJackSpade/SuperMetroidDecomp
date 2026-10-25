using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Audio.AudioInstrumentsVerification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks editable SPC music definitions against uploaded APU bytes and rendered title audio.</summary>
internal static class AudioMusicProgramsVerification
{
    /// <summary>Matches compiled stock banks to APU RAM, then checks an authored note edit changes audible PCM and malformed references fail.</summary>
    internal static void VerifyEditableMusicPrograms()
    {
        string sourceDirectory = RepositoryInstallation.Installation.AudioDirectory;
        ExtractedAudioAssetCatalog stock = ExtractedAudioAssetCatalog.Load(sourceDirectory);
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
                    SnesLittleEndian.Word(raw.ReadApuByteForVerification(pointer), raw.ReadApuByteForVerification(pointer + 1)));
                AssertEqual(bank.MusicTracks[track].Address, actual, $"{bank.Name} track {track} route");
            }
        }

        using var editedDirectoryScratch = new TestScratchDirectory("audio-music-programs");
        string editedDirectory = editedDirectoryScratch.Root;
        CopyAudioDirectory(sourceDirectory, editedDirectory);
        try
        {
            string manifestPath = Path.Combine(
                editedDirectory,
                ExtractedAudioAssetCatalog.ManifestFileName);
            AudioAssetManifest manifest = ReadAudioManifest(manifestPath);
            AudioBankMetadata title = manifest.Banks.Single(
                bank => bank.SnesAddress == (int)AudioUploadSourceDefinitions.TitleSequence);
            int titleTrackIndex = AudioRomDataMusicTracks.Title;
            AudioMusicTrackMetadata titleTrack = title.MusicTracks[titleTrackIndex];
            HashSet<int> titlePhraseAddresses = titleTrack.Instructions
                .Where(instruction => instruction.Operation == AudioMusicInstructionOperations.PlayPhrase)
                .Select(instruction => instruction.Value)
                .ToHashSet();
            string[] candidateIds = title.MusicPhrases
                .Where(phrase => titlePhraseAddresses.Contains(phrase.Address))
                .SelectMany(phrase => phrase.ChannelPrograms)
                .Where(id => id is not null)
                .Cast<string>()
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            AudioBankMetadata? editedTitle = null;
            AudioMusicProgramMetadata? editedProgram = null;
            foreach (string candidateId in candidateIds)
            {
                AudioMusicProgramMetadata candidate = title.MusicPrograms.Single(
                    program => program.Id == candidateId);
                int noteIndex = candidate.Instructions.ToList().FindIndex(instruction =>
                    instruction.Operation == AudioMusicInstructionOperations.Note &&
                    instruction.Opcode <= SpcDriverDefinitions.Music.TieNote - 0x0d);
                if (noteIndex < 0)
                    continue;
                AudioMusicInstructionMetadata[] instructions = [.. candidate.Instructions];
                instructions[noteIndex] = instructions[noteIndex] with
                {
                    Opcode = unchecked((byte)(instructions[noteIndex].Opcode + 0x0c)),
                };
                AudioMusicProgramMetadata changed = candidate with { Instructions = instructions };
                AudioBankMetadata proposed = title with
                {
                    MusicPrograms = title.MusicPrograms.Select(program => program.Id == candidateId
                        ? changed
                        : program).ToArray(),
                };
                try
                {
                    _ = SpcMusicDefinitionCodec.CompileBank(proposed);
                    editedTitle = proposed;
                    editedProgram = changed;
                    break;
                }
                catch (InvalidDataException)
                {
                    // Some native entry points deliberately alias a suffix of another program.
                    // Keep searching for a directly authored byte that has one unambiguous owner.
                }
            }
            AssertTrue(editedTitle is not null && editedProgram is not null,
                "title fixture contains an independently editable audible note");
            AudioAssetManifest editedManifest = manifest with
            {
                Banks = manifest.Banks.Select(bank => bank.SnesAddress == title.SnesAddress
                    ? editedTitle ?? throw new InvalidOperationException("Verification requires an editable title bank.")
                    : bank).ToArray(),
            };
            WriteAudioManifest(manifestPath, editedManifest);

            ExtractedAudioAssetCatalog edited = ExtractedAudioAssetCatalog.Load(editedDirectory);
            var stockRenderer = AudioSessionAccess.CreateSynthesizing(stock);
            var editedRenderer = AudioSessionAccess.CreateSynthesizing(edited);
            AudioRequest common = new AudioRequest.ChangeMusicDataSet(null);
            AudioRequest titleUpload = new AudioRequest.ChangeMusicDataSet(MusicDataSet.TitleSequence);
            AudioRequest titleCommand = AudioRequest.ForTrack(new MusicTrack(unchecked((byte)titleTrackIndex)));
            stockRenderer.RenderFrame([common]);
            editedRenderer.RenderFrame([common]);
            stockRenderer.RenderFrame([titleUpload]);
            editedRenderer.RenderFrame([titleUpload]);
            stockRenderer.RenderFrame([titleCommand]);
            editedRenderer.RenderFrame([titleCommand]);

            foreach ((int address, byte value) in SpcMusicDefinitionCodec.CompileBank((editedTitle ?? throw new InvalidOperationException("Verification requires editedTitle."))))
            {
                AssertEqual(
                    value,
                    editedRenderer.PlayerForVerification.ReadApuByteForVerification(address),
                    $"edited title music byte ${address:X4}");
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
            AssertTrue(stockAudible, "stock decoded title music is audible");
            AssertTrue(editedAudible, "edited decoded title music is audible");
            AssertTrue(pcmDiffers, "decoded title note edit changes rendered PCM");

            AudioMusicProgramMetadata invalidProgram = (editedProgram ?? throw new InvalidOperationException("Verification requires editedProgram.")) with
            {
                ByteCapacity = editedProgram.ByteCapacity + 1,
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
                () => ExtractedAudioAssetCatalog.Load(editedDirectory),
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
                () => ExtractedAudioAssetCatalog.Load(editedDirectory),
                "music manifest rejects missing channel program");
        }
        finally
        {
            if (Directory.Exists(editedDirectory))
                Directory.Delete(editedDirectory, recursive: true);
        }

        Console.WriteLine(
            $"Editable music: {trackCount} tracks/{phraseCount} phrases/{programCount} programs/" +
            $"{instructionCount} instructions, exact stock bytes, live audible note edit and strict failures pass.");
    }

    /// <summary>Runs the extracted music-program byte and audible-edit verification.</summary>
    [VerificationCommand("--audio-music-programs")]
    internal static int AudioMusicProgramsCommand()
    {
        Suite(nameof(VerifyEditableMusicPrograms), () => VerifyEditableMusicPrograms());
        return 0;
    }
}
