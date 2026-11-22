using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What one beat of a Kraid part's animation shows (#2210): an installed picture, placed where the beat puts it, at the
/// depth the beat draws it. The foot's walk shows each of its five pictures at a run of placements as the foot slides
/// along the floor and lifts, and its collision moves with the picture; the parts Kraid shows while he rises from below
/// the floor sit behind the room's front tiles.
/// </summary>
/// <param name="Picture">The installed picture the part shows.</param>
internal readonly partial record struct KraidPartPose(EnemyFrameId Picture)
{
    /// <summary>
    /// Where the picture sits from where the part's animations first show it: the foot's step along its walk; none for
    /// every other pose. The foot's collision components move with it.
    /// </summary>
    internal Offset16<RoomPixels> Placement { get; init; }

    /// <summary>
    /// The depth the part is drawn at, or null for its picture's own: the arm's swing while Kraid rises or sinks and the
    /// foot's pose before his rise ends sit behind the room's front tiles.
    /// </summary>
    internal ActorDepth? Depth { get; init; }
}
