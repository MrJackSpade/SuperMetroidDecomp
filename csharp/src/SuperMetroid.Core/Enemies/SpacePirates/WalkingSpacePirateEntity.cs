namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One WalkingSpacePirate occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The WalkingSpacePirate species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WalkingSpacePirateEntity(WalkingSpacePirateBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal WalkingSpacePirateEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => WalkingSpacePirateBehavior.RunWalkingSpacePirateInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunWalkingSpacePirateMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceWalkingSpacePirateTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback) => SpacePirateHitboxes.Find(Slot, probe, out callback);
}
