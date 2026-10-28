using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Lossless bounded codec for authored music flow, phrase routing and channel bytecode.
/// Address arithmetic and playback remain managed-driver mechanics; definitions retain their
/// native fixed slots so edits cannot silently relocate into adjacent music or sample data.
/// </summary>
[EncodingOwner("sound driver music track, phrase and channel program bytecode")]
internal static class SpcMusicDefinitionCodec
{
    /// <summary>Walks selected track pointers and their referenced phrase and channel programs within uploaded APU RAM.</summary>
    internal static SpcMusicDefinitionBundle DecodeBank(
        int dataIndex,
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

    /// <summary>Encodes one track control flow and requires it to fit its fixed native byte slot.</summary>
    internal static byte[] EncodeTrack(AudioMusicTrackMetadata track)
    {
        Ensure.NotNull(track);
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
                    if ((LittleEndianBytes.HighByte(checked((ushort)instruction.Value))) < SpcDriverDefinitions.Music.PatternPointerHighByteMinimum ||
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
                    if (!MusicTrackControls.Is(instruction.Value, MusicTrackControl.FastForwardOn) ||
                        instruction.Target is not null)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, instruction.Value);
                    break;
                case AudioMusicInstructionOperations.FastForwardOff:
                    if (!MusicTrackControls.Is(instruction.Value, MusicTrackControl.FastForwardOff) ||
                        instruction.Target is not null)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, instruction.Value);
                    break;
                case AudioMusicInstructionOperations.Repeat:
                    if ((LittleEndianBytes.HighByte(checked((ushort)instruction.Value))) != 0 || instruction.Value == 0 ||
                        MusicTrackControls.TryDecode(instruction.Value, out _) || instruction.Target is null)
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

    /// <summary>Encodes one channel instruction program with validated opcodes, timing, and operands.</summary>
    internal static byte[] EncodeProgram(AudioMusicProgramMetadata program)
    {
        Ensure.NotNull(program);
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
            // Index the operand lists: enumerating an IReadOnlyList boxes an enumerator per list.
            for (int index = 0; index < instruction.Timing.Count; index++)
                bytes.Add(instruction.Timing[index]);

            if (instruction.Operation == AudioMusicInstructionOperations.End)
            {
                if (instruction.Opcode != 0 || instruction.Timing.Count != 0 || instruction.Arguments.Count != 0)
                    throw InvalidProgramInstruction(program, instruction);
                bytes.Add(0);
                ended = true;
                continue;
            }
            if (instruction.Opcode >= SpcDriverDefinitions.Music.FirstEffect)
            {
                int effectIndex = instruction.Opcode - SpcDriverDefinitions.Music.FirstEffect;
                if ((uint)effectIndex >= SpcMusicTables.EffectCount ||
                    instruction.Operation != AudioMusicInstructionOperations.ForEffect(
                        (SpcMusicEffect)instruction.Opcode) ||
                    instruction.Arguments.Count != SpcMusicTables.EffectByteLength(effectIndex))
                    throw InvalidProgramInstruction(program, instruction);
                bytes.Add(checked((byte)instruction.Opcode));
                for (int index = 0; index < instruction.Arguments.Count; index++)
                    bytes.Add(instruction.Arguments[index]);
                continue;
            }
            string expected = GetNoteOperation(instruction.Opcode);
            if (instruction.Operation != expected || instruction.Opcode < SpcDriverDefinitions.Music.CommandMarker ||
                instruction.Arguments.Count != 0)
                throw InvalidProgramInstruction(program, instruction);
            bytes.Add(checked((byte)instruction.Opcode));
        }
        if (!ended)
            throw new InvalidDataException($"Music program '{program.Id}' has no end instruction.");
        ValidateCapacity(program.Id, program.ByteCapacity, bytes.Count);
        return [.. bytes];
    }

