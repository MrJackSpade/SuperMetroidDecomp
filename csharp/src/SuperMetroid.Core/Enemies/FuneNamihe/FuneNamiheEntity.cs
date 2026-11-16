namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One FuneNamihe occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The FuneNamihe species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FuneNamiheEntity(FuneNamiheBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => FuneNamiheBehavior.InitializeFuneNamihe(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => FuneNamiheBehavior.RunFuneNamiheMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceFuneNamiheTypedAnimation(Place, context);
}
