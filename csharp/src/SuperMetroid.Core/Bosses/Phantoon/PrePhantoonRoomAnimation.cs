using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room projectile's typed animation (#641, #1600): it shows nothing, hidden for good while the room's
/// setup holds its projectile place; the hold itself is counted by the place (<see cref="PrePhantoonRoomHeldProjectilePlace"/>).
/// </summary>
internal sealed class PrePhantoonRoomAnimation()
    : EnemyProjectileTypedAnimation<PrePhantoonRoomSegment, EnemyProjectileFrameId, NoAnimationCue>(BlankHeld)
{
    /// <summary>The one update's hiding, after which the animation holds.</summary>
    private static readonly AnimationSegments<PrePhantoonRoomSegment, EnemyProjectileFrameId, NoAnimationCue> RoomSegments = new(
        new Dictionary<PrePhantoonRoomSegment, AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>[]>
        {
            [PrePhantoonRoomSegment.Blank] =
            [
                new AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.Hidden(1),
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
    private protected override AnimationSegments<PrePhantoonRoomSegment, EnemyProjectileFrameId, NoAnimationCue> Segments => RoomSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PrePhantoonRoomSegment, Animation<PrePhantoonRoomSegment>> StartingAt => RoomStartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EnemyProjectileFrameId frame) => frame;
}
