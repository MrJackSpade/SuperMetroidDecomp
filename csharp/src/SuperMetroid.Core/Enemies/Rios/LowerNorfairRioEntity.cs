namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Lower Norfair Rio parent (#2164): it waits on its perch and dives at Samus, its animation's cues showing and hiding
/// the flames of the follower its room lists right after it (<see cref="LowerNorfairRioFollowerEntity"/>).
/// </summary>
/// <param name="species">The Lower Norfair Rio species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class LowerNorfairRioEntity(LowerNorfairRioBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => LowerNorfairRioBehavior.InitializeLowerNorfairRio(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunLowerNorfairRioMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => LowerNorfairRioBehavior.AdvanceLowerNorfairRioTypedAnimation(Place, context);
}
