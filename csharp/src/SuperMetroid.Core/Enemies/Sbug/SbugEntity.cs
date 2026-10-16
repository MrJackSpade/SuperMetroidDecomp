namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>One Sbug occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Sbug species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SbugEntity(SbugBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal SbugEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => SbugBehavior.RunSbugInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunSbugMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => SbugBehavior.AdvanceSbugTypedAnimation;
}
