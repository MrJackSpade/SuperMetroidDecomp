using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifyEditableMusicPrograms()
    {
        string sourceDirectory = Path.GetFullPath("standalone-assets/audio");
        ExtractedAudioAssetCatalog stock = ExtractedAudioAssetCatalog.Load(sourceDirectory);
        int trackCount = 0;
        int phraseCount = 0;
        int programCount = 0;
        int instructionCount = 0;
        foreach (AudioUploadAssetDefinition definition in AudioAssetCatalogData.All)
        {
            AudioBankMetadata bank = stock.GetMusicBank(definition.SnesAddress);
            trackCount += bank.MusicTracks.Count;
            phraseCount += bank.MusicPhrases.Count;
            programCount += bank.MusicPrograms.Count;
            instructionCount += bank.MusicPrograms.Sum(program => program.Instructions.Count);
            var raw = new ManagedSpcPlayer();
            raw.Upload(stock.GetUpload(AudioUploadAddresses.SpcEngine).Span);
            if (definition.SnesAddress != AudioUploadAddresses.SpcEngine)
                raw.Upload(stock.GetUpload(definition.SnesAddress).Span);
            foreach ((int address, byte value) in SpcMusicDefinitionCodec.CompileBank(bank))
            {
                AssertEqual(
                    value,
                    raw.ReadApuByteForVerification(address),
                    $"{bank.Name} compiled music byte ${address:X4}");
            }
            for (int track = 0; track < bank.MusicTracks.Count; track++)
            {
                int pointer = SpcDriverData.Ram.DefaultMusicPointer + track * 2;
                ushort actual = unchecked((ushort)(
                    raw.ReadApuByteForVerification(pointer) |
                    (raw.ReadApuByteForVerification(pointer + 1) << 8)));
                AssertEqual(bank.MusicTracks[track].Address, actual, $"{bank.Name} track {track} route");
            }
        }

        string editedDirectory = Path.Combine(
            Path.GetTempPath(), $"sm-audio-music-programs-{Guid.NewGuid():N}");
        CopyAudioDirectory(sourceDirectory, editedDirectory);
        try
        {
            string manifestPath = Path.Combine(
                editedDirectory,
                ExtractedAudioAssetCatalog.ManifestFileName);
            AudioAssetManifest manifest = ReadAudioManifest(manifestPath);
            AudioBankMetadata title = manifest.Banks.Single(
                bank => bank.SnesAddress == AudioUploadAddresses.TitleSequence);
            int titleTrackIndex = AudioRomData.MusicTracks.Title;
            AudioMusicTrackMetadata titleTrack = title.MusicTracks[titleTrackIndex];
            HashSet<ushort> titlePhraseAddresses = titleTrack.Instructions
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
                    instruction.Opcode <= SpcDriverData.Music.TieNote - 0x0d);
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
                    ? editedTitle!
                    : bank).ToArray(),
            };
            WriteAudioManifest(manifestPath, editedManifest);

            ExtractedAudioAssetCatalog edited = ExtractedAudioAssetCatalog.Load(editedDirectory);
            var stockRenderer = new CartridgeAudioRenderer(stock);
            var editedRenderer = new CartridgeAudioRenderer(edited);
            CartridgeAudioCommand common = CartridgeAudioCommand.Upload(AudioUploadAddresses.SpcEngine);
            CartridgeAudioCommand titleUpload = CartridgeAudioCommand.Upload(AudioUploadAddresses.TitleSequence);
            CartridgeAudioCommand titleCommand = CartridgeAudioCommand.WritePort(
                AudioRomData.Apu.MusicPort,
                unchecked((byte)titleTrackIndex));
            stockRenderer.RenderFrame([common]);
            editedRenderer.RenderFrame([common]);
            stockRenderer.RenderFrame([titleUpload]);
            editedRenderer.RenderFrame([titleUpload]);
            stockRenderer.RenderFrame([titleCommand]);
            editedRenderer.RenderFrame([titleCommand]);

            foreach ((int address, byte value) in SpcMusicDefinitionCodec.CompileBank(editedTitle!))
            {
                AssertEqual(
                    value,
                    editedRenderer.Player.ReadApuByteForVerification(address),
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
                pcmDiffers |= !stockPcm.AsSpan().SequenceEqual(editedPcm);
            }
            AssertTrue(stockAudible, "stock decoded title music is audible");
            AssertTrue(editedAudible, "edited decoded title music is audible");
            AssertTrue(pcmDiffers, "decoded title note edit changes rendered PCM");

            AudioMusicProgramMetadata invalidProgram = editedProgram! with
            {
                ByteCapacity = editedProgram!.ByteCapacity + 1,
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
}
