using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Shitroid occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Shitroid species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShitroidEntity(ShitroidBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize(EnemySpawnSurroundings surroundings) => species.RunShitroidInitializationAi(Slot, surroundings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunShitroidMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => (enemy, _) => species.AdvanceShitroidAnimation(enemy);

    /// <summary>The Baby Metroid's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.ShitroidShot;
}
