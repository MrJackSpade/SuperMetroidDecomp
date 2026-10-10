namespace SuperMetroid.Core.Audio;

/// <summary>Stable JSON operation names for the resident SPC sound-effect language.</summary>
public static class AudioSoundInstructionOperations
{
    /// <summary>JSON operation for native $F9: four byte arguments hold ADSR1, ADSR2, and two preserved reserved bytes; the envelope override is applied when the next note is programmed.</summary>
    public const string SetAdsr = "setAdsr";
    /// <summary>JSON operation for native $F5: subnote-delta and target-note byte arguments enable sliding with legato continuity before the following note packet.</summary>
    public const string PitchSlideLegato = "pitchSlideLegato";
    /// <summary>JSON operation for native $F8: subnote-delta and target-note byte arguments enable the non-legato pitch-slide form before the following note packet.</summary>
    public const string PitchSlide = "pitchSlide";
    /// <summary>Argument-free native $FF channel terminator, releasing the borrowed DSP voice and restoring its music-channel state; no serialized instructions may follow it.</summary>
    public const string End = "end";
    /// <summary>JSON operation for native $FE: one byte argument sets the repeat counter and records the following instruction as the repeat point.</summary>
    public const string BeginRepeat = "beginRepeat";
    /// <summary>Argument-free native $FB jump to the current repeat point without decrementing the counter; terminal in the serialized program even though playback continues looping.</summary>
    public const string RepeatForever = "repeatForever";
    /// <summary>Argument-free native $FD counted-loop terminator: decrement the byte repeat counter and return to its repeat point unless the counter reaches zero.</summary>
    public const string EndRepeat = "endRepeat";
    /// <summary>Argument-free native $FC instruction enabling DSP noise for the sound channel's borrowed voice before continuing to the next instruction.</summary>
    public const string EnableNoise = "enableNoise";
    /// <summary>Five-byte note packet ordered as instrument, volume, pan, note, and duration; the instrument itself occupies the opcode position, reserved instruction values are rejected, and note $F6 retains the previous note.</summary>
    public const string PlayNote = "playNote";
}

