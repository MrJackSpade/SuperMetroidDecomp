using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Checks every installed visual rule against the compiled cartridge dispatcher,
    /// then changes a tall reveal without changing its two-block copy behavior.
    /// </summary>
    private static void VerifyXrayRevealVisualInstallation(GameInstallation installed)
    {
        XrayRevealVisualCatalog stock = installed.LoadXrayRevealVisuals();
        int drawable = 0;
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? native = XrayRevealTable.Find(type, unchecked((byte)bts));
            if (native is not { } definition ||
                !XrayRevealVisualCatalog.IsDrawable(definition.Command)) continue;
            AssertEqual(definition, stock.Apply(type, unchecked((byte)bts), definition),
                $"stock installed X-ray visual {type}/BTS ${bts:X2}");
            drawable++;
        }
        AssertEqual(305, drawable, "complete pinned drawable X-ray rule count");
        XrayOverlayVisualCatalog overlays = stock.Overlays ??
            throw new InvalidDataException("Installed X-ray visuals omit item and room overlays.");
        var nativeBus = new SuperMetroidAddressSpace(File.ReadAllBytes(installed.RomPath));
        ushort NativeWord(int address) => unchecked((ushort)(nativeBus.ReadByte(address) |
            nativeBus.ReadByte(address + 1) << 8));
        for (int slot = 0; slot < XrayOverlayRomData.DynamicGraphicsSlots * 2; slot++)
        {
            ushort pointer = NativeWord(XrayOverlayRomData.ItemDrawPointers + slot * 2);
            AssertEqual((ushort)(NativeWord(XrayOverlayRomData.ItemBank | (pointer + 2)) & 0x0fff),
                overlays.ItemMetatile(slot), $"stock X-ray item graphics slot {slot}");
        }
        int specialRecords = 0;
        foreach (ushort pointer in RoomStateDefinitions.All.Select(state => state.XrayPointer)
                     .Where(pointer => pointer != 0).Distinct().OrderBy(pointer => pointer))
        {
            IReadOnlyList<XrayRoomOverlayVisual> tiles = overlays.RoomTiles(pointer);
            for (int index = 0; index < tiles.Count; index++)
            {
                ushort coordinates = NativeWord(XrayOverlayRomData.RoomBank | (pointer + index * 4));
                AssertEqual(new XrayRoomOverlayVisual((byte)coordinates,
                    (byte)(coordinates >> 8), NativeWord(XrayOverlayRomData.RoomBank |
                        (pointer + index * 4 + 2))), tiles[index],
                    $"stock X-ray room overlay ${pointer:X4} record {index}");
                specialRecords++;
            }
            AssertEqual((ushort)0, NativeWord(XrayOverlayRomData.RoomBank |
                (pointer + tiles.Count * 4)), $"X-ray room overlay ${pointer:X4} terminator");
        }

        string stockPath = Path.Combine(installed.XrayRevealVisualDirectory,
            XrayRevealVisualFiles.VisualFileName);
        JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
            ?? throw new InvalidDataException("Installed X-ray visuals are empty.");
        JsonArray entries = document["entries"]!.AsArray();
        JsonObject tall = entries.Select(node => node!.AsObject()).Single(entry =>
            entry["collisionType"]!.GetValue<string>() == nameof(RoomCollisionType.ShootableBlock) &&
            entry["btsValues"]!.AsArray().Any(value => value!.GetValue<int>() == 2));
        AssertEqual("tall", tall["shape"]!.GetValue<string>(),
            "shot-block BTS 2 uses the native tall copy command");
        ushort originalTop = unchecked((ushort)tall["topLeft"]!.GetValue<int>());
        tall["topLeft"] = originalTop + 1;
        JsonArray items = document["itemMetatiles"]!.AsArray();
        ushort originalItem = unchecked((ushort)items[0]!.GetValue<int>());
        ushort editedItem = (ushort)((originalItem & 0x0c00) |
            ((originalItem + 1) & 0x03ff));
        items[0] = editedItem;
        JsonObject roomOverlay = document["rooms"]!.AsArray()[0]!.AsObject();
        ushort roomPointer = unchecked((ushort)roomOverlay["pointer"]!.GetValue<int>());
        JsonObject lastRoomTile = roomOverlay["tiles"]!.AsArray().Last()!.AsObject();
        ushort originalRoomWord = unchecked((ushort)lastRoomTile["word"]!.GetValue<int>());
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
        var definitions = new byte[1024 * 8];
        for (int i = 0; i < definitions.Length / 2; i++)
        {
            definitions[i * 2] = (byte)i;
            definitions[i * 2 + 1] = (byte)(i >> 8);
        }
        var level = new RoomLevelData(32, 32,
            Enumerable.Repeat((ushort)0x8000, 1024).ToArray(), new byte[1024],
            new ushort[1024], definitions);
        level.SetPlmForegroundEntry(33, (ushort)((int)RoomCollisionType.ShootableBlock << 12));
        level.SetPlmBehavior(33, 2);
        RoomCollisionBlock before = level.GetPlmCollisionBlockByIndex(33);
        ushort[] originalMap = XrayRevealTilemap.Build(level, vram, 0, 0, 16, 16,
            (byte)AreaId.Brinstar, stock);
        ushort[] editedMap = XrayRevealTilemap.Build(level, vram, 0, 0, 16, 16,
            (byte)AreaId.Brinstar, edited);
        AssertEqual(originalTop * 4, originalMap[0], "stock tall reveal top-left metatile");
        AssertEqual((originalTop + 1) * 4, editedMap[0],
            "edited X-ray metatile changes live reveal tilemap");
        AssertEqual(0xb8 * 4, editedMap[64],
            "edited tall reveal still copies the native lower block");
        AssertEqual(before, level.GetPlmCollisionBlockByIndex(33),
            "X-ray visual override leaves collision type and BTS untouched");

        var item = new CollectiblePlmSnapshot(RoomPlmHeaders.ExposedEnergyTank, 33, 1,
            InWorldCollectibleKind.Bombs, CollectiblePresentation.Exposed,
            CollectiblePhase.Visible, 0);
        var overlayLevel = new RoomLevelData(32, 32, new ushort[1024], new byte[1024],
            new ushort[1024], definitions);
        var noRomVisualData = new TestAddressSpace();
        ushort[] itemMap = new ushort[XrayTilemapLayout.BufferWords];
        XrayRevealOverlays.Apply(noRomVisualData, overlayLevel, itemMap, [item],
            new Bank80SystemState(), 0, 16, 16, edited);
        AssertEqual(((editedItem & 0x03ff) * 4) + ((editedItem & 0x0800) != 0 ? 2 : 0),
            itemMap[0], "edited item art renders without a visual ROM read");
        ushort[] roomMap = new ushort[XrayTilemapLayout.BufferWords];
        XrayRevealOverlays.Apply(noRomVisualData, overlayLevel, roomMap, [],
            new Bank80SystemState(), roomPointer, 16, 16, edited);
        AssertEqual(((editedRoomWord & 0x03ff) * 4) +
            ((editedRoomWord & 0x0800) != 0 ? 2 : 0), roomMap[0],
            "edited room overlay renders at its edited position without a ROM read");

        _ = GameAssetInstaller.EnsureInstalled(installed.Root)
            ?? throw new InvalidOperationException("X-ray repair lost the installation.");
        AssertEqual((ushort)(originalTop + 1), installed.LoadXrayRevealVisuals()
                .Apply(RoomCollisionType.ShootableBlock, 2,
                    XrayRevealTable.Find(RoomCollisionType.ShootableBlock, 2)!.Value).TopLeft,
            "X-ray override survives stock installation validation");
        AssertEqual(editedItem, installed.LoadXrayRevealVisuals().Overlays!.ItemMetatile(0),
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
