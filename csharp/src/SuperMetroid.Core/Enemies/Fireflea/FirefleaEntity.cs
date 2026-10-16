namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>One Fireflea occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Fireflea species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FirefleaEntity(FirefleaBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal FirefleaEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunFirefleaInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => FirefleaBehavior.RunFirefleaMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => FirefleaBehavior.AdvanceFirefleaTypedAnimation;
}
