namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>One Waver occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Waver species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WaverEntity(WaverBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => WaverBehavior.InitializeWaver(Occupant, Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunWaverMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => WaverBehavior.AdvanceWaverTypedAnimation(Place, context);
}
