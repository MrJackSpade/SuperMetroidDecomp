using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Lossless codec for the bounded channel programs consumed by the resident SFX driver.
/// Routing, voice allocation and program addresses remain compiled engine definitions;
/// authored instrument/note/envelope/timing operands are editable content.
/// </summary>
[EncodingOwner("sound driver sound-effect channel program bytecode")]
internal static class SpcSoundEffectProgramCodec
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
    private static int OpcodeByte(SpcSoundEffectOpcode instruction) =>
        opcodes.First(pair => pair.Value == instruction).Key;

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
            (int opcode, int argumentCount, bool terminal) = instruction.Operation switch
            {
                AudioSoundInstructionOperations.SetAdsr => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.SetAdsr), 4, false),
                AudioSoundInstructionOperations.PitchSlideLegato => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.PitchSlideLegato), 2, false),
                AudioSoundInstructionOperations.PitchSlide => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.PitchSlide), 2, false),
                AudioSoundInstructionOperations.End => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.End), 0, true),
                AudioSoundInstructionOperations.BeginRepeat => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.BeginRepeat), 1, false),
                AudioSoundInstructionOperations.RepeatForever => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.RepeatForever), 0, true),
                AudioSoundInstructionOperations.EndRepeat => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.EndRepeat), 0, false),
                AudioSoundInstructionOperations.EnableNoise => new SoundEffectOpcodeShape(OpcodeByte(SpcSoundEffectOpcode.EnableNoise), 0, false),
                AudioSoundInstructionOperations.PlayNote => new SoundEffectOpcodeShape(0, 5, false),
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
                if (opcodes.ContainsKey(instruction.Arguments[0]))
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

    /// <summary>Reads a command's fixed number of arguments from uploaded APU memory.</summary>
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
