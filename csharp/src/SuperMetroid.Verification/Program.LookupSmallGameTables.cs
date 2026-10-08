using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// #1165: one proof per converted small game table. Each compares the calculation with every
    /// native word of its cartridge source and asserts the bounded domain.
    /// </summary>
    private static void VerifyLookupSmallGameTables(ISnesAddressSpace rom)
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

    private static ushort SmallTableWord(ISnesAddressSpace rom, int address) =>
        (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

    private static T InvokePrivateStatic<T>(Type owner, string method, params object[] arguments) =>
        (T)owner.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, arguments)!;

    private static void VerifyMotherBrainFallingTubeBodies(ISnesAddressSpace rom)
    {
        for (int tube = 0; tube < 5; tube++)
        {
            var body = InvokePrivateStatic<(ushort XRadius, ushort YRadius, ushort Floor)>(
                typeof(RoomEnemySystem), "MotherBrainFallingTubeBody", tube);
            AssertEqual(SmallTableWord(rom, 0xa98b5d + 2 * tube), body.XRadius, $"Falling tube {tube} X radius");
            AssertEqual(SmallTableWord(rom, 0xa98b67 + 2 * tube), body.YRadius, $"Falling tube {tube} Y radius");
            AssertEqual(SmallTableWord(rom, 0xa98b71 + 2 * tube), body.Floor, $"Falling tube {tube} floor");
        }
        AssertThrows<TargetInvocationException>(() => InvokePrivateStatic<(ushort, ushort, ushort)>(
            typeof(RoomEnemySystem), "MotherBrainFallingTubeBody", 5), "Falling tube upper bound");
    }

    private static void VerifyMotherBrainBombBounceAccelerations(ISnesAddressSpace rom)
    {
        for (int stage = 0; stage < MotherBrainBombBounceDefinitions.StageCount; stage++)
            AssertEqual(SmallTableWord(rom, 0x86c550 + 2 * stage),
                MotherBrainBombBounceDefinitions.YAcceleration(stage), $"Bomb bounce stage {stage} gravity");
        AssertEqual(SmallTableWord(rom, 0x86c550), MotherBrainBombBounceDefinitions.FallAcceleration,
            "Stage zero shares the ordinary fall gravity");
        AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainBombBounceDefinitions.YAcceleration(10), "Bomb stage bound");
    }

    private static void VerifyCrocomireBridgeDustPositions(ISnesAddressSpace rom)
    {
        // $A4:8F35..: each unrolled spawn is LDA #X / STA / LDA #Y / STA / LDA #$15 / JSL, twenty bytes.
        for (int puff = 0; puff < 7; puff++)
        {
            int call = 0xa48f35 + 20 * puff;
            AssertEqual((byte)0xa9, rom.ReadByte(call), $"Dust {puff} X operand opcode");
            AssertEqual((byte)0xa9, rom.ReadByte(call + 5), $"Dust {puff} Y operand opcode");
            (ushort x, ushort y) = RoomEnemySystem.CrocomireBridgeDustPosition(puff);
            AssertEqual(SmallTableWord(rom, call + 1), x, $"Crocomire bridge dust {puff} X");
            AssertEqual(SmallTableWord(rom, call + 6), y, $"Crocomire bridge dust {puff} Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => RoomEnemySystem.CrocomireBridgeDustPosition(7), "Dust upper bound");
    }

    private static void VerifyCeresFallingDebrisColumns(ISnesAddressSpace rom)
    {
        for (int column = 0; column < 16; column++)
            AssertEqual(SmallTableWord(rom, 0x8fe551 + 2 * column), SuperMetroidRuntime.CeresFallingDebrisX(column),
                $"Ceres falling debris column {column}");
        AssertThrows<ArgumentOutOfRangeException>(() => SuperMetroidRuntime.CeresFallingDebrisX(16), "Debris column bound");
    }

    private static void VerifyRinkaSpawnResources(ISnesAddressSpace rom)
    {
        for (int index = 0; index < 11; index++)
        {
            var resource = InvokePrivateStatic<RinkaSpawnResource>(typeof(RoomEnemySystem), "RinkaSpawnResourceAt", index);
            int record = 0xa2b75b + 6 * index;
            AssertEqual(SmallTableWord(rom, record), resource.XPosition, $"Rinka spawn {index} X");
            AssertEqual(SmallTableWord(rom, record + 2), resource.YPosition, $"Rinka spawn {index} Y");
            AssertEqual(SmallTableWord(rom, record + 4), resource.Token, $"Rinka spawn {index} token");
        }
    }

    private static void VerifyDeadTourianCorpseLayouts(ISnesAddressSpace rom)
    {
        const int bank = 0xa90000, workBuffer = 0x2000;
        int? tileSheetBase = null;
        foreach ((string field, DeadTourianCorpseSpecies species) in new[]
        {
            ("DeadZoomerProfile", DeadTourianCorpseSpecies.Zoomer),
            ("DeadRipperProfile", DeadTourianCorpseSpecies.Ripper),
            ("DeadSkreeProfile", DeadTourianCorpseSpecies.Skree),
        })
        {
            var profile = (RoomEnemySystem.DeadTourianCorpseProfile)typeof(RoomEnemySystem)
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
            for (int variant = 0; variant < profile.Variants.Length; variant++)
            {
                var layout = profile.Variants[variant];
                DeadTourianCorpseDefinition definition = DeadTourianCorpseDefinitions.For(species, variant);
                string name = $"Dead {species} variant {variant}";

                // Rot-entry copy routine: per column, LDA rotEntryYOffset / CMP #limit / BPL / LDA src,X ...
                int copy = bank | definition.CopyFunction;
                for (int column = 0; column < layout.ColumnWordOffsets.Length; column++)
                {
                    int block = copy + 20 * column;
                    AssertEqual((byte)0xc9, rom.ReadByte(block + 3), $"{name} column {column} CMP opcode");
                    AssertEqual(layout.MaximumY, SmallTableWord(rom, block + 4), $"{name} column {column} rot depth");
                    AssertEqual((byte)0xbd, rom.ReadByte(block + 8), $"{name} column {column} LDA abs,X opcode");
                    AssertEqual((ushort)(workBuffer / 2 + layout.ColumnWordOffsets[column]),
                        (ushort)(SmallTableWord(rom, block + 9) / 2), $"{name} column {column} word offset");
                }
                AssertEqual((byte)0x60, rom.ReadByte(copy + 20 * layout.ColumnWordOffsets.Length),
                    $"{name} copy routine ends after its last column");

                // Initialization routine: per tile row, PHB / LDX #sheet / LDY #work / LDA #length-1 / MVN / PLB.
                // Production does not call the graphics-initialization routine; locate it through
                // the native configuration record's sixth word.
                int init = bank | SmallTableWord(rom, bank | (definition.ConfigurationPointer + 10));
                for (int row = 0; row < layout.InitialGraphicsCopies.Length; row++)
                {
                    var expected = layout.InitialGraphicsCopies[row];
                    int group = init + 14 * row;
                    AssertEqual((byte)0xa2, rom.ReadByte(group + 1), $"{name} row {row} LDX opcode");
                    AssertEqual((byte)0xa0, rom.ReadByte(group + 4), $"{name} row {row} LDY opcode");
                    AssertEqual((byte)0x54, rom.ReadByte(group + 10), $"{name} row {row} MVN opcode");
                    int sheetBase = SmallTableWord(rom, group + 2) - expected.SourceOffset;
                    tileSheetBase ??= sheetBase;
                    AssertEqual(tileSheetBase.Value, sheetBase, $"{name} row {row} shares the corpse tile sheet");
                    AssertEqual(workBuffer + expected.DestinationOffset, SmallTableWord(rom, group + 5), $"{name} row {row} destination");
                    AssertEqual(expected.Length - 1, SmallTableWord(rom, group + 8), $"{name} row {row} length");
                }
                AssertEqual((byte)0x60, rom.ReadByte(init + 14 * layout.InitialGraphicsCopies.Length), $"{name} copies end with RTS");
            }
        }
    }
}