/// <summary>
/// Lossless codec for the bounded channel programs consumed by the resident SFX driver.
/// Routing, voice allocation and program addresses remain compiled engine definitions;
/// authored instrument/note/envelope/timing operands are editable content.
/// </summary>
internal static class SpcSoundEffectProgramCodec
{
    internal static AudioSoundProgramMetadata Decode(
        string id,
        ushort address,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written)
    {
        var instructions = new List<AudioSoundInstructionMetadata>();
        int cursor = address;
        for (int count = 0; count < SpcDriverData.ApuRamSize; count++)
        {
            byte opcode = ReadByte(ram, written, ref cursor, address);
            var command = (SpcSoundEffectOpcode)opcode;
            if (!Enum.IsDefined(command))
            {
                // Every other byte begins a note packet whose first byte is the instrument.
                byte[] arguments = new byte[5];
                arguments[0] = opcode;
                for (int index = 1; index < arguments.Length; index++)
                    arguments[index] = ReadByte(ram, written, ref cursor, address);
                instructions.Add(new(AudioSoundInstructionOperations.PlayNote, arguments));
                continue;
            }

            switch (command)
            {
                case SpcSoundEffectOpcode.SetAdsr:
                    instructions.Add(Read(
                        AudioSoundInstructionOperations.SetAdsr, 4,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcode.PitchSlideLegato:
                    instructions.Add(Read(
                        AudioSoundInstructionOperations.PitchSlideLegato, 2,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcode.PitchSlide:
                    instructions.Add(Read(
                        AudioSoundInstructionOperations.PitchSlide, 2,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcode.End:
                    instructions.Add(new(AudioSoundInstructionOperations.End, []));
                    return new(id, address, cursor - address, instructions);
                case SpcSoundEffectOpcode.BeginRepeat:
                    instructions.Add(Read(
                        AudioSoundInstructionOperations.BeginRepeat, 1,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcode.RepeatForever:
                    instructions.Add(new(AudioSoundInstructionOperations.RepeatForever, []));
                    return new(id, address, cursor - address, instructions);
                case SpcSoundEffectOpcode.EndRepeat:
                    instructions.Add(new(AudioSoundInstructionOperations.EndRepeat, []));
                    break;
                case SpcSoundEffectOpcode.EnableNoise:
                    instructions.Add(new(AudioSoundInstructionOperations.EnableNoise, []));
                    break;
                default:
                    throw new InvalidOperationException($"Undefined SpcSoundEffectOpcode {command}.");
            }
        }
        throw new InvalidDataException(
            $"SPC sound program '{id}' at ${address:X4} has no terminal instruction.");
    }

    internal static byte[] Encode(AudioSoundProgramMetadata program)
    {
        ArgumentNullException.ThrowIfNull(program);
        if (program.Instructions is null)
            throw new InvalidDataException($"SPC sound program '{program.Id}' has no instruction list.");
        var bytes = new List<byte>(program.ByteCapacity);
        bool terminated = false;
        for (int index = 0; index < program.Instructions.Count; index++)
        {
            AudioSoundInstructionMetadata instruction = program.Instructions[index]
                ?? throw new InvalidDataException(
                    $"SPC sound program '{program.Id}' contains a null instruction at {index}.");
            (int opcode, int argumentCount, bool terminal) = instruction.Operation switch
            {
                AudioSoundInstructionOperations.SetAdsr => ((int)SpcSoundEffectOpcode.SetAdsr, 4, false),
                AudioSoundInstructionOperations.PitchSlideLegato => ((int)SpcSoundEffectOpcode.PitchSlideLegato, 2, false),
                AudioSoundInstructionOperations.PitchSlide => ((int)SpcSoundEffectOpcode.PitchSlide, 2, false),
                AudioSoundInstructionOperations.End => ((int)SpcSoundEffectOpcode.End, 0, true),
                AudioSoundInstructionOperations.BeginRepeat => ((int)SpcSoundEffectOpcode.BeginRepeat, 1, false),
                AudioSoundInstructionOperations.RepeatForever => ((int)SpcSoundEffectOpcode.RepeatForever, 0, true),
                AudioSoundInstructionOperations.EndRepeat => ((int)SpcSoundEffectOpcode.EndRepeat, 0, false),
                AudioSoundInstructionOperations.EnableNoise => ((int)SpcSoundEffectOpcode.EnableNoise, 0, false),
                AudioSoundInstructionOperations.PlayNote => (0, 5, false),
                _ => throw new InvalidDataException(
                    $"SPC sound program '{program.Id}' uses unknown operation '{instruction.Operation}'."),
            };
            if (instruction.Arguments is null)
            {
                throw new InvalidDataException(
                    $"SPC sound program '{program.Id}' operation '{instruction.Operation}' has no argument list.");
            }
            if (instruction.Arguments.Count != argumentCount)
            {
                throw new InvalidDataException(
                    $"SPC sound program '{program.Id}' operation '{instruction.Operation}' has " +
                    $"{instruction.Arguments.Count} arguments; expected {argumentCount}.");
            }
            if (terminated)
            {
                throw new InvalidDataException(
                    $"SPC sound program '{program.Id}' contains instructions after its terminal operation.");
            }
            if (instruction.Operation == AudioSoundInstructionOperations.PlayNote)
            {
                if (Enum.IsDefined((SpcSoundEffectOpcode)instruction.Arguments[0]))
                {
                    throw new InvalidDataException(
                        $"SPC sound program '{program.Id}' playNote instrument ${instruction.Arguments[0]:X2} " +
                        "is reserved by the instruction language.");
                }
                bytes.Add(instruction.Arguments[0]);
            }
            else
                bytes.Add(unchecked((byte)opcode));
            int firstArgument = instruction.Operation == AudioSoundInstructionOperations.PlayNote ? 1 : 0;
            for (int argument = firstArgument; argument < instruction.Arguments.Count; argument++)
                bytes.Add(instruction.Arguments[argument]);
            terminated = terminal;
        }
        if (!terminated)
            throw new InvalidDataException($"SPC sound program '{program.Id}' has no terminal operation.");
        if (bytes.Count != program.ByteCapacity)
        {
            throw new InvalidDataException(
                $"SPC sound program '{program.Id}' encodes to {bytes.Count} bytes; its fixed native slot is " +
                $"{program.ByteCapacity} bytes. Edit operands or same-size operations without relocating programs.");
        }
        return [.. bytes];
    }

    private static AudioSoundInstructionMetadata Read(
        string operation,
        int count,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        ref int cursor,
        ushort start)
    {
        byte[] arguments = new byte[count];
        for (int index = 0; index < count; index++)
            arguments[index] = ReadByte(ram, written, ref cursor, start);
        return new(operation, arguments);
    }

    private static byte ReadByte(
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written,
        ref int cursor,
        ushort start)
    {
        if ((uint)cursor >= ram.Length || (uint)cursor >= written.Length || !written[cursor])
        {
            throw new InvalidDataException(
                $"SPC sound program at ${start:X4} reads outside uploaded content at ${cursor:X4}.");
        }
        return ram[cursor++];
    }
}
