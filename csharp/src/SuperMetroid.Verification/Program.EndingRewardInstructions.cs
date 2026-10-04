using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingRewardInstructions(ISnesAddressSpace bus)
    {
        for (int pointer = EndingRewardInstructionDefinitions.Start;
             pointer < EndingRewardInstructionDefinitions.End; pointer += sizeof(ushort))
        {
            int address = (int)new SnesAddress(0x8b, (ushort)pointer);
            ushort nativeWord = (ushort)(bus.ReadByte(address) |
                bus.ReadByte(address + 1) << 8);
            AssertEqual(nativeWord, EndingRewardInstructionDefinitions.ReadWord((ushort)pointer),
                $"ending reward instruction $8B:{pointer:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(() =>
            EndingRewardInstructionDefinitions.ReadWord(EndingRewardInstructionDefinitions.End),
            "ending reward reader rejects the next actor family");
        AssertThrows<InvalidDataException>(() =>
            EndingRewardInstructionDefinitions.ReadWord(
                unchecked((ushort)(EndingRewardInstructionDefinitions.Start + 1))),
            "ending reward reader rejects unaligned instructions");

        for (int stage = 0; stage < 4; stage++)
            AssertEqual((ushort)2, (ushort)(bus.ReadByte(0x8c9c7c + 12 * stage) | bus.ReadByte(0x8c9c7d + 12 * stage) << 8),
                $"helmetless head stage {stage} has a two-part OAM record");

        ushort[] starts =
        [
            0xed1d, 0xed25, 0xed2d, 0xed59, 0xed7f, 0xed95, 0xed9d,
            0xedb1, 0xedb9, 0xedc1, 0xedc9, 0xedd3, 0xee0f, 0xee15,
            0xee27, 0xee3d, 0xee4d,
        ];
        foreach (ushort start in starts)
        {
            var native = new IntroDiscoverySprite(120, 120, 0x0a00, start);
            var installed = new IntroDiscoverySprite(120, 120, 0x0a00, start);
            for (int frame = 0; frame < 480; frame++)
            {
                // Scene callbacks own actor allocation and velocity; the
                // independent list interpreters advance across each callback.
                var nativeCallbacks = new List<ushort>();
                var generatedCallbacks = new List<ushort>();
                native.Step(bus, (opcode, cursor) => { nativeCallbacks.Add(opcode); return cursor; }, pointer =>
                    (ushort)(bus.ReadByte(0x8b0000 | pointer) | bus.ReadByte(0x8b0000 | (pointer + 1)) << 8));
                installed.Step(bus, (opcode, cursor) => { generatedCallbacks.Add(opcode); return cursor; },
                    EndingRewardInstructionDefinitions.ReadWord);
                AssertTrue(nativeCallbacks.SequenceEqual(generatedCallbacks),
                    $"reward actor ${start:X4} callback order/timing at frame {frame}");
                AssertEqual(native.InstructionPointer, installed.InstructionPointer,
                    $"reward actor ${start:X4} cursor at frame {frame}");
                AssertEqual(native.SpriteMapPointer, installed.SpriteMapPointer,
                    $"reward actor ${start:X4} visual frame at frame {frame}");
                AssertEqual(native.PreInstructionPointer, installed.PreInstructionPointer,
                    $"reward actor ${start:X4} pre-instruction at frame {frame}");
                AssertEqual(native.IsActive, installed.IsActive,
                    $"reward actor ${start:X4} lifetime at frame {frame}");
            }
        }
    }
}
