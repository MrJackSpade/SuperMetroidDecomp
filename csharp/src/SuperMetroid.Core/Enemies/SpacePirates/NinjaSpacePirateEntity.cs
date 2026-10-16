namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One NinjaSpacePirate occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The NinjaSpacePirate species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NinjaSpacePirateEntity(NinjaSpacePirateBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal NinjaSpacePirateEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunNinjaSpacePirateInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunNinjaSpacePirateMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceNinjaSpacePirateTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback) => SpacePirateHitboxes.Find(Slot, probe, out callback);
}
