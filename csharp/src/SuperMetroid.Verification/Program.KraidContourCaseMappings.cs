using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Evaluates the retail body-contour table with its wrapped signed CMP/BPL selection rule.</summary>
    /// <param name="rom">Loaded cartridge address space containing the seven contour records.</param>
    /// <param name="relativeY">Wrapped vertical displacement from Kraid's body origin.</param>
    /// <returns>The selected body's left-edge displacement from its origin.</returns>
    private static short NativeKraidBodyLeftEdge(SuperMetroidAddressSpace rom, short relativeY)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int record = 0; record < 7; record++)
        {
            int address = 0xa7b161 + 4 * record;
            // Native CMP/BPL uses the sign of wrapped subtraction, not ordered signed comparison.
            if (unchecked((short)(relativeY - Word(address))) >= 0 ||
                unchecked((short)(relativeY - Word(address + 4))) >= 0)
                return unchecked((short)Word(address + 2));
        }
        throw new InvalidOperationException("Native Kraid body contour did not terminate within its records.");
    }

    /// <summary>Confirms compiled contour selection across every wrapped vertical value and checks strict collision at the contour edge.</summary>
    /// <param name="rom">Cartridge address space used as the native contour-table oracle.</param>
    private static void VerifyKraidBodyContourCases(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            short relativeY = unchecked((short)raw);
            AssertEqual(NativeKraidBodyLeftEdge(rom, relativeY), KraidBodyContour.LeftEdge(relativeY),
                $"Kraid contour native CMP/BPL selection at Y={relativeY}");
        }
        var enemies = new RoomEnemySystem();
        var overlaps = CreateKraidBodyContourProbe();
        var body = enemies.Slots[0];
        var shot = new SamusProjectileSystem().Slots[0];
        body.XPosition = 256;
        body.YPosition = 32768;
        shot.XRadius = 0;
        foreach (short y in new short[] { -32768, -31746, -31745 })
        {
            shot.YPosition = unchecked((ushort)(body.YPosition + y));
            int edge = body.XPosition + NativeKraidBodyLeftEdge(rom, y);
            for (int delta = -1; delta <= 1; delta++)
            {
                shot.XPosition = (ushort)(edge + delta);
                AssertEqual(delta > 0, overlaps(body, shot), "Wrapped contour production edge remains strict");
            }
        }
    }

    /// <summary>Decodes the native growth animation's resume instruction and timer for a tilemap selector.</summary>
    /// <param name="rom">Cartridge address space containing the native selector and resume operands.</param>
    /// <param name="tilemap">Tilemap value used to select one of the native growth branches.</param>
    /// <returns>The instruction pointer and timer selected by the retail routine.</returns>
    private static KraidHeadResumeDefinition NativeKraidGrowthResume(SuperMetroidAddressSpace rom, ushort tilemap)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        // Decode the native LDY/CMP immediates rather than using compiled selector constants.
        ushort offset = Word(0xa7c042);
        for (int selector = 0; selector < 3; selector++)
        {
            int instruction = 0xa7c029 + 8 * selector;
            if (tilemap != Word(instruction + 4)) continue;
            offset = Word(instruction + 1);
            break;
        }
        return new((ushort)(Word(0xa7c047) + offset), Word(0xa70000 | (Word(0xa7c04d) + offset)));
    }

    /// <summary>Compares compiled growth-resume definitions with the native selection for every possible tilemap word.</summary>
    /// <param name="rom">Cartridge address space used to decode the native growth routine.</param>
    private static void VerifyKraidGrowthResumeCases(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            AssertEqual(NativeKraidGrowthResume(rom, (ushort)raw), KraidHeadInstructionDefinitions.GrowthResume((ushort)raw),
                "Kraid native tilemap-dependent resume pointer and timer");
    }
}
