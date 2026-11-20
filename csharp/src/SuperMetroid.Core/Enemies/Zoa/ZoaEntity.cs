namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>One Zoa occupant: its own AI, with the shared reactions.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ZoaEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => ZoaBehavior.InitializeZoa(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ZoaBehavior.RunZoaMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ZoaBehavior.AdvanceZoaAnimation(Place);
}
