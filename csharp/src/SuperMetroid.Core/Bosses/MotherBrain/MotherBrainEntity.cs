using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of Mother Brain's occupants: her body (<see cref="MotherBrainBodyEntity"/>), her head
/// (<see cref="MotherBrainHeadEntity"/>) or the falling tubes of her fake death (<see cref="MotherBrainTubesEntity"/>).
/// Every part answers her hitboxes; each runs its own AI and animation.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract partial class MotherBrainEntity(MotherBrainBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <summary>The MotherBrain species owner.</summary>
    protected MotherBrainBehavior Species => species;

    /// <summary>The encounter the part is part of, which it holds from its setup (#2218, #2554).</summary>
    internal abstract MotherBrainEnemyState Encounter { get; }

    /// <summary>The encounter <paramref name="part"/> is part of, which the part holds (#2218).</summary>
    /// <param name="part">A place holding one of Mother Brain's parts, set up.</param>
    /// <exception cref="InvalidOperationException">The place holds no part of Mother Brain's that has set up.</exception>
    internal static MotherBrainEnemyState EncounterOf(RoomEnemyPlace part) =>
        Ensure.NotNull(part).Entity is MotherBrainEntity entity
            ? entity.Encounter
            : throw new InvalidOperationException($"Enemy place {part.Ordinal} holds no part of Mother Brain's.");

    /// <summary>Takes a shot meeting her body, in the encounter the part is part of (<see cref="MotherBrainCombat.ReactToBodyShot"/>).</summary>
    /// <param name="c">The shot meeting the body.</param>
    internal bool ReactToBodyShot(in EnemyShotContext c) => Species.Combat.ReactToBodyShot(c, Encounter);

    /// <summary>Takes a shot meeting her head, in the encounter the part is part of (<see cref="MotherBrainCombat.ReactToHeadShot"/>).</summary>
    /// <param name="c">The shot meeting the head.</param>
    internal bool ReactToHeadShot(in EnemyShotContext c) => Species.Combat.ReactToHeadShot(c, Encounter);

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <summary>None: only her body and her head have hitboxes of their own, which their entities find (#2301).</summary>
    public virtual EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
    {
        throw EnemyCollision.UncoveredFrame(Place);
    }
}
