namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One WallSpacePirate occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The WallSpacePirate species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WallSpacePirateEntity(WallSpacePirateBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => WallSpacePirateBehavior.InitializeWallSpacePirate;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => WallSpacePirateBehavior.RunWallSpacePirateMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceWallSpacePirateTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback) => SpacePirateHitboxes.Find(Slot, probe, out callback);

    /// <summary>Pirate contact and shots both enter the extended-spritemap walker over the Pirate's engine-owned hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes;

    /// <summary>The Pirate shot callbacks, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        SpacePirateHitboxes.OwnsShotCallback(callback);
}
