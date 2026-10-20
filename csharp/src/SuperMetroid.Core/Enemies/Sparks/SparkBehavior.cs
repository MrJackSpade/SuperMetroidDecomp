using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Sparks.Artwork;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The Spark species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class SparkBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The pass-through shot reaction for this species' private shot callback.</summary>
    internal static EnemyShotReaction PassThroughShot { get; } =
        new(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Spark &&
                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.SparkShot)
        {
            AfterSuperMissileQuake = ProjectilePassThrough.Shot,
        };

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Spark];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new SparkEntity(this, slot, common);

    /// <summary>Room services for the live RNG, boss flag, and memory bus consulted during spark setup.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Owner of the falling-spark projectile emitted by one spark function.</summary>
    private readonly FallingSparksBehavior _fallingSparksBehavior;

    /// <summary>Binds the spark species to its room services and falling-spark producer.</summary>
    internal SparkBehavior(EnemyRoomBinding room, FallingSparksBehavior fallingSparksBehavior)
    {
        _fallingSparksBehavior = fallingSparksBehavior;
        _room = room;
    }

    /// <summary>Carries out one cue of Spark's animations.</summary>
    private static SparkProgram? ApplySparkCue(RoomEnemySlot slot, SparkCue cue)
    {
        switch (cue.Kind)
        {
            case SparkCueKind.BecomeIntangible:
                // Flicker-out: property bit $0400 removes the actor from every ordinary Samus, beam and grapple
                // collision pass.
                slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                break;
            case SparkCueKind.BecomeTangible:
                // Flicker-on runs this before the first visible activation frame, so collision and art become live
                // together.
                slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SparkCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Ports <c>InitAI_Spark</c> at <c>$A8:E637</c>.</summary>
    internal void InitializeSpark(RoomEnemySlot slot)
    {
        var state = new SparkEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Native masks the selector to two bits, but both tables contain only three authored
        // words. The compiled definition preserves selector three's two adjacent-code reads;
        // a later main pass still reports its exact nonsensical function if selected.
        SparkMovementDefinition definition = SparkMovementDefinitions.InitialState(checked((ushort)slot.Spawn.Population.Parameter1));
        state.Function = definition.Function;
        state.BaseFunctionTime = slot.Spawn.Population.Parameter2;
        SetSparkFunctionTimer(state, additionalTime: 0);

        slot.InstructionTimer = NativeCountdown.Start(1);
        state.Program = definition.Program is { } program ? AnimationProgramCursor<SparkProgram>.Start(program) : null;
        slot.Timer = NativeCountdown.Start(0);

        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
            return;

        // This is intentionally *not* `ORA #$0100`, which would force invisibility. The retail
        // instruction is the absolute-memory form `ORA $0100`, a disassembly-noted bug; see
        // SparkMovementDefinitions.StackPagePropertyOverlay for the exempted value it ORs.
        slot.Properties = EnemyPropertyWord.FromNativeWord((ushort)(slot.Properties.ToNativeWord() | SparkMovementDefinitions.StackPagePropertyOverlay));
    }

    /// <summary>Ports the indirect main dispatcher at <c>$A8:E68E</c>.</summary>
    private void RunSparkMain(RoomEnemySlot slot, SparkEnemyState state)
    {
        switch (state.Function)
        {
            case SparkEnemyFunction.AlwaysActive:
                return;

            case SparkEnemyFunction.IntermittentInactive:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                state.Function = SparkEnemyFunction.IntermittentActive;
                InstallSparkProgram(slot, state, SparkProgram.FlickerOn);
                SetSparkFunctionTimer(state, additionalTime: 0);
                return;

            case SparkEnemyFunction.IntermittentActive:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                state.Function = SparkEnemyFunction.IntermittentInactive;
                InstallSparkProgram(slot, state, SparkProgram.FlickerOut);
                SetSparkFunctionTimer(state, additionalTime: 8);
                return;

            case SparkEnemyFunction.EmitFallingSparks:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                _fallingSparksBehavior.SpawnFallingSpark(slot);
                SetSparkFunctionTimer(state, additionalTime: 0);
                return;

            default:
                throw new InvalidDataException(
                    $"Spark function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Reproduces <c>DEC; BEQ</c>. A zero countdown wraps to <c>$FFFF</c> and therefore does
    /// not expire for another 65,535 actor frames; it must not be treated as immediately due.
    /// </summary>
    internal static bool DecrementSparkTimerAndTestExpired(SparkEnemyState state)
    {
        NativeWordCounterStep step = state.FunctionTimer.Decrement();
        if (step.IsZero)
            return true;

        state.FunctionTimer = step.Remaining;
        return false;
    }

    /// <summary>Ports <c>SetSparkFunctionTimer</c> at <c>$A8:E6F6</c>.</summary>
    private void SetSparkFunctionTimer(SparkEnemyState state, ushort additionalTime)
    {
        int baseTime = state.BaseFunctionTime;
        if (unchecked((short)baseTime) < 0)
        {
            Func<int> nextRandom = _room.Random.Generate;
            baseTime = (nextRandom() & 63) + 4;
        }

        state.FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(baseTime + additionalTime)));
    }

    /// <summary>Selects an animation and makes its first step due on the next pass.</summary>
    private static void InstallSparkProgram(RoomEnemySlot slot, SparkEnemyState state, SparkProgram program)
    {
        state.Program = AnimationProgramCursor<SparkProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Returns initialized state for the enemy slot or reports a broken lifecycle.</summary>
    internal static SparkEnemyState RequireSparkState(RoomEnemySlot slot) =>
        slot.SpeciesState<SparkEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Spark state.");

    /// <summary>Ports Spark's initialization AI.</summary>
    internal void RunSparkInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeSpark(slot);
    }

    /// <summary>Ports Spark's main AI.</summary>
    internal void RunSparkMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunSparkMain(slot, RequireSparkState(slot));
    }

    /// <summary>Advances Spark's typed animation.</summary>
    internal static void AdvanceSparkTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        SparkEnemyState state = RequireSparkState(slot);
        // Selector three's list word is the main-AI function pointer that follows the table: code, not a list.
        AnimationProgramCursor<SparkProgram> cursor = state.Program ?? throw new InvalidDataException(
            "Spark selector three starts at the main-AI function pointer, which is not an animation list.");
        EnemyProgramAnimation.Run(slot, SparkAnimations.Programs, ref cursor, SparkFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplySparkCue(slot, cue));
        state.Program = cursor;
    }
}
