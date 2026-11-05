using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks compact native tables for falling tubes, bomb bounce, Crocomire debris and Rinkas.</summary>
internal static class LookupSmallGameTablesVerification
{
    /// <summary>
    /// #1165: one proof per converted small game table. Each compares the calculation with every
    /// native word of its cartridge source and asserts the bounded domain.
    /// </summary>
    internal static void VerifyLookupSmallGameTables(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyMotherBrainFallingTubeBodies), () => VerifyMotherBrainFallingTubeBodies(rom));
        Suite(nameof(VerifyMotherBrainBombBounceAccelerations), () => VerifyMotherBrainBombBounceAccelerations(rom));
        Suite(nameof(VerifyCrocomireBridgeDustPositions), () => VerifyCrocomireBridgeDustPositions(rom));
        Suite(nameof(VerifyCeresFallingDebrisColumns), () => VerifyCeresFallingDebrisColumns(rom));
        Suite(nameof(VerifyRinkaSpawnResources), () => VerifyRinkaSpawnResources(rom));
        Console.WriteLine("Small game tables: falling-tube bodies, bomb bounce gravity, " +
            "Crocomire bridge dust, Ceres debris columns, and Rinka resource tokens match every native word.");
    }

    /// <summary>Checks five falling-tube body radius and floor records against bank $A9.</summary>
    private static void VerifyMotherBrainFallingTubeBodies(ISnesAddressSpace rom)
    {
        for (int tube = 0; tube < 5; tube++)
        {
            var body = MotherBrainFakeDeath.MotherBrainFallingTubeBody(tube);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa98b5d + 2 * tube)), body.Radius.X, $"Falling tube {tube} X radius");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa98b67 + 2 * tube)), body.Radius.Y, $"Falling tube {tube} Y radius");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa98b71 + 2 * tube)), body.Floor, $"Falling tube {tube} floor");
        }
        AssertThrows<InvalidDataException>(() => MotherBrainFakeDeath.MotherBrainFallingTubeBody(5), "Falling tube upper bound");
    }

    /// <summary>Checks every bomb-bounce gravity stage and its shared ordinary fall value.</summary>
    private static void VerifyMotherBrainBombBounceAccelerations(ISnesAddressSpace rom)
    {
        for (int stage = 0; stage < MotherBrainBombBounceDefinitions.StageCount; stage++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c550 + 2 * stage)),
                MotherBrainBombBounces.YAcceleration(stage), $"Bomb bounce stage {stage} gravity");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x86c550), MotherBrainBombBounceDefinitions.FallAcceleration,
            "Stage zero shares the ordinary fall gravity");
        AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainBombBounces.YAcceleration(10), "Bomb stage bound");
    }

    /// <summary>Matches seven unrolled Crocomire bridge-dust spawn operands to their X/Y positions.</summary>
    private static void VerifyCrocomireBridgeDustPositions(ISnesAddressSpace rom)
    {
        // $A4:8F35..: each unrolled spawn is LDA #X / STA / LDA #Y / STA / LDA #$15 / JSL, twenty bytes.
        for (int puff = 0; puff < 7; puff++)
        {
            int call = 0xa48f35 + 20 * puff;
            AssertEqual((byte)0xa9, CrocomireBridgeDustPositionsRomByte(rom, call), $"Dust {puff} X operand opcode");
            AssertEqual((byte)0xa9, CrocomireBridgeDustPositionsRomByte(rom, call + 5), $"Dust {puff} Y operand opcode");
            var dust = CrocomireBridgeCollapse.CrocomireBridgeDustPosition(puff);
            ushort x = dust.X, y = dust.Y;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((call + 1)), x, $"Crocomire bridge dust {puff} X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((call + 6)), y, $"Crocomire bridge dust {puff} Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireBridgeCollapse.CrocomireBridgeDustPosition(7), "Dust upper bound");
    }

    /// <summary>Reads a retail opcode byte when checking Crocomire's unrolled dust spawns.</summary>
    private static byte CrocomireBridgeDustPositionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Matches all sixteen Ceres falling-debris column positions to their source words.</summary>
    private static void VerifyCeresFallingDebrisColumns(ISnesAddressSpace rom)
    {
        for (int column = 0; column < 16; column++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8fe551 + 2 * column)), RoomMainRoutines.CeresFallingDebrisX(column),
                $"Ceres falling debris column {column}");
        AssertThrows<ArgumentOutOfRangeException>(() => RoomMainRoutines.CeresFallingDebrisX(16), "Debris column bound");
    }

    /// <summary>Matches eleven Rinka spawn positions and resource tokens to bank $A2 records.</summary>
    private static void VerifyRinkaSpawnResources(ISnesAddressSpace rom)
    {
        for (int index = 0; index < 11; index++)
        {
            var resource = RinkaBehavior.RinkaSpawnResourceAt(index);
            int record = 0xa2b75b + 6 * index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(record), resource.Position.X, $"Rinka spawn {index} X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2)), resource.Position.Y, $"Rinka spawn {index} Y");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 4)), resource.Token, $"Rinka spawn {index} token");
        }
    }
}
