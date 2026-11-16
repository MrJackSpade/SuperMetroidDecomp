using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>The Skultera species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SkulteraBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Skultera];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new SkulteraEntity(this, place, common);

    /// <summary>Room collision helper used by Skultera's horizontal and vertical swim steps.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Skultera movement to the room enemy terrain rules.</summary>
    internal SkulteraBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of Skultera's animations.</summary>
    private static void ApplySkulteraCue(SkulteraEnemyState state, SkulteraCue cue)
    {
        switch (cue.Kind)
        {
            case SkulteraCueKind.FinishTurn:
                // Turns rest immediately after publishing this flag. Main AI consumes it on the following frame
                // and starts the steady swim.
                state.TurnFinished = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SkulteraCueKind)} {cue.Kind}.");
        }
    }


    /// <summary>Initializes the Skultera.</summary>
    internal static void InitializeSkultera(RoomEnemyPlace place)
    {
        // The placement names the way it starts swimming and the speed it swims at either way.
        SkulteraSpawnSettings settings = place.SpawnSettings<SkulteraSpawnSettings>();
        bool startsLeft = settings.StartDirection == HorizontalDirection.Left;
        var state = new SkulteraEnemyState(startsLeft
            ? SkulteraAction.SwimmingLeft
            : SkulteraAction.SwimmingRight);
        place.SetSpeciesState(state);

        state.Playback = new(SkulteraAnimations.StartingWith(startsLeft
            ? SkulteraSegment.SwimmingLeft
            : SkulteraSegment.SwimmingRight), AnimationPlace.StartOfAnimation);
        EnemyLinearSpeed speeds = new(settings.Speed);
        state.RightVelocity = speeds.Positive;
        state.LeftVelocity = speeds.Negative;

        // The wave's step starts positive and becomes negative only when a turn negates it.
        state.Radius = settings.Radius;
        state.AngleDelta = settings.AngleDelta;
        state.Angle = RotationAngle.Zero;
        state.TurnFinished = false;
        state.CurrentYOffset = 0;
        state.PreviousYOffset = EnemyMath.CircleNegativeSinePixels(state.Angle.ToTrigonometryAngle(), state.Radius);
    }

    /// <summary>Runs the Skultera's main AI.</summary>
    private void RunSkulteraMain(
        RoomEnemyPlace place,
        SkulteraEnemyState state,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case SkulteraAction.SwimmingLeft:
                SwimSkultera(place, state, level, direction: HorizontalDirection.Left);
                return;
            case SkulteraAction.SwimmingRight:
                SwimSkultera(place, state, level, direction: HorizontalDirection.Right);
                return;
            case SkulteraAction.TurningRight:
                FinishSkulteraTurn(place, state, direction: HorizontalDirection.Right);
                return;
            case SkulteraAction.TurningLeft:
                FinishSkulteraTurn(place, state, direction: HorizontalDirection.Left);
                return;
            default:
                throw new InvalidDataException(
                    $"Skultera has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the mirrored swimming actions.</summary>
    private void SwimSkultera(
        RoomEnemyPlace place,
        SkulteraEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction)
    {
        SignedSixteenSixteen horizontalDisplacement = direction == HorizontalDirection.Left
            ? state.LeftVelocity
            : state.RightVelocity;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                horizontalDisplacement))
        {
            BeginSkulteraTurn(place, state, currentlyMovingLeft: direction == HorizontalDirection.Left);
            // Horizontal collision skips straight to the shared tail. Consequently
            // CurrentYOffset remains the preceding frame's value and is copied below again.
            state.PreviousYOffset = state.CurrentYOffset;
            return;
        }

        state.CurrentYOffset = EnemyMath.CircleNegativeSinePixels(state.Angle.ToTrigonometryAngle(), state.Radius);
        // Both offsets lie within the radius either way, so their difference never wraps.
        int yDifference = state.CurrentYOffset - state.PreviousYOffset;
        if (_terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(yDifference)))
        {
            BeginSkulteraTurn(place, state, currentlyMovingLeft: direction == HorizontalDirection.Left);
        }
        else
        {
            // Only unobstructed movement advances the phase, which wraps after a full turn.
            state.AdvanceAngle();
        }

        // Both successful movement and vertical collision retain the newly calculated
        // offset. This prevents a failed vertical step from being retried after the turn.
        state.PreviousYOffset = state.CurrentYOffset;
    }

    /// <summary>Switches swimming AI to its opposite-facing turn animation after a terrain collision.</summary>
    internal static void BeginSkulteraTurn(
        RoomEnemyPlace place,
        SkulteraEnemyState state,
        bool currentlyMovingLeft)
    {
        state.Action = currentlyMovingLeft
            ? SkulteraAction.TurningRight
            : SkulteraAction.TurningLeft;
        state.Playback = EnemyAnimationPlayback.Start(place, SkulteraAnimations.StartingWith(currentlyMovingLeft
            ? SkulteraSegment.TurningRight
            : SkulteraSegment.TurningLeft));
    }

    /// <summary>Runs the mirrored turn completions.</summary>
    private static void FinishSkulteraTurn(
        RoomEnemyPlace place,
        SkulteraEnemyState state,
        HorizontalDirection direction)
    {
        if (!state.TurnFinished)
            return;

        state.TurnFinished = false;
        state.Action = direction == HorizontalDirection.Left
            ? SkulteraAction.SwimmingLeft
            : SkulteraAction.SwimmingRight;
        state.AngleDelta = -state.AngleDelta;
        state.Playback = EnemyAnimationPlayback.Start(place, SkulteraAnimations.StartingWith(direction == HorizontalDirection.Left
            ? SkulteraSegment.SwimmingLeft
            : SkulteraSegment.SwimmingRight));
    }

    /// <summary>Runs Skultera's main AI.</summary>
    internal void RunSkulteraMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunSkulteraMain(place, place.RequireSpeciesState<SkulteraEnemyState>(), frame.Level);
    }

    /// <summary>Advances Skultera's typed animation.</summary>
    internal static void AdvanceSkulteraTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, SkulteraAnimations.Segments,
            static (SkulteraEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplySkulteraCue(state, cue);
                return null;
            });
}
