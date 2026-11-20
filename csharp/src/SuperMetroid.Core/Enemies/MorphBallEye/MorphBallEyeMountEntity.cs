namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// A Morph Ball eye's beam mount (#2164), which shares the eye's definition: it shows its mount pose on its side of the
/// eye and resets the room's beam when it is placed. It neither tracks Samus nor owns the beam, which follows the eye
/// that opens, so its main AI does nothing.
/// </summary>
/// <param name="species">The MorphBallEye species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the mount occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MorphBallEyeMountEntity(MorphBallEyeBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeMorphBallEyeMount(Occupant, Place);

    /// <summary>A mount only shows its pose: its main AI does nothing.</summary>
    /// <param name="frame">The update's frame, which the mount does not read.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeMountTypedAnimation(Place);
}
