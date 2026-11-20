using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room projectile's typed animation (#641, #1600): it shows nothing, hidden for good while the room's
/// setup holds its projectile place; the hold itself is counted by the place (<see cref="PrePhantoonRoomHeldProjectilePlace"/>).
/// </summary>
internal sealed class PrePhantoonRoomAnimation()
    : EnemyProjectileSilentAnimation<PrePhantoonRoomSegment, EnemyProjectileFrameId>(BlankHeld)
{
    /// <summary>The updates the projectile's one hidden beat lasts before the animation holds it hidden for good.</summary>
    private const int HiddenUpdates = 1;

    /// <summary>The one update's hiding, after which the animation holds.</summary>
    private static readonly HidingUnpausedAnimationSegments<PrePhantoonRoomSegment, EnemyProjectileFrameId, NoAnimationCue> RoomSegments = new(
        new Dictionary<PrePhantoonRoomSegment, IUnpausedHidingAnimationBeat<EnemyProjectileFrameId, NoAnimationCue>[]>
        {
            [PrePhantoonRoomSegment.Blank] =
            [
                new AnimationHiddenBeat<EnemyProjectileFrameId, NoAnimationCue>(HiddenUpdates),
            ],
        });

    /// <summary>The hiding, held once played.</summary>
    private static readonly Animation<PrePhantoonRoomSegment> BlankHeld =
        new([RoomSegments.Whole(PrePhantoonRoomSegment.Blank)], AnimationEnd.Holds);

    /// <summary>The one animation, by the segment it starts at.</summary>
    private static readonly Dictionary<PrePhantoonRoomSegment, Animation<PrePhantoonRoomSegment>> RoomStartingAt = new()
    {
        [PrePhantoonRoomSegment.Blank] = BlankHeld,
    };

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<PrePhantoonRoomSegment, EnemyProjectileFrameId, NoAnimationCue> Segments => RoomSegments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<PrePhantoonRoomSegment, Animation<PrePhantoonRoomSegment>>> StartingAt => RoomStartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EnemyProjectileFrameId frame) => frame;
}
