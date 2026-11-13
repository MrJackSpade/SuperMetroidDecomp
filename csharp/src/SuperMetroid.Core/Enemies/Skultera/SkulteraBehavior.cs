using SuperMetroid.Core.Programs;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new SkulteraEntity(this, slot, common);

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
                // Turns sleep immediately after publishing this flag. Main AI consumes it on the following frame
                // and starts the steady swim.
                state.TurnFinished = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SkulteraCueKind)} {cue.Kind}.");
        }
    }


    /// <summary>Initializes the Skultera.</summary>
    internal static void InitializeSkultera(RoomEnemySlot slot)
    {
        var state = new SkulteraEnemyState();
        slot.SetSpeciesState(state);

        // The placement names the way it starts swimming and the speed it swims at either way.
        SkulteraSpawnSettings settings = slot.SpawnSettings<SkulteraSpawnSettings>();
        bool startsLeft = settings.StartDirection == HorizontalDirection.Left;
        state.Playback = new(SkulteraAnimations.StartingWith(startsLeft
            ? SkulteraProgram.SwimmingLeft
            : SkulteraProgram.SwimmingRight), AnimationPlace.StartOfAnimation);
        state.Function = startsLeft
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;

        EnemyLinearSpeedRow speeds = EnemyLinearSpeedRow.Moving(settings.Speed);
        state.RightVelocity = speeds.Positive;
        state.LeftVelocity = speeds.Negative;

        // The wave's step starts positive and becomes negative only when a turn negates it.
        state.Radius = settings.Radius;
        state.AngleDelta = settings.AngleDelta;
        state.Angle = RotationAngle.Zero;
        state.TurnFinished = false;
        state.CurrentYOffset = 0;
        state.PreviousYOffset = EnemyMath.ReadEightBitNegativeSineProduct(state.Angle.ToTrigonometryAngle(), state.Radius);
    }

    /// <summary>Runs the Skultera's main AI.</summary>
    private void RunSkulteraMain(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Skultera AI requires active room collision data.");

        switch (state.Function)
        {
            case SkulteraEnemyFunction.SwimmingLeft:
                SwimSkultera(slot, state, level, direction: HorizontalDirection.Left);
                return;
            case SkulteraEnemyFunction.SwimmingRight:
                SwimSkultera(slot, state, level, direction: HorizontalDirection.Right);
                return;
            case SkulteraEnemyFunction.TurningRight:
                FinishSkulteraTurn(slot, state, direction: HorizontalDirection.Right);
                return;
            case SkulteraEnemyFunction.TurningLeft:
                FinishSkulteraTurn(slot, state, direction: HorizontalDirection.Left);
                return;
            default:
                throw new InvalidDataException(
                    $"Skultera function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Runs the mirrored swimming functions.</summary>
    private void SwimSkultera(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction)
    {
        SignedSixteenSixteen horizontalDisplacement = direction == HorizontalDirection.Left
            ? state.LeftVelocity
            : state.RightVelocity;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: direction == HorizontalDirection.Left);
            // Horizontal collision skips straight to the shared tail. Consequently
            // CurrentYOffset remains the preceding frame's value and is copied below again.
            state.PreviousYOffset = state.CurrentYOffset;
            return;
        }

        state.CurrentYOffset = EnemyMath.ReadEightBitNegativeSineProduct(state.Angle.ToTrigonometryAngle(), state.Radius);
        // Both offsets lie within the radius either way, so their difference never wraps.
        int yDifference = state.CurrentYOffset - state.PreviousYOffset;
        if (_terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(yDifference)))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: direction == HorizontalDirection.Left);
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
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        bool currentlyMovingLeft)
    {
        state.Function = currentlyMovingLeft
            ? SkulteraEnemyFunction.TurningRight
            : SkulteraEnemyFunction.TurningLeft;
        state.Playback = EnemyProgramAnimation.Start(slot, SkulteraAnimations.StartingWith(currentlyMovingLeft
            ? SkulteraProgram.TurningRight
            : SkulteraProgram.TurningLeft));
    }

    /// <summary>Runs the mirrored turn completion functions.</summary>
    private static void FinishSkulteraTurn(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        HorizontalDirection direction)
    {
        if (!state.TurnFinished)
            return;

        state.TurnFinished = false;
        state.Function = direction == HorizontalDirection.Left
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;
        state.AngleDelta = -state.AngleDelta;
        state.Playback = EnemyProgramAnimation.Start(slot, SkulteraAnimations.StartingWith(direction == HorizontalDirection.Left
            ? SkulteraProgram.SwimmingLeft
            : SkulteraProgram.SwimmingRight));
    }

    /// <summary>Runs Skultera's main AI.</summary>
    internal void RunSkulteraMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunSkulteraMain(slot, slot.RequireSpeciesState<SkulteraEnemyState>(), frame.Level);
    }

    /// <summary>Advances Skultera's typed animation.</summary>
    internal static void AdvanceSkulteraTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, SkulteraAnimations.Segments,
            static (SkulteraEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplySkulteraCue(state, cue);
                return null;
            });
}
