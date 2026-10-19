namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>One Waver occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Waver species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WaverEntity(WaverBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => WaverBehavior.RunWaverInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunWaverMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => WaverBehavior.AdvanceWaverTypedAnimation;
}
