namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One Kago occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KagoEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => KagoBehavior.RunKagoInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => KagoBehavior.RunKagoMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => KagoBehavior.AdvanceKagoTypedAnimation;
}
