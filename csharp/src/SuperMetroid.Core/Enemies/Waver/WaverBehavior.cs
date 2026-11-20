using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>The Waver species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WaverBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Waver];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new WaverEntity(this, occupant, place, common));

    /// <summary>Room collision service used to reflect horizontal motion and the sine wave on impact.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Connects Waver AI to room terrain collision.</summary>
    internal WaverBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out Waver's one animation cue.</summary>
    private static void ApplyWaverCue(WaverEnemyState state, WaverCue cue)
    {
        switch (cue.Kind)
        {
            case WaverCueKind.FinishSpin:
                // The four-frame spin hands its completion back to main AI rather than
                // going on to the steady animation itself (#2450).
                if (state.Action != WaverAction.Spinning)
                    throw new InvalidOperationException($"A Waver's spin finished while it was {state.Action}.");
                state.Action = WaverAction.SpinFinished;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(WaverCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initializes the Waver.</summary>
    internal static void InitializeWaver(RoomEnemyOccupant occupant, WaverSpawnSettings waverSpawnSettings, RoomEnemyPlace place)
    {
        HorizontalDirection facing = waverSpawnSettings.Facing;
        var state = new WaverEnemyState
        {
            Facing = facing,
            Angle = RotationAngle.Zero,
            Action = WaverAction.Gliding,
        };
        place.SetSpeciesState(state);

        // A Waver starts with the left steady animation; a right-facing Waver then turns it to face right, while a
        // left-facing one keeps the animation already playing and its clock.
        state.Playback = new(WaverAnimations.StartingWith(WaverSegment.SteadyFacingLeft), AnimationPlace.StartOfAnimation);
        SetWaverAnimation(place, state, WaverAnimationChoices.Steady(state.Facing));
    }

    /// <summary>Runs the Waver's main AI.</summary>
    private void RunWaverMain(
        RoomEnemyPlace place,
        WaverEnemyState state,
        RoomLevelData level)
    {
        // A finished spin gives way to the steady pose at the end of this update, whatever the flight does first.
        bool spinFinished = state.Action == WaverAction.SpinFinished;
        SignedSixteenSixteen horizontalDisplacement = state.XVelocity;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                horizontalDisplacement))
        {
            // A wall turns Waver around, reversing its speed exactly, fraction included, and ends any spin.
            state.Facing = state.Facing == HorizontalDirection.Left ? HorizontalDirection.Right : HorizontalDirection.Left;
            SetWaverAnimation(place, state, WaverAnimationChoices.Steady(state.Facing));
        }
        else
        {
            int verticalPixels = EnemyMath.CircleNegativeSinePixels(
                state.Angle.ToTrigonometryAngle(),
                WaverMotionDefinitions.WaveHeight);
            if (_terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(verticalPixels)))
            {
                state.ReflectWave();
            }
            else
            {
                state.AdvanceWave();
            }
        }

        // Once per half-wave, the phase reaches the spin's place within its half-turn. Waver temporarily swaps to
        // the four-frame spin without changing its facing.
        if (state.Angle.ToTrigonometryAngle().StepsIntoHalfTurn == WaverMotionDefinitions.SpinStepsIntoHalfTurn)
        {
            SetWaverAnimation(place, state, WaverAnimationChoices.Spinning(state.Facing));
        }

        // The spin animation finishes after its own frames. Main AI owns
        // the return to steady art, matching the cue/AI seam rather than timing it here.
        if (spinFinished)
            SetWaverAnimation(place, state, WaverAnimationChoices.Steady(state.Facing));
    }

    /// <summary>
    /// Starts a changed facing or spin animation and restarts its frame timer (#2145); the Waver spins while a spin plays
    /// and glides while a steady pose shows (#2450).
    /// </summary>
    private static void SetWaverAnimation(RoomEnemyPlace place, WaverEnemyState state, WaverSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, WaverAnimations.StartingWith(segment));
        state.Action = WaverAnimationChoices.IsSpin(segment) ? WaverAction.Spinning : WaverAction.Gliding;
    }

    /// <summary>Runs Waver's main AI.</summary>
    internal void RunWaverMainAi(RoomEnemyPlace place, WaverEnemyState waverEnemyState, IEnemyAiFrame frame)
    {
        RunWaverMain(place, waverEnemyState, frame.Level);
    }

    /// <summary>Advances Waver's typed animation.</summary>
    internal static void AdvanceWaverTypedAnimation(RoomEnemyPlace place, WaverEnemyState waverEnemyState) =>
        EnemyAnimationPlayback.Advance(place, waverEnemyState, WaverAnimations.Segments,
            static (WaverEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyWaverCue(state, cue);
                return null;
            });
}
