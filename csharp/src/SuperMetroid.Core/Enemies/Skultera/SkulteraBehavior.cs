using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>The Skultera species owner: its per-room state, AI routines and the interactions it registers.</summary>
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
    private static SkulteraProgram? ApplySkulteraCue(RoomEnemySlot slot, SkulteraEnemyState state, SkulteraCue cue)
    {
        switch (cue.Kind)
        {
            case SkulteraCueKind.DrawAboveScenery:
                // The right-facing swim promotes the fish above foreground scenery only after its first
                // instruction tick, exactly like the ROM.
                slot.Layer = 6;
                break;
            case SkulteraCueKind.DrawBelowScenery:
                // The left-facing swim draws below the matching scenery layer.
                slot.Layer = 2;
                break;
            case SkulteraCueKind.FinishTurn:
                // Turns sleep immediately after publishing this flag. Main AI consumes it on the following frame
                // and starts the steady swim.
                state.TurnFinished = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SkulteraCueKind)} {cue.Kind}.");
        }
        return null;
    }


    /// <summary>Ports <c>InitAI_Skultera</c> at <c>$A3:90B5</c>.</summary>
    internal static void InitializeSkultera(RoomEnemySlot slot)
    {
        var state = new SkulteraEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Parameter one's high byte selects initial facing: zero means right, any nonzero
        // value means left. Its low byte is multiplied by eight because each common speed
        // magnitude stores adjacent {right whole, right fraction, left whole, left fraction}
        // words. Keep the byte offset visible; treating it as an array index is a common
        // source of subtly wrong half-pixel speeds in ports of this AI.
        bool startsLeft = (SnesLittleEndian.HighByteInPlace(slot.Spawn.Population.Parameter1)) != 0;
        state.Program = AnimationProgramCursor<SkulteraProgram>.Start(startsLeft
            ? SkulteraProgram.SwimmingLeft
            : SkulteraProgram.SwimmingRight);
        state.Function = startsLeft
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;

        ushort speedTableOffset = unchecked((ushort)((SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter1)) * 8));
        SnesSignedSixteenSixteen right = EnemyMath.ReadLinearEnemySpeed(speedTableOffset);
        state.RightVelocity = right;
        SnesSignedSixteenSixteen left = EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));
        state.LeftVelocity = left;

        // Parameter two packs radius in its low byte and unsigned initial angle delta in
        // its high byte. The delta becomes negative only when a turn negates it.
        state.Radius = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter2);
        state.AngleDelta = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2);
        state.Angle = SnesAngle.Zero;
        state.TurnFinished = false;
        state.CurrentYOffset = 0;
        state.PreviousYOffset = EnemyMath.ReadEightBitNegativeSineProduct(
            state.Angle.ToByteAngle(),
            checked((ushort)state.Radius));
    }

    /// <summary>Ports <c>MainAI_Skultera</c> at <c>$A3:912B</c>.</summary>
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

    /// <summary>Ports the mirrored swimming functions at <c>$A3:9132/$91AB</c>.</summary>
    private void SwimSkultera(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction)
    {
        int horizontalDisplacement = (direction == HorizontalDirection.Left
            ? state.LeftVelocity
            : state.RightVelocity).RawValue;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: direction == HorizontalDirection.Left);
            // Horizontal collision jumps directly to the native merge label. Consequently
            // CurrentYOffset remains the preceding frame's word and is copied below again.
            state.PreviousYOffset = state.CurrentYOffset;
            return;
        }

        state.CurrentYOffset = EnemyMath.ReadEightBitNegativeSineProduct(
            state.Angle.ToByteAngle(),
            checked((ushort)state.Radius));
        // Both offsets lie within the radius either way, so their difference never wraps.
        int yDifference = state.CurrentYOffset - state.PreviousYOffset;
        if (_terrain.MoveEnemyVertically(level, slot, SnesSixteenSixteen.FromWhole(yDifference)))
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
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<SkulteraProgram>.Start(currentlyMovingLeft
            ? SkulteraProgram.TurningRight
            : SkulteraProgram.TurningLeft);
    }

    /// <summary>Ports the mirrored turn completion functions at <c>$A3:9224/$9256</c>.</summary>
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
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<SkulteraProgram>.Start(direction == HorizontalDirection.Left
            ? SkulteraProgram.SwimmingLeft
            : SkulteraProgram.SwimmingRight);
    }

    /// <summary>Ports Skultera's initialization AI.</summary>
    internal static void RunSkulteraInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeSkultera(slot);
    }

    /// <summary>Ports Skultera's main AI.</summary>
    internal void RunSkulteraMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunSkulteraMain(slot, slot.RequireSpeciesState<SkulteraEnemyState>(), frame.Level);
    }

    /// <summary>Advances Skultera's typed animation.</summary>
    internal static void AdvanceSkulteraTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, SkulteraAnimations.Programs, SkulteraFrameDefinitions.ShownFrames.Of,
            static (SkulteraEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplySkulteraCue(slot, state, cue));
}
