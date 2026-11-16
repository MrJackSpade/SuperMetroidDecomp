namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One Namihe occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fune/Namihe species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NamiheEntity(FuneNamiheBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => FuneNamiheBehavior.InitializeNamihe(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        FuneNamiheBehavior.RunNamiheMain(Place, Place.RequireSpeciesState<NamiheEnemyState>(), Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceFuneNamiheTypedAnimation<NamiheEnemyState>(Place);
}
