namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>One Skultera occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Skultera species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SkulteraEntity(SkulteraBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal SkulteraEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunSkulteraInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunSkulteraMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => SkulteraBehavior.AdvanceSkulteraTypedAnimation;
}
