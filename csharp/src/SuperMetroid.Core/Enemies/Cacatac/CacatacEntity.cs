namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One Cacatac occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Cacatac species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CacatacEntity(CacatacBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => CacatacBehavior.InitializeCacatac(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunCacatacMainAi(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceCacatacTypedAnimation(Place);
}
