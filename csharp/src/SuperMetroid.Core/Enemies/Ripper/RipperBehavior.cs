using SuperMetroid.Core.Programs;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The Ripper species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RipperBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. RipperPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new RipperEntity(this, slot, common);

    /// <summary>Resolves horizontal wall collisions for all three Ripper variants.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Shares room collision behavior across ordinary Ripper, GRipper, and Ripper II AI.</summary>
    internal RipperBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Ports <c>InitAI_GRipper</c> at $A2:E1D3.</summary>
    internal static void InitializeGRipper(RoomEnemySlot slot)
    {
        var state = CreateRipperState(slot);

        // The population entry names the patrol speed record and the starting direction.
        GRipperSpawnSettings settings = slot.SpawnSettings<GRipperSpawnSettings>();
        state.Speeds = EnemyLinearSpeeds.Row(settings.SpeedIndex);
        LoadRipperVelocity(state, settings.Direction switch
        {
            GRipperStartDirection.Left => false,
            GRipperStartDirection.Right => true,
            _ => throw new InvalidOperationException($"GRipper start direction {settings.Direction} is not a direction."),
        });
        SetRipperProgram(
            slot,
            state.XVelocity.WholePixels < 0 ? RipperProgram.GRipperMovingLeft : RipperProgram.GRipperMovingRight);

        // Unlike the wall-collision helper, patrol-bound reversal does not clamp the actor
        // back to either endpoint. The one-frame fractional overshoot is native and becomes
        // visible again when the direction reverses on the next pass.
        state.MinimumXPosition = settings.MinimumX;
        state.MaximumXPosition = settings.MaximumX;
    }

    /// <summary>Ports <c>MainAI_GRipper</c> at $A2:E221.</summary>
    private void RunGRipperMain(
        RoomEnemySlot slot,
        RipperEnemyState state,
        RoomLevelData? level)
    {
        RequireRipperTerrain(level, "GRipper");
        int displacement = state.XVelocity.RawValue;
        bool reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            displacement);
        if (!reverse && state.XVelocity.WholePixels < 0)
        {
            reverse = CoordinateAxis.Difference(slot.Position.X.Whole, state.MinimumXPosition) < 0;
        }
        if (!reverse && state.XVelocity.WholePixels >= 0)
        {
            reverse = CoordinateAxis.Difference(slot.Position.X.Whole, state.MaximumXPosition) >= 0;
        }
        if (reverse)
        {
            ReverseRipperVariant(
                slot,
                positiveProgram: RipperProgram.GRipperMovingRight,
                negativeProgram: RipperProgram.GRipperMovingLeft);
        }
    }

    /// <summary>Ports <c>InitAI_Ripper2</c> at $A2:E318.</summary>
    internal static void InitializeRipper2(RoomEnemySlot slot)
    {
        RipperEnemyState state = CreateRipperState(slot);
        state.Speeds = EnemyLinearSpeeds.Row(slot.SpawnSettings<RipperSpawnSettings>().SpeedIndex);

        // Ripper II's list labels appear inverted relative to the displayed shared maps.
        // Keep the native list names instead of normalizing them: init1 zero uses the
        // negative speed pair with the moving-right list, while nonzero uses positive speed
        // with the moving-left list.
        bool movingPositive = slot.SpawnSettings<RipperSpawnSettings>().Direction == HorizontalDirection.Right;
        LoadRipperVelocity(state, movingPositive);
        SetRipperProgram(slot, movingPositive ? RipperProgram.Ripper2MovingLeft : RipperProgram.Ripper2MovingRight);
    }

    /// <summary>Ports <c>MainAI_Ripper2</c> at $A2:E353.</summary>
    private void RunRipper2Main(RoomEnemySlot slot, RipperEnemyState state, RoomLevelData? level)
    {
        RequireRipperTerrain(level, "Ripper II");
        int displacement = state.XVelocity.RawValue;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        ReverseRipperVariant(
            slot,
            positiveProgram: RipperProgram.Ripper2MovingLeft,
            negativeProgram: RipperProgram.Ripper2MovingRight);
    }

    /// <summary>Ports <c>Ripper_Init</c> at $A2:E49F.</summary>
    internal static void InitializeRipper(RoomEnemySlot slot)
    {
        // The spawn's direction chooses the positive speed and the right-facing list, or the negative speed and the
        // left-facing list. Unlike a reversal, initialization keeps the population's instruction timer.
        bool movingRight = slot.SpawnSettings<RipperSpawnSettings>().Direction == HorizontalDirection.Right;
        RipperEnemyState state = CreateRipperState(slot);
        state.Program = AnimationProgramCursor<RipperProgram>.Start(
            movingRight ? RipperProgram.RipperMovingRight : RipperProgram.RipperMovingLeft);

        // The state retains the speed row because the reversal routine reuses it.
        state.Speeds = EnemyLinearSpeeds.Row(slot.SpawnSettings<RipperSpawnSettings>().SpeedIndex);
        LoadRipperVelocity(state, movingRight);
    }

    /// <summary>Ports <c>Ripper_Main</c> at $A2:E4DA.</summary>
    private void RunRipperMain(RoomEnemySlot slot, RoomLevelData? level)
    {
        RequireRipperTerrain(level, "Ripper");

        // $A0:C6AB consumes the signed 16.16 velocity as a displacement. On collision the
        // native helper aligns the actor flush with the wall before Ripper swaps direction
        // and animation.
        RipperEnemyState state = slot.RequireSpeciesState<RipperEnemyState>();
        int displacement = state.XVelocity.RawValue;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        bool wasMovingLeft = state.XVelocity.WholePixels < 0;
        bool nowMovingRight = wasMovingLeft;
        LoadRipperVelocity(state, nowMovingRight);
        SetRipperProgram(slot, nowMovingRight ? RipperProgram.RipperMovingRight : RipperProgram.RipperMovingLeft);
    }

    /// <summary>Loads a Ripper-family enemy's velocity from its linear-speed row.</summary>
    /// <param name="state">The enemy's state, whose speed row is read.</param>
    /// <param name="movingRight">Whether the row's positive speed is loaded rather than its negative speed.</param>
    internal static void LoadRipperVelocity(RipperEnemyState state, bool movingRight) =>
        state.XVelocity = movingRight ? state.Speeds.Positive : state.Speeds.Negative;

    /// <summary>
    /// Ports the common post-shot tail at $A2:E3AD. Normal shot AI owns health, death,
    /// impact, and freeze timers; this family-specific tail only pins the direction-specific
    /// two-piece frozen map when the accepted projectile actually froze the actor.
    /// </summary>
    private static void ResolveGRipperRipper2ShotAfterCommon(RoomEnemySlot slot)
    {
        if (slot.FrozenTimer.IsZero)
            return;
        slot.Frame = slot.RequireSpeciesState<RipperEnemyState>().XVelocity.WholePixels < 0
            ? EnemyFrameId.RipperSharedFrozenLeft
            : EnemyFrameId.RipperSharedFrozenRight;
    }

    /// <summary>Starts a Ripper-family enemy's state on its slot.</summary>
    private static RipperEnemyState CreateRipperState(RoomEnemySlot slot)
    {
        var state = new RipperEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        return state;
    }

    /// <summary>Rejects movement outside a loaded collision allocation.</summary>
    private static void RequireRipperTerrain([NotNull] RoomLevelData? level, string species)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                $"{species} movement requires the active room collision allocation.");
        }
    }

    /// <summary>Flips a GRipper or Ripper II speed pair and restarts the corresponding loop.</summary>
    internal static void ReverseRipperVariant(
        RoomEnemySlot slot,
        RipperProgram positiveProgram,
        RipperProgram negativeProgram)
    {
        RipperEnemyState state = slot.RequireSpeciesState<RipperEnemyState>();
        bool wasMovingNegative = state.XVelocity.WholePixels < 0;
        LoadRipperVelocity(state, movingRight: wasMovingNegative);
        SetRipperProgram(slot, wasMovingNegative ? positiveProgram : negativeProgram);
    }

    /// <summary>Installs a flying loop from its first step on the next enemy frame.</summary>
    private static void SetRipperProgram(RoomEnemySlot slot, RipperProgram program)
    {
        slot.RequireSpeciesState<RipperEnemyState>().Program = AnimationProgramCursor<RipperProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Advances Ripper's typed animation.</summary>
    internal static void AdvanceRipperTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        RipperEnemyState state = slot.RequireSpeciesState<RipperEnemyState>();
        AnimationProgramCursor<RipperProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, RipperAnimations.Programs, ref cursor, RipperFrameDefinitions.ShownFrames.Of);
        state.Program = cursor;
    }

    /// <summary>Preserves the direction-specific frozen map after common bomb reactions.</summary>
    internal static void AddRipperBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.GRipperRipper2Shot)
        {
            AfterCommon = c => ResolveGRipperRipper2ShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Ports GRipper's main AI.</summary>
    internal void RunGRipperMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGRipperMain(slot, slot.RequireSpeciesState<RipperEnemyState>(), frame.Level);
    }

    /// <summary>Ports Ripper2's main AI.</summary>
    internal void RunRipper2MainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunRipper2Main(slot, slot.RequireSpeciesState<RipperEnemyState>(), frame.Level);
    }

    /// <summary>Ports Ripper's main AI.</summary>
    internal void RunRipperMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunRipperMain(slot, frame.Level);
    }

    /// <summary>Pins GRipper and Ripper II frozen visuals after accepted shot reactions.</summary>
    internal static void AddRipperShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.GRipperRipper2Shot)
        {
            AfterCommon = c => ResolveGRipperRipper2ShotAfterCommon(c.Enemy),
        });
    }
}