    /// <summary>Combines encoded tracks, phrases, and programs while rejecting conflicting APU RAM writes.</summary>
    internal static IReadOnlyDictionary<int, byte> CompileBank(AudioBankMetadata bank)
    {
        Dictionary<string, AudioMusicProgramMetadata> programs = bank.MusicPrograms
            .ToDictionary(program => program.Id, StringComparer.Ordinal);
        // Programs dominate the image; size the table once rather than rehashing as it grows.
        int expectedBytes = bank.MusicPhrases.Count * SpcDriverDefinitions.ChannelCount * 2;
        foreach (AudioMusicProgramMetadata program in bank.MusicPrograms) expectedBytes += program.ByteCapacity;
        var writes = new Dictionary<int, byte>(expectedBytes);
        foreach (AudioMusicTrackMetadata track in bank.MusicTracks)
        {
            byte[] encoded = EncodeTrack(track);
            if (encoded.Length != 0)
                Merge(writes, track.Address, encoded, track.Id);
        }
        foreach (AudioMusicPhraseMetadata phrase in bank.MusicPhrases)
        {
            if (phrase.ChannelPrograms is null || phrase.ChannelPrograms.Count != SpcDriverDefinitions.ChannelCount)
                throw new InvalidDataException($"Music phrase '{phrase.Id}' must route eight channels.");
            byte[] bytes = new byte[SpcDriverDefinitions.ChannelCount * 2];
            for (int channel = 0; channel < phrase.ChannelPrograms.Count; channel++)
            {
                string? programId = phrase.ChannelPrograms[channel];
                int address = 0;
                if (programId is not null)
                {
                    if (!programs.TryGetValue(programId, out AudioMusicProgramMetadata? program))
                    {
                        throw new InvalidDataException(
                            $"Music phrase '{phrase.Id}' references unknown program '{programId}'.");
                    }
                    address = program.Address;
                }
                bytes[channel * 2] = LittleEndianBytes.LowByte(checked((ushort)address));
                bytes[channel * 2 + 1] = LittleEndianBytes.HighByte(checked((ushort)address));
            }
            Merge(writes, phrase.Address, bytes, phrase.Id);
        }
        foreach (AudioMusicProgramMetadata program in bank.MusicPrograms)
            Merge(writes, program.Address, EncodeProgram(program), program.Id);
        return writes;
    }

    /// <summary>Reads a bounded track control stream and collects its referenced phrase addresses.</summary>
    private static AudioMusicTrackMetadata DecodeTrack(
        int dataIndex,
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
        for (int count = 0; count < SpcDriverDefinitions.ApuRamSize; count++)
        {
            ushort value = ReadWord(ram, written, ref cursor, id);
            if ((LittleEndianBytes.HighByte(value)) >= SpcDriverDefinitions.Music.PatternPointerHighByteMinimum)
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
            if (MusicTrackControls.TryDecode(value, out MusicTrackControl control))
                instructions.Add(new(control switch
                {
                    MusicTrackControl.FastForwardOn => AudioMusicInstructionOperations.FastForwardOn,
                    MusicTrackControl.FastForwardOff => AudioMusicInstructionOperations.FastForwardOff,
                    _ => throw new InvalidOperationException($"Undefined {nameof(MusicTrackControl)} {(int)control}."),
                }, value, null));
            else
            {
                ushort target = ReadWord(ram, written, ref cursor, id);
                instructions.Add(new(AudioMusicInstructionOperations.Repeat, value, target));
            }
        }
        throw new InvalidDataException($"Music track '{id}' has no bounded end record.");
    }

