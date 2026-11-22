using SuperMetroid.Core.Rooms;

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
/// <param name="settings">What the enemy's population entry tells its species.</param>
[RoomLifetime]
internal sealed partial class MorphBallEyeMountEntity(MorphBallEyeBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, MorphBallEyeMountSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation
{
    /// <summary>The <c>species</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
    private readonly MorphBallEyeBehavior species = species;

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal MorphBallEyeMountSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly MorphBallEyeMountState state = species.InitializeMorphBallEyeMount(settings, place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MorphBallEyeBehavior.AdvanceMorphBallEyeMountTypedAnimation(Place, state);
}
