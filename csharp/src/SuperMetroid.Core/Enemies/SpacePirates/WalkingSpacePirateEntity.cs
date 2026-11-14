namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One walking Space Pirate occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The walking Space Pirate species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WalkingSpacePirateEntity(WalkingSpacePirateBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => WalkingSpacePirateBehavior.InitializeWalkingSpacePirate(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunWalkingSpacePirateMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceWalkingSpacePirateTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => SpacePirateHitboxes.Find(Slot, probe, out reaction);

    /// <summary>Pirate contact and shots both meet the Pirate's engine-owned frame hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The Pirate shot reactions, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        SpacePirateHitboxes.OwnsShotReaction(reaction);
}
