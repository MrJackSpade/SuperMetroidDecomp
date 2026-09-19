namespace SuperMetroid.Core.Audio;

/// <summary>Stable JSON operation names for the resident SPC sound-effect language.</summary>
public static class AudioSoundInstructionOperations
{
    public const string SetAdsr = "setAdsr";
    public const string PitchSlideLegato = "pitchSlideLegato";
    public const string PitchSlide = "pitchSlide";
    public const string End = "end";
    public const string BeginRepeat = "beginRepeat";
    public const string RepeatForever = "repeatForever";
    public const string EndRepeat = "endRepeat";
    public const string EnableNoise = "enableNoise";
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
            switch (opcode)
            {
                case SpcSoundEffectOpcodes.SetAdsr:
                    instructions.Add(Read(
                        AudioSoundInstructionOperations.SetAdsr, 4,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcodes.PitchSlideLegato:
                    instructions.Add(Read(
                        AudioSoundInstructionOperations.PitchSlideLegato, 2,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcodes.PitchSlide:
                    instructions.Add(Read(
                        AudioSoundInstructionOperations.PitchSlide, 2,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcodes.End:
                    instructions.Add(new(AudioSoundInstructionOperations.End, []));
                    return new(id, address, cursor - address, instructions);
                case SpcSoundEffectOpcodes.BeginRepeat:
                    instructions.Add(Read(
                        AudioSoundInstructionOperations.BeginRepeat, 1,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcodes.RepeatForever:
                    instructions.Add(new(AudioSoundInstructionOperations.RepeatForever, []));
                    return new(id, address, cursor - address, instructions);
                case SpcSoundEffectOpcodes.EndRepeat:
                    instructions.Add(new(AudioSoundInstructionOperations.EndRepeat, []));
                    break;
                case SpcSoundEffectOpcodes.EnableNoise:
                    instructions.Add(new(AudioSoundInstructionOperations.EnableNoise, []));
                    break;
                default:
                    byte[] arguments = new byte[5];
                    arguments[0] = opcode;
                    for (int index = 1; index < arguments.Length; index++)
                        arguments[index] = ReadByte(ram, written, ref cursor, address);
                    instructions.Add(new(AudioSoundInstructionOperations.PlayNote, arguments));
                    break;
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
                AudioSoundInstructionOperations.SetAdsr => (SpcSoundEffectOpcodes.SetAdsr, 4, false),
                AudioSoundInstructionOperations.PitchSlideLegato => (SpcSoundEffectOpcodes.PitchSlideLegato, 2, false),
                AudioSoundInstructionOperations.PitchSlide => (SpcSoundEffectOpcodes.PitchSlide, 2, false),
                AudioSoundInstructionOperations.End => (SpcSoundEffectOpcodes.End, 0, true),
                AudioSoundInstructionOperations.BeginRepeat => (SpcSoundEffectOpcodes.BeginRepeat, 1, false),
                AudioSoundInstructionOperations.RepeatForever => (SpcSoundEffectOpcodes.RepeatForever, 0, true),
                AudioSoundInstructionOperations.EndRepeat => (SpcSoundEffectOpcodes.EndRepeat, 0, false),
                AudioSoundInstructionOperations.EnableNoise => (SpcSoundEffectOpcodes.EnableNoise, 0, false),
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
                if (instruction.Arguments[0] is
                    SpcSoundEffectOpcodes.PitchSlideLegato or
                    SpcSoundEffectOpcodes.PitchSlide or
                    SpcSoundEffectOpcodes.SetAdsr or
                    >= SpcSoundEffectOpcodes.RepeatForever)
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
