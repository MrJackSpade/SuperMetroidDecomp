namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>One Beetom occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Beetom species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BeetomEntity(BeetomBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemySurroundingsInitializationAiHandler? FindSurroundingsInitialization() => species.RunBeetomInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunBeetomMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => BeetomBehavior.AdvanceBeetomTypedAnimation;

    /// <summary>The Beetom's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.BeetomShot;
}
