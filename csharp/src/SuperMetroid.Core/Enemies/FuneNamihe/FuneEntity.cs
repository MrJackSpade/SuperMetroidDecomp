namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One Fune occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fune/Namihe species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FuneEntity(FuneNamiheBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => FuneNamiheBehavior.InitializeFune(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => FuneNamiheBehavior.RunFuneMain(Place, Place.RequireSpeciesState<FuneEnemyState>());

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceFuneNamiheTypedAnimation<FuneEnemyState>(Place);
}
