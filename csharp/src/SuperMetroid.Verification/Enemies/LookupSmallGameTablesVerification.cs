using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks compact native tables for falling tubes, bomb bounce, Crocomire debris, Rinkas, and corpse layouts.</summary>
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
        Suite(nameof(VerifyDeadTourianCorpseLayouts), () => VerifyDeadTourianCorpseLayouts(rom));
        Console.WriteLine("Small game tables: falling-tube bodies, bomb bounce gravity, " +
            "Crocomire bridge dust, Ceres debris columns, Rinka resource tokens and dead Tourian corpse layouts match every native word.");
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
                MotherBrainBombBounceDefinitions.YAcceleration(stage), $"Bomb bounce stage {stage} gravity");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x86c550), MotherBrainBombBounceDefinitions.FallAcceleration,
            "Stage zero shares the ordinary fall gravity");
        AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainBombBounceDefinitions.YAcceleration(10), "Bomb stage bound");
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
            (ushort x, ushort y) = CrocomireBridgeCollapse.CrocomireBridgeDustPosition(puff);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((call + 1)), x, $"Crocomire bridge dust {puff} X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((call + 6)), y, $"Crocomire bridge dust {puff} Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireBridgeCollapse.CrocomireBridgeDustPosition(7), "Dust upper bound");
    }

    /// <summary>Reads a retail opcode byte when checking Crocomire's unrolled dust spawns.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire bridge dust positions")]
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

    /// <summary>Matches dead Tourian corpse copy geometry and initialization tile moves to native routines.</summary>
    private static void VerifyDeadTourianCorpseLayouts(ISnesAddressSpace rom)
    {
        const int bank = 0xa90000, workBuffer = 0x2000;
        int? tileSheetBase = null;
        foreach ((DeadTourianBehavior.DeadTourianCorpseProfile profile, DeadTourianCorpseSpecies species) in new[]
        {
            new VerifyDeadTourianCorpseLayoutsCase(DeadTourianCorpseProfileDefinitions.DeadZoomerProfile, DeadTourianCorpseSpecies.Zoomer),
            new VerifyDeadTourianCorpseLayoutsCase(DeadTourianCorpseProfileDefinitions.DeadRipperProfile, DeadTourianCorpseSpecies.Ripper),
            new VerifyDeadTourianCorpseLayoutsCase(DeadTourianCorpseProfileDefinitions.DeadSkreeProfile, DeadTourianCorpseSpecies.Skree),
        })
        {
            for (int variant = 0; variant < profile.Variants.Length; variant++)
            {
                var layout = profile.Variants[variant];
                string name = $"Dead {species} variant {variant}";

                // Rot-entry copy routine: per column, LDA rotEntryYOffset / CMP #limit / BPL / LDA src,X ...
                int configuration = bank | Corpses.DeadTourianCorpseDefinitionsVerification.ConfigurationPointer(rom, species, variant);
                int copy = bank | CartridgeImportSource.Require(rom).ReadCartridgeWord(configuration + 4);
                for (int column = 0; column < layout.ColumnWordOffsets.Length; column++)
                {
                    int block = copy + 20 * column;
                    AssertEqual((byte)0xc9, DeadTourianCorpseLayoutsRomByte(rom, block + 3), $"{name} column {column} CMP opcode");
                    AssertEqual(layout.MaximumY, CartridgeImportSource.Require(rom).ReadCartridgeWord((block + 4)), $"{name} column {column} rot depth");
                    AssertEqual((byte)0xbd, DeadTourianCorpseLayoutsRomByte(rom, block + 8), $"{name} column {column} LDA abs,X opcode");
                    AssertEqual((ushort)(workBuffer / 2 + layout.ColumnWordOffsets[column]),
                        (ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord((block + 9)) / 2), $"{name} column {column} word offset");
                }
                AssertEqual((byte)0x60, DeadTourianCorpseLayoutsRomByte(rom, copy + 20 * layout.ColumnWordOffsets.Length),
                    $"{name} copy routine ends after its last column");

                // Initialization routine: per tile row, PHB / LDX #sheet / LDY #work / LDA #length-1 / MVN / PLB.
                // Production does not call the graphics-initialization routine; locate it through
                // the native configuration record's sixth word.
                int init = bank | CartridgeImportSource.Require(rom).ReadCartridgeWord(configuration + 10);
                for (int row = 0; row < layout.InitialGraphicsCopies.Length; row++)
                {
                    var expected = layout.InitialGraphicsCopies[row];
                    int group = init + 14 * row;
                    AssertEqual((byte)0xa2, DeadTourianCorpseLayoutsRomByte(rom, group + 1), $"{name} row {row} LDX opcode");
                    AssertEqual((byte)0xa0, DeadTourianCorpseLayoutsRomByte(rom, group + 4), $"{name} row {row} LDY opcode");
                    AssertEqual((byte)0x54, DeadTourianCorpseLayoutsRomByte(rom, group + 10), $"{name} row {row} MVN opcode");
                    int sheetBase = CartridgeImportSource.Require(rom).ReadCartridgeWord((group + 2)) - expected.SourceOffset;
                    tileSheetBase ??= sheetBase;
                    AssertEqual(tileSheetBase.Value, sheetBase, $"{name} row {row} shares the corpse tile sheet");
                    AssertEqual(workBuffer + expected.DestinationOffset, CartridgeImportSource.Require(rom).ReadCartridgeWord((group + 5)), $"{name} row {row} destination");
                    AssertEqual(expected.Length - 1, CartridgeImportSource.Require(rom).ReadCartridgeWord((group + 8)), $"{name} row {row} length");
                }
                AssertEqual((byte)0x60, DeadTourianCorpseLayoutsRomByte(rom, init + 14 * layout.InitialGraphicsCopies.Length), $"{name} copies end with RTS");
            }
        }
    }

    /// <summary>Reads a retail opcode byte when checking dead-corpse copy and initialization routines.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for dead Tourian corpse layouts")]
    private static byte DeadTourianCorpseLayoutsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
