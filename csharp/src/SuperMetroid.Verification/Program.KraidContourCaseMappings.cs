using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid body contour left-edge records")]
    private static short NativeKraidBodyLeftEdge(SuperMetroidAddressSpace rom, short relativeY)
    {
        ushort Word(int address) => (ushort)(rom.ReadCartridgeByte(address) | rom.ReadCartridgeByte(address + 1) << 8);
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
        body.Position = body.Position.WithWhole(new(256, 32768));
        shot.Radius = shot.Radius.WithX(0);
        foreach (short y in new short[] { -32768, -31746, -31745 })
        {
            shot.Position = shot.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + y)));
            int edge = body.Position.X.Whole + NativeKraidBodyLeftEdge(rom, y);
            for (int delta = -1; delta <= 1; delta++)
            {
                shot.Position = shot.Position.WithWholeX((ushort)(edge + delta));
                AssertEqual(delta > 0, overlaps(body, shot), "Wrapped contour production edge remains strict");
            }
        }
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid growth head-resume immediates")]
    private static KraidHeadResumeDefinition NativeKraidGrowthResume(SuperMetroidAddressSpace rom, ushort tilemap)
    {
        ushort Word(int address) => (ushort)(rom.ReadCartridgeByte(address) | rom.ReadCartridgeByte(address + 1) << 8);
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

    private static void VerifyKraidGrowthResumeCases(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            AssertEqual(NativeKraidGrowthResume(rom, (ushort)raw), KraidHeadInstructionDefinitions.GrowthResume((ushort)raw),
                "Kraid native tilemap-dependent resume pointer and timer");
    }
}
