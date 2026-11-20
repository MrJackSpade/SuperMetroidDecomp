namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// A Kzan's bottom (#2146), which follows its top and has no animation of its own: its own AI, with the shared reactions.
/// </summary>
/// <param name="species">The Kzan species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the bottom occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KzanBottomEntity(KzanBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeKzanBottom(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKzanBottomMain(Place);
}
