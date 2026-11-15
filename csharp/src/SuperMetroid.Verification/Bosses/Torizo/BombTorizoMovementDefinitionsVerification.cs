using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks that Torizo's posture and walking cues move it as they say (#1891). The animation oracle compares each cue's
/// displacement and velocity with the cartridge's tables; this checks the consumers apply them.
/// </summary>
internal static class BombTorizoMovementDefinitionsVerification
{
    /// <summary>
    /// Every posture cue's displacement moves the Torizo by itself standing up and back sitting down, and every Bomb
    /// Torizo walking cue's velocity becomes the Torizo's velocity and movement.
    /// </summary>
    internal static void VerifyBombTorizoMovementDefinitions()
    {
        TorizoCue[] cues =
        [
            .. Enum.GetValues<TorizoAnimation>().SelectMany(animation => TorizoAnimations.Segments.Beats(animation))
                .OfType<AnimationBeat<EnemyFrameId, TorizoCue>.Event>().Select(beat => beat.Raised).Distinct(),
        ];
        Offset16<RoomPixels>[] displacements = [.. cues.OfType<TorizoCue.Posture>().Select(cue => cue.Displacement).Distinct()];
        AssertTrue(displacements.Length > 1, "Torizo's animations carry posture displacements");
        foreach (Offset16<RoomPixels> displacement in displacements)
        foreach (bool sitDown in new[] { false, true })
        {
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 500)),
            };
            TorizoBehavior.ApplyTorizoPosture(slot, displacement, sitDown ? TorizoPostureMove.SitDown : TorizoPostureMove.StandUp);
            int sign = sitDown ? -1 : 1;
            AssertEqual(unchecked((ushort)(1000 + sign * displacement.X)), slot.Position.X.Whole, $"Torizo posture X {displacement} sit {sitDown}");
            AssertEqual(unchecked((ushort)(500 + sign * displacement.Y)), slot.Position.Y.Whole, $"Torizo posture Y {displacement} sit {sitDown}");
        }

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(64, 64), new RoomLevelPlanes(new RoomTerrainBlock[4096], new RoomBlockBehavior[4096], new RoomBlockAppearance[4096]), RoomLevelData.NothingBeyondGrid());
        int[] velocities = [.. cues.OfType<TorizoCue.Walk>().Where(cue => cue.Kind != TorizoWalkKind.WalkGoldenTorizo).Select(cue => cue.Velocity).Distinct()];
        AssertTrue(velocities.Length > 1, "Bomb Torizo's animations carry walking velocities");
        foreach (int velocity in velocities)
        {
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)),
                Radius = new(8, 8),
            };
            var state = new TorizoEnemyState(isGolden: false);
            slot.SetSpeciesState(state);
            var next = enemies.TorizoBehavior.Choices.ProcessBombTorizoWalkAnimation(slot, state, null, level,
                new TorizoWalkStep { Velocity = velocity, CollisionFacingRight = TorizoAnimation.TurningLeft, CollisionFacingLeft = TorizoAnimation.TurningRight });
            AssertEqual(velocity, state.Velocity.X.InTwoHundredFiftySixthsOfPixel, $"Bomb Torizo walk consumer velocity {velocity}");
            AssertTrue(next is null, $"Bomb Torizo walk consumer runs on without a wall at velocity {velocity}");
        }

        Console.WriteLine(
            $"Torizo movement cues: {displacements.Length} posture displacements both ways and {velocities.Length} Bomb Torizo walking velocities move it as they say.");
    }

    /// <summary>Runs the Torizo movement cue checks.</summary>
    [VerificationCommand("--lookup-stream2-torizo-movement")]
    internal static int LookupStream2TorizoMovementCommand()
    {
        Suite(nameof(VerifyBombTorizoMovementDefinitions), VerifyBombTorizoMovementDefinitions);
        return 0;
    }
}
