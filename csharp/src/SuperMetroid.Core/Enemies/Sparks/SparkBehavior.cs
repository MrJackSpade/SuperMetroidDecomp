using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Sparks.Artwork;
using SuperMetroid.Core.Enemies.FallingSparks;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The Spark species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SparkBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The pass-through shot reaction for this species' private shot callback.</summary>
    internal static EnemyShotReaction PassThroughShot { get; } =
        new(enemy => enemy.DefinitionId == EnemyDefinitionId.Spark &&
                     EnemyCollision.ShotAi(enemy) == EnemyReaction.SparkShot)
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
                // Flicker-out: the intangibility property removes the actor from every ordinary Samus, beam and grapple
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

    /// <summary>Initializes the Spark.</summary>
    internal void InitializeSpark(RoomEnemySlot slot)
    {
        var state = new SparkEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        SparkMovementDefinition definition = SparkStarts.InitialState(slot.SpawnSettings<SparkSpawnSettings>().InitialState);
        state.Function = definition.Function;
        state.BaseFunctionTime = slot.SpawnSettings<SparkSpawnSettings>().BaseFunctionTime;
        SetSparkFunctionTimer(state, additionalTime: 0);

        slot.FrameHold = WrappingCountdown.Start(1);
        state.Program = AnimationProgramCursor<SparkProgram>.Start(definition.Program);
        slot.Timer = WrappingCountdown.Start(0);
        // Spark keeps its population properties: the original initializer's extra property change before Phantoon's
        // defeat is exempt under the emulation scope limits (owner decision on #1318).
    }

    /// <summary>Runs the indirect main dispatcher.</summary>
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

            case null:
            default:
                throw new InvalidDataException(
                    $"Spark function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Reproduces the decrement-then-test-zero countdown. A zero countdown wraps to -1 and therefore does
    /// not expire for another 65,535 actor frames; it must not be treated as immediately due.
    /// </summary>
    internal static bool DecrementSparkTimerAndTestExpired(SparkEnemyState state)
    {
        CountdownStep step = state.FunctionTimer.Decrement();
        if (step.IsZero)
            return true;

        state.FunctionTimer = step.Remaining;
        return false;
    }

    /// <summary>Sets the Spark's function timer.</summary>
    private void SetSparkFunctionTimer(SparkEnemyState state, int additionalTime)
    {
        int baseTime = state.BaseFunctionTime;
        if (WordWrap.Signed(baseTime) < 0)
        {
            Func<int> nextRandom = _room.Random.Generate;
            baseTime = (nextRandom() & 63) + 4;
        }

        state.FunctionTimer = WrappingCountdown.FromRaw(baseTime).AddFrames(additionalTime);
    }

    /// <summary>Selects an animation and makes its first step due on the next pass.</summary>
    private static void InstallSparkProgram(RoomEnemySlot slot, SparkEnemyState state, SparkProgram program)
    {
        state.Program = AnimationProgramCursor<SparkProgram>.Start(program);
        slot.FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>Ports Spark's main AI.</summary>
    internal void RunSparkMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunSparkMain(slot, slot.RequireSpeciesState<SparkEnemyState>());
    }

    /// <summary>Advances Spark's typed animation.</summary>
    internal static void AdvanceSparkTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        SparkEnemyState state = slot.RequireSpeciesState<SparkEnemyState>();
        AnimationProgramCursor<SparkProgram> cursor = state.Program ?? throw new InvalidOperationException(
            "Spark animation runs before the Spark is initialized.");
        EnemyProgramAnimation.Run(slot, SparkAnimations.Programs, ref cursor, SparkFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplySparkCue(slot, cue));
        state.Program = cursor;
    }
}
