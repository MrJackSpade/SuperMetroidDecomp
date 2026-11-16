namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One Norfair Rio occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Norfair Rio species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairRioEntity(NorfairRioBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeNorfairRio(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairRioMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NorfairRioBehavior.AdvanceNorfairRioTypedAnimation(Place, context);
}
