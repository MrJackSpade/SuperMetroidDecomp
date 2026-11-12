using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Abilities.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Every writer of blocks into a running room the installation holds the pictures of (#1636), read from the installed room
/// object catalogs and X-ray reveal, each with the room states it can run in. Every block a room object writes comes from
/// one of these catalogs: the game's room-object drawing draws only their installed frames and the block pairs a
/// collectible that shows its own picture writes under it, a block reaction's setup writes only the installed setup blocks,
/// and every other terrain write keeps the block already there or is drawn over in the same update before it shows. The writers that draw only the common room elements' blocks run in every Zebes room,
/// where those blocks are loaded; the writers of a particular room's own blocks (a station's, a boss's, a statue's) run
/// only in the room states that place or hold what writes them, as the blocks of a tileset mean nothing in another's rooms.
/// No writer runs in Ceres, whose rooms place no room object.
/// </summary>
internal static class RoomBlockWriters
{
    /// <summary>The writers the installed <paramref name="catalogs"/> and <paramref name="xray"/> reveal hold the blocks of.</summary>
    /// <exception cref="InvalidDataException">An installed X-ray reveal has no rule that draws it.</exception>
    internal static IReadOnlyList<RoomBlockWriter> Of(RoomObjectVisualCatalogSet catalogs, InstalledXrayReveals xray)
    {
        Ensure.NotNull(catalogs);
        Ensure.NotNull(xray);
        return
        [
            // The common room elements' doors, gates, block reactions, collectibles, setups and boss-room clearings, which any
            // Zebes room can come to show.
            new("shot blocks", FrameBlocks(catalogs.ShotBlock.Images), AnyZebesRoom),
            new("Grapple blocks", FrameBlocks(catalogs.GrappleBlock.Images), AnyZebesRoom),
            new("blue door caps", FrameBlocks(catalogs.BlueDoor.Images), AnyZebesRoom),
            new("coloured doors", FrameBlocks(catalogs.ColoredDoor.Images), AnyZebesRoom),
            new("grey doors", FrameBlocks(catalogs.GreyDoor.Images), AnyZebesRoom),
            new("eye doors", FrameBlocks(catalogs.EyeDoor.Images), AnyZebesRoom),
            new("downward gates", FrameBlocks(catalogs.DownwardGate.Images), AnyZebesRoom),
            new("linked-block restorations", FrameBlocks(catalogs.LinkedRestore.Images), AnyZebesRoom),
            new("Tourian access floor", FrameBlocks(catalogs.TourianAccess.Images), AnyZebesRoom),
            new("Speed Booster blocks", FrameBlocks(catalogs.SpeedBooster.Images), AnyZebesRoom),
            new("Spore Spawn's ceiling", FrameBlocks(catalogs.SporeSpawnCeiling.Images), AnyZebesRoom),
            new("Botwoon's wall", FrameBlocks(catalogs.BotwoonWall.Images), AnyZebesRoom),
            new("collectibles", FrameBlocks(catalogs.Collectible.Images), AnyZebesRoom),
            new("collectibles' block pairs", BlockPairBlocks(), AnyZebesRoom),
            new("bombed reveals and walls", FrameBlocks(catalogs.RevealAndWall.Images), AnyZebesRoom),
            new("setup blocks", BlocksOf(catalogs.SetupBlocks.All.Values), AnyZebesRoom),
            // A particular room's own blocks, written only where what writes them is placed or held.
            new("stations", FrameBlocks(catalogs.Station.Images), Placing(StationKindDefinitions.Kinds.Keys)),
            new("elevator platforms", FrameBlocks(catalogs.ElevatorPlatform.Images), Placing([RoomObjectHeaderId.ElevatorPlatform])),
            new("Bomb Torizo's hand", FrameBlocks(catalogs.BombTorizoHand.Images), Placing([RoomObjectHeaderId.BombTorizoHand])),
            new("Draygon's cannons", FrameBlocks(catalogs.DraygonCannon.Images), Placing(DraygonCannonDefinitions.Setups.Keys)),
            new("Mother Brain's glass", FrameBlocks(catalogs.MotherBrainGlass.Images), Placing([RoomObjectHeaderId.MotherBrainGlass])),
            new("Mother Brain's escape gate", FrameBlocks(catalogs.EscapeGate.Images), Placing([RoomObjectHeaderId.MotherBrainEscapeRoomGate])),
            new("n00b tube", FrameBlocks(catalogs.NoobTube.Images), Placing([RoomObjectHeaderId.NoobTube])),
            new("Kraid's room", FrameBlocks(catalogs.Kraid.Images), Holding([EnemyDefinitionId.Kraid])),
            new("Crocomire's arena", FrameBlocks(catalogs.Crocomire.Images), Holding([EnemyDefinitionId.Crocomire])),
            new("Mother Brain's fake death", FrameBlocks(catalogs.MotherBrainFakeDeath.Images), Holding([EnemyDefinitionId.MotherBrainBody])),
            new("Chozo statues", FrameBlocks(catalogs.ChozoStatue.Images), Holding(ChozoStatuePartDefinitions.Parts.Keys)),
            // Brinstar's air reactions bite with its plants, wherever a Brinstar room places one.
            new("Samus Eater plants", FrameBlocks(catalogs.SamusEater.Images), state => !state.InCeres && state.Area == AreaId.Brinstar),
            new("Maridia elevatube", FrameBlocks(catalogs.MaridiaElevatube.Images), state => state.MainCallback == RoomMainCallback.MaridiaElevatube),
            // The X-ray reveal shows its blocks in any Zebes room, and Brinstar's own only in Brinstar; a room's overlay only there.
            new("X-ray reveals", XrayBlocks(xray, brinstar: false), AnyZebesRoom),
            new("X-ray reveals of Brinstar", XrayBlocks(xray, brinstar: true), state => !state.InCeres && state.Area == AreaId.Brinstar),
            new("X-ray tanks and extension exits", BlocksOf([RoomBlockAppearance.Of(xray.ExtensionExitBlock), .. xray.Overlays.tankMetatiles]), AnyZebesRoom),
            .. xray.Overlays.rooms.Select(overlay => new RoomBlockWriter($"X-ray overlay {overlay.Key.Name}",
                BlocksOf(overlay.Value.Select(tile => tile.Appearance)), state => Equals(state.XrayOverlay, overlay.Key))),
        ];
    }

    /// <summary>
    /// The blocks a collectible that shows its own picture writes under it, a block pair's frames in turn, which a room shows
    /// where the picture does not cover them (#1538).
    /// </summary>
    private static HashSet<int> BlockPairBlocks() =>
        [.. Enumerable.Range(RoomObjectCollectibleDrawDefinitions.OwnPictureFirstBlock,
            CollectibleObjectDraws.CountOf(RoomObjectCollectibleDrawDefinitions.DrawGroup.OwnPicture))];

    /// <summary>The blocks <paramref name="shown"/> show, each once.</summary>
    private static HashSet<int> BlocksOf(IEnumerable<RoomBlockAppearance> shown) => [.. shown.Select(block => block.Block)];

    /// <summary>Whether a room state's room lies on Zebes, whose rooms load the common room elements' blocks.</summary>
    private static bool AnyZebesRoom(InventoriedRoomState state) => !state.InCeres;

    /// <summary>Runs in a room state whose population places one of <paramref name="headers"/>.</summary>
    private static Func<InventoriedRoomState, bool> Placing(IEnumerable<RoomObjectHeaderId> headers)
    {
        HashSet<RoomObjectHeaderId> placed = [.. headers];
        return state => state.Headers.Overlaps(placed);
    }

    /// <summary>Runs in a room state whose population holds one of <paramref name="enemies"/>.</summary>
    private static Func<InventoriedRoomState, bool> Holding(IEnumerable<EnemyDefinitionId> enemies)
    {
        HashSet<EnemyDefinitionId> held = [.. enemies];
        return state => state.Enemies.Overlaps(held);
    }

    /// <summary>Every block any of a room object's installed frames shows.</summary>
    private static HashSet<int> FrameBlocks<TFrame>(RoomObjectFrameImages<TFrame> images) where TFrame : notnull =>
        [.. images.All.Values.SelectMany(image => image.Cells.Values).Select(shown => shown.Block)];

    /// <summary>
    /// The blocks the X-ray reveal copies over the room's for the blocks it reveals: Brinstar's own reveals' where
    /// <paramref name="brinstar"/> says so, every other reveal's otherwise; each reveal's blocks as many as its shape copies.
    /// </summary>
    /// <exception cref="InvalidDataException">An installed reveal has no rule that draws it.</exception>
    private static HashSet<int> XrayBlocks(InstalledXrayReveals xray, bool brinstar)
    {
        var blocks = new HashSet<int>();
        foreach ((XrayRevealVisualKey key, XrayRevealVisualBlocks visual) in xray.blocks)
        {
            XrayRevealCommand command = XrayRevealTable.Find(key.Type, RoomBlockBehavior.Reacting(key.Reaction))
                ?? throw new InvalidDataException($"X-ray visual {key.Type}/reaction {key.Reaction} has no reveal rule.");
            if ((command == XrayRevealCommand.CopyBrinstar) != brinstar)
                continue;
            blocks.Add(visual.TopLeft);
            if (command is XrayRevealCommand.CopyWide or XrayRevealCommand.CopySquare)
                blocks.Add(visual.TopRight);
            if (command is XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare)
                blocks.Add(visual.BottomLeft);
            if (command == XrayRevealCommand.CopySquare)
                blocks.Add(visual.BottomRight);
        }
        return blocks;
    }
}
