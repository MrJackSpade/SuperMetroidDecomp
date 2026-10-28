using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks installed X-ray block, item, and room overlay visuals against native records and live rendering.</summary>
internal static class XrayRevealVisualInstallationVerification
{

    /// <summary>
    /// Checks every installed visual rule against the compiled cartridge dispatcher,
    /// then changes a tall reveal without changing its two-block copy behavior.
    /// </summary>
    internal static void VerifyXrayRevealVisualInstallation(GameInstallation installed)
    {
        XrayRevealVisualCatalog stock = installed.LoadXrayRevealVisuals();
        int drawable = 0;
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? native = XrayRevealTable.Find(type, unchecked((byte)bts));
            if (native is not { } definition ||
                !XrayRevealVisualCatalog.IsDrawable(definition.Command)) continue;
            AssertEqual((XrayRevealDefinition?)definition, stock.Apply(type, unchecked((byte)bts)),
                $"stock installed X-ray visual {type}/BTS ${bts:X2}");
            drawable++;
        }
        AssertEqual(305, drawable, "complete pinned drawable X-ray rule count");
        XrayOverlayVisualCatalog overlays = stock.Overlays ??
            throw new InvalidDataException("Installed X-ray visuals omit item and room overlays.");
        var nativeBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(File.ReadAllBytes(installed.RomPath));
        ushort NativeWord(int address) => CartridgeImportSource.Require(nativeBus).ReadCartridgeWord(address);
        for (int slot = 0; slot < XrayOverlayVisualCatalog.DynamicGraphicsSlots * 2; slot++)
        {
            ushort pointer = NativeWord(SamusSourceDefinitions.ItemDrawPointers.Value + slot * 2);
            AssertEqual((ushort)(NativeWord(SamusSourceDefinitions.XrayItemDrawBank.Value | (pointer + 2)) & 0x0fff),
                overlays.ItemMetatile(slot), $"stock X-ray item graphics slot {slot}");
        }
        int specialRecords = 0;
        foreach (ushort pointer in XrayRoomOverlaySources.Pointers)
        {
            IReadOnlyList<XrayRoomOverlayVisual> tiles = overlays.RoomTiles(XrayRoomOverlaySources.Overlay(pointer));
            for (int index = 0; index < tiles.Count; index++)
            {
                ushort coordinates = NativeWord(SamusSourceDefinitions.XrayRoomOverlayBank.Value | (pointer + index * 4));
                AssertEqual(new XrayRoomOverlayVisual(new((byte)coordinates, SnesLittleEndian.HighByte(coordinates)), NativeWord(SamusSourceDefinitions.XrayRoomOverlayBank.Value |
                        (pointer + index * 4 + 2))), tiles[index],
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
        JsonArray entries = document.Required("entries").AsArray();
        JsonObject tall = entries.RequiredElements().Select(node => node.AsObject()).Single(entry =>
            entry.Required("collisionType").GetValue<string>() == nameof(RoomCollisionType.ShootableBlock) &&
            entry.Required("btsValues").AsArray().RequiredElements().Any(value => value.GetValue<int>() == 2));
        AssertEqual("tall", tall.Required("shape").GetValue<string>(),
            "shot-block BTS 2 uses the native tall copy command");
        ushort originalTop = unchecked((ushort)tall.Required("topLeft").GetValue<int>());
        tall["topLeft"] = originalTop + 1;
        JsonArray items = document.Required("itemMetatiles").AsArray();
        ushort originalItem = unchecked((ushort)items.Required(0).GetValue<int>());
        ushort editedItem = (ushort)((originalItem & 0x0c00) |
            ((originalItem + 1) & 0x03ff));
        items[0] = editedItem;
        JsonObject roomOverlay = document.Required("rooms").AsArray().Required(0).AsObject();
        ushort roomPointer = unchecked((ushort)roomOverlay.Required("pointer").GetValue<int>());
        JsonObject lastRoomTile = (roomOverlay.Required("tiles").AsArray().Last() ?? throw new InvalidOperationException("Verification requires roomOverlay.Required(\"tiles\").AsArray().Last().")).AsObject();
        ushort originalRoomWord = unchecked((ushort)lastRoomTile.Required("word").GetValue<int>());
        ushort editedRoomWord = (ushort)((originalRoomWord & 0x0c00) |
            ((originalRoomWord + 1) & 0x03ff));
        lastRoomTile["x"] = 1;
        lastRoomTile["y"] = 1;
        lastRoomTile["word"] = editedRoomWord;
        Directory.CreateDirectory(installed.XrayRevealVisualOverrideDirectory);
        string overridePath = Path.Combine(installed.XrayRevealVisualOverrideDirectory,
            XrayRevealVisualFiles.VisualFileName);
        File.WriteAllText(overridePath, document.ToJsonString());
        XrayRevealVisualCatalog edited = installed.LoadXrayRevealVisuals();

        var vram = new SnesVram();
        var definitions = new BackgroundTile[1024 * 4];
        for (int i = 0; i < definitions.Length; i++)
        {
            definitions[i] = BackgroundTileWordBytes.Tile(i);
        }
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(Enumerable.Repeat((ushort)0x8000, 1024).ToArray(), new byte[1024], new ushort[1024]), definitions);
        level.SetPlmForegroundEntry(33, (int)RoomCollisionType.ShootableBlock << 12);
        level.SetPlmBehavior(33, 2);
        RoomCollisionBlock before = level.GetPlmCollisionBlockByIndex(33);
        ushort[] originalMap = BackgroundTileWordBytes.Words(XrayRevealTilemap.Build(level, XrayBg1Fixtures.Tilemap(vram), new(0, 0), new(16, 16),
            AreaId.Brinstar, stock));
        ushort[] editedMap = BackgroundTileWordBytes.Words(XrayRevealTilemap.Build(level, XrayBg1Fixtures.Tilemap(vram), new(0, 0), new(16, 16),
            AreaId.Brinstar, edited));
        AssertEqual(originalTop * 4, originalMap[0], "stock tall reveal top-left metatile");
        AssertEqual((originalTop + 1) * 4, editedMap[0],
            "edited X-ray metatile changes live reveal tilemap");
        AssertEqual(0xb8 * 4, editedMap[64],
            "edited tall reveal still copies the native lower block");
        AssertEqual(before, level.GetPlmCollisionBlockByIndex(33),
            "X-ray visual override leaves collision type and BTS untouched");

        var item = new CollectiblePlmSnapshot(PlmHeaderId.ExposedEnergyTank, 33, 1,
            InWorldCollectibleKind.Bombs, 0);
        var overlayLevel = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]), definitions);
        var noRomVisualData = new TestAddressSpace();
        BackgroundTile[] itemMap = new BackgroundTile[XrayTilemapLayout.BufferWords];
        XrayRevealOverlays.Apply(overlayLevel, itemMap, [item],
            new SharedGameState(), null, new(16, 16), edited);
        AssertEqual(((editedItem & 0x03ff) * 4) + ((editedItem & 0x0800) != 0 ? 2 : 0),
            itemMap[0].PackedWord, "edited item art renders without a visual ROM read");
        BackgroundTile[] roomMap = new BackgroundTile[XrayTilemapLayout.BufferWords];
        XrayRevealOverlays.Apply(overlayLevel, roomMap, [],
            new SharedGameState(), XrayRoomOverlaySources.Overlay(roomPointer), new(16, 16), edited);
        AssertEqual(((editedRoomWord & 0x03ff) * 4) +
            ((editedRoomWord & 0x0800) != 0 ? 2 : 0), roomMap[0].PackedWord,
            "edited room overlay renders at its edited position without a ROM read");

        _ = GameAssetInstaller.EnsureInstalled(installed.Root)
            ?? throw new InvalidOperationException("X-ray repair lost the installation.");
        AssertEqual((ushort)(originalTop + 1), (installed.LoadXrayRevealVisuals()
                .Apply(RoomCollisionType.ShootableBlock, 2) ?? throw new InvalidOperationException("Verification requires installed.LoadXrayRevealVisuals() .Apply(RoomCollisionType.ShootableBlock, 2).")).TopLeft,
            "X-ray override survives stock installation validation");
        AssertEqual(editedItem, (installed.LoadXrayRevealVisuals().Overlays ?? throw new InvalidOperationException("Verification requires installed.LoadXrayRevealVisuals().Overlays.")).ItemMetatile(0),
            "item overlay edit survives stock installation validation");

        tall["shape"] = "wide";
        File.WriteAllText(overridePath, document.ToJsonString());
        try
        {
            _ = installed.LoadXrayRevealVisuals();
            throw new InvalidOperationException("An X-ray command edit was accepted as artwork.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains(XrayRevealVisualFiles.VisualFileName,
                StringComparison.Ordinal), "invalid X-ray override identifies its file");
        }
        tall["shape"] = "tall";
        roomOverlay["pointer"] = roomPointer + 1;
        File.WriteAllText(overridePath, document.ToJsonString());
        try
        {
            _ = installed.LoadXrayRevealVisuals();
            throw new InvalidOperationException("An X-ray room identity edit was accepted as artwork.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains("room-overlay", StringComparison.Ordinal),
                "invalid X-ray room identity identifies its contract");
        }
        Console.WriteLine($"  X-ray visuals: {drawable} reveal rules, eight item slots, " +
            $"and {specialRecords} special-room records match the cartridge; edited visual tiles " +
            "render without ROM reads, survive repair, and leave collision/copy shape unchanged.");
    }
}
