namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// A Morph Ball eye's body (#2164): it opens when Samus comes near, tracks her and fires the room's beam, which follows
/// the eye that opens. Its placement names it the eye rather than a beam mount (<see cref="MorphBallEyeMountEntity"/>).
/// </summary>
/// <param name="species">The MorphBallEye species owner.</param>
/// <param name="place">The place the eye occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class MorphBallEyeEntity(MorphBallEyeBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, MorphBallEyeBodySpawnSettings settings)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MorphBallEyeEnemyState State { get; } = MorphBallEyeBehavior.InitializeMorphBallEyeBody(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMorphBallEyeMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeTypedAnimation(Place, State, context);
}
