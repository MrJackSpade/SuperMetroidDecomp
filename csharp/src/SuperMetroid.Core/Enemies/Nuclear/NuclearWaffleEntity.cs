namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One Nuclear Waffle occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Nuclear Waffle species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NuclearWaffleEntity(NuclearWaffleBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal NuclearWaffleEnemyState State { get; } = species.InitializeNuclearWaffle(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNuclearWaffleMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NuclearWaffleBehavior.AdvanceNuclearWaffleTypedAnimation(Place, context);
}
