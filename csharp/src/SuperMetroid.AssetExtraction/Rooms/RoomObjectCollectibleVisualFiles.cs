using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectCollectibleDrawDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed pictures of the collectibles' frames (#1557), and the published names that key them. The installed file
/// holds the 16 frames whose picture is seen: the eight block-pair frames lie under the item's own picture (#1538) and
/// write only their terrain, which the import checks against the cartridge with the block pairs' selectors.
/// </summary>
public static class RoomObjectCollectibleVisualFiles
{
    /// <summary>Editable JSON filename containing the sixteen editable collectible frames.</summary>
    public const string VisualFileName = "collectibles.json";
    /// <summary>The collectibles as the import pictures them: every installed frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Collectible", Frames, draw => VisualId(RoomObjectDraw.FrameOf<Draw>(draw, "collectible")));

    /// <summary>
    /// Checks the block pairs' frame selectors and the frames lying under the item's own picture against the cartridge,
    /// then pictures every installed frame from the cartridge's draw lists and writes the installed file (#1557).
    /// </summary>
    /// <param name="bus">The cartridge, whose selectors and draw lists are checked against the frames the game draws.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A selector, a draw list's geometry or a block pair's terrain differs from what the game draws.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        VerifyBlockPairs(bus);
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);
    }

    /// <summary>Loads the installed collectible pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectCollectibleVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// The artwork identifier of a draw: the installed file's name for an editable frame, and for a block-pair frame, which
    /// the file does not hold, the name its native checks report.
    /// </summary>
    internal static string VisualId(Draw draw) => draw.Group switch
    {
        DrawGroup.Empty => "empty",
        DrawGroup.Orb => $"chozo-orb-{draw.Index}",
        DrawGroup.OrbBurst => "chozo-orb-burst",
        DrawGroup.Tank => $"{TankKindName(draw.Index / 2)}-tank-{draw.Index % 2}",
        DrawGroup.OwnPicture => $"block-pair-{draw.Index / 2}-frame-{draw.Index % 2}",
        DrawGroup.ShotReveal => $"shot-reveal-{draw.Index}",
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {draw.Group}."),
    };

    /// <summary>Gets the stable visual ID stem for one of the four tank pickup kinds.</summary>
    private static string TankKindName(int kind) => kind switch
    {
        0 => "energy",
        1 => "missile",
        2 => "super-missile",
        _ => "power-bomb",
    };

    /// <summary>Whether the installed file holds <paramref name="draw"/>: every frame but a block pair's, which lies under the item's own picture (#1538).</summary>
    internal static bool IsEditable(Draw draw) => draw.Group != DrawGroup.OwnPicture;

    /// <summary>The sixteen frames the installed file holds, in their published order.</summary>
    internal static IEnumerable<RoomObjectCollectibleDrawFrame> Editable => All.Where(frame => IsEditable(frame.Draw));

    /// <summary>Enumerates all 24 collectible one-cell draws in their published order.</summary>
    internal static IEnumerable<RoomObjectCollectibleDrawFrame> All
    {
        get
        {
            foreach (DrawGroup group in Enum.GetValues<DrawGroup>())
            {
                for (int index = 0; index < CollectibleObjectDraws.CountOf(group); index++)
                {
                    var draw = new Draw(group, index);
                    CollectibleObjectDraws.TryGetCollision(RoomObjectDraw.Of(draw), out RoomCollisionType collision);
                    yield return new(draw, collision, VisualId(draw));
                }
            }
        }
    }

    /// <summary>Every installed frame's one-cell draw list, built from the collision the game writes.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> Frames =>
        Editable.Select(frame => RoomObjectDrawListExport.SingleRun(RoomObjectDraw.Of(frame.Draw), RoomObjectRunDirection.Horizontal, [frame.Collision]));

    /// <summary>
    /// Checks the eight selectors of the frames each block pair's collectible writes (<see cref="CollectibleBlockPairs"/>),
    /// and that each frame lying under the item's own picture is one horizontal cell writing the terrain the game writes
    /// there, appearance included, since the game shows that terrain as it is.
    /// </summary>
    /// <exception cref="InvalidDataException">A selector or a block pair's frame differs from the cartridge.</exception>
    private static void VerifyBlockPairs(ISnesAddressSpace bus)
    {
        for (int selector = 0; selector < CollectibleBlockPairs.InTurn.Count; selector++)
        for (int animation = 0; animation < CollectiblePicture.Frames; animation++)
        {
            int table = animation == 0
                ? RoomObjectDrawSourceDefinitions.CollectibleDynamicFrame0Table.Offset
                : RoomObjectDrawSourceDefinitions.CollectibleDynamicFrame1Table.Offset;
            int native = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, (checked((ushort)(table + selector * 2)))));
            int compiled = RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.VisibleFrame(
                InWorldCollectibleKind.Bombs, animation, CollectibleBlockPairs.InTurn[selector]));
            if (native != compiled)
                throw new InvalidDataException(
                    $"Collectible block pair selector $84:{table + selector * 2:X4} differs from the cartridge.");
        }
        foreach (RoomObjectCollectibleDrawFrame frame in All.Where(frame => !IsEditable(frame.Draw)))
        {
            int address = RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(frame.Draw));
            if (!CollectibleObjectDraws.TryGetBlockPairTerrain(RoomObjectDraw.Of(frame.Draw), out RoomTerrainBlock terrain))
                throw new InvalidOperationException($"Collectible draw {frame.Id} lies under the item's own picture but writes no block pair's terrain.");
            if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, address)) != 1 ||
                RoomLevelWordCodec.Decode(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, (checked((ushort)(address + 2)))))) != terrain ||
                CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, (checked((ushort)(address + 4))))) != 0)
                throw new InvalidDataException(
                    $"Collectible draw {frame.Id} at $84:{address:X4} differs from compiled geometry or level word.");
        }
    }
}
