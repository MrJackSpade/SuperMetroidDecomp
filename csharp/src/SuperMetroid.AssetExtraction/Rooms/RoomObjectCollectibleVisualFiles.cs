using System.Text.Json;
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
public static partial class RoomObjectCollectibleVisualFiles
{
    /// <summary>Editable JSON filename containing the sixteen editable collectible frames.</summary>
    internal const string VisualFileName = "collectibles.json";

    /// <summary>Every collectible frame in its published order (#2567): the empty block, the Chozo orb's stages, the tanks, the block pairs' looks and the shot block's stages.</summary>
    private static readonly IReadOnlyList<CollectibleDraw> PublishedOrder = Ensure.EveryMemberOnce<CollectibleDraw>(
    [
        CollectibleDraw.Empty,
        CollectibleDraw.ChozoOrbDim,
        CollectibleDraw.ChozoOrbGlowing,
        CollectibleDraw.ChozoOrbBright,
        CollectibleDraw.ChozoOrbBurst,
        CollectibleDraw.EnergyTankBright,
        CollectibleDraw.EnergyTankDim,
        CollectibleDraw.MissileTankDim,
        CollectibleDraw.MissileTankBright,
        CollectibleDraw.SuperMissileTankDim,
        CollectibleDraw.SuperMissileTankBright,
        CollectibleDraw.PowerBombTankBright,
        CollectibleDraw.PowerBombTankDim,
        CollectibleDraw.SolidPairPrimaryLook,
        CollectibleDraw.SolidPairAlternateLook,
        CollectibleDraw.FirstDoorPairPrimaryLook,
        CollectibleDraw.FirstDoorPairAlternateLook,
        CollectibleDraw.SecondDoorPairPrimaryLook,
        CollectibleDraw.SecondDoorPairAlternateLook,
        CollectibleDraw.ThirdDoorPairPrimaryLook,
        CollectibleDraw.ThirdDoorPairAlternateLook,
        CollectibleDraw.ShotBlockIntact,
        CollectibleDraw.ShotBlockCracking,
        CollectibleDraw.ShotBlockDust,
    ]);
    /// <summary>The collectibles as the import pictures them: every installed frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Collectible", Frames, draw => VisualId(RoomObjectDrawIdentity.FrameOf<CollectibleDraw>(draw, "collectible")));

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
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// The artwork identifier of a draw: the installed file's name for an editable frame, and for a block-pair frame, which
    /// the file does not hold, the name its native checks report.
    /// </summary>
    /// <exception cref="InvalidOperationException">The draw is no collectible's frame.</exception>
    internal static string VisualId(CollectibleDraw draw) => Enum.IsDefined(draw)
        ? JsonNamingPolicy.KebabCaseLower.ConvertName(CollectibleDrawKeys.Installed.Of(draw))
        : throw new InvalidOperationException($"Undefined {nameof(CollectibleDraw)} {draw}.");

    /// <summary>Whether the installed file holds <paramref name="draw"/>: every frame but a block pair's, which lies under the item's own picture (#1538).</summary>
    internal static bool IsEditable(CollectibleDraw draw) => CollectibleObjectDraws.GroupOf(draw) != DrawGroup.OwnPicture;

    /// <summary>The sixteen frames the installed file holds, in their published order.</summary>
    internal static IEnumerable<RoomObjectCollectibleDrawFrame> Editable => All.Where(frame => IsEditable(frame.Draw));

    /// <summary>Enumerates all 24 collectible one-cell draws in their published order.</summary>
    internal static IEnumerable<RoomObjectCollectibleDrawFrame> All
    {
        get
        {
            foreach (CollectibleDraw draw in PublishedOrder)
                yield return new(draw, CollectibleObjectDraws.CollisionOf(draw), VisualId(draw));
        }
    }

    /// <summary>Every installed frame's one-cell draw list, built from the collision the game writes.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> Frames =>
        Editable.Select(frame => RoomObjectDrawListExport.SingleRun(CollectibleObjectDraws.Family.Of(frame.Draw), RoomObjectRunDirection.Horizontal, [frame.Collision]));

    /// <summary>
    /// Checks the eight selectors of the frames each block pair's collectible writes (<see cref="CollectibleBlockPairs"/>),
    /// and that each frame lying under the item's own picture is one horizontal cell writing the terrain the game writes
    /// there, its pair's block stored as the number the import gives it (<see cref="CollectiblePairBlockNumbers"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">A selector or a block pair's frame differs from the cartridge.</exception>
    private static void VerifyBlockPairs(ISnesAddressSpace bus)
    {
        for (int selector = 0; selector < CollectibleBlockPairs.InTurn.Count; selector++)
        foreach (CollectibleLook look in Enum.GetValues<CollectibleLook>())
        {
            int table = look == CollectibleLook.Primary
                ? RoomObjectDrawSourceDefinitions.CollectibleDynamicFrame0Table.Offset
                : RoomObjectDrawSourceDefinitions.CollectibleDynamicFrame1Table.Offset;
            int native = bus.ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, (checked((ushort)(table + selector * 2)))));
            int compiled = RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.BlockPairFrame(
                CollectibleBlockPairs.InTurn[selector], look));
            if (native != compiled)
                throw new InvalidDataException(
                    $"Collectible block pair selector $84:{table + selector * 2:X4} differs from the cartridge.");
        }
        foreach (RoomObjectCollectibleDrawFrame frame in All.Where(frame => !IsEditable(frame.Draw)))
        {
            int address = RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.Family.Of(frame.Draw));
            RoomTerrainBlock terrain = CollectibleObjectDraws.BlockPairTerrain(frame.Draw)
                ?? throw new InvalidOperationException($"Collectible draw {frame.Id} lies under the item's own picture but writes no block pair's terrain.");
            if (bus.ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, address)) != 1 ||
                RoomLevelWordCodec.Decode(bus.ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, (checked((ushort)(address + 2)))))) != RoomLevelWordCodec.AsStored(terrain) ||
                bus.ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, (checked((ushort)(address + 4))))) != 0)
                throw new InvalidDataException(
                    $"Collectible draw {frame.Id} at $84:{address:X4} differs from compiled geometry or level word.");
        }
    }
}
