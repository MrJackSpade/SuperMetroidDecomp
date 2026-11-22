using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks Torizo's widened state: a stepping cue counts a decision step, the space-jump count wraps to zero past 65535
/// (#1945), the animation lock follows its cues, and the posture and behaviour cues replace or add only what they name:
/// a step the whole posture, the turning cue the turn alone (#2237).
/// </summary>
internal static class TorizoStateQuantitiesVerification
{
    /// <summary>Runs the count, lock, acceleration and flag-word checks.</summary>
    internal static void VerifyTorizoStateQuantities()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        // Golden Torizo as his room's population sets him up, his state the one his setup built.
        RoomEnemyPlace place = enemies.PlaceTorizo(0, new(512, 512), TorizoVariant.Golden);
        TorizoEnemyState state = place.EntityOf<TorizoEntity>().State;
        var inputs = new TorizoCueInputs { Torizo = LivingTorizo.Instance, Samus = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), Level = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), ControllerInput = 0, FrameCounter = 0 };
        void ApplyCue(TorizoCue cue) =>
            AssertTrue(enemies.TorizoBehavior.ApplyTorizoCue(place.HeldOccupant(), place, state, inputs, cue,
                    TorizoAnimations.Start(TorizoAnimation.BombTorizoAwakening)) is null,
                $"{cue} runs on");
        void Apply(TorizoCueKind kind) => ApplyCue(new TorizoCue.Signal(kind));

        int decisions = state.DecisionCounter;
        ApplyCue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Left));
        AssertEqual(decisions + 1, state.DecisionCounter, "a stepping cue counts one decision step");

        int spaceJumpFrames = state.SamusSpaceJumpFrames;
        state.CountSamusSpaceJumpFrame();
        AssertEqual(spaceJumpFrames + 1, state.SamusSpaceJumpFrames, "a space-jumping frame counts one frame");
        // Samus space-jumps on until the count reaches its largest.
        while (state.SamusSpaceJumpFrames != SuperMetroid.Core.Compatibility.WrappingCounter.LargestCount)
            state.CountSamusSpaceJumpFrame();
        state.CountSamusSpaceJumpFrame();
        AssertEqual(0, state.SamusSpaceJumpFrames, "counting past the largest space-jump count wraps to zero");

        Apply(TorizoCueKind.LockAnimation);
        AssertTrue(state.AnimationLocked, "the lock cue locks the animation");
        Apply(TorizoCueKind.UnlockAnimation);
        AssertTrue(!state.AnimationLocked, "the unlock cue unlocks the animation");

        Apply(TorizoCueKind.SetTurningAround);
        AssertEqual(TorizoPosture.Initial.TurningAround, state.Posture, "the turning cue turns the Torizo standing as he starts");
        ApplyCue(new TorizoCue.Stepped(HorizontalDirection.Right, Foot: HorizontalDirection.Left));
        AssertEqual(TorizoPosture.AfterStep(HorizontalDirection.Right, HorizontalDirection.Left), state.Posture,
            "a stepping cue replaces the whole posture: its facing, its leading foot and the turn");
        Apply(TorizoCueKind.SetTurningAround);
        AssertEqual(TorizoPosture.AfterStep(HorizontalDirection.Right, HorizontalDirection.Left).TurningAround,
            state.Posture, "the turning cue sets the turn alone");

        // A bomb stuns Golden Torizo while his animation is unlocked.
        var ordinaryProjectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads,
            new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), InstalledFixtures.SamusArtwork(), new RoomEarthquake());
        var bomb = new SamusBomb(0);
        bomb.OccupyAsPlaced(SamusProjectileType.Of(SamusProjectileFamily.Bomb));
        bomb.HeldBomb().Position = FixedPosition<RoomPixels>.FromWhole(place.Position.Whole);
        enemies.TorizoBehavior.Combat.ReactToGoldenTorizoBomb(new EnemyBombContext(place.HeldOccupant(), bomb.HeldBomb(), ordinaryProjectiles,
            inputs.Samus, TorizoReactions.Shot, UsesFrameHitboxes: true, Position16<RoomPixels>.Zero), state);
        AssertTrue(state.Stunned, "a bomb stuns the unlocked Golden Torizo");
        Apply(TorizoCueKind.UnmarkStunned);
        AssertTrue(!state.Stunned, "the unmark cue clears the stun");
        Apply(TorizoCueKind.MarkFaceBlownUp);
        AssertTrue(state.FaceBlownUp, "the mark cue blows up the face");

        Console.WriteLine("Torizo state quantities: decision count, space-jump count wrap, animation lock, posture and behaviour latches pass.");
    }

    /// <summary>Runs Torizo's state quantity checks.</summary>
    [VerificationCommand("--torizo-state-quantities")]
    internal static int TorizoStateQuantitiesCommand()
    {
        Suite(nameof(VerifyTorizoStateQuantities), () => VerifyTorizoStateQuantities());
        return 0;
    }
}
