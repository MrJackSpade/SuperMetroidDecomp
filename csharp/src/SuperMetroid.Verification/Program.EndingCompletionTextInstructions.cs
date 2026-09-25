using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingCompletionTextInstructions(ISnesAddressSpace bus)
    {
        for (int pointer = EndingCompletionTextInstructionDefinitions.Start;
             pointer < EndingCompletionTextInstructionDefinitions.End; pointer += sizeof(ushort))
        {
            int address = (int)new SnesAddress(0x8b, (ushort)pointer);
            ushort nativeWord = (ushort)(bus.ReadByte(address) |
                bus.ReadByte(address + 1) << 8);
            AssertEqual(nativeWord, EndingCompletionTextInstructionDefinitions.ReadWord((ushort)pointer),
                $"ending completion text instruction $8B:{pointer:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(() =>
            EndingCompletionTextInstructionDefinitions.ReadWord(
                EndingCompletionTextInstructionDefinitions.End),
            "ending completion reader rejects an address after the colon list");
        AssertThrows<InvalidDataException>(() =>
            EndingCompletionTextInstructionDefinitions.ReadWord(
                unchecked((ushort)(EndingCompletionTextInstructionDefinitions.Start + 1))),
            "ending completion reader rejects an unaligned address");

        ushort[] starts =
        [
            0xeb91, 0xebd7, 0xec35, 0xec81, 0xec89, 0xec91, 0xec99, 0xeca1,
            0xeca9, 0xecb1, 0xecb9, 0xecc1, 0xecc9, 0xecd1,
        ];
        foreach (ushort start in starts)
        {
            var native = new IntroDiscoverySprite(120, 72, 0x0800, start);
            var installed = new IntroDiscoverySprite(120, 72, 0x0800, start);
            for (int frame = 0; frame < 480; frame++)
            {
                // The scene owns private callbacks; here both actor interpreters
                // advance across the same callback without duplicating its effects.
                native.Step(bus, (_, cursor) => cursor);
                installed.Step(bus, (_, cursor) => cursor,
                    EndingCompletionTextInstructionDefinitions.ReadWord);
                AssertEqual(native.InstructionPointer, installed.InstructionPointer,
                    $"completion actor ${start:X4} cursor at frame {frame}");
                AssertEqual(native.SpriteMapPointer, installed.SpriteMapPointer,
                    $"completion actor ${start:X4} visual frame at frame {frame}");
                AssertEqual(native.IsActive, installed.IsActive,
                    $"completion actor ${start:X4} lifetime at frame {frame}");
            }
        }
    }
}
