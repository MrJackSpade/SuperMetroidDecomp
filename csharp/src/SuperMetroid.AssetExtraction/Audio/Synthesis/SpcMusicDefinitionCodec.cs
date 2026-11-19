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
    /// <summary>
    /// The effects in opcode order: the driver's effect opcodes are consecutive bytes from
    /// <see cref="SpcDriverDefinitions.Music.FirstEffect"/>, one per effect.
    /// </summary>
    private static readonly SpcMusicEffect[] effectsByOpcode =
    [
        SpcMusicEffect.SetInstrument, SpcMusicEffect.SetPan, SpcMusicEffect.FadePan, SpcMusicEffect.EnableVibrato,
        SpcMusicEffect.DisableVibrato, SpcMusicEffect.SetMasterVolume, SpcMusicEffect.FadeMasterVolume, SpcMusicEffect.SetTempo,
        SpcMusicEffect.FadeTempo, SpcMusicEffect.SetGlobalTransposition, SpcMusicEffect.SetChannelTransposition,
        SpcMusicEffect.EnableTremolo, SpcMusicEffect.DisableTremolo, SpcMusicEffect.SetChannelVolume,
        SpcMusicEffect.FadeChannelVolume, SpcMusicEffect.CallPattern, SpcMusicEffect.FadeVibrato, SpcMusicEffect.PitchEnvelopeTo,
        SpcMusicEffect.PitchEnvelopeFrom, SpcMusicEffect.DisablePitchEnvelope, SpcMusicEffect.SetFineTune,
        SpcMusicEffect.EnableEcho, SpcMusicEffect.DisableEcho, SpcMusicEffect.ConfigureEcho, SpcMusicEffect.FadeEchoVolume,
        SpcMusicEffect.PitchSlide, SpcMusicEffect.SetPercussionBase, SpcMusicEffect.SkipByte, SpcMusicEffect.CutKey,
        SpcMusicEffect.FastForwardForFrames, SpcMusicEffect.SetFastForward,
    ];

    /// <summary>The number of effect opcodes, which follow one another from the first.</summary>
    internal static int EffectCount => effectsByOpcode.Length;

    /// <summary>The effect <paramref name="opcode"/> selects; a byte outside the effect opcodes is invalid data.</summary>
    internal static SpcMusicEffect DecodeEffect(int opcode)
    {
        int index = opcode - SpcDriverDefinitions.Music.FirstEffect;
        if ((uint)index >= (uint)effectsByOpcode.Length)
            throw new InvalidDataException($"${opcode:X2} is not an SPC music effect opcode.");
        return effectsByOpcode[index];
    }

    /// <summary>The opcode byte of <paramref name="effect"/>.</summary>
    internal static int EffectOpcode(SpcMusicEffect effect)
    {
        int index = Array.IndexOf(effectsByOpcode, effect);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(effect), effect, "Not an SPC music effect.");
        return SpcDriverDefinitions.Music.FirstEffect + index;
    }

    /// <summary>The identity of the phrase at <paramref name="place"/> among bank <paramref name="dataIndex"/>'s phrases, in their slots' order (#2548).</summary>
    internal static string PhraseId(int dataIndex, int place) => $"music-{dataIndex:x2}-phrase-{place:000}";

    /// <summary>The identity of the program at <paramref name="place"/> among bank <paramref name="dataIndex"/>'s programs, in their slots' order (#2548).</summary>
    internal static string ProgramId(int dataIndex, int place) => $"music-{dataIndex:x2}-program-{place:000}";

    /// <summary>The identity of track <paramref name="track"/> of bank <paramref name="dataIndex"/>.</summary>
    internal static string TrackId(int dataIndex, int track) => $"music-{dataIndex:x2}-track-{track:00}";

    /// <summary>
    /// Walks selected track pointers and their referenced phrase and channel programs within uploaded APU RAM. Phrases and
    /// programs are named by their place among the bank's in their slots' order (#2548); a slot's address is the codec's
    /// routing alone, and no identity spells one.
    /// </summary>
    internal static SpcMusicDefinitionBundle DecodeBank(
        int dataIndex,
        IReadOnlyList<ushort> trackPointers,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written)
    {
        var tracks = new List<DecodedTrack>(trackPointers.Count);
        Dictionary<ushort, ushort[]> phrases = [];
        Dictionary<ushort, DecodedProgram> programs = [];
        for (int track = 0; track < trackPointers.Count; track++)
        {
            DecodedTrack definition = DecodeTrack(dataIndex, track, trackPointers[track], ram, written);
            tracks.Add(definition);
            foreach (TrackWord word in definition.Words)
            {
                if (word.Phrase is { } phraseAddress)
                    DecodePhrase(dataIndex, phraseAddress, ram, written, phrases, programs);
            }
        }
        ushort[] phraseSlots = [.. phrases.Keys.Order()];
        ushort[] programSlots = [.. programs.Keys.Order()];
        var phraseIds = new Dictionary<ushort, string>(phraseSlots.Length);
        for (int place = 0; place < phraseSlots.Length; place++)
            phraseIds.Add(phraseSlots[place], PhraseId(dataIndex, place));
        var programIds = new Dictionary<ushort, string>(programSlots.Length);
        for (int place = 0; place < programSlots.Length; place++)
            programIds.Add(programSlots[place], ProgramId(dataIndex, place));
        return new(
            Array.AsReadOnly(tracks.Select(track => track.Build(phraseIds)).ToArray()),
            Array.AsReadOnly(phraseSlots.Select(slot => new AudioMusicPhraseMetadata(phraseIds[slot], slot,
                [.. phrases[slot].Select(program => program == 0 ? null : programIds[program])])).ToArray()),
            Array.AsReadOnly(programSlots.Select(slot => new AudioMusicProgramMetadata(programIds[slot], slot,
                programs[slot].ByteCapacity, programs[slot].Instructions)).ToArray()));
    }

    /// <summary>
    /// Encodes one track control flow and requires it to fit its fixed native byte slot: each phrase it plays at the slot
    /// <paramref name="phrases"/> names for it, and each repeat to the slot of the instruction it repeats from.
    /// </summary>
    /// <exception cref="InvalidDataException">An instruction is malformed or names no phrase of the bank, or the flow does not fit its slot.</exception>
    internal static byte[] EncodeTrack(AudioMusicTrackMetadata track, IReadOnlyDictionary<string, AudioMusicPhraseMetadata> phrases)
    {
        Ensure.NotNull(track);
        Ensure.NotNull(phrases);
        if (track.Instructions is null)
            throw new InvalidDataException($"Music track '{track.Id}' has no instruction list.");
        if (track.Address == 0)
        {
            if (track.ByteCapacity != 0 || track.Instructions.Count != 0)
                throw new InvalidDataException($"Inactive music track '{track.Id}' must have no program data.");
            return [];
        }
        int[] offsets = new int[track.Instructions.Count];
        int offset = 0;
        for (int index = 0; index < offsets.Length; index++)
        {
            offsets[index] = offset;
            offset += track.Instructions[index] is AudioMusicRepeat ? 4 : 2;
        }
        var bytes = new List<byte>(track.ByteCapacity);
        bool ended = false;
        foreach (AudioMusicTrackInstructionMetadata instruction in track.Instructions)
        {
            if (instruction is null)
                throw new InvalidDataException($"Music track '{track.Id}' contains a null instruction.");
            if (ended)
                throw new InvalidDataException($"Music track '{track.Id}' contains data after end.");
            switch (instruction)
            {
                case AudioMusicPlayPhrase play:
                    if (play.Phrase is null || !phrases.TryGetValue(play.Phrase, out AudioMusicPhraseMetadata? phrase))
                        throw new InvalidDataException($"Music track '{track.Id}' plays missing phrase '{play.Phrase}'.");
                    if (LittleEndianBytes.HighByte(checked((ushort)phrase.Address)) < SpcDriverDefinitions.Music.PatternPointerHighByteMinimum)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, phrase.Address);
                    break;
                case AudioMusicTrackEnd:
                    WriteWord(bytes, 0);
                    ended = true;
                    break;
                case AudioMusicFastForwardOn:
                    WriteWord(bytes, MusicTrackControls.Encode(MusicTrackControl.FastForwardOn));
                    break;
                case AudioMusicFastForwardOff:
                    WriteWord(bytes, MusicTrackControls.Encode(MusicTrackControl.FastForwardOff));
                    break;
                case AudioMusicRepeat repeat:
                    if (repeat.Count is <= 0 or > byte.MaxValue || MusicTrackControls.TryDecode(repeat.Count, out _) ||
                        repeat.Target >= offsets.Length)
                        throw InvalidTrackInstruction(track, instruction);
                    WriteWord(bytes, repeat.Count);
                    WriteWord(bytes, track.Address + offsets[repeat.Target]);
                    break;
                default:
                    throw new InvalidDataException($"Music track '{track.Id}' holds an undefined instruction {instruction}.");
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

            switch (instruction.Kind)
            {
                case AudioMusicInstructionKind.End:
                    if (instruction.Opcode != 0 || instruction.Timing.Count != 0 || instruction.Arguments.Count != 0)
                        throw InvalidProgramInstruction(program, instruction);
                    bytes.Add(0);
                    ended = true;
                    break;
                case AudioMusicInstructionKind.Effect:
                    int effectIndex = instruction.Opcode - SpcDriverDefinitions.Music.FirstEffect;
                    if (effectIndex < 0 || effectIndex >= EffectCount ||
                        instruction.Arguments.Count != SpcMusicTables.EffectByteLength(effectIndex))
                        throw InvalidProgramInstruction(program, instruction);
                    bytes.Add(checked((byte)instruction.Opcode));
                    for (int index = 0; index < instruction.Arguments.Count; index++)
                        bytes.Add(instruction.Arguments[index]);
                    break;
                case AudioMusicInstructionKind.Note or AudioMusicInstructionKind.Tie or AudioMusicInstructionKind.Rest or AudioMusicInstructionKind.Percussion:
                    if (instruction.Opcode < SpcDriverDefinitions.Music.CommandMarker || instruction.Opcode >= SpcDriverDefinitions.Music.FirstEffect ||
                        NoteKind(instruction.Opcode) != instruction.Kind || instruction.Arguments.Count != 0)
                        throw InvalidProgramInstruction(program, instruction);
                    bytes.Add(checked((byte)instruction.Opcode));
                    break;
                default:
                    throw InvalidProgramInstruction(program, instruction);
            }
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
        Dictionary<string, AudioMusicPhraseMetadata> phrases = bank.MusicPhrases
            .ToDictionary(phrase => phrase.Id, StringComparer.Ordinal);
        // Programs dominate the image; size the table once rather than rehashing as it grows.
        int expectedBytes = bank.MusicPhrases.Count * SpcDriverDefinitions.ChannelCount * 2;
        foreach (AudioMusicProgramMetadata program in bank.MusicPrograms) expectedBytes += program.ByteCapacity;
        var writes = new Dictionary<int, byte>(expectedBytes);
        foreach (AudioMusicTrackMetadata track in bank.MusicTracks)
        {
            byte[] encoded = EncodeTrack(track, phrases);
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

    /// <summary>Reads a bounded track control stream: its words, each phrase it plays by the phrase's slot.</summary>
    private static DecodedTrack DecodeTrack(
        int dataIndex,
        int track,
        ushort address,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written)
    {
        string id = TrackId(dataIndex, track);
        if (address == 0)
            return new(track, id, 0, 0, []);
        var words = new List<TrackWord>();
        int cursor = address;
        for (int count = 0; count < SpcDriverDefinitions.ApuRamSize; count++)
        {
            int offset = cursor - address;
            ushort value = ReadWord(ram, written, ref cursor, id);
            if (LittleEndianBytes.HighByte(value) >= SpcDriverDefinitions.Music.PatternPointerHighByteMinimum)
            {
                words.Add(new(offset, value, Phrase: value));
                continue;
            }
            if (value == 0)
            {
                words.Add(new(offset, value));
                return new(track, id, address, cursor - address, words);
            }
            if (MusicTrackControls.TryDecode(value, out _))
                words.Add(new(offset, value));
            else
                words.Add(new(offset, value, RepeatTarget: ReadWord(ram, written, ref cursor, id)));
        }
        throw new InvalidDataException($"Music track '{id}' has no bounded end record.");
    }

    /// <summary>Reads eight channel-program slots and decodes each unique referenced program.</summary>
    private static void DecodePhrase(
        int dataIndex,
        ushort address,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        Dictionary<ushort, ushort[]> phrases,
        Dictionary<ushort, DecodedProgram> programs)
    {
        if (phrases.ContainsKey(address))
            return;
        string where = $"music bank {dataIndex:x2} phrase slot ${address:X4}";
        int cursor = address;
        ushort[] channelPrograms = new ushort[SpcDriverDefinitions.ChannelCount];
        for (int channel = 0; channel < channelPrograms.Length; channel++)
        {
            ushort programAddress = ReadWord(ram, written, ref cursor, where);
            if (LittleEndianBytes.HighByte(programAddress) == 0)
                continue;
            if (!written[programAddress])
            {
                throw new InvalidDataException(
                    $"Music {where} channel {channel} routes to non-uploaded program " +
                    $"${programAddress:X4}.");
            }
            channelPrograms[channel] = programAddress;
        }
        phrases.Add(address, channelPrograms);
        foreach (ushort programAddress in channelPrograms.Where(program => program != 0))
            DecodeProgram(dataIndex, programAddress, ram, written, programs);
    }

    /// <summary>Decodes a bounded channel bytecode stream through its end command.</summary>
    private static void DecodeProgram(
        int dataIndex,
        ushort address,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        Dictionary<ushort, DecodedProgram> programs)
    {
        if (programs.ContainsKey(address))
            return;
        string where = $"music bank {dataIndex:x2} program slot ${address:X4}";
        var instructions = new List<AudioMusicInstructionMetadata>();
        var callTargets = new List<ushort>();
        int cursor = address;
        for (int count = 0; count < SpcDriverDefinitions.ApuRamSize; count++)
        {
            byte command = ReadByte(ram, written, ref cursor, where);
            if (command == 0)
            {
                instructions.Add(new(AudioMusicInstructionKind.End, 0, [], []));
                programs.Add(address, new(cursor - address, instructions));
                foreach (ushort target in callTargets)
                    DecodeProgram(dataIndex, target, ram, written, programs);
                return;
            }

            var timing = new List<byte>(2);
            if ((command & SpcDriverDefinitions.Music.CommandMarker) == 0)
            {
                timing.Add(command);
                command = ReadByte(ram, written, ref cursor, where);
                if ((command & SpcDriverDefinitions.Music.CommandMarker) == 0)
                {
                    timing.Add(command);
                    command = ReadByte(ram, written, ref cursor, where);
                }
            }
            if (command >= SpcDriverDefinitions.Music.FirstEffect)
            {
                int effectIndex = command - SpcDriverDefinitions.Music.FirstEffect;
                if ((uint)effectIndex >= EffectCount)
                    throw new InvalidDataException($"Music {where} uses unknown effect ${command:X2}.");
                int argumentCount = SpcMusicTables.EffectByteLength(effectIndex);
                byte[] arguments = new byte[argumentCount];
                for (int index = 0; index < arguments.Length; index++)
                    arguments[index] = ReadByte(ram, written, ref cursor, where);
                instructions.Add(new(AudioMusicInstructionKind.Effect, command, timing, arguments));
                if (DecodeEffect(command) == SpcMusicEffect.CallPattern)
                {
                    ushort target = unchecked(LittleEndianBytes.Word(arguments[0], arguments[1]));
                    callTargets.Add(target);
                }
            }
            else
            {
                instructions.Add(new(NoteKind(command), command, timing, []));
            }
        }
        throw new InvalidDataException($"Music {where} has no bounded end command.");
    }

    /// <summary>Distinguishes pitched notes, tie, rest, and percussion in the note opcode range.</summary>
    private static AudioMusicInstructionKind NoteKind(int opcode)
    {
        if (Enum.IsDefined((NoteCommand)opcode))
        {
            return (NoteCommand)opcode switch
            {
                NoteCommand.Tie => AudioMusicInstructionKind.Tie,
                NoteCommand.Rest => AudioMusicInstructionKind.Rest,
                _ => throw new InvalidOperationException($"Undefined {nameof(NoteCommand)} {opcode:X2}."),
            };
        }
        return opcode >= SpcDriverDefinitions.Music.FirstPercussionNote
            ? AudioMusicInstructionKind.Percussion
            : AudioMusicInstructionKind.Note;
    }

    /// <summary>One word of a track's control stream as the codec reads it, before its phrases are named.</summary>
    /// <param name="Offset">The word's byte offset from the track's slot.</param>
    /// <param name="Value">The word.</param>
    /// <param name="Phrase">The slot of the phrase a play word plays; null for any other word.</param>
    /// <param name="RepeatTarget">The slot a repeat word repeats from; null for any other word.</param>
    private readonly record struct TrackWord(int Offset, ushort Value, ushort? Phrase = null, ushort? RepeatTarget = null);

    /// <summary>A track's control stream as the codec read it, named once the bank's phrases are.</summary>
    /// <param name="Track">The track's number.</param>
    /// <param name="Id">The track's identity.</param>
    /// <param name="Address">The track's slot, or zero for an inactive track.</param>
    /// <param name="ByteCapacity">The bytes its slot holds.</param>
    /// <param name="Words">Its words in stream order, its end last.</param>
    private sealed record DecodedTrack(int Track, string Id, ushort Address, int ByteCapacity, IReadOnlyList<TrackWord> Words)
    {
        /// <summary>The track's metadata: each phrase it plays named by <paramref name="phraseIds"/>, each repeat by the place of the word it repeats from.</summary>
        /// <exception cref="InvalidDataException">A repeat's slot begins none of the track's words.</exception>
        internal AudioMusicTrackMetadata Build(IReadOnlyDictionary<ushort, string> phraseIds)
        {
            var places = new Dictionary<int, int>();
            for (int place = 0; place < Words.Count; place++)
                places.Add(Address + Words[place].Offset, place);
            AudioMusicTrackInstructionMetadata[] instructions = [.. Words.Select(word =>
                word.Phrase is { } phrase ? new AudioMusicPlayPhrase(phraseIds[phrase])
                : word.RepeatTarget is { } target ? new AudioMusicRepeat(word.Value, places.TryGetValue(target, out int repeated) ? repeated
                    : throw new InvalidDataException($"Music track '{Id}' repeats from ${target:X4}, which begins none of its words."))
                : word.Value == 0 ? new AudioMusicTrackEnd()
                : MusicTrackControls.Is(word.Value, MusicTrackControl.FastForwardOn) ? new AudioMusicFastForwardOn()
                : MusicTrackControls.Is(word.Value, MusicTrackControl.FastForwardOff) ? new AudioMusicFastForwardOff()
                : (AudioMusicTrackInstructionMetadata)throw new InvalidOperationException($"Music track '{Id}' word ${word.Value:X4} is no instruction."))];
            return new(Track, Id, Address, ByteCapacity, instructions);
        }
    }

    /// <summary>A channel program as the codec read it, named once the bank's programs are.</summary>
    /// <param name="ByteCapacity">The bytes its slot holds.</param>
    /// <param name="Instructions">Its commands, its end last.</param>
    private sealed record DecodedProgram(int ByteCapacity, IReadOnlyList<AudioMusicInstructionMetadata> Instructions);

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
            $"Music track '{track.Id}' has invalid {instruction} operands.");

    /// <summary>Creates a diagnostic for a channel opcode and operation mismatch.</summary>
    private static InvalidDataException InvalidProgramInstruction(
        AudioMusicProgramMetadata program,
        AudioMusicInstructionMetadata instruction) =>
        new InvalidDataException(
            $"Music program '{program.Id}' command {instruction.Kind} does not match " +
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
