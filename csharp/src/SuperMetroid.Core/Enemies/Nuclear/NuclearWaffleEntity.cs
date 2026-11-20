namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One Nuclear Waffle occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Nuclear Waffle species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NuclearWaffleEntity(NuclearWaffleBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeNuclearWaffle(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNuclearWaffleMainAi(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NuclearWaffleBehavior.AdvanceNuclearWaffleTypedAnimation(Place);
}
