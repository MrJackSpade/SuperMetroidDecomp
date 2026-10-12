using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static KraidBodyContourVerification;

/// <summary>Represents kraid contour case mappings verification.</summary>
internal static class KraidContourCaseMappingsVerification
{
    /// <summary>Native kraid body left edge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid body contour left-edge records")]
    internal static short NativeKraidBodyLeftEdge(SuperMetroidAddressSpace rom, short relativeY)
    {
        for (int record = 0; record < 7; record++)
        {
            int address = 0xa7b161 + 4 * record;
            // Native CMP/BPL uses the sign of wrapped subtraction, not ordered signed comparison.
            if (unchecked((short)(relativeY - CartridgeImportSource.Require(rom).ReadCartridgeWord(address))) >= 0 ||
                unchecked((short)(relativeY - CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)))) >= 0)
                return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)));
        }
        throw new InvalidOperationException("Native Kraid body contour did not terminate within its records.");
    }

    /// <summary>Verifies kraid body contour cases.</summary>
    internal static void VerifyKraidBodyContourCases(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            short relativeY = unchecked((short)raw);
            AssertEqual(NativeKraidBodyLeftEdge(rom, relativeY), KraidBodyContour.LeftEdge(relativeY),
                $"Kraid contour native CMP/BPL selection at Y={relativeY}");
        }
        var enemies = new RoomEnemySystem();
        var overlaps = CreateKraidBodyContourProbe();
        var body = enemies.SlotTable.Slots[0];
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

    /// <summary>Native kraid growth resume.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid growth head-resume immediates")]
    internal static KraidHeadResumeDefinition NativeKraidGrowthResume(SuperMetroidAddressSpace rom, ushort tilemap)
    {
        // Decode the native LDY/CMP immediates rather than using compiled selector constants.
        ushort offset = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa7c042);
        for (int selector = 0; selector < 3; selector++)
        {
            int instruction = 0xa7c029 + 8 * selector;
            if (tilemap != CartridgeImportSource.Require(rom).ReadCartridgeWord((instruction + 4))) continue;
            offset = CartridgeImportSource.Require(rom).ReadCartridgeWord((instruction + 1));
            break;
        }
        return new((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa7c047) + offset), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa70000 | (CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa7c04d) + offset))));
    }

    /// <summary>Verifies kraid growth resume cases.</summary>
    internal static void VerifyKraidGrowthResumeCases(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            AssertEqual(NativeKraidGrowthResume(rom, (ushort)raw), KraidHeadInstructionDefinitions.GrowthResume((ushort)raw),
                "Kraid native tilemap-dependent resume pointer and timer");
    }
}
