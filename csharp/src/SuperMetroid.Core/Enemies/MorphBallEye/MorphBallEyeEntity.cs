namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>One MorphBallEye occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The MorphBallEye species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MorphBallEyeEntity(MorphBallEyeBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeMorphBallEye(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMorphBallEyeMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeTypedAnimation(Place, context);
}
