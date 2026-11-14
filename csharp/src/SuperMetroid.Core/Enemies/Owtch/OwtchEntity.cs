namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>One Owtch occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Owtch species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class OwtchEntity(OwtchBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => OwtchBehavior.InitializeOwtch(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunOwtchMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => OwtchBehavior.AdvanceOwtchTypedAnimation;

    /// <summary>The Owtch's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.OwtchShot;
}
