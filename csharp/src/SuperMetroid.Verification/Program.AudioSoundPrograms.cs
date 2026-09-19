using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifyEditableSoundEffectPrograms()
    {
        string sourceDirectory = Path.GetFullPath("standalone-assets/audio");
        ExtractedAudioAssetCatalog stock = ExtractedAudioAssetCatalog.Load(sourceDirectory);
        var rawPlayer = new ManagedSpcPlayer();
        rawPlayer.Upload(stock.GetUpload(AudioUploadAddresses.SpcEngine).Span);

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
                    ushort address = unchecked((ushort)(
                        rawPlayer.ReadApuByteForVerification(pointer) |
                        (rawPlayer.ReadApuByteForVerification(pointer + 1) << 8)));
                    AssertEqual(program.Address, address, $"{effect.Id} channel {channel} routing");
                }
            }
        }

        string editedDirectory = Path.Combine(
            Path.GetTempPath(), $"sm-audio-sfx-programs-{Guid.NewGuid():N}");
        CopyAudioDirectory(sourceDirectory, editedDirectory);
        try
        {
            string manifestPath = Path.Combine(
                editedDirectory,
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
            byte[] editedArguments = [.. originalPlay.Arguments];
            editedArguments[3] = editedArguments[3] <= 0xe8
                ? unchecked((byte)(editedArguments[3] + 0x0c))
                : unchecked((byte)(editedArguments[3] - 0x0c));
            AudioSoundInstructionMetadata[] editedInstructions = [.. originalProgram.Instructions];
            editedInstructions[playIndex] = originalPlay with { Arguments = editedArguments };
            AudioSoundProgramMetadata editedProgram = originalProgram with
            {
                Instructions = editedInstructions,
            };
            AudioAssetManifest editedManifest = manifest with
            {
                SoundPrograms = manifest.SoundPrograms.Select(program => program.Id == programId
                    ? editedProgram
                    : program).ToArray(),
            };
            WriteAudioManifest(manifestPath, editedManifest);

            ExtractedAudioAssetCatalog edited = ExtractedAudioAssetCatalog.Load(editedDirectory);
            var stockRenderer = new CartridgeAudioRenderer(stock);
            var editedRenderer = new CartridgeAudioRenderer(edited);
            CartridgeAudioCommand common = CartridgeAudioCommand.Upload(AudioUploadAddresses.SpcEngine);
            stockRenderer.RenderFrame([common]);
            editedRenderer.RenderFrame([common]);
            for (int offset = 0; offset < editedProgram.ByteCapacity; offset++)
            {
                byte expected = SpcSoundEffectProgramCodec.Encode(editedProgram)[offset];
                AssertEqual(
                    expected,
                    editedRenderer.Player.ReadApuByteForVerification(editedProgram.Address + offset),
                    $"edited SFX program live byte {offset}");
            }

            CartridgeAudioCommand trigger = CartridgeAudioCommand.WritePort(
                AudioRomData.Apu.FirstSoundPort,
                effect.Command);
            bool stockAudible = false;
            bool editedAudible = false;
            bool pcmDiffers = false;
            for (int frame = 0; frame < 120; frame++)
            {
                short[] stockPcm = stockRenderer.RenderFrame(frame == 0 ? [trigger] : []);
                short[] editedPcm = editedRenderer.RenderFrame(frame == 0 ? [trigger] : []);
                stockAudible |= stockPcm.Any(sample => sample != 0);
                editedAudible |= editedPcm.Any(sample => sample != 0);
                pcmDiffers |= !stockPcm.AsSpan().SequenceEqual(editedPcm);
            }
            AssertTrue(stockAudible, $"stock {effect.Id} fixture is audible");
            AssertTrue(editedAudible, $"edited {effect.Id} fixture is audible");
            AssertTrue(pcmDiffers, "decoded SFX note edit changes rendered PCM");

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
                () => ExtractedAudioAssetCatalog.Load(editedDirectory),
                "SFX manifest rejects unknown operation");

            WriteAudioManifest(manifestPath, manifest with
            {
                SoundPrograms = manifest.SoundPrograms.Select(program => program.Id == programId
                    ? program with { ByteCapacity = program.ByteCapacity + 1 }
                    : program).ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(editedDirectory),
                "SFX manifest rejects encoded-size mismatch");

            WriteAudioManifest(manifestPath, manifest with
            {
                SoundPrograms = manifest.SoundPrograms.Where(program => program.Id != programId).ToArray(),
            });
            AssertThrows<InvalidDataException>(
                () => ExtractedAudioAssetCatalog.Load(editedDirectory),
                "SFX manifest rejects missing referenced program");
        }
        finally
        {
            if (Directory.Exists(editedDirectory))
                Directory.Delete(editedDirectory, recursive: true);
        }

        Console.WriteLine(
            $"Editable SFX programs: {stock.SoundPrograms.Count} programs/{instructionCount} named instructions, " +
            "exact stock bytes, live audible note edit and strict failures pass.");
    }
}
