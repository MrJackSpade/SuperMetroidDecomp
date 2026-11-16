using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of Mother Brain's occupants: her body (<see cref="MotherBrainBodyEntity"/>), her head
/// (<see cref="MotherBrainHeadEntity"/>) or the falling tubes of her fake death (<see cref="MotherBrainTubesEntity"/>).
/// Every part answers her engine-owned hitboxes; each runs its own AI and animation.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class MotherBrainEntity(MotherBrainBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <summary>The MotherBrain species owner.</summary>
    protected MotherBrainBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <summary>None: the body and tubes name shared touch reactions.</summary>
    public virtual EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the tubes name a shared shot reaction.</summary>
    public virtual EnemyReaction? SpeciesShot => null;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => MotherBrainCombat.FindMotherBrainHitboxReaction(Place, probe, out reaction);
}
