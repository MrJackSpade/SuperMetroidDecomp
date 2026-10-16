namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One Cacatac occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Cacatac species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CacatacEntity(CacatacBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal CacatacEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => CacatacBehavior.RunCacatacInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunCacatacMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceCacatacTypedAnimation;
}
