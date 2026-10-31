namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One Choot occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChootEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => ChootBehavior.InitializeChoot;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => ChootBehavior.RunChootMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => ChootBehavior.AdvanceChootTypedAnimation;
}
