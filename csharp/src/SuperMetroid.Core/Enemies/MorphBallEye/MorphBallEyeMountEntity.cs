namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// A Morph Ball eye's beam mount (#2164), which shares the eye's definition: it shows its mount pose on its side of the
/// eye, resets the room's beam when it is placed and links the beam to the eye listed right after it. It neither tracks
/// Samus nor owns the beam, so its main AI does nothing.
/// </summary>
/// <param name="species">The MorphBallEye species owner.</param>
/// <param name="place">The place the mount occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MorphBallEyeMountEntity(MorphBallEyeBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeMorphBallEyeMount(Place);

    /// <summary>A mount only shows its pose: its main AI does nothing.</summary>
    /// <param name="frame">The update's frame, which the mount does not read.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeMountTypedAnimation(Place);
}
