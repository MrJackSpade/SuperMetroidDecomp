namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Norfair Rio parent (#2164): it waits on its perch and dives at Samus, its animation's cues placing the flames of the
/// follower its room lists right after it (<see cref="NorfairRioFollowerEntity"/>).
/// </summary>
/// <param name="species">The Norfair Rio species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairRioEntity(NorfairRioBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => NorfairRioBehavior.InitializeNorfairRio(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairRioMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NorfairRioBehavior.AdvanceNorfairRioTypedAnimation(Place);
}
