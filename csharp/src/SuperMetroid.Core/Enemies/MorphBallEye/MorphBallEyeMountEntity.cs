namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// A Morph Ball eye's beam mount (#2164), which shares the eye's definition: it shows its mount pose on its side of the
/// eye and resets the room's beam when it is placed. It neither tracks Samus nor owns the beam, which follows the eye
/// that opens, so its main AI does nothing.
/// </summary>
/// <param name="species">The MorphBallEye species owner.</param>
/// <param name="place">The place the mount occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MorphBallEyeMountEntity(MorphBallEyeBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MorphBallEyeMountState State { get; } = species.InitializeMorphBallEyeMount(place);

    /// <summary>A mount only shows its pose: its main AI does nothing.</summary>
    /// <param name="frame">The update's frame, which the mount does not read.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeMountTypedAnimation(Place, State);
}
