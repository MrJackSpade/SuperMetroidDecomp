namespace SuperMetroid.Core.Audio;

/// <summary>Stable JSON operation names for decoded SPC music content.</summary>
public static class AudioMusicInstructionOperations
{
    public const string End = "end";
    public const string Note = "note";
    public const string Tie = "tie";
    public const string Rest = "rest";
    public const string Percussion = "percussion";
    public const string PlayPhrase = "playPhrase";
    public const string FastForwardOn = "fastForwardOn";
    public const string FastForwardOff = "fastForwardOff";
    public const string Repeat = "repeat";

    internal static string ForEffect(SpcMusicEffect effect)
    {
        string name = effect.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

internal sealed record SpcMusicDefinitionBundle(
    IReadOnlyList<AudioMusicTrackMetadata> Tracks,
    IReadOnlyList<AudioMusicPhraseMetadata> Phrases,
    IReadOnlyList<AudioMusicProgramMetadata> Programs);

/// <summary>
/// Lossless bounded codec for authored music flow, phrase routing and channel bytecode.
/// Address arithmetic and playback remain managed-driver mechanics; definitions retain their
/// native fixed slots so edits cannot silently relocate into adjacent music or sample data.
/// </summary>
internal static class SpcMusicDefinitionCodec
{
    internal static SpcMusicDefinitionBundle DecodeBank(
        byte dataIndex,
        IReadOnlyList<ushort> trackPointers,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written)
    {
        var tracks = new List<AudioMusicTrackMetadata>(trackPointers.Count);
        Dictionary<ushort, AudioMusicPhraseMetadata> phrases = [];
        Dictionary<ushort, AudioMusicProgramMetadata> programs = [];
        for (int track = 0; track < trackPointers.Count; track++)
        {
            ushort address = trackPointers[track];
            AudioMusicTrackMetadata definition = DecodeTrack(
                dataIndex, track, address, ram, written, out ushort[] phraseAddresses);
            tracks.Add(definition);
            foreach (ushort phraseAddress in phraseAddresses)
                DecodePhrase(dataIndex, phraseAddress, ram, written, phrases, programs);
        }
        return new(
            tracks.AsReadOnly(),
            Array.AsReadOnly(phrases.Values.OrderBy(phrase => phrase.Address).ToArray()),
            Array.AsReadOnly(programs.Values.OrderBy(program => program.Address).ToArray()));
    }

    internal static byte[] EncodeTrack(AudioMusicTrackMetadata track)
    {
        ArgumentNullException.ThrowIfNull(track);
        if (track.Instructions is null)
            throw new InvalidDataException($"Music track '{track.Id}' has no instruction list.");
        if (track.Address == 0)
        {
            if (track.ByteCapacity != 0 || track.Instructions.Count != 0)
                throw new InvalidDataException($"Inactive music track '{track.Id}' must have no program data.");
            return [];
        }
        var bytes = new List<byte>(track.ByteCapacity);
        bool ended = false;
        foreach (AudioMusicTrackInstructionMetadata instruction in track.Instructions)
        {
            if (instruction is null)
                throw new InvalidDataException($"Music track '{track.Id}' contains a null instruction.");
            if (ended)
                throw new InvalidDataException($"Music track '{track.Id}' contains data after end.");
            switch (instruction.Operation)
            {
                case AudioMusicInstructionOperations.PlayPhrase:
                    if ((instruction.Value >> 8) < SpcDriverData.Music.PatternPointerHighByteMinimum ||
                        instruction.Target is not null)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, instruction.Value);
                    break;
                case AudioMusicInstructionOperations.End:
                    if (instruction.Value != 0 || instruction.Target is not null)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, 0);
                    ended = true;
                    break;
                case AudioMusicInstructionOperations.FastForwardOn:
                    if (instruction.Value != SpcDriverData.Music.PatternFastForwardOn ||
                        instruction.Target is not null)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, instruction.Value);
                    break;
                case AudioMusicInstructionOperations.FastForwardOff:
                    if (instruction.Value != SpcDriverData.Music.PatternFastForwardOff ||
                        instruction.Target is not null)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, instruction.Value);
                    break;
                case AudioMusicInstructionOperations.Repeat:
                    if ((instruction.Value >> 8) != 0 || instruction.Value is 0 or
                        SpcDriverData.Music.PatternFastForwardOn or
                        SpcDriverData.Music.PatternFastForwardOff || instruction.Target is null)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, instruction.Value);
                    WriteWord(bytes, instruction.Target.Value);
                    break;
                default:
                    throw new InvalidDataException(
                        $"Music track '{track.Id}' uses unknown operation '{instruction.Operation}'.");
            }
        }
        if (!ended)
            throw new InvalidDataException($"Music track '{track.Id}' has no end instruction.");
        ValidateCapacity(track.Id, track.ByteCapacity, bytes.Count);
        return [.. bytes];
    }

    internal static byte[] EncodeProgram(AudioMusicProgramMetadata program)
    {
        ArgumentNullException.ThrowIfNull(program);
        if (program.Instructions is null)
            throw new InvalidDataException($"Music program '{program.Id}' has no instruction list.");
        var bytes = new List<byte>(program.ByteCapacity);
        bool ended = false;
        foreach (AudioMusicInstructionMetadata instruction in program.Instructions)
        {
            if (instruction is null)
                throw new InvalidDataException($"Music program '{program.Id}' contains a null instruction.");
            if (instruction.Timing is null || instruction.Arguments is null)
                throw new InvalidDataException($"Music program '{program.Id}' has a null operand list.");
            if (ended)
                throw new InvalidDataException($"Music program '{program.Id}' contains data after end.");
            ValidateTiming(program.Id, instruction.Timing);
            foreach (byte timing in instruction.Timing)
                bytes.Add(timing);

            if (instruction.Operation == AudioMusicInstructionOperations.End)
            {
                if (instruction.Opcode != 0 || instruction.Timing.Count != 0 || instruction.Arguments.Count != 0)
                    throw InvalidProgramInstruction(program, instruction);
                bytes.Add(0);
                ended = true;
                continue;
            }
            if (instruction.Opcode >= SpcDriverData.Music.FirstEffect)
            {
                int effectIndex = instruction.Opcode - SpcDriverData.Music.FirstEffect;
                if ((uint)effectIndex >= SpcMusicTables.EffectByteLengths.Length ||
                    instruction.Operation != AudioMusicInstructionOperations.ForEffect(
                        (SpcMusicEffect)instruction.Opcode) ||
                    instruction.Arguments.Count != SpcMusicTables.EffectByteLengths[effectIndex])
                    throw InvalidProgramInstruction(program, instruction);
                bytes.Add(instruction.Opcode);
                foreach (byte argument in instruction.Arguments)
                    bytes.Add(argument);
                continue;
            }
            string expected = GetNoteOperation(instruction.Opcode);
            if (instruction.Operation != expected || instruction.Opcode < SpcDriverData.Music.CommandMarker ||
                instruction.Arguments.Count != 0)
                throw InvalidProgramInstruction(program, instruction);
            bytes.Add(instruction.Opcode);
        }
        if (!ended)
            throw new InvalidDataException($"Music program '{program.Id}' has no end instruction.");
        ValidateCapacity(program.Id, program.ByteCapacity, bytes.Count);
        return [.. bytes];
    }

    internal static IReadOnlyDictionary<int, byte> CompileBank(AudioBankMetadata bank)
    {
        Dictionary<string, AudioMusicProgramMetadata> programs = bank.MusicPrograms
            .ToDictionary(program => program.Id, StringComparer.Ordinal);
        var writes = new Dictionary<int, byte>();
        foreach (AudioMusicTrackMetadata track in bank.MusicTracks)
        {
            byte[] encoded = EncodeTrack(track);
            if (encoded.Length != 0)
                Merge(writes, track.Address, encoded, track.Id);
        }
        foreach (AudioMusicPhraseMetadata phrase in bank.MusicPhrases)
        {
            if (phrase.ChannelPrograms is null || phrase.ChannelPrograms.Count != SpcDriverData.ChannelCount)
                throw new InvalidDataException($"Music phrase '{phrase.Id}' must route eight channels.");
            byte[] bytes = new byte[SpcDriverData.ChannelCount * 2];
            for (int channel = 0; channel < phrase.ChannelPrograms.Count; channel++)
            {
                string? programId = phrase.ChannelPrograms[channel];
                ushort address = 0;
                if (programId is not null)
                {
                    if (!programs.TryGetValue(programId, out AudioMusicProgramMetadata? program))
                    {
                        throw new InvalidDataException(
                            $"Music phrase '{phrase.Id}' references unknown program '{programId}'.");
                    }
                    address = program.Address;
                }
                bytes[channel * 2] = unchecked((byte)address);
                bytes[channel * 2 + 1] = unchecked((byte)(address >> 8));
            }
            Merge(writes, phrase.Address, bytes, phrase.Id);
        }
        foreach (AudioMusicProgramMetadata program in bank.MusicPrograms)
            Merge(writes, program.Address, EncodeProgram(program), program.Id);
        return writes;
    }

    private static AudioMusicTrackMetadata DecodeTrack(
        byte dataIndex,
        int track,
        ushort address,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        out ushort[] phraseAddresses)
    {
        string id = $"music-{dataIndex:x2}-track-{track:00}";
        if (address == 0)
        {
            phraseAddresses = [];
            return new(track, id, 0, 0, []);
        }
        var instructions = new List<AudioMusicTrackInstructionMetadata>();
        var phrases = new List<ushort>();
        int cursor = address;
        for (int count = 0; count < SpcDriverData.ApuRamSize; count++)
        {
            ushort value = ReadWord(ram, written, ref cursor, id);
            if ((value >> 8) >= SpcDriverData.Music.PatternPointerHighByteMinimum)
            {
                instructions.Add(new(AudioMusicInstructionOperations.PlayPhrase, value, null));
                phrases.Add(value);
                continue;
            }
            if (value == 0)
            {
                instructions.Add(new(AudioMusicInstructionOperations.End, value, null));
                phraseAddresses = [.. phrases];
                return new(track, id, address, cursor - address, instructions);
            }
            if (value == SpcDriverData.Music.PatternFastForwardOn)
                instructions.Add(new(AudioMusicInstructionOperations.FastForwardOn, value, null));
            else if (value == SpcDriverData.Music.PatternFastForwardOff)
                instructions.Add(new(AudioMusicInstructionOperations.FastForwardOff, value, null));
            else
            {
                ushort target = ReadWord(ram, written, ref cursor, id);
                instructions.Add(new(AudioMusicInstructionOperations.Repeat, value, target));
            }
        }
        throw new InvalidDataException($"Music track '{id}' has no bounded end record.");
    }

    private static void DecodePhrase(
        byte dataIndex,
        ushort address,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        Dictionary<ushort, AudioMusicPhraseMetadata> phrases,
        Dictionary<ushort, AudioMusicProgramMetadata> programs)
    {
        if (phrases.ContainsKey(address))
            return;
        string id = $"music-{dataIndex:x2}-phrase-{address:x4}";
        int cursor = address;
        var channelPrograms = new string?[SpcDriverData.ChannelCount];
        var programAddresses = new List<ushort>();
        for (int channel = 0; channel < channelPrograms.Length; channel++)
        {
            ushort programAddress = ReadWord(ram, written, ref cursor, id);
            if ((programAddress >> 8) == 0)
                continue;
            if (!written[programAddress])
            {
                throw new InvalidDataException(
                    $"Music phrase '{id}' channel {channel} routes to non-uploaded program " +
                    $"${programAddress:X4}.");
            }
            string programId = $"music-{dataIndex:x2}-program-{programAddress:x4}";
            channelPrograms[channel] = programId;
            programAddresses.Add(programAddress);
        }
        phrases.Add(address, new(id, address, channelPrograms));
        foreach (ushort programAddress in programAddresses)
            DecodeProgram(dataIndex, programAddress, ram, written, programs);
    }

    private static void DecodeProgram(
        byte dataIndex,
        ushort address,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        Dictionary<ushort, AudioMusicProgramMetadata> programs)
    {
        if (programs.ContainsKey(address))
            return;
        string id = $"music-{dataIndex:x2}-program-{address:x4}";
        var instructions = new List<AudioMusicInstructionMetadata>();
        var callTargets = new List<ushort>();
        int cursor = address;
        for (int count = 0; count < SpcDriverData.ApuRamSize; count++)
        {
            byte command = ReadByte(ram, written, ref cursor, id);
            if (command == 0)
            {
                instructions.Add(new(AudioMusicInstructionOperations.End, 0, [], []));
                programs.Add(address, new(id, address, cursor - address, instructions));
                foreach (ushort target in callTargets)
                    DecodeProgram(dataIndex, target, ram, written, programs);
                return;
            }

            var timing = new List<byte>(2);
            if ((command & SpcDriverData.Music.CommandMarker) == 0)
            {
                timing.Add(command);
                command = ReadByte(ram, written, ref cursor, id);
                if ((command & SpcDriverData.Music.CommandMarker) == 0)
                {
                    timing.Add(command);
                    command = ReadByte(ram, written, ref cursor, id);
                }
            }
            if (command >= SpcDriverData.Music.FirstEffect)
            {
                int effectIndex = command - SpcDriverData.Music.FirstEffect;
                if ((uint)effectIndex >= SpcMusicTables.EffectByteLengths.Length)
                    throw new InvalidDataException($"Music program '{id}' uses unknown effect ${command:X2}.");
                int argumentCount = SpcMusicTables.EffectByteLengths[effectIndex];
                byte[] arguments = new byte[argumentCount];
                for (int index = 0; index < arguments.Length; index++)
                    arguments[index] = ReadByte(ram, written, ref cursor, id);
                var effect = (SpcMusicEffect)command;
                instructions.Add(new(
                    AudioMusicInstructionOperations.ForEffect(effect),
                    command,
                    timing,
                    arguments));
                if (effect == SpcMusicEffect.CallPattern)
                {
                    ushort target = unchecked((ushort)(arguments[0] | (arguments[1] << 8)));
                    callTargets.Add(target);
                }
            }
            else
            {
                instructions.Add(new(GetNoteOperation(command), command, timing, []));
            }
        }
        throw new InvalidDataException($"Music program '{id}' has no bounded end command.");
    }

    private static string GetNoteOperation(byte opcode) => opcode switch
    {
        SpcDriverData.Music.TieNote => AudioMusicInstructionOperations.Tie,
        SpcDriverData.Music.RestNote => AudioMusicInstructionOperations.Rest,
        >= SpcDriverData.Music.FirstPercussionNote => AudioMusicInstructionOperations.Percussion,
        _ => AudioMusicInstructionOperations.Note,
    };

    private static void ValidateTiming(string id, IReadOnlyList<byte> timing)
    {
        if (timing.Count > 2 || timing.Any(value =>
                value == 0 || value >= SpcDriverData.Music.CommandMarker))
            throw new InvalidDataException($"Music program '{id}' has an invalid timing prefix.");
    }

    private static void Merge(Dictionary<int, byte> writes, int address, byte[] bytes, string owner)
    {
        if (address < 0 || address > SpcDriverData.ApuRamSize - bytes.Length)
            throw new InvalidDataException($"Music definition '{owner}' overruns APU RAM at ${address:X4}.");
        for (int index = 0; index < bytes.Length; index++)
        {
            int destination = address + index;
            if (writes.TryGetValue(destination, out byte existing) && existing != bytes[index])
            {
                throw new InvalidDataException(
                    $"Music definition '{owner}' conflicts with another authored object at ${destination:X4}.");
            }
            writes[destination] = bytes[index];
        }
    }

    private static byte ReadByte(
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        ref int cursor,
        string id)
    {
        if ((uint)cursor >= ram.Length || (uint)cursor >= written.Length || !written[cursor])
            throw new InvalidDataException($"Music definition '{id}' reads outside uploaded content at ${cursor:X4}.");
        return ram[cursor++];
    }

    private static ushort ReadWord(
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        ref int cursor,
        string id)
    {
        byte low = ReadByte(ram, written, ref cursor, id);
        byte high = ReadByte(ram, written, ref cursor, id);
        return unchecked((ushort)(low | (high << 8)));
    }

    private static void WriteWord(List<byte> bytes, ushort value)
    {
        bytes.Add(unchecked((byte)value));
        bytes.Add(unchecked((byte)(value >> 8)));
    }

    private static InvalidDataException InvalidTrackInstruction(
        AudioMusicTrackMetadata track,
        AudioMusicTrackInstructionMetadata instruction) =>
        new InvalidDataException(
            $"Music track '{track.Id}' has invalid '{instruction.Operation}' operands.");

    private static InvalidDataException InvalidProgramInstruction(
        AudioMusicProgramMetadata program,
        AudioMusicInstructionMetadata instruction) =>
        new InvalidDataException(
            $"Music program '{program.Id}' operation '{instruction.Operation}' does not match " +
            $"opcode ${instruction.Opcode:X2} or its operand shape.");

    private static void ValidateCapacity(string id, int expected, int actual)
    {
        if (expected <= 0 || expected != actual)
        {
            throw new InvalidDataException(
                $"Music definition '{id}' encodes to {actual} bytes; its fixed native slot is " +
                $"{expected} bytes. Edit operands or same-size operations without relocating content.");
        }
    }
}
