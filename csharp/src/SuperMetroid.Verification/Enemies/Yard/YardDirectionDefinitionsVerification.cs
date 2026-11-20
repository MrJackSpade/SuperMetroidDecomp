using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>Checks Yard direction tables and airborne transitions against the cartridge definitions and guarded runtime paths.</summary>
internal static class YardDirectionDefinitionsVerification
{
    /// <summary>Compares every direction and airborne-list word with ROM, then exercises initialization, turning, and launch paths without table reads.</summary>
    internal static void VerifyYardDirectionDefinitions(CartridgeImportAddressSpace rom)
    {
        const int directionAddress = 0xa3cd42;
        const int oppositeAddress = 0xa3cdc2;
        const int movementAddress = 0xa3cdd2;
        int[] airborneAddresses = [0xa3d1ab, 0xa3d50f, 0xa3d5a4];

        for (ushort direction = 0; direction < 8; direction++)
        {
            YardDirectionDefinition definition =
                YardDirections.ForDirection(SuperMetroid.AssetExtraction.Enemies.YardDirectionCodec.At(direction));
            int record = directionAddress + direction * 8;
            AssertEqual(rom.ReadCartridgeWord(record), NativeEnemyAnimationOracle.EntryOf(YardNativeListAddresses.Entries, definition.CrawlingAnimation),
                $"Yard direction {direction} crawling list");
            AssertEqual(rom.ReadCartridgeWord((record + 2)), SuperMetroid.AssetExtraction.Enemies.CrawlerOrientationOrder.Orientations.PositionOf(definition.BlockSide),
                $"Yard direction {direction} block side");
            AssertEqual(rom.ReadCartridgeWord((record + 4)), NativeEnemyAnimationOracle.EntryOf(YardNativeListAddresses.Entries, definition.HidingAnimation),
                $"Yard direction {direction} hiding list");
            AssertEqual(rom.ReadCartridgeWord((record + 6)), definition.AirborneFacingDirection == HorizontalDirection.Right ? (ushort)1 : (ushort)0,
                $"Yard direction {direction} airborne facing");
            AssertEqual(rom.ReadCartridgeWord((oppositeAddress + direction * 2)),
                SuperMetroid.AssetExtraction.Enemies.YardDirectionCodec.NumberOf(definition.OppositeDirection),
                $"Yard direction {direction} opposite");
            AssertEqual(rom.ReadCartridgeWord((movementAddress + direction * 2)),
                YardNativeWords.Crawls.Word(definition.Crawl),
                $"Yard direction {direction} movement function");
        }

        foreach (int source in airborneAddresses)
        foreach (HorizontalDirection facing in Enum.GetValues<HorizontalDirection>())
        {
            int row = facing == HorizontalDirection.Right ? 1 : 0;
            YardAirborneAnimationDefinition definition =
                YardDirections.ForAirborneFacing(facing);
            AssertEqual(rom.ReadCartridgeWord((source + row * 4)),
                NativeEnemyAnimationOracle.EntryOf(YardNativeListAddresses.Entries, definition.VisibleAnimation),
                $"Yard airborne ${source:X6} facing {facing} visible list");
            AssertEqual(rom.ReadCartridgeWord((source + row * 4 + 2)),
                NativeEnemyAnimationOracle.EntryOf(YardNativeListAddresses.Entries, definition.HidingAnimation),
                $"Yard airborne ${source:X6} facing {facing} hiding list");
        }


        AssertThrows<InvalidDataException>(
            () => YardDirections.ForAirborneFacing((HorizontalDirection)2),
            "Yard invalid airborne facing definition");

        var guarded = NewYardDirectionReadGuard(rom);

        for (ushort direction = 0; direction < 8; direction++)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.SetSpawnWords(EnemyDefinitionId.Yard, _ => new SuperMetroid.AssetExtraction.Enemies.PopulationWords(direction, 0, 0, 0, 0));
            place.Contact = EnemyContact.None;
            place.Processing = EnemyProcessing.None;
            place.Presence = EnemyPresence.RespawnsIfKilled;
            place.Bind(EnemyDefinitionId.Yard, place.KeptDefinition);
            YardEnemyBehavior.InitializeYard(place.HeldOccupant(), place);

            YardEnemyState state = enemies.YardStates[0] ??
                throw new InvalidDataException("Yard initializer produced no typed state.");
            YardDirectionDefinition initial =
                YardDirections.ForDirection(SuperMetroid.AssetExtraction.Enemies.YardDirectionCodec.At(direction));
            AssertYardDirectionState(state, initial,
                $"Yard initializer direction {direction}",
                assertMovementFunction: false);
            AssertEqual(YardMovement.HoldingStill, state.Movement,
                $"Yard initializer direction {direction} pending function");

            // The population selector does not initialize the live direction word;
            // animation bytecode normally publishes it before a turn. Seed each of the
            // eight legal bytecode values to exercise the production turn dispatcher.
            state.Direction = SuperMetroid.AssetExtraction.Enemies.YardDirectionCodec.At(direction);
            state.Movement = YardMovement.Crawling;
            state.Crawl = initial.Crawl;
            YardEnemyBehavior.TurnYardAround(state);
            YardDirectionDefinition opposite =
                YardDirections.ForDirection(initial.OppositeDirection);
            AssertEqual(initial.OppositeDirection, state.Direction,
                $"Yard direction {direction} production opposite");
            AssertYardDirectionState(state, opposite,
                $"Yard turn direction {direction}",
                assertMovementFunction: true);
        }

