using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>The Skultera species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SkulteraBehavior : IEnemySpecies, IEnemySpawnFactory<SkulteraSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Skultera];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, SkulteraSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new SkulteraEntity(this, occupant, place, common, settings));

    /// <summary>The room's blocks Skultera's horizontal and vertical swim steps meet.</summary>
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
                // Turns rest immediately after finishing (#2450). Main AI starts the steady swim on the following
                // update.
                if (state.Action != SkulteraAction.Turning)
                    throw new InvalidOperationException($"A Skultera's turn finished while it was {state.Action}.");
                state.Action = SkulteraAction.TurnFinished;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SkulteraCueKind)} {cue.Kind}.");
        }
    }


    /// <summary>Initializes the Skultera.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static SkulteraEnemyState InitializeSkultera(SkulteraSpawnSettings settings)
    {
        // The placement names the way it starts swimming and the speed it swims at either way.
        bool startsLeft = settings.StartDirection == HorizontalDirection.Left;
        var state = new SkulteraEnemyState(SkulteraAction.Swimming, settings.StartDirection);

        state.Playback = new(SkulteraAnimations.StartingWith(startsLeft
            ? SkulteraSegment.SwimmingLeft
            : SkulteraSegment.SwimmingRight), AnimationPlace.StartOfAnimation);

        state.PreviousYOffset = EnemyMath.CircleNegativeSinePixels(state.Angle.ToTrigonometryAngle(), settings.Radius);
        return state;
    }

    /// <summary>Runs the Skultera's main AI.</summary>
    private void RunSkulteraMain(
        RoomEnemyOccupant occupant, SkulteraSpawnSettings skulteraSpawnSettings, RoomEnemyPlace place,
        SkulteraEnemyState state,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case SkulteraAction.Swimming:
                SwimSkultera(occupant, skulteraSpawnSettings, place, state, level, state.Heading);
                return;
            case SkulteraAction.Turning:
                return;
            case SkulteraAction.TurnFinished:
                FinishSkulteraTurn(place, state, state.Heading);
                return;
            default:
                throw new InvalidDataException(
                    $"Skultera has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the mirrored swimming actions.</summary>
    private void SwimSkultera(
        RoomEnemyOccupant occupant, SkulteraSpawnSettings settings, RoomEnemyPlace place,
        SkulteraEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction)
    {
        SignedSixteenSixteen horizontalDisplacement = SwimVelocity(settings, direction);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                horizontalDisplacement))
        {
            // A wall turns the Skultera before it bobs: its vertical offset stays the previous update's.
            BeginSkulteraTurn(place, state, direction);
            return;
        }

        int currentYOffset = EnemyMath.CircleNegativeSinePixels(state.Angle.ToTrigonometryAngle(), settings.Radius);
        // Both offsets lie within the radius either way, so their difference never wraps.
        int yDifference = currentYOffset - state.PreviousYOffset;
        if (_terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(yDifference)))
        {
            BeginSkulteraTurn(place, state, direction);
        }
        else
        {
            // Only unobstructed movement advances the phase, which wraps after a full turn.
            state.AdvanceAngle(settings);
        }

        // The new offset stands whether the vertical move succeeded or hit terrain, so a blocked step is not taken
        // again after the turn.
        state.PreviousYOffset = currentYOffset;
    }

    /// <summary>Switches swimming AI, swimming toward <paramref name="swimming"/>, to its opposite-facing turn animation after a terrain collision.</summary>
    private static void BeginSkulteraTurn(
        RoomEnemyPlace place,
        SkulteraEnemyState state,
        HorizontalDirection swimming)
    {
        state.Action = SkulteraAction.Turning;
        state.Heading = swimming == HorizontalDirection.Left ? HorizontalDirection.Right : HorizontalDirection.Left;
        state.Playback = EnemyAnimationPlayback.Start(place, SkulteraAnimations.StartingWith(swimming == HorizontalDirection.Left
            ? SkulteraSegment.TurningRight
            : SkulteraSegment.TurningLeft));
    }

    /// <summary>
    /// The swimming displacement of the Skultera in <paramref name="place"/> toward <paramref name="direction"/>: its
    /// spawn speed, reversed leftward (#2278).
    /// </summary>
    private static SignedSixteenSixteen SwimVelocity(SkulteraSpawnSettings skulteraSpawnSettings, HorizontalDirection direction)
    {
        SignedSixteenSixteen speed = skulteraSpawnSettings.Speed;
        return speed.Toward(direction);
    }

    /// <summary>
    /// Runs the mirrored turn completions: the Skultera swims toward <paramref name="direction"/>, the way it turned, and
    /// its wave steps the other way from then on (<see cref="SkulteraEnemyState.AdvanceAngle"/>).
    /// </summary>
    private static void FinishSkulteraTurn(
        RoomEnemyPlace place,
        SkulteraEnemyState state,
        HorizontalDirection direction)
    {
        state.Action = SkulteraAction.Swimming;
        state.Playback = EnemyAnimationPlayback.Start(place, SkulteraAnimations.StartingWith(direction == HorizontalDirection.Left
            ? SkulteraSegment.SwimmingLeft
            : SkulteraSegment.SwimmingRight));
    }

    /// <summary>Runs Skultera's main AI.</summary>
    internal void RunSkulteraMainAi(RoomEnemyOccupant occupant, SkulteraSpawnSettings skulteraSpawnSettings, RoomEnemyPlace place, SkulteraEnemyState skulteraEnemyState, IEnemyAiFrame frame)
    {
        RunSkulteraMain(occupant, skulteraSpawnSettings, place, skulteraEnemyState, frame.Level);
    }

    /// <summary>Advances Skultera's typed animation.</summary>
    internal static void AdvanceSkulteraTypedAnimation(RoomEnemyPlace place, SkulteraEnemyState skulteraEnemyState) =>
        EnemyAnimationPlayback.Advance(place, skulteraEnemyState, SkulteraAnimations.Segments,
            static (SkulteraEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplySkulteraCue(state, cue);
                return null;
            });
}
