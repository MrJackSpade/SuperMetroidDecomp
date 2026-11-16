namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// A Morph Ball eye's body (#2164): it opens when Samus comes near, tracks her and calls the beam its mount's room links
/// to it. Its placement names it the eye rather than a beam mount (<see cref="MorphBallEyeMountEntity"/>).
/// </summary>
/// <param name="species">The MorphBallEye species owner.</param>
/// <param name="place">The place the eye occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MorphBallEyeEntity(MorphBallEyeBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeMorphBallEyeBody(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMorphBallEyeMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeTypedAnimation(Place, context);
}
