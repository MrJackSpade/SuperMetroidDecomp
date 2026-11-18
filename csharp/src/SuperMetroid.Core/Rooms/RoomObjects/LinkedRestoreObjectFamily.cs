using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The linked block restorations of bomb blocks and Samus-contact crumble blocks. Each kind has a restoration for each
/// arrangement but a single block, side by side, stacked and square, that rebuilds a parent block and its linked
/// children. The kinds differ only in the parent's collision kind; the children and their links are the same. What each
/// restoration shows is its installed picture (#1557); what it changes is its block shape (#1862), which its draw's
/// arrangement selects (#2484). Each kind's restorations are drawn by their own draw family (#2034).
/// </summary>
internal static class LinkedRestoreObjectFamily
{
    /// <summary>How a bomb-block restoration's parent collides.</summary>
    private const RoomCollisionType BombBlockParent = RoomCollisionType.BombableBlock;

    /// <summary>How a contact crumble restoration's parent collides.</summary>
    private const RoomCollisionType ContactCrumbleParent = RoomCollisionType.SpecialBlock;

    /// <summary>The blocks a bomb-block restoration of each arrangement rebuilds: a bombable parent with its children linked to it.</summary>
    internal static IReadOnlyDictionary<BlockArrangement, RoomBlockShape> BombBlockRestored { get; } = Linked(BombBlockParent);

    /// <summary>The blocks a contact crumble restoration of each arrangement rebuilds: a special-block parent with its children linked to it.</summary>
    internal static IReadOnlyDictionary<BlockArrangement, RoomBlockShape> ContactCrumbleRestored { get; } = Linked(ContactCrumbleParent);

    /// <summary>The bomb blocks' restorations, pictured by the installed linked restorations.</summary>
    internal static RoomObjectDrawFamily<BombBlockRestoreDraw> BombBlockDraws { get; } =
        RoomObjectDrawFamily<BombBlockRestoreDraw>.Pictured<InstalledRoomObjectLinkedRestoreVisuals>("linked restoration",
            frame => frame.Blocks, (visuals, draw) => visuals.Image(draw));

    /// <summary>The contact crumble blocks' restorations, pictured by the installed linked restorations.</summary>
    internal static RoomObjectDrawFamily<ContactCrumbleRestoreDraw> ContactCrumbleDraws { get; } =
        RoomObjectDrawFamily<ContactCrumbleRestoreDraw>.Pictured<InstalledRoomObjectLinkedRestoreVisuals>("linked restoration",
            frame => frame.Blocks, (visuals, draw) => visuals.Image(draw));

    /// <summary>
    /// The blocks a restoration of each arrangement but a single block rebuilds: the top-left block carries
    /// <paramref name="parent"/>, and the others link to it.
    /// </summary>
    private static Dictionary<BlockArrangement, RoomBlockShape> Linked(RoomCollisionType parent) =>
        BlockArrangementDefinitions.Children.Where(arrangement => arrangement.Key != BlockArrangement.Single)
            .ToDictionary(arrangement => arrangement.Key, arrangement => RoomBlockShapes.Linked(parent, arrangement.Value));
}
