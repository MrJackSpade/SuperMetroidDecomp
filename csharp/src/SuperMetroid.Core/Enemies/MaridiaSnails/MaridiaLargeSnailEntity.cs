namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>One MaridiaLargeSnail occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MaridiaLargeSnail species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MaridiaLargeSnailEntity(MaridiaLargeSnailBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MaridiaLargeSnailBehavior.RunMaridiaLargeSnailInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMaridiaLargeSnailMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceMaridiaLargeSnailTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback) => MaridiaLargeSnailBehavior.FindHitboxCallback(Slot, probe, out callback);

    /// <summary>The large snail's contact and shots both enter the extended-spritemap walker over its own hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes;

    /// <summary>The large snail's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback == EnemyInteractionCallback.MaridiaLargeSnailShot;
}
