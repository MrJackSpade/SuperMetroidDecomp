using SuperMetroid.Core;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Yard;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Yard turn lookahead and animation records against the cartridge and live branch behavior.</summary>
internal static class YardTurnDefinitionsVerification
{
    /// <summary>Matches Yard turn lookahead and animation lists to cartridge records, then exercises actual inside/outside branches.</summary>
    internal static void VerifyYardTurnDefinitions(CartridgeImportAddressSpace rom)
    {
        YardTurnDefinitionsRecord[] records =
        [
            new YardTurnDefinitionsRecord(YardDirection.OnFloorMovingLeft, false, 0xa3cce2),
            new YardTurnDefinitionsRecord(YardDirection.OnRightWallMovingDown, false, 0xa3ccea),
            new YardTurnDefinitionsRecord(YardDirection.OnCeilingMovingRight, false, 0xa3ccf2),
            new YardTurnDefinitionsRecord(YardDirection.OnLeftWallMovingUp, false, 0xa3ccfa),
            new YardTurnDefinitionsRecord(YardDirection.OnFloorMovingRight, false, 0xa3cd02),
            new YardTurnDefinitionsRecord(YardDirection.OnLeftWallMovingDown, false, 0xa3cd0a),
            new YardTurnDefinitionsRecord(YardDirection.OnCeilingMovingLeft, false, 0xa3cd12),
            new YardTurnDefinitionsRecord(YardDirection.OnRightWallMovingUp, false, 0xa3cd1a),
            new YardTurnDefinitionsRecord(YardDirection.OnFloorMovingLeft, true, 0xa3cd22),
            new YardTurnDefinitionsRecord(YardDirection.OnCeilingMovingRight, true, 0xa3cd2a),
            new YardTurnDefinitionsRecord(YardDirection.OnFloorMovingRight, true, 0xa3cd32),
            new YardTurnDefinitionsRecord(YardDirection.OnCeilingMovingLeft, true, 0xa3cd3a),
        ];

        foreach ((YardDirection movement, bool suppressed, int address) in records)
        {
            YardCrawlStep definition =
                YardTurns.ForCrawl(movement, suppressed);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address)), definition.Lookahead.X,
                $"Yard turn {movement}/{suppressed} lookahead X");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2))), definition.Lookahead.Y,
                $"Yard turn {movement}/{suppressed} lookahead Y");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)), NativeEnemyAnimationOracle.EntryOf(YardNativeListAddresses.Entries, definition.WhenSurfaceEnds),
                $"Yard turn {movement}/{suppressed} outside list");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)), NativeEnemyAnimationOracle.EntryOf(YardNativeListAddresses.Entries, definition.WhenSurfaceMet),
                $"Yard turn {movement}/{suppressed} inside list");
        }

        // The four crawls along a wall have no suppressed turn.
        foreach (YardDirection movement in new[]
        {
            YardDirection.OnRightWallMovingDown,
            YardDirection.OnLeftWallMovingUp,
            YardDirection.OnLeftWallMovingDown,
            YardDirection.OnRightWallMovingUp,
        })
        {
            AssertEqual(
                YardTurns.ForCrawl(movement, false),
                YardTurns.ForCrawl(movement, true),
                $"Yard vertical turn {movement} has no suppressed turn");
        }

        Suite(nameof(VerifyYardTurnProductionBranches), () => VerifyYardTurnProductionBranches(records));
        Console.WriteLine(
            "Yard turn definitions: all 48 native words, four suppression aliases, and " +
            "all 24 real outside/inside crawl branches pass with the complete turn table forbidden.");
    }

    /// <summary>Runs Yard crawling movement against empty and solid level data for each turn record.</summary>
    private static void VerifyYardTurnProductionBranches(
        IReadOnlyList<YardTurnDefinitionsRecord> records)
    {
        RoomLevelData empty = VerificationHelpersVerification.CreateEmptyRoom(new(64, 64));
        RoomLevelData solid = VerificationHelpersVerification.CreateSolidRowsRoom(new(64, 64), firstSolidRow: 0, solidRowCount: 64);

        foreach ((YardDirection movement, bool suppressed, _) in records)
        {
            YardCrawlStep definition =
                YardTurns.ForCrawl(movement, suppressed);
            VerifyYardTurnProductionBranch(
                movement, suppressed, empty, definition.WhenSurfaceEnds,
                $"Yard outside turn {movement}/{suppressed}");
            VerifyYardTurnProductionBranch(
                movement, suppressed, solid, definition.WhenSurfaceMet,
                $"Yard inside turn {movement}/{suppressed}");
        }

        static void VerifyYardTurnProductionBranch(
            YardDirection movement,
            bool suppressed,
            RoomLevelData level,
            YardSegment expectedProgram,
            string context)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemyPlace, YardEnemyState, RoomLevelData> run = enemies.YardBehavior.RunYardCrawlingMovement;
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Position = place.Position.WithWhole(new(0x0200, 0x0200));
            place.Radius = new(8, 8);
            var state = (new YardEnemyState(YardMovement.HoldingStill)
            {
                // A hiding animation is never a turn's result, so the assertion sees the turn's own choice.
                Playback = new(YardAnimations.StartingWith(YardSegment.HidingOnFloorMovingLeft), AnimationPlace.StartOfAnimation),
                Movement = YardMovement.Crawling,
                Crawl = movement,
                CrawlingVelocity = new PixelVelocity<RoomPixels>(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256)),
                TurnSuppressionPassesLeft = UpdateCountdown.For(suppressed ? YardMotionDefinitions.TurnSuppressionPasses : 0)
            });

            run(place, state, level);

            AssertEqual(new AnimationPlayback<YardSegment>(YardAnimations.StartingWith(expectedProgram), AnimationPlace.StartOfAnimation), state.Playback,
                $"{context} production instruction");
            AssertEqual(YardMotionDefinitions.TurnSuppressionPasses, state.TurnSuppressionPassesLeft.UpdatesLeft,
                $"{context} suppresses turns for a whole run of slope-free passes");
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
