using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// A Morph Ball eye's beam mount (#2164), which shares the eye's definition: it shows its mount pose on its side of the
/// eye and resets the room's beam when it is placed. It neither tracks Samus nor owns the beam, which follows the eye
/// that opens, so its main AI does nothing.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the mount occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
[RoomLifetime]
internal sealed partial class MorphBallEyeMountEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, MorphBallEyeMountState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The MorphBallEye species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the mount occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    internal MorphBallEyeMountEntity(MorphBallEyeBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        MorphBallEyeMountSpawnSettings settings)
        : this(occupant, place, common, species.InitializeMorphBallEyeMount(settings, place))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly MorphBallEyeMountState state = state;

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeMountTypedAnimation(Place, state);
}
