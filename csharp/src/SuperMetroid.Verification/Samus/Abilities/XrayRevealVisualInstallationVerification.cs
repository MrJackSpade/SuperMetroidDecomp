using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks installed X-ray block, item, and room overlay visuals against native records and live rendering.</summary>
internal static class XrayRevealVisualInstallationVerification
{

    /// <summary>
    /// Checks every installed visual rule, item slot and special-room record against the cartridge, and the stock tall
    /// reveal's two-block copy.
    /// </summary>
    internal static void VerifyXrayRevealVisualInstallation(GameInstallation installed)
    {
        static RevealedBlock Defined(int block) => new(block, RevealedBlockArrangement.AsDefined);
        InstalledXrayReveals stock = installed.LoadXrayRevealVisuals();
        int drawable = 0;
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? native = XrayRevealTable.Find(type, new RoomBlockBehavior(bts));
            if (native is not { } definition ||
                !InstalledXrayReveals.IsDrawable(definition.Command)) continue;
            AssertEqual((XrayRevealDefinition?)definition, stock.Apply(type, new RoomBlockBehavior(bts)),
                $"stock installed X-ray visual {type}/BTS ${bts:X2}");
            drawable++;
        }
        AssertEqual(305, drawable, "complete pinned drawable X-ray rule count");
        InstalledXrayOverlays overlays = stock.Overlays ??
            throw new InvalidDataException("Installed X-ray visuals omit item and room overlays.");
        var nativeBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(File.ReadAllBytes(installed.RomPath));
        ushort NativeWord(int address) => CartridgeImportSource.Require(nativeBus).ReadCartridgeWord(address);
        for (int slot = 0; slot < InstalledXrayOverlays.DynamicGraphicsSlots * 2; slot++)
        {
            ushort pointer = NativeWord(SamusSourceDefinitions.ItemDrawPointers.Value + slot * 2);
            AssertEqual(RoomLevelWordCodec.DecodeAppearance(NativeWord(SamusSourceDefinitions.XrayItemDrawBank.Value | (pointer + 2))),
                overlays.ItemMetatile(slot), $"stock X-ray item graphics slot {slot}");
        }
        int specialRecords = 0;
        foreach (ushort pointer in XrayRoomOverlaySources.Pointers)
        {
            IReadOnlyList<XrayRoomOverlayVisual> tiles = overlays.RoomTiles(XrayRoomOverlaySources.Overlay(pointer));
            for (int index = 0; index < tiles.Count; index++)
            {
                ushort coordinates = NativeWord(SamusSourceDefinitions.XrayRoomOverlayBank.Value | (pointer + index * 4));
                AssertEqual(new XrayRoomOverlayVisual(new((byte)coordinates, LittleEndianBytes.HighByte(coordinates)), RoomLevelWordCodec.DecodeAppearance(NativeWord(SamusSourceDefinitions.XrayRoomOverlayBank.Value |
                        (pointer + index * 4 + 2)))), tiles[index],
                    $"stock X-ray room overlay ${pointer:X4} record {index}");
                specialRecords++;
            }
            AssertEqual((ushort)0, NativeWord(SamusSourceDefinitions.XrayRoomOverlayBank.Value |
                (pointer + tiles.Count * 4)), $"X-ray room overlay ${pointer:X4} terminator");
        }

        string stockPath = Path.Combine(installed.XrayRevealVisualDirectory,
            XrayRevealVisualFiles.VisualFileName);
        JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
            ?? throw new InvalidDataException("Installed X-ray visuals are empty.");
        JsonObject tall = document.Required("entries").AsArray().RequiredElements().Select(node => node.AsObject()).Single(entry =>
            entry.Required("collisionType").GetValue<string>() == nameof(RoomCollisionType.ShootableBlock) &&
            entry.Required("behaviorValues").AsArray().RequiredElements().Any(value => value.GetValue<int>() == 2));
        AssertEqual("tall", tall.Required("shape").GetValue<string>(),
            "shot-block BTS 2 uses the native tall copy command");
        int originalTop = tall.Required("topLeft").GetValue<int>();

        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(Enumerable.Repeat(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.SolidBlock), 1024).ToArray(), new RoomBlockBehavior[1024], new RoomBlockAppearance[1024]));
        level.SetPlmTerrain(33, RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock));
        level.SetPlmBehavior(33, new RoomBlockBehavior(2));
        RoomCollisionBlock before = level.GetPlmCollisionBlockByIndex(33);
        // Each reveal cell is the whole block shown and its arrangement (#1431).
        RevealedBlock?[] originalMap = XrayRevealMap.Build(level, new(16, 16), default, AreaId.Brinstar, stock);
        AssertEqual(Defined(originalTop), originalMap[0], "stock tall reveal top-left metatile");
        AssertEqual(Defined(0xb8), originalMap[XrayRevealLayout.MetatileColumns],
            "stock tall reveal copies the native lower block");
        AssertEqual(before, level.GetPlmCollisionBlockByIndex(33),
            "X-ray reveal leaves collision type and BTS untouched");
        Console.WriteLine($"  X-ray visuals: {drawable} reveal rules, eight item slots, " +
            $"and {specialRecords} special-room records match the cartridge; the stock tall reveal copies its two blocks " +
            "and leaves collision unchanged.");
    }
}
