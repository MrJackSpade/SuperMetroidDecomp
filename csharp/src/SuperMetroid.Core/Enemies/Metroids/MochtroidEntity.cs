namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Mochtroid occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Mochtroid species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MochtroidEntity(MochtroidBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => MochtroidBehavior.InitializeMochtroid(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMochtroidMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => MochtroidBehavior.AdvanceMochtroidTypedAnimation;

    /// <summary>The Mochtroid's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.MochtroidShot;
}
