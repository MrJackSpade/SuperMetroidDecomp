using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Rinka occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Rinka species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RinkaEntity(RinkaBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeRinka(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunRinkaMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => RinkaBehavior.AdvanceRinkaTypedAnimation;

    /// <summary>The Rinka's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.RinkaShot;
}
