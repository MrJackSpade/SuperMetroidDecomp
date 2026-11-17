using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of Mother Brain's occupants: her body (<see cref="MotherBrainBodyEntity"/>), her head
/// (<see cref="MotherBrainHeadEntity"/>) or the falling tubes of her fake death (<see cref="MotherBrainTubesEntity"/>).
/// Every part answers her hitboxes; each runs its own AI and animation.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class MotherBrainEntity(MotherBrainBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <summary>The MotherBrain species owner.</summary>
    protected MotherBrainBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <summary>The encounter the part is part of, which it holds from its setup (#2218).</summary>
    /// <exception cref="InvalidOperationException">The part has not set up.</exception>
    protected MotherBrainEnemyState Encounter => Place.RequireSpeciesState<MotherBrainEnemyState>();

    /// <summary>The encounter <paramref name="part"/> is part of, which the part holds (#2218).</summary>
    /// <param name="part">A place holding one of Mother Brain's parts, set up.</param>
    /// <exception cref="InvalidOperationException">The place holds no part of Mother Brain's that has set up.</exception>
    internal static MotherBrainEnemyState EncounterOf(RoomEnemyPlace part) =>
        Ensure.NotNull(part).Entity is MotherBrainEntity entity
            ? entity.Encounter
            : throw new InvalidOperationException($"Enemy place {part.Ordinal} holds no part of Mother Brain's.");

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <summary>None: the body and tubes name shared touch reactions.</summary>
    public virtual EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the tubes name a shared shot reaction.</summary>
    public virtual EnemyReaction? SpeciesShot => null;

    /// <summary>None: only her body and her head have hitboxes of their own, which their entities find (#2301).</summary>
    public virtual bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return null;
    }
}