    /// <summary>Reads eight channel-program pointers and decodes each unique referenced program.</summary>
    private static void DecodePhrase(
        int dataIndex,
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
        var channelPrograms = new string?[SpcDriverDefinitions.ChannelCount];
        var programAddresses = new List<ushort>();
        for (int channel = 0; channel < channelPrograms.Length; channel++)
        {
            ushort programAddress = ReadWord(ram, written, ref cursor, id);
            if (LittleEndianBytes.HighByte(programAddress) == 0)
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

    /// <summary>Decodes a bounded channel bytecode stream through its end command.</summary>
    private static void DecodeProgram(
        int dataIndex,
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
        for (int count = 0; count < SpcDriverDefinitions.ApuRamSize; count++)
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
            if ((command & SpcDriverDefinitions.Music.CommandMarker) == 0)
            {
                timing.Add(command);
                command = ReadByte(ram, written, ref cursor, id);
                if ((command & SpcDriverDefinitions.Music.CommandMarker) == 0)
                {
                    timing.Add(command);
                    command = ReadByte(ram, written, ref cursor, id);
                }
            }
            if (command >= SpcDriverDefinitions.Music.FirstEffect)
            {
                int effectIndex = command - SpcDriverDefinitions.Music.FirstEffect;
                if ((uint)effectIndex >= SpcMusicTables.EffectCount)
                    throw new InvalidDataException($"Music program '{id}' uses unknown effect ${command:X2}.");
                int argumentCount = SpcMusicTables.EffectByteLength(effectIndex);
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
                    ushort target = unchecked(LittleEndianBytes.Word(arguments[0], arguments[1]));
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

    /// <summary>Distinguishes pitched notes, tie, rest, and percussion in the note opcode range.</summary>
    private static string GetNoteOperation(int opcode)
    {
        if (Enum.IsDefined((NoteCommand)opcode))
        {
            return (NoteCommand)opcode switch
            {
                NoteCommand.Tie => AudioMusicInstructionOperations.Tie,
                NoteCommand.Rest => AudioMusicInstructionOperations.Rest,
                _ => throw new InvalidOperationException($"Undefined {nameof(NoteCommand)} {opcode:X2}."),
            };
        }
        return opcode >= SpcDriverDefinitions.Music.FirstPercussionNote
            ? AudioMusicInstructionOperations.Percussion
            : AudioMusicInstructionOperations.Note;
    }

    /// <summary>The two note-range bytes that are not pitches: tie and rest. Pitches and percussion stay numeric.</summary>
    private enum NoteCommand : byte
    {
        /// <summary>Extends the previous note.</summary>
        Tie = SpcDriverDefinitions.Music.TieNote,
        /// <summary>Silences the track for the note duration.</summary>
        Rest = SpcDriverDefinitions.Music.RestNote,
    }

    /// <summary>Allows at most two nonzero timing bytes below the SPC command marker.</summary>
    private static void ValidateTiming(string id, IReadOnlyList<byte> timing)
    {
        if (timing.Count > 2 || timing.Any(value =>
                value is 0 or >= SpcDriverDefinitions.Music.CommandMarker))
            throw new InvalidDataException($"Music program '{id}' has an invalid timing prefix.");
    }

    /// <summary>Writes an authored object into APU RAM without overruns or conflicting shared bytes.</summary>
    private static void Merge(Dictionary<int, byte> writes, int address, byte[] bytes, string owner)
    {
        if (address < 0 || address > SpcDriverDefinitions.ApuRamSize - bytes.Length)
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

    /// <summary>Consumes one byte only when the upload map marks its APU RAM address as written.</summary>
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

    /// <summary>Consumes one little-endian word through the checked uploaded-byte reader.</summary>
    private static ushort ReadWord(
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        ref int cursor,
        string id)
    {
        byte low = ReadByte(ram, written, ref cursor, id);
        byte high = ReadByte(ram, written, ref cursor, id);
        return unchecked(LittleEndianBytes.Word(low, high));
    }

    /// <summary>Appends a native little-endian word to an encoded music stream.</summary>
    private static void WriteWord(List<byte> bytes, int value)
    {
        ushort word = checked((ushort)value);
        bytes.Add(LittleEndianBytes.LowByte(word));
        bytes.Add(LittleEndianBytes.HighByte(word));
    }

    /// <summary>Creates a diagnostic for a track operation with invalid operand shape.</summary>
    private static InvalidDataException InvalidTrackInstruction(
        AudioMusicTrackMetadata track,
        AudioMusicTrackInstructionMetadata instruction) =>
        new InvalidDataException(
            $"Music track '{track.Id}' has invalid '{instruction.Operation}' operands.");

    /// <summary>Creates a diagnostic for a channel opcode and operation mismatch.</summary>
    private static InvalidDataException InvalidProgramInstruction(
        AudioMusicProgramMetadata program,
        AudioMusicInstructionMetadata instruction) =>
        new InvalidDataException(
            $"Music program '{program.Id}' operation '{instruction.Operation}' does not match " +
            $"opcode ${instruction.Opcode:X2} or its operand shape.");

    /// <summary>Requires an encoded object to occupy exactly its original fixed byte slot.</summary>
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
