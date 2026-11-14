namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One Norfair Rio occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Norfair Rio species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairRioEntity(NorfairRioBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeNorfairRio(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairRioMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NorfairRioBehavior.AdvanceNorfairRioTypedAnimation(Slot, context);
}
