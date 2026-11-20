using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room projectile's typed animation (#641, #1600): one blank frame, shown for good while the room's
/// setup holds its projectile place; the hold itself is counted by the place (<see cref="PrePhantoonRoomHeldProjectilePlace"/>).
/// </summary>
internal sealed class PrePhantoonRoomAnimation()
    : EnemyProjectileSilentAnimation<PrePhantoonRoomSegment, PrePhantoonRoomFrame>(BlankHeld)
{
    /// <summary>The blank frame.</summary>
    private static readonly AnimationSegments<PrePhantoonRoomSegment, PrePhantoonRoomFrame, NoAnimationCue> RoomSegments = new(
        new Dictionary<PrePhantoonRoomSegment, AnimationBeat<PrePhantoonRoomFrame, NoAnimationCue>[]>
        {
            [PrePhantoonRoomSegment.Blank] =
            [
                new AnimationBeat<PrePhantoonRoomFrame, NoAnimationCue>.Frame(default, 1),
            ],
        });

    /// <summary>The blank frame, held once shown.</summary>
    private static readonly Animation<PrePhantoonRoomSegment> BlankHeld =
        new([RoomSegments.Whole(PrePhantoonRoomSegment.Blank)], AnimationEnd.Holds);

    /// <summary>The one animation, by the segment it starts at.</summary>
    private static readonly Dictionary<PrePhantoonRoomSegment, Animation<PrePhantoonRoomSegment>> RoomStartingAt = new()
    {
        [PrePhantoonRoomSegment.Blank] = BlankHeld,
    };

    /// <inheritdoc/>
    private protected override AnimationSegments<PrePhantoonRoomSegment, PrePhantoonRoomFrame, NoAnimationCue> Segments => RoomSegments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<PrePhantoonRoomSegment, Animation<PrePhantoonRoomSegment>>> StartingAt => RoomStartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PrePhantoonRoomFrame frame) =>
        EnemyProjectileFrameId.Blank;
}
