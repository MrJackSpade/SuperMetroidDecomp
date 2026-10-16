namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One Choot occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Choot species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChootEntity(ChootBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal ChootEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunChootInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => ChootBehavior.RunChootMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => ChootBehavior.AdvanceChootTypedAnimation;
}