        foreach (ushort direction in new ushort[] { 0, 1 })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.SetSpawnWords(EnemyDefinitionId.Yard, _ => new SuperMetroid.AssetExtraction.Enemies.PopulationWords(direction, 0, 0, 0, 0));
            place.Bind(EnemyDefinitionId.Yard, place.KeptDefinition);
            YardEnemyBehavior.InitializeYard(place.HeldOccupant(), place);
            YardEnemyState state = (enemies.YardStates[0] ?? throw new InvalidOperationException("Verification requires enemies.YardStates[0]."));
            HorizontalDirection facing = state.AirborneFacingDirection;
            YardAirborneAnimationDefinition expected =
                YardDirections.ForAirborneFacing(facing);

            YardEnemyBehavior.DropYard(place, state);
            AssertYardAirborneLists(state, expected,
                $"Yard detach facing {facing}");

            state.Behavior = YardBehavior.NormalCrawling;
            state.Movement = YardMovement.Crawling;
            state.Crawl = YardDirection.OnFloorMovingLeft;
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Pose = facing == 0
                    ? SamusPoseId.FacingLeftNormalPose
                    : SamusPoseId.FacingRightNormalPose,
            };
            var kickDistance = new SuperMetroid.Core.Compatibility.YardKickTrackingDistance();
            kickDistance.Record(SignedSixteenSixteen.FromParts(1, 32768));
            YardEnemyBehavior.KickYardIntoAir(place, state, samus, kickDistance);
            AssertYardAirborneLists(state, expected,
                $"Yard contact kick facing {facing}");

            YardEnemyBehavior.ShootYardIntoAir(place, state, samus);
            AssertYardAirborneLists(state, expected,
                $"Yard shot launch facing {facing}");
        }

        Console.WriteLine(
            "Yard direction definitions: 48 direction words and all 24 duplicated airborne-list words match ROM; every real initializer, turn, detach, contact-kick and shot-launch handoff passes with the fixed tables forbidden.");
    }

    /// <summary>Checks the instruction, property, hiding, facing, and optionally movement values installed for a Yard direction.</summary>
    private static void AssertYardDirectionState(
        YardEnemyState state,
        YardDirectionDefinition expected,
        string name,
        bool assertMovementFunction)
    {
        AssertEqual(new AnimationPlayback<YardSegment>(YardAnimations.StartingWith(expected.CrawlingAnimation), AnimationPlace.StartOfAnimation), state.Playback,
            $"{name} crawling list");
        AssertEqual(expected.BlockSide, state.BlockSide,
            $"{name} block side");
        AssertEqual((YardSegment?)expected.HidingAnimation, state.HidingAnimation,
            $"{name} hiding list");
        AssertEqual(expected.AirborneFacingDirection, state.AirborneFacingDirection,
            $"{name} airborne facing");
        if (assertMovementFunction)
        {
            AssertEqual(YardMovement.Crawling, state.Movement,
                $"{name} movement function");
            AssertEqual(expected.Crawl, state.Crawl,
                $"{name} crawl");
        }
    }

    /// <summary>Checks the visible and hiding lists selected when a Yard becomes airborne.</summary>
    private static void AssertYardAirborneLists(
        YardEnemyState state,
        YardAirborneAnimationDefinition expected,
        string name)
    {
        AssertEqual(new AnimationPlayback<YardSegment>(YardAnimations.StartingWith(expected.VisibleAnimation), AnimationPlace.StartOfAnimation), state.Playback,
            $"{name} visible list");
        AssertEqual((YardSegment?)expected.HidingAnimation, state.HidingAnimation,
            $"{name} hiding list");
    }

    /// <summary>Rejects reads from the Yard direction and airborne-list ranges that the migrated definitions replace.</summary>
    private static CartridgeReadGuard NewYardDirectionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa3cd42 and < 0xa3cd82 or
                >= 0xa3cdc2 and < 0xa3cde2 or
                >= 0xa3d1ab and < 0xa3d1b3 or
                >= 0xa3d50f and < 0xa3d517 or
                >= 0xa3d5a4 and < 0xa3d5ac,
            address => $"Yard attempted migrated direction/list read ${address:X6}.");
}
