namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One Fune occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fune/Namihe species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FuneEntity(FuneNamiheBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => FuneNamiheBehavior.InitializeFune(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => FuneNamiheBehavior.RunFuneMain(Slot, Slot.RequireSpeciesState<FuneEnemyState>());

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceFuneNamiheTypedAnimation<FuneEnemyState>(Slot);
}
