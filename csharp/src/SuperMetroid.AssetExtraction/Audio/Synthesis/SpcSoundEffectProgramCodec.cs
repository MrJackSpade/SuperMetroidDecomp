using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Lossless codec for the bounded channel programs consumed by the resident SFX driver.
/// Routing, voice allocation and program addresses remain compiled engine definitions;
/// authored instrument/note/envelope/timing operands are editable content.
/// </summary>
internal static partial class SpcSoundEffectProgramCodec
{
    /// <summary>The opcode byte of each instruction; every other byte begins a note packet.</summary>
    private static readonly Dictionary<int, SpcSoundEffectOpcode> opcodes = new()
    {
        [0xf5] = SpcSoundEffectOpcode.PitchSlideLegato,
        [0xf8] = SpcSoundEffectOpcode.PitchSlide,
        [0xf9] = SpcSoundEffectOpcode.SetAdsr,
        [0xfb] = SpcSoundEffectOpcode.RepeatForever,
        [0xfc] = SpcSoundEffectOpcode.EnableNoise,
        [0xfd] = SpcSoundEffectOpcode.EndRepeat,
        [0xfe] = SpcSoundEffectOpcode.BeginRepeat,
        [0xff] = SpcSoundEffectOpcode.End,
    };

    /// <summary>The opcode byte of <paramref name="instruction"/>.</summary>
    internal static int OpcodeByte(SpcSoundEffectOpcode instruction) =>
        opcodes.First(pair => pair.Value == instruction).Key;

    /// <summary>Whether the sound stream's <paramref name="commandByte"/> is <paramref name="instruction"/>'s opcode (#2563).</summary>
    /// <param name="commandByte">A byte read from a sound effect's stream where a command stands.</param>
    /// <param name="instruction">The instruction asked about.</param>
    internal static bool Is(int commandByte, SpcSoundEffectOpcode instruction) =>
        opcodes.TryGetValue(commandByte, out SpcSoundEffectOpcode command) && command == instruction;

    /// <summary>Decodes uploaded SPC bytes into bounded instructions until End or RepeatForever.</summary>
    internal static AudioSoundProgramMetadata Decode(
        string id,
        ushort address,
        ReadOnlySpan<byte> ram,
        ReadOnlySpan<bool> written)
    {
        var instructions = new List<AudioSoundInstructionMetadata>();
        int cursor = address;
        for (int count = 0; count < SpcDriverDefinitions.ApuRamSize; count++)
        {
            byte opcode = ReadByte(ram, written, ref cursor, address);
            if (!opcodes.TryGetValue(opcode, out SpcSoundEffectOpcode command))
            {
                // Every other byte begins a note packet whose first byte is the instrument.
                byte[] arguments = new byte[5];
                arguments[0] = opcode;
                for (int index = 1; index < arguments.Length; index++)
                    arguments[index] = ReadByte(ram, written, ref cursor, address);
                instructions.Add(new(AudioSoundOperation.PlayNote, arguments));
                continue;
            }

            switch (command)
            {
                case SpcSoundEffectOpcode.SetAdsr:
                    instructions.Add(Read(
                        AudioSoundOperation.SetAdsr, 4,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcode.PitchSlideLegato:
                    instructions.Add(Read(
                        AudioSoundOperation.PitchSlideLegato, 2,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcode.PitchSlide:
                    instructions.Add(Read(
                        AudioSoundOperation.PitchSlide, 2,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcode.End:
                    instructions.Add(new(AudioSoundOperation.End, []));
                    return new(id, address, cursor - address, instructions);
                case SpcSoundEffectOpcode.BeginRepeat:
                    instructions.Add(Read(
                        AudioSoundOperation.BeginRepeat, 1,
                        ram, written, ref cursor, address));
                    break;
                case SpcSoundEffectOpcode.RepeatForever:
                    instructions.Add(new(AudioSoundOperation.RepeatForever, []));
                    return new(id, address, cursor - address, instructions);
                case SpcSoundEffectOpcode.EndRepeat:
                    instructions.Add(new(AudioSoundOperation.EndRepeat, []));
                    break;
                case SpcSoundEffectOpcode.EnableNoise:
                    instructions.Add(new(AudioSoundOperation.EnableNoise, []));
                    break;
                default:
                    throw new InvalidOperationException($"Undefined SpcSoundEffectOpcode {command}.");
            }
        }
        throw new InvalidDataException(
            $"SPC sound program '{id}' at ${address:X4} has no terminal instruction.");
    }

    /// <summary>Validates and encodes an edited sound program without changing its fixed native byte capacity.</summary>
    internal static byte[] Encode(AudioSoundProgramMetadata program)
    {
        Ensure.NotNull(program);
        if (program.Instructions is null)
            throw new InvalidDataException($"SPC sound program '{program.Id}' has no instruction list.");
        var bytes = new List<byte>(program.ByteCapacity);
        bool terminated = false;
        for (int index = 0; index < program.Instructions.Count; index++)
        {
            AudioSoundInstructionMetadata instruction = program.Instructions[index]
                ?? throw new InvalidDataException(
                    $"SPC sound program '{program.Id}' contains a null instruction at {index}.");
            SoundEffectInstructionShape shape = instruction.Operation switch
            {
                AudioSoundOperation.SetAdsr => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.SetAdsr), 4, false),
                AudioSoundOperation.PitchSlideLegato => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.PitchSlideLegato), 2, false),
                AudioSoundOperation.PitchSlide => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.PitchSlide), 2, false),
                AudioSoundOperation.End => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.End), 0, true),
                AudioSoundOperation.BeginRepeat => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.BeginRepeat), 1, false),
                AudioSoundOperation.RepeatForever => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.RepeatForever), 0, true),
                AudioSoundOperation.EndRepeat => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.EndRepeat), 0, false),
                AudioSoundOperation.EnableNoise => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.EnableNoise), 0, false),
                AudioSoundOperation.PlayNote => new SoundEffectNoteShape(5),
                _ => throw new InvalidDataException(
                    $"SPC sound program '{program.Id}' uses unknown operation '{instruction.Operation}'."),
            };
            if (instruction.Arguments is null)
            {
                throw new InvalidDataException(
                    $"SPC sound program '{program.Id}' operation '{instruction.Operation}' has no argument list.");
            }
            int argumentCount = shape.ArgumentCount;
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
            switch (shape)
            {
                case SoundEffectOpcodeShape command:
                    bytes.Add(unchecked((byte)command.Opcode));
                    break;
                case SoundEffectNoteShape when opcodes.ContainsKey(instruction.Arguments[0]):
                    throw new InvalidDataException(
                        $"SPC sound program '{program.Id}' playNote instrument ${instruction.Arguments[0]:X2} " +
                        "is reserved by the instruction language.");
                case SoundEffectNoteShape:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined sound-effect instruction shape {shape}.");
            }
            // A command's arguments follow its opcode; a note's first argument is its instrument.
            foreach (byte argument in instruction.Arguments)
                bytes.Add(argument);
            terminated = shape.Terminal;
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

    /// <summary>Reads a command's fixed number of arguments from uploaded APU memory.</summary>
    private static AudioSoundInstructionMetadata Read(
        AudioSoundOperation operation,
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

    /// <summary>Reads one uploaded byte and rejects unwritten or out-of-range program content.</summary>
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
