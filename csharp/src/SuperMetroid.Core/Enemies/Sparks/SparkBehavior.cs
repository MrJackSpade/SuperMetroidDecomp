using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.FallingSparks;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The Spark species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SparkBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The pass-through shot reaction for this species' private shot reaction.</summary>
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

    /// <summary>Owner of the falling-spark projectile emitted by one spark action.</summary>
    private readonly FallingSparksBehavior _fallingSparksBehavior;

    /// <summary>Binds the spark species to its falling-spark producer.</summary>
    internal SparkBehavior(FallingSparksBehavior fallingSparksBehavior) => _fallingSparksBehavior = fallingSparksBehavior;

    /// <summary>Carries out one cue of Spark's animations.</summary>
    private static void ApplySparkCue(RoomEnemySlot slot, SparkCue cue)
    {
        switch (cue.Kind)
        {
            case SparkCueKind.BecomeIntangible:
                // Flicker-out: the intangibility property removes the actor from every ordinary Samus, beam and grapple
                // collision pass.
                slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
                break;
            case SparkCueKind.BecomeTangible:
                // Flicker-on runs this before the first visible activation frame, so collision and art become live
                // together.
                slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SparkCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initializes the Spark.</summary>
    internal static void InitializeSpark(RoomEnemySlot slot)
    {
        var state = new SparkEnemyState();
        slot.SetSpeciesState(state);

        SparkMovementDefinition definition = slot.SpawnSettings<SparkSpawnSettings>().Start;
        state.Action = definition.Action;
        state.BaseActionTime = slot.SpawnSettings<SparkSpawnSettings>().BaseActionTime;
        SetSparkActionTimer(state, additionalTime: 0);

        slot.AnimationClock.StepOnNextUpdate();
        state.Playback = new(SparkAnimations.StartingWith(definition.Animation), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.ClearRepeats();
        // Spark keeps its population properties: the initializer's extra property change before Phantoon's
        // defeat is exempt under the emulation scope limits (owner decision on #1318).
    }

    /// <summary>Runs the indirect main dispatcher.</summary>
    private void RunSparkMain(RoomEnemySlot slot, SparkEnemyState state)
    {
        switch (state.Action)
        {
            case SparkAction.AlwaysActive:
                return;

            case SparkAction.IntermittentInactive:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                state.Action = SparkAction.IntermittentActive;
                StartSparkAnimation(slot, state, SparkSegment.FlickerOn);
                SetSparkActionTimer(state, additionalTime: 0);
                return;

            case SparkAction.IntermittentActive:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                state.Action = SparkAction.IntermittentInactive;
                StartSparkAnimation(slot, state, SparkSegment.FlickerOut);
                SetSparkActionTimer(state, additionalTime: 8);
                return;

            case SparkAction.EmitFallingSparks:
                if (!DecrementSparkTimerAndTestExpired(state))
                    return;

                _fallingSparksBehavior.SpawnFallingSpark(slot);
                SetSparkActionTimer(state, additionalTime: 0);
                return;

            case null:
            default:
                throw new InvalidDataException(
                    $"Spark has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>
    /// Expires on the update whose step would reach zero, leaving the countdown at one for its reset. A countdown
    /// already at zero steps below zero instead and does not expire; it must not be treated as immediately due.
    /// </summary>
    internal static bool DecrementSparkTimerAndTestExpired(SparkEnemyState state)
    {
        if (state.ActionTimer == 1)
            return true;

        state.ActionTimer--;
        return false;
    }

    /// <summary>Sets the Spark's action timer.</summary>
    private static void SetSparkActionTimer(SparkEnemyState state, int additionalTime)
    {
        state.ActionTimer = state.BaseActionTime + additionalTime;
    }

    /// <summary>Selects an animation and makes its first step due on the next pass.</summary>
    private static void StartSparkAnimation(RoomEnemySlot slot, SparkEnemyState state, SparkSegment segment)
    {
        state.Playback = new(SparkAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs Spark's main AI.</summary>
    internal void RunSparkMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunSparkMain(slot, slot.RequireSpeciesState<SparkEnemyState>());
    }

    /// <summary>Advances Spark's typed animation.</summary>
    internal static void AdvanceSparkTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        SparkEnemyState state = slot.RequireSpeciesState<SparkEnemyState>();
        AnimationPlayback<SparkSegment> playback = state.Playback ?? throw new InvalidOperationException(
            "Spark animation runs before the Spark is initialized.");
        EnemyAnimationPlayback.Play(slot, SparkAnimations.Segments, ref playback,
            cue =>
            {
                ApplySparkCue(slot, cue);
                return null;
            });
        state.Playback = playback;
    }
}
