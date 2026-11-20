namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Geruta parent (#2164): it waits on its perch and dives at Samus, its animation's cues placing the flames of the
/// follower its room lists right after it (<see cref="GerutaFollowerEntity"/>).
/// </summary>
/// <param name="species">The Geruta species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GerutaEntity(GerutaBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => GerutaBehavior.InitializeGeruta(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunGerutaMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GerutaBehavior.AdvanceGerutaTypedAnimation(Place);
}
