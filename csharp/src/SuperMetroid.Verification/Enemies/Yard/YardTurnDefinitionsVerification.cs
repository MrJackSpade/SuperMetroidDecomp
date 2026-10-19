using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Yard;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Yard turn lookahead and animation records against the cartridge and live branch behavior.</summary>
internal static class YardTurnDefinitionsVerification
{
    /// <summary>Matches Yard turn lookahead and animation lists to cartridge records, then exercises actual inside/outside branches.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Yard turn definitions")]
    internal static void VerifyYardTurnDefinitions(SuperMetroidAddressSpace rom)
    {
        (YardMovementFunction Movement, bool Disabled, int Address)[] records =
        [
            (YardMovementFunction.CrawlingUpsideUpMovingLeft, false, 0xa3cce2),
            (YardMovementFunction.CrawlingUpsideLeftMovingDown, false, 0xa3ccea),
            (YardMovementFunction.CrawlingUpsideDownMovingRight, false, 0xa3ccf2),
            (YardMovementFunction.CrawlingUpsideRightMovingUp, false, 0xa3ccfa),
            (YardMovementFunction.CrawlingUpsideUpMovingRight, false, 0xa3cd02),
            (YardMovementFunction.CrawlingUpsideRightMovingDown, false, 0xa3cd0a),
            (YardMovementFunction.CrawlingUpsideDownMovingLeft, false, 0xa3cd12),
            (YardMovementFunction.CrawlingUpsideLeftMovingUp, false, 0xa3cd1a),
            (YardMovementFunction.CrawlingUpsideUpMovingLeft, true, 0xa3cd22),
            (YardMovementFunction.CrawlingUpsideDownMovingRight, true, 0xa3cd2a),
            (YardMovementFunction.CrawlingUpsideUpMovingRight, true, 0xa3cd32),
            (YardMovementFunction.CrawlingUpsideDownMovingLeft, true, 0xa3cd3a),
        ];

        foreach ((YardMovementFunction movement, bool disabled, int address) in records)
        {
            YardTurnDefinition definition =
                YardTurnDefinitions.ForMovement(movement, disabled);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address)), definition.Lookahead.X,
                $"Yard turn {movement}/{disabled} lookahead X");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2))), definition.Lookahead.Y,
                $"Yard turn {movement}/{disabled} lookahead Y");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)), NativeEnemyProgramOracle.EntryOf(YardNativeListAddresses.Entries, definition.OutsideTurnProgram),
                $"Yard turn {movement}/{disabled} outside list");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)), NativeEnemyProgramOracle.EntryOf(YardNativeListAddresses.Entries, definition.InsideTurnProgram),
                $"Yard turn {movement}/{disabled} inside list");
        }

        // The four vertical crawling states do not consult the suppression flag.
        foreach (YardMovementFunction movement in new[]
        {
            YardMovementFunction.CrawlingUpsideLeftMovingDown,
            YardMovementFunction.CrawlingUpsideRightMovingUp,
            YardMovementFunction.CrawlingUpsideRightMovingDown,
            YardMovementFunction.CrawlingUpsideLeftMovingUp,
        })
        {
            AssertEqual(
                YardTurnDefinitions.ForMovement(movement, false),
                YardTurnDefinitions.ForMovement(movement, true),
                $"Yard vertical turn {movement} ignores suppression flag");
        }
        AssertThrows<InvalidDataException>(
            () => YardTurnDefinitions.ForMovement(YardMovementFunction.Airborne, false),
            "Yard airborne state has no surface-turn definition");

        Suite(nameof(VerifyYardTurnProductionBranches), () => VerifyYardTurnProductionBranches(records));
        Console.WriteLine(
            "Yard turn definitions: all 48 native words, four suppression aliases, and " +
            "all 24 real outside/inside crawl branches pass with the complete turn table forbidden.");
    }

    /// <summary>Runs Yard crawling movement against empty and solid level data for each turn record.</summary>
    private static void VerifyYardTurnProductionBranches(
        IReadOnlyList<(YardMovementFunction Movement, bool Disabled, int Address)> records)
    {
        var emptyWords = new ushort[64 * 64];
        var solidWords = Enumerable.Repeat((ushort)0x8000, emptyWords.Length).ToArray();
        RoomLevelData empty = CreateYardTurnLevel(emptyWords);
        RoomLevelData solid = CreateYardTurnLevel(solidWords);

        foreach ((YardMovementFunction movement, bool disabled, _) in records)
        {
            YardTurnDefinition definition =
                YardTurnDefinitions.ForMovement(movement, disabled);
            VerifyYardTurnProductionBranch(
                movement, disabled, empty, definition.OutsideTurnProgram,
                $"Yard outside turn {movement}/{disabled}");
            VerifyYardTurnProductionBranch(
                movement, disabled, solid, definition.InsideTurnProgram,
                $"Yard inside turn {movement}/{disabled}");
        }

        static RoomLevelData CreateYardTurnLevel(ushort[] words) =>
            new(new Extent16<RoomBlocks>(64, 64), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);

        static void VerifyYardTurnProductionBranch(
            YardMovementFunction movement,
            bool disabled,
            RoomLevelData level,
            YardProgram expectedProgram,
            string context)
        {
            var enemies = new RoomEnemySystem(new EnemySession());
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot, YardEnemyState, RoomLevelData> run = enemies.YardBehavior.RunYardCrawlingMovement;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Position = slot.Position.WithWhole(new(0x0200, 0x0200));
            slot.Radius = new(8, 8);
            var state = new YardEnemyState()
            {
                // A hiding animation is never a turn's result, so the assertion sees the turn's own choice.
                Program = AnimationProgramCursor<YardProgram>.Start(YardProgram.HidingUpsideUpMovingLeft),
                MovementFunction = movement,
                TurnTransitionDisabled = disabled,
                CrawlingVelocity = new EightEightVector<RoomPixels>(0x0100, 0x0100),
            };

            run(slot, state, level);

            AssertEqual(AnimationProgramCursor<YardProgram>.Start(expectedProgram), state.Program,
                $"{context} production instruction");
            AssertTrue(state.TurnTransitionDisabled,
                $"{context} disables the next turn transition");
            AssertEqual((ushort)0, state.TurnTransitionDisableCounter,
                $"{context} clears transition-disable counter");
        }
    }


    /// <summary>Runs the Yard turn definition and production branch suite.</summary>
    [VerificationCommand("--yard-turn-definitions")]
    internal static int YardTurnDefinitionsCommand()
    {
        Suite(nameof(VerifyYardTurnDefinitions), () => VerifyYardTurnDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
