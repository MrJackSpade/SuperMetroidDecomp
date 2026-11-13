using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One Botwoon occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Botwoon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BotwoonEntity(BotwoonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeBotwoon;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunBotwoonMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceBotwoonTypedAnimation;

    /// <summary>Botwoon's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.BotwoonShot;
}
