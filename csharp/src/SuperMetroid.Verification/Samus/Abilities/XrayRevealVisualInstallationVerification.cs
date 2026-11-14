using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
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
        var nativeBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(File.ReadAllBytes(installed.RomPath));
        int drawable = 0;
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            // Every copy's blocks come from the cartridge's reveal operands, as the import extracted them (#1596).
            XrayRevealDefinition? native = XrayRevealVerification.ReadNativeXrayRevealDefinition(nativeBus, type, unchecked((byte)bts));
            if (native is not { } definition ||
                !InstalledXrayReveals.IsDrawable(definition.Shape)) continue;
            AssertEqual((XrayRevealDefinition?)definition, stock.Apply(type, RoomBlockBehaviorCodec.Decode(bts)),
                $"stock installed X-ray visual {type}/BTS ${bts:X2}");
            drawable++;
        }
        AssertEqual(305, drawable, "complete pinned drawable X-ray rule count");
        AssertEqual((XrayRevealDefinition?)new XrayRevealDefinition(XrayRevealShape.CopySquare, XrayRevealVisualBlocks.Square(0x99, 0x9a, 0xb9, 0xba)),
            stock.Apply(RoomCollisionType.ShootableBlock, RoomBlockBehaviorCodec.Decode(3)),
            "stock square shot-block reveal installs all four row-major blocks");
        InstalledXrayOverlays overlays = stock.Overlays;
        ushort NativeWord(int address) => CartridgeImportSource.Require(nativeBus).ReadCartridgeWord(address);
        // The tanks' draws follow the four that reveal the item in each block pair (#1538).
        for (int tank = 0; tank < InstalledXrayOverlays.TankCount; tank++)
        {
            ushort pointer = NativeWord(SamusSourceDefinitions.ItemDrawPointers.Value + (SuperMetroid.Core.Compatibility.CollectibleBlockPairs.InTurn.Count + tank) * 2);
            InWorldCollectibleKind kind = InWorldCollectibleKind.EnergyTank + tank;
            AssertEqual(RoomLevelWordCodec.DecodeAppearance(NativeWord(SamusSourceDefinitions.XrayItemDrawBank.Value | (pointer + 2))),
                overlays.TankBlock(kind), $"stock X-ray {kind} reveal");
        }
        int specialRecords = 0;
        foreach (ushort pointer in XrayRoomOverlaySources.Pointers)
        {
            IReadOnlyList<XrayRoomOverlayVisual> tiles = overlays.RoomBlocks(XrayRoomOverlaySources.Overlay(pointer));
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
            entry.Required("reactions").AsArray().RequiredElements().Any(reaction =>
                reaction.GetValue<string>() == nameof(ShootableReaction.RespawningBreak1x2)));
        AssertEqual("tall", tall.Required("shape").GetValue<string>(),
            "the two-tall respawning shot block uses the native tall copy command");        // The installed rooms name their overlays; the pointer of each list stays the import's own knowledge (#1623).
        JsonObject[] rooms = [.. document.Required("rooms").AsArray().RequiredElements().Select(node => node.AsObject())];
        AssertTrue(rooms.Select(room => room.Required("overlay").GetValue<string>())
                .SequenceEqual(SuperMetroid.Core.Rooms.Xray.XrayRoomOverlayDefinitions.All.Select(overlay => overlay.Name)),
            "installed X-ray rooms name every retail overlay, in order");
        AssertTrue(rooms.All(room => !room.ContainsKey("pointer")), "installed X-ray rooms hold no cartridge pointer");
        // A rule lists the blocks its shape copies and no others: a tall reveal its top and bottom ones (#1756).
        int[] tallBlocks = [.. tall.Required("blocks").AsArray().RequiredElements().Select(block => block.GetValue<int>())];
        AssertEqual(2, tallBlocks.Length, "the tall reveal lists its two blocks");
        AssertTrue(document.AsObject().ContainsKey("tanks") && !document.AsObject().ContainsKey("tankMetatiles"),
            "the tanks' reveal blocks are installed as tanks");
        int originalTop = tallBlocks[0];

        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(Enumerable.Repeat(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.SolidBlock), 1024).ToArray(), new RoomBlockBehavior[1024], new RoomBlockAppearance[1024]), RoomLevelData.NothingBeyondGrid());
        level.SetObjectTerrain(33, RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock));
        level.SetObjectBehavior(33, RoomBlockBehaviorCodec.Decode(2));
        RoomCollisionBlock before = level.GetObjectCollisionBlockByIndex(33);
        // Each reveal cell is the whole block shown and its arrangement (#1431).
        RevealedBlock?[] originalMap = XrayRevealMap.Build(level, new(16, 16), AreaId.Brinstar, stock);
        AssertEqual(Defined(originalTop), originalMap[0], "stock tall reveal top-left metatile");
        AssertEqual(Defined(0xb8), originalMap[XrayRevealArea.Columns],
            "stock tall reveal copies the native lower block");
        AssertEqual(before, level.GetObjectCollisionBlockByIndex(33),
            "X-ray reveal leaves collision type and BTS untouched");
        Console.WriteLine($"  X-ray visuals: {drawable} reveal rules, eight item slots, " +
            $"and {specialRecords} special-room records match the cartridge; the stock tall reveal copies its two blocks " +
            "and leaves collision unchanged.");
    }
}
