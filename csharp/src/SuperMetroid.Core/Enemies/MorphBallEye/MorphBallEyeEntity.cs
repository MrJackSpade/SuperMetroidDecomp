namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// A Morph Ball eye's body (#2164): it opens when Samus comes near, tracks her and fires the room's beam, which follows
/// the eye that opens. Its placement names it the eye rather than a beam mount (<see cref="MorphBallEyeMountEntity"/>).
/// </summary>
/// <param name="species">The MorphBallEye species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the eye occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MorphBallEyeEntity(MorphBallEyeBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => MorphBallEyeBehavior.InitializeMorphBallEyeBody(Occupant, Occupant.SpawnSettings<MorphBallEyeBodySpawnSettings>(), Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMorphBallEyeMainAi(Occupant, Occupant.SpawnSettings<MorphBallEyeBodySpawnSettings>(), Place, Place.RequireSpeciesState<MorphBallEyeEnemyState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeTypedAnimation(Place, Place.RequireSpeciesState<MorphBallEyeEnemyState>());
}
