using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Metroids;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies.Artwork;

/// <summary>Checks compiled Metroid escape movement and random cry words against retail sources.</summary>
internal static class MetroidBehaviorDefinitionsVerification
{
    /// <summary>Compares source tables and exercises the production escape and cry consumers with those reads forbidden.</summary>
    internal static void VerifyMetroidBehaviorDefinitions(CartridgeImportAddressSpace rom)
    {
        // The cartridge picks each move by the countdown's two lowest bits; the port lists the moves in the order they
        // happen, the first with all four left (#2343).
        for (int movesLeft = 1; movesLeft <= MetroidBehaviorDefinitions.EscapeDuration; movesLeft++)
        {
            Offset16<RoomPixels> actual =
                MetroidReactions.EscapeDisplacement(movesLeft);
            int entry = movesLeft & 3;
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((0xa3ea3f + entry * 2))), actual.X,
                $"Metroid escape X displacement with {movesLeft} moves left");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((0xa3ea47 + entry * 2))), actual.Y,
                $"Metroid escape Y displacement with {movesLeft} moves left");
        }

        for (ushort index = 0; index < 8; index++)
        {
            SoundEffectId cry = MetroidBehaviorDefinitions.Cry.Choose(index);
            AssertEqual(SoundEffectCommands.DecodeCartridge(cry.Library, rom.ReadCartridgeWord((0xa3ead6 + index * 2))),
                cry,
                $"Metroid random cry {index}");
        }

        Suite(nameof(VerifyMetroidEscapeConsumer), () => VerifyMetroidEscapeConsumer());
        Suite(nameof(VerifyMetroidCryConsumer), () => VerifyMetroidCryConsumer(rom));
        Console.WriteLine(
            "Metroid behavior definitions: eight displacement words, eight cry words, every escape count and all eight production instruction selections pass with source tables forbidden.");
    }

    /// <summary>
    /// Checks every count of moves left for its move, countdown and homing handoff, the four an escape counts down from its
    /// duration; a frozen escaping Metroid's strain, which spends the moves to none, flashing, and stops there (#2373); and
    /// that a count outside the escape's four moves is refused.
    /// </summary>
    private static void VerifyMetroidEscapeConsumer()
    {
        // One system serves every timer: the escape step is static and keeps no per-call state.

        // An escape starts at its duration and ends on the step that reaches zero.
        for (int timer = 1; timer <= MetroidBehaviorDefinitions.EscapeDuration; timer++)
        {
            var place = new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry.Common, new RemovedEnemyLeftovers<IEnemyEntity>())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0001, 0xffff)),
            };
            var state = new MetroidEnemyState(
                new RoomEffectActor(RoomEffectActorKind.MetroidElectricity, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard),
                new RoomEffectActor(RoomEffectActorKind.MetroidShell, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard))
            {
                EscapeMovesLeft = UpdateCountdown.For(timer),
                Movement = MetroidMovement.PowerBombEscape,
                Playback = new(MetroidAnimations.StartingWith(MetroidSegment.DrainingSamus), AnimationPlace.StartOfAnimation),
            };

            Offset16<RoomPixels> expected =
                MetroidReactions.EscapeDisplacement(timer);
            MetroidBehavior.RunMetroidPowerBombEscape(place, state);
            AssertEqual(unchecked((ushort)(1 + expected.X)), place.Position.X.Whole,
                $"production Metroid escape X timer {timer}");
            AssertEqual(unchecked((ushort)(0xffff + expected.Y)), place.Position.Y.Whole,
                $"production Metroid escape Y timer {timer}");
            AssertEqual(timer - 1, state.EscapeMovesLeft.UpdatesLeft,
                $"production Metroid escape countdown {timer}");
            AssertEqual(timer == 1 ? MetroidMovement.Homing : MetroidMovement.PowerBombEscape,
                state.Movement,
                $"production Metroid escape function {timer}");
            AssertEqual(
                new AnimationPlayback<MetroidSegment>(
                    MetroidAnimations.StartingWith(timer == 1 ? MetroidSegment.ChasingSamus : MetroidSegment.DrainingSamus),
                    AnimationPlace.StartOfAnimation),
                state.Playback,
                $"production Metroid escape animation {timer}");
        }

        // A Metroid frozen while it escapes strains its moves away, flashing, and keeps none once they are spent.
        var frozenPlace = new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry.Common, new RemovedEnemyLeftovers<IEnemyEntity>());
        var frozen = new MetroidEnemyState(
            new RoomEffectActor(RoomEffectActorKind.MetroidElectricity, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard),
            new RoomEffectActor(RoomEffectActorKind.MetroidShell, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard))
        {
            EscapeMovesLeft = UpdateCountdown.Lasting(2),
            Movement = MetroidMovement.Homing,
        };
        frozenPlace.SetSpeciesState(frozen);
        var strain = new List<(int MovesLeft, int Flash)>();
        for (int update = 0; update < 3; update++)
        {
            frozenPlace.HurtFlash.End();
            MetroidBehavior.RunMetroidFrozen(frozenPlace, frozenPlace.RequireSpeciesState<MetroidEnemyState>());
            strain.Add((frozen.EscapeMovesLeft.UpdatesLeft, frozenPlace.HurtFlash.UpdatesLeft));
        }
        AssertTrue(strain.SequenceEqual([(1, MetroidBehaviorDefinitions.EscapeFlashDuration), (0, MetroidBehaviorDefinitions.EscapeFlashDuration), (0, 0)]),
            "a frozen escaping Metroid strains its two moves away, flashing, then stays still with none");
    }

    /// <summary>Checks all eight random cry selections through the real instruction interpreter.</summary>
    private static void VerifyMetroidCryConsumer(CartridgeImportAddressSpace rom)
    {
        var guarded = NewMetroidBehaviorReadGuard(rom);

        for (ushort random = 0; random < 8; random++)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            ushort selectedRandom = random;
            enemies.VerificationServices().NextRandom = () => selectedRandom;
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Metroid, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            place.AnimationClock.StepOnNextUpdate();
            // Start at the chasing loop's random-cry cue, after its four pulses.
            enemies.Places.InUpdateOrder[0].SetSpeciesState(new MetroidEnemyState(new RoomEffectActor(RoomEffectActorKind.MetroidElectricity, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard), new RoomEffectActor(RoomEffectActorKind.MetroidShell, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard))
            {
                Playback = new(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus),
                    AnimationSegments<MetroidSegment, EnemyFrameId, MetroidCue>.PlaceOf(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus),
                        MetroidSegment.ChasingCry, 0)),
            });

            EnemyAnimationPlayer.AdvanceAnimation(place, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);

            AssertEqual((SoundEffectId?)MetroidBehaviorDefinitions.Cry.Choose(random),
                enemies.Audio.LatestSound,
                $"production Metroid random cry {random}");
            AssertEqual(16, place.AnimationClock.Hold,
                $"production Metroid script timer {random}");
            AssertEqual(rom.ReadCartridgeWord(0xa3e9d1), place.SpritemapPointer,
                $"production Metroid script spritemap {random}");
            // The cue ends the pass; the loop shows its first pose and stands before its second, its four pulses all to come.
            AssertEqual(new AnimationPlayback<MetroidSegment>(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus), new AnimationPlace(0, 1).WithPlaysLeft(4)),
                (enemies.Places.InUpdateOrder[0].EntityState<MetroidEntity, MetroidEnemyState>() ?? throw new InvalidOperationException("The Metroid has no state.")).Playback,
                $"production Metroid script continuation {random}");
        }
    }

    /// <summary>Forbids reads from converted Metroid displacement and cry tables.</summary>
    private static CartridgeReadGuard NewMetroidBehaviorReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa3ea3f and < 0xa3ea4f or >= 0xa3ead6 and < 0xa3eae6,
                address => $"Metroid behavior attempted migrated table read ${address:X6}.");
}
