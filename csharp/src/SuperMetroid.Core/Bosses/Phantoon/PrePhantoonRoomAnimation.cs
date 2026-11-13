using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room projectile's typed animation: one blank frame held while the room's setup holds its projectile
/// place (<see cref="PrePhantoonRoomHeldProjectilePlace"/>), then deletion (#641, #1600).
/// </summary>
internal sealed class PrePhantoonRoomAnimation()
    : EnemyProjectileTypedAnimation<PrePhantoonRoomSegment, PrePhantoonRoomFrame, PrePhantoonRoomCue>(BlankThenDelete)
{
    /// <summary>How long the blank frame lasts.</summary>
    private const int HoldDuration = PrePhantoonRoomHeldProjectilePlace.HeldUpdates;

    /// <summary>The blank frame and its deletion.</summary>
    private static readonly AnimationSegments<PrePhantoonRoomSegment, PrePhantoonRoomFrame, PrePhantoonRoomCue> RoomSegments = new(
        new Dictionary<PrePhantoonRoomSegment, AnimationBeat<PrePhantoonRoomFrame, PrePhantoonRoomCue>[]>
        {
            [PrePhantoonRoomSegment.BlankThenDelete] =
            [
                new AnimationBeat<PrePhantoonRoomFrame, PrePhantoonRoomCue>.Frame(default, HoldDuration),
                new AnimationBeat<PrePhantoonRoomFrame, PrePhantoonRoomCue>.Event(PrePhantoonRoomCue.Delete),
            ],
        });

    /// <summary>The blank frame and deletion, played once; the deleted projectile then rests.</summary>
    private static readonly Animation<PrePhantoonRoomSegment> BlankThenDelete =
        new([new(PrePhantoonRoomSegment.BlankThenDelete, 0, 2, 1)], AnimationEnd.Holds);

    /// <summary>The one animation, by the segment it starts at.</summary>
    private static readonly Dictionary<PrePhantoonRoomSegment, Animation<PrePhantoonRoomSegment>> RoomStartingAt = new()
    {
        [PrePhantoonRoomSegment.BlankThenDelete] = BlankThenDelete,
    };

    /// <inheritdoc/>
    private protected override AnimationSegments<PrePhantoonRoomSegment, PrePhantoonRoomFrame, PrePhantoonRoomCue> Segments => RoomSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PrePhantoonRoomSegment, Animation<PrePhantoonRoomSegment>> StartingAt => RoomStartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PrePhantoonRoomFrame frame) =>
        EnemyProjectileFrameId.Blank;
}
