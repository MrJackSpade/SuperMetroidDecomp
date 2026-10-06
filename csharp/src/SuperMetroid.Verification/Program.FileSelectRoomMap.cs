using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyFileSelectRoomMapGraphics()
    {
        VerifyFileSelectStationMarker();
        VerifyFileSelectMapScroll();
        VerifyFileSelectMapNavigation();
        VerifySavedGameMapFrontend();
        VerifyFileSelectMapIcons();
        VerifyFileSelectMapAnimations();
        VerifyMapCancelPresentation();
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int areaIndex = 0; areaIndex < FileSelectMapRomData.AreaCount; areaIndex++)
        foreach (bool downloaded in new[] { false, true })
        {
            AreaId area = (AreaId)areaIndex;
            var system = new Bank80SystemState();
            AreaMapCartridgeData map = SuperMetroid.AssetExtraction.AreaMapImporter.Load(bus, area);
            byte[] original = map.RawTilemapBytes.ToArray();
            // Deliberately cross both native pages and explore cells outside the map
            // station's mask: explored cells win, even for secret rooms.
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 64; x++)
                if ((x + y) % 7 == 0) system.MarkExploredMapTile(area, x, y);
            if (downloaded) system.SetAreaMapAcquired(area);
            var graphics = new FileSelectRoomMapGraphics(bus, system, area, mapPresentation: RetailPresentationFixture());
            byte[] pause = AreaMapTilemapBuilder.Build(map, system, MapTileWords.PauseBlank);
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 64; x++)
            {
                int offset = AreaMapLayout.GetTilemapWordIndex(x, y) * 2;
                ushort raw = BinaryPrimitives.ReadUInt16LittleEndian(original.AsSpan(offset));
                bool explored = (x + y) % 7 == 0;
                // Independent transcription of $82:9517's two branches.
                ushort expected = explored ? (ushort)(raw & 0xfbff)
                    : downloaded && map.IsRevealedByMapStation(x, y) ? raw
                    : downloaded ? (ushort)31 : (ushort)15;
                AssertEqual(expected, graphics.Vram.ReadWord((ushort)(0x5000 + offset / 2)),
                    $"room-select area {areaIndex} downloaded={downloaded} cell {x},{y}");
                ushort pauseExpected = !explored && !(downloaded && map.IsRevealedByMapStation(x, y))
                    ? (ushort)31 : expected;
                AssertEqual(pauseExpected, BinaryPrimitives.ReadUInt16LittleEndian(pause.AsSpan(offset)),
                    "shared builder retains pause map hidden character");
                AssertEqual(explored, system.IsMapTileExplored(area, x, y), "map projection does not mark exploration");
            }
            AssertTrue(original.SequenceEqual(map.RawTilemapBytes), "map projection preserves cartridge map");
            ushort label = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.RoomLabelPointers + areaIndex * 2);
            for (int word = 0; word < 1024; word++)
            {
                ushort expected = word < 800
                    ? RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.RoomFrame + word * 2)
                    : word < 960
                        ? RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.RoomFrameFooter + (word - 799) * 2)
                        : (ushort)0x2801;
                if (word >= 170 && word < 182)
                    expected = (ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x820000 | (label + (word - 170) * 2)) & 0xefff);
                AssertEqual(expected, graphics.Vram.ReadWord((ushort)(0x5800 + word)), "native room-select frame and area label");
            }
            Rgba32[] pixels = graphics.RenderBackgrounds(0, unchecked((ushort)-40));
            AssertEqual(256 * 224, pixels.Length, "room map visible viewport");
            AssertTrue(pixels.All(pixel => pixel.A == 255), "room map has opaque backdrop");
            var retailIcons = new FileSelectMapIcons(system, area);
            retailIcons.BindStations(RetailPresentationFixture().Stations);
            retailIcons.BindLandmarks(RetailPresentationFixture().Landmarks);
            retailIcons.BindSprites(RetailPresentationFixture().Sprites);
            var retailOam = new OamBuffer();
            retailOam.BeginFrame();
            retailIcons.DrawBeforeMarker(retailOam, 0, 0);
            retailIcons.DrawAfterMarker(retailOam, 0, 0);
            retailOam.FinalizeFrame();
            AssertTrue(retailOam.LastFinalizedSpriteCount < OamBuffer.SpriteCount,
                $"area {areaIndex} retail map-icon lists fit OAM");
            if (downloaded)
                PngWriter.WriteRgba(Path.GetFullPath($"csharp/test-temp/file-select-map/room-{areaIndex}.png"), 256, 224, pixels);
        }
    }

    private static void VerifyMapCancelPresentation()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var saves = new SuperMetroidSaveRam(bus, RetailPresentationFixture());
        var snapshot = new SuperMetroidSaveSnapshot { Area = 4, SaveStation = 0, Health = 99, MaxHealth = 99 };
        snapshot.MapStationBytes[4] = 1;
        snapshot.UsedSaveStationBytes[8] = 1;
        saves.SaveSlot(0, snapshot);
        var slot = saves.ReadSlot(0)!;
        var cancel = new FileSelectMapMenuState(bus, new SuperMetroid.Core.Audio.CartridgeAudioState(), slot, 0, RetailPresentationFixture());
        var control = new FileSelectMapMenuState(bus, new SuperMetroid.Core.Audio.CartridgeAudioState(), slot, 0, RetailPresentationFixture());
        foreach (var menu in new[] { cancel, control })
        {
            for (int i = 0; i < 48; i++) menu.Step(0);
            AssertEqual(FileSelectMapNavigationPhase.Area, menu.Phase, "cancel fixture finishes area reveal");
            menu.Step(0x1000);
            for (int i = 0; i < 54; i++) menu.Step(0);
            AssertEqual(FileSelectMapNavigationPhase.Room, menu.Phase, "cancel fixture reaches live room map");
        }
        cancel.Step(0x8000);
        control.Step(0);
        AssertTrue(cancel.Render().SequenceEqual(control.Render()),
            "room cancel must retain the map/icons drawn before input until next coroutine call");
        var system = new Bank80SystemState();
        system.LoadMapStationBytes(slot.MapStationBytes);
        Rgba32[] roomFrame = new FileSelectRoomMapGraphics(bus, system, AreaId.Maridia, mapPresentation: RetailPresentationFixture()).RenderFrameOnly();
        var maps = RetailPresentationFixture();
        var areaGraphics = new FileSelectAreaMapGraphics(bus, 4, maps.Tiles, maps.Palettes, maps.Screens, maps.WorldArtwork, maps.Sprites);
        areaGraphics.BindLabels(maps.Labels);
        Rgba32[] area = areaGraphics.Render(new ushort[] { 0, 0, 0, 0, 1, 0 });
        for (int tick = 1; tick <= 4; tick++)
        {
            cancel.Step(0);
            AssertTrue(cancel.Render().SequenceEqual(roomFrame), "return transfer stages retain only BG2 frame");
        }
        cancel.Step(0);
        Rgba32[] setup = cancel.Render();
        for (int y = 0; y < 224; y++)
        for (int x = 0; x < 256; x++)
            AssertEqual(x >= 127 && x <= 129 && y >= 111 && y < 113 ? area[y * 256 + x] : roomFrame[y * 256 + x],
                setup[y * 256 + x], "return setup temporarily exposes native centered entry window");
        cancel.Step(0);
        AssertTrue(cancel.Render().SequenceEqual(FileSelectMapWindowCompositor.Composite(area, roomFrame,
                FileSelectMapWindow.CreateReturn(bus, 4, maps.Labels))),
            "return contraction uses normal area backdrop addition, unlike forward transition");
    }

    private static void VerifyFileSelectMapAnimations()
    {
        var bus = new TestAddressSpace();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var arrows = new Dictionary<string, MapArrowEntry>();
        foreach (MapScrollDirection direction in new[] { MapScrollDirection.Left, MapScrollDirection.Right, MapScrollDirection.Up, MapScrollDirection.Down })
            arrows[direction.ToString()] = new() { X = 32 + ((int)direction - 1) * 24, Y = 79, DurationTicks = [3, 2] };
        using var arrowJson = new MemoryStream();
        MapArrowPresentation.Write(arrowJson, new() { Version = 1, Arrows = arrows });
        arrowJson.Position = 0;
        var animations = new FileSelectMapAnimations(bus, MapArrowPresentation.Load(arrowJson));
        var paletteFrames = Enumerable.Range(0, 2).Select(frame => new MapPaletteCycleFrame
        {
            DurationTicks = frame == 0 ? 3 : 2,
            Colors = Enumerable.Range(0, 16).Select(color => new PaletteRgb5
            {
                Red = (100 + frame * 16 + color) & 31,
                Green = ((100 + frame * 16 + color) >> 5) & 31, Blue = 0,
            }).ToArray(),
        }).ToArray();
        using var paletteJson = new MemoryStream();
        MapPaletteCycle.Write(paletteJson, new() { Version = 1, Frames = paletteFrames });
        paletteJson.Position = 0;
        animations.BindPalette(MapPaletteCycle.Load(paletteJson));
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var artwork = SuperMetroid.AssetExtraction.MapSpriteExtractor.Extract(retail);
        var spriteDocument = System.Text.Json.JsonSerializer.Deserialize<MapSpriteDocument>(artwork[MapSpriteFormat.JsonFile], options)!;
        foreach (MapScrollDirection direction in new[] { MapScrollDirection.Left, MapScrollDirection.Right, MapScrollDirection.Up, MapScrollDirection.Down })
        {
            ushort id = MapArrowDefinitions.SpriteBase(direction);
            string name = MapSpriteDefinitions.Frames.ToArray().Single(frame => frame.NativeId == id).Name;
            spriteDocument.Frames[name] = [new SpriteVisualPart { OffsetX = 0, OffsetY = 0,
                TileColumn = 0, TileRow = 1, Size = 8, Priority = 3, Palette = null, FlipX = false, FlipY = false }];
        }
        var sprites = MapSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(spriteDocument, options)),
            new MemoryStream(artwork[MapSpriteFormat.PngFile]));
        int ArrowPhase(int index)
        {
            const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var states = (Array)typeof(FileSelectMapAnimations).GetField("arrows", fields)!.GetValue(animations)!;
            object state = states.GetValue(index)!;
            return (int)state.GetType().GetField("Frame")!.GetValue(state)!;
        }
        var cgram = new SnesCgram();
        cgram.SetColor(175, 77); cgram.SetColor(192, 88);
        AssertTrue(!animations.StepPalette(cgram), "initial map palette tick advances to frame one without loop sound");
        for (int color = 0; color < 16; color++)
            AssertEqual(116 + color, cgram.Colors[176 + color], "map palette copies frame one including transparent entry");
        AssertTrue(!animations.StepPalette(cgram), "palette delay does not queue a sound");
        AssertTrue(animations.StepPalette(cgram), "palette sentinel queues exactly one loop sound");
        for (int color = 0; color < 16; color++)
            AssertEqual(100 + color, cgram.Colors[176 + color], "palette sentinel restores frame zero");
        AssertEqual(77, cgram.Colors[175], "palette update preserves preceding color");
        AssertEqual(88, cgram.Colors[192], "palette update preserves following color");
        animations.StepArrows(direction => direction == MapScrollDirection.Left);
        OamBuffer Draw()
        {
            var oam = new OamBuffer(); oam.BeginFrame(); animations.DrawArrows(oam, sprites); oam.FinalizeFrame(); return oam;
        }
        OamBuffer first = Draw();
        AssertEqual(1, first.LastFinalizedSpriteCount, "only available arrows draw");
        AssertEqual(32, first.LowTable[0], "arrow uses cartridge X");
        AssertEqual(79, first.LowTable[1], "arrow subtracts native one-pixel Y offset");
        AssertEqual(1, ArrowPhase(0), "zero-initialized arrow timer advances before drawing");
        AssertEqual(0x10, first.LowTable[2], "installed arrow shape reaches OAM");
        AssertTrue(first.LowTable.ToArray().SequenceEqual(Draw().LowTable.ToArray()), "repaint does not advance arrow animation");
        animations.StepArrows(direction => direction == MapScrollDirection.Left);
        AssertEqual(1, ArrowPhase(0), "arrow holds its full two-tick delay");
        animations.StepArrows(direction => direction == MapScrollDirection.Left);
        AssertEqual(0, ArrowPhase(0), "arrow sentinel wraps to frame zero");
        for (int i = 0; i < 10; i++) animations.StepArrows(_ => false);
        AssertEqual(0, Draw().LastFinalizedSpriteCount, "unavailable arrows disappear");
        animations.StepArrows(direction => direction is MapScrollDirection.Left or MapScrollDirection.Right);
        OamBuffer resumed = Draw();
        AssertEqual(0, ArrowPhase(0), "hidden arrow timer pauses");
        AssertEqual(1, ArrowPhase(1), "newly visible arrow owns an independent timer");
        AssertEqual(2, resumed.LastFinalizedSpriteCount, "both available direction shapes draw");
    }

    private static void VerifyFileSelectMapIcons()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var system = new Bank80SystemState();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var landmarks = MapLandmarkDefinitions.AllIds().ToDictionary(id => id, _ => new MapLabelPoint(511, 255));
        landmarks[MapLandmarkDefinitions.Bosses(AreaId.Brinstar).ToArray().First(id => id is not null)!] = new(32, 40);
        var destination = MapLandmarkDefinitions.Elevators(AreaId.Brinstar).ToArray().First(label =>
            label.Destination == AreaId.Crateria);
        landmarks[destination.Id] = new(96, 112);
        using var landmarkJson = new MemoryStream();
        MapLandmarkLayout.Write(landmarkJson, new() { Version = 1, Markers = landmarks });
        landmarkJson.Position = 0;
        var stations = MapStationDiscoveryRules.All.ToArray().ToDictionary(rule => rule.Id, _ => new MapLabelPoint(511, 255));
        var station = MapStationDiscoveryRules.Get(AreaId.Brinstar, MapStationKind.Missile).First();
        stations[station.Id] = new(64, 80);
        using var stationJson = new MemoryStream();
        MapStationLayout.Write(stationJson, new() { Version = 1, Markers = stations });
        stationJson.Position = 0;
        var artwork = SuperMetroid.AssetExtraction.MapSpriteExtractor.Extract(bus);
        var document = System.Text.Json.JsonSerializer.Deserialize<MapSpriteDocument>(artwork[MapSpriteFormat.JsonFile], options)!;
        foreach (ushort id in new ushort[] { 9, 0x62, 0x0b, 0x59, 0x5b, 0x5d })
        {
            string name = MapSpriteDefinitions.Frames.ToArray().Single(frame => frame.NativeId == id).Name;
            document.Frames[name] = [new SpriteVisualPart { OffsetX = 0, OffsetY = 0,
                TileColumn = (id == 0x59 ? 0x50 : id) % 16, TileRow = (id == 0x59 ? 0x50 : id) / 16, Size = 8, Priority = 3,
                Palette = null, FlipX = false, FlipY = false }];
        }
        var sprites = MapSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)),
            new MemoryStream(artwork[MapSpriteFormat.PngFile]));
        var icons = new FileSelectMapIcons(system, AreaId.Brinstar);
        icons.BindLandmarks(MapLandmarkLayout.Load(landmarkJson));
        icons.BindStations(MapStationLayout.Load(stationJson));
        icons.BindSprites(sprites);
        OamBuffer Draw()
        {
            var oam = new OamBuffer();
            oam.BeginFrame();
            icons.DrawBeforeMarker(oam, 8, 16);
            icons.DrawAfterMarker(oam, 8, 16);
            oam.FinalizeFrame();
            return oam;
        }
        AssertEqual(0, Draw().LastFinalizedSpriteCount, "unexplored map hides station, live boss and destination icons");
        system.SetAreaMapAcquired(AreaId.Brinstar);
        OamBuffer downloaded = Draw();
        AssertEqual(6, downloaded.LastFinalizedSpriteCount, "download emits one boss and all five Brinstar destinations, without unvisited refill");
        AssertEqual(9, downloaded.LowTable[2], "live boss uses native marker");
        AssertEqual(0x50, downloaded.LowTable[6], "destination selects its authored spritemap");
        AssertEqual(88, downloaded.LowTable[4], "destination subtracts map X scroll");
        AssertEqual(96, downloaded.LowTable[5], "destination subtracts map Y scroll");
        AssertEqual(0x30, downloaded.LowTable[7], "destination uses palette zero and retained priority");
        system.MarkExploredMapTile(AreaId.Brinstar, station.CellX, station.CellY);
        OamBuffer explored = Draw();
        AssertEqual(7, explored.LastFinalizedSpriteCount, "visited refill adds one icon to the boss and five destinations");
        AssertEqual(0x0b, explored.LowTable[6], "refill draws between boss and destination");
        AssertEqual(56, explored.LowTable[4], "refill exact scrolled X");
        AssertEqual(64, explored.LowTable[5], "refill exact scrolled Y");
        system.SetBossBits(AreaId.Brinstar, BossBits.AreaBoss);
        OamBuffer defeated = Draw();
        AssertEqual(8, defeated.LastFinalizedSpriteCount, "defeated boss adds its overlay ahead of the dim marker, refill and five destinations");
        AssertEqual(0x62, defeated.LowTable[2], "defeated overlay precedes boss marker");
        AssertEqual(9, defeated.LowTable[6], "defeated boss retains marker identity");
        AssertEqual(0x3c, defeated.LowTable[7], "defeated boss changes to palette six");
    }

    private static void VerifySavedGameMapFrontend()
    {
        VerifySavedGameMapFrontend(4);
        VerifySavedGameMapFrontend(6);
    }

    private static void VerifySavedGameMapFrontend(ushort savedArea)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var saves = new SuperMetroidSaveRam(bus, RetailPresentationFixture());
        var snapshot = new SuperMetroidSaveSnapshot { Area = savedArea, SaveStation = 0, Health = 99, MaxHealth = 99,
            LoadingGameState = savedArea == 6 ? SaveLoadingGameStates.CeresElevatorArrival : SaveLoadingGameStates.MainGame };
        snapshot.UsedSaveStationBytes[savedArea * 2] = 1;
        if (savedArea < 6)
        {
            var station = SuperMetroid.AssetExtraction.LoadStationEntryImporter.Load(bus, (AreaId)savedArea, 0);
            var room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, station.RoomPointer);
            int x = room.MapX + (station.SamusX >> 8);
            int y = room.MapY + (station.SamusY >> 8) + 1;
            snapshot.ExploredMapBytes[savedArea * 256 + AreaMapLayout.GetBitByteIndex(x, y)] |= AreaMapLayout.GetBitMask(x);
        }
        saves.SaveSlot(0, snapshot);
        saves.SelectSlot(0);
        var game = CreateRetailGameFixture(bus, gameOptions: null, renderGameplayFrames: false);
        game.BindMapPresentation(RetailPresentationFixture());
        FrontendFrame frame = game.Step(0);
        int expansionFrames = 0, returnFrames = 0;
        frame = game.Step(0x1000);
        Until(() => frame.Phase == nameof(TitleSequencePhase.TitleScreen), 150);
        frame = game.Step(0x1000);
        Until(() => frame.GameState == SuperMetroidGameState.FileSelectMenus, 150);
        // Native entry dispatches and fade-in end on the 35th update.
        Until(() => frame.Phase == nameof(FileSelectPhase.Main), 40);
        frame = game.Step(0x0080);
        Until(() => frame.GameState == SuperMetroidGameState.GameOptionsMenu, 200);
        for (int i = 0; i < 16; i++) frame = game.Step(0);
        frame = game.Step(0x0080);
        if (savedArea == 6)
        {
            Until(() => game.RuntimeForVerification is not null, 200);
            AssertEqual(AreaId.Ceres, game.RuntimeForVerification!.ActiveRoom!.AreaIndex,
                "Ceres checkpoint bypasses Zebes map and retains elevator arrival");
            AssertTrue(game.RuntimeForVerification.CeresElevatorArrival is not null,
                "Ceres checkpoint still owns its arrival sequence");
            return;
        }
        Until(() => frame.GameState == SuperMetroidGameState.FileSelectMap, 200);
        for (int i = 0; i < 70; i++)
        {
            frame = game.Step(0x1000);
            if (i == 32) Capture("live-entry-reveal");
        }
        AssertEqual("Area", frame.Phase, "holding confirm during initial reveal cannot skip area selection");
        Capture("live-area");
        AssertTrue(game.RuntimeForVerification is null, "saved-game map does not construct gameplay before confirmation");
        frame = game.Step(0x8000);
        Until(() => frame.GameState == SuperMetroidGameState.GameOptionsMenu, 30);
        for (int i = 0; i < 16; i++) frame = game.Step(0);
        frame = game.Step(0x0080);
        Until(() => frame.GameState == SuperMetroidGameState.FileSelectMap && frame.Phase == "Area", 200);
        for (int i = 0; i < 10; i++) frame = game.Step(0);
        frame = game.Step(0x1000);
        Until(() => frame.Phase == "Room", 80);
        for (int i = 0; i < 15; i++) frame = game.Step(0);
        PngWriter.WriteRgba(Path.GetFullPath("csharp/test-temp/file-select-map/live-maridia-room.png"), 256, 224, frame.Pixels);
        AssertTrue(game.RuntimeForVerification is null, "room map does not implicitly load the save");
        frame = game.Step(0x8000);
        Until(() => frame.Phase == "Area", 80);
        frame = game.Step(0x1000);
        Until(() => frame.Phase == "Room", 80);
        frame = game.Step(0x1000);
        Until(() => game.RuntimeForVerification is not null, 100);
        AssertEqual(SuperMetroid.AssetExtraction.LoadStationEntryImporter.Load(bus, AreaId.Maridia, 0).RoomPointer,
            game.RuntimeForVerification!.ActiveRoom!.Pointer, "second map confirmation loads selected SRAM station");
        void Until(Func<bool> predicate, int limit)
        {
            for (int tick = 0; tick < limit && !predicate(); tick++)
            {
                frame = game.Step(0);
                if (frame.Phase == "ExpandingWindow" && ++expansionFrames == 20) Capture("live-area-to-room");
                if (frame.Phase == "AreaReturnRequested" && ++returnFrames == 20) Capture("live-room-to-area");
            }
            AssertTrue(predicate(), $"saved map frontend reached requested boundary; current {frame.GameState}/{frame.Phase}");
        }
        void Capture(string name) => PngWriter.WriteRgba(
            Path.GetFullPath($"csharp/test-temp/file-select-map/{name}.png"), 256, 224, frame.Pixels);
    }

    private static void VerifyFileSelectMapNavigation()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ushort confirm in new ushort[] { 0x1000, 0x0080 })
        {
            var navigation = new FileSelectMapNavigation(bus, 4, confirm);
            navigation.BindLabels(RetailPresentationFixture().Labels);
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.Area, navigation.Phase,
                "options confirmation carried into map must not immediately confirm the area");
            navigation.Step(0x0f20);
            AssertEqual(FileSelectMapNavigationPhase.Area, navigation.Phase, "non-debug direction/select cannot cycle area");
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.PreparingWindow, navigation.Phase, "area confirm preserves prep boundary");
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.ExpandingWindow, navigation.Phase, "map prep initializes expansion separately");
            for (int frame = 0; frame < 52; frame++) navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.InitializingRoom, navigation.Phase,
                "Maridia window completes before installing room map");
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.Room, navigation.Phase, "room map requires separate confirmation");
            for (int frame = 0; frame < 20; frame++) navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.Room, navigation.Phase, "holding confirm cannot skip room map");
            navigation.Step(0);
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.LoadRequested, navigation.Phase, "fresh second confirm requests gameplay");
            navigation.Step(0x8000);
            AssertEqual(FileSelectMapNavigationPhase.LoadRequested, navigation.Phase, "load request remains pending until frontend handles it");
        }
        var cancel = new FileSelectMapNavigation(bus, 4);
        cancel.BindLabels(RetailPresentationFixture().Labels);
        cancel.Step(0x9180);
        AssertEqual(FileSelectMapNavigationPhase.Area, cancel.Phase,
            "native non-debug direction branch suppresses simultaneous cancel and confirm");
        cancel.Step(0);
        cancel.Step(0x9080);
        AssertEqual(FileSelectMapNavigationPhase.OptionsRequested, cancel.Phase, "B takes precedence over confirm on area map");
        var back = new FileSelectMapNavigation(bus, 4);
        back.BindLabels(RetailPresentationFixture().Labels);
        back.Step(0x1000);
        for (int frame = 0; frame < 54; frame++) back.Step(0);
        back.Step(0x8000);
        AssertEqual(FileSelectMapNavigationPhase.AreaReturnRequested, back.Phase, "room cancel requests animated return");
        back.Step(0x1000);
        back.CompleteAreaReturn();
        back.Step(0x1000);
        AssertEqual(FileSelectMapNavigationPhase.Area, back.Phase, "return transition consumes held input without deferred confirmation");
        AssertThrows<InvalidOperationException>(() => back.CompleteAreaReturn(), "unsolicited area-return completion fails");
    }

    private static void VerifyFileSelectMapScroll()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AreaMapCartridgeData map = SuperMetroid.AssetExtraction.AreaMapImporter.Load(bus, AreaId.Crateria);
        var system = new Bank80SystemState();
        system.MarkExploredMapTile(AreaId.Crateria, 0, 0);
        system.MarkExploredMapTile(AreaId.Crateria, 63, 31);
        ushort[] inputs = [0x0200, 0x0100, 0x0800, 0x0400];
        for (int direction = 1; direction <= 4; direction++)
        {
            var scroll = new FileSelectMapScroll(bus, map, system, 240, 128);
            AssertEqual(124, scroll.Horizontal, "room map centers native horizontal bounds");
            AssertEqual(32, scroll.Vertical, "room map rounds the 112px center offset down to a tile boundary");
            AssertEqual(24, scroll.MinimumY, "room-select upper arrow boundary gets delayed 24px adjustment");
            for (int frame = 1; frame <= 8; frame++)
            {
                bool sound = scroll.Step(frame == 1 ? inputs[direction - 1] : (ushort)0);
                int delta = frame >= 4 ? 8 : 0;
                AssertEqual(124 + (direction == 1 ? -delta : direction == 2 ? delta : 0), scroll.Horizontal,
                    "map scroll horizontal exact pulse frame");
                AssertEqual(32 + (direction == 3 ? -delta : direction == 4 ? delta : 0), scroll.Vertical,
                    "map scroll vertical exact pulse frame");
                AssertEqual(frame == 8, sound, "map scroll sound occurs at completion, not displacement");
            }
            AssertEqual(MapScrollDirection.None, scroll.Direction, "released direction completes its accepted step");
        }
        var precedence = new FileSelectMapScroll(bus, map, system, 240, 128);
        precedence.Step(0x0f00);
        AssertEqual(MapScrollDirection.Left, precedence.Direction, "simultaneous map input uses native arrow order");
        var empty = new FileSelectMapScroll(bus, map, new Bank80SystemState(), 216, 48);
        AssertEqual(208, empty.MinimumX, "empty map uses retail left default");
        AssertEqual(224, empty.MaximumX, "empty map uses retail right default");
        AssertEqual(32, empty.MinimumY, "empty map uses top default then file-select adjustment");
        AssertEqual(88, empty.MaximumY, "empty map uses retail bottom default");
        // Repeated stepping must stop at the native inequality, not run past the edge.
        for (int frame = 0; frame < 1000; frame++) precedence.Step(0x0200);
        AssertTrue(!precedence.CanScroll(MapScrollDirection.Left), "held left stops at map boundary");
        ushort stoppedX = precedence.Horizontal;
        for (int frame = 0; frame < 16; frame++) precedence.Step(0x0200);
        AssertEqual(stoppedX, precedence.Horizontal, "held input cannot scroll beyond unavailable arrow");
    }

    private static void VerifyFileSelectStationMarker()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var positions = System.Text.Json.JsonSerializer.Deserialize<MapSaveMarkerDocument>(
            SuperMetroid.AssetExtraction.MapSaveMarkerExtractor.Extract(bus), options)!;
        positions.Markers[MapSaveMarkerDefinitions.Id(AreaId.Crateria, 0)] = new(100, 80);
        using var positionJson = new MemoryStream();
        MapSaveMarkerLayout.Write(positionJson, positions);
        positionJson.Position = 0;
        var layout = MapSaveMarkerLayout.Load(positionJson);
        var artwork = SuperMetroid.AssetExtraction.MapSpriteExtractor.Extract(bus);
        var document = System.Text.Json.JsonSerializer.Deserialize<MapSpriteDocument>(artwork[MapSpriteFormat.JsonFile], options)!;
        // Preserve the original fixture's one-part (-2,-3) sprite and priority three.
        foreach (ushort id in new ushort[] { 0x12, 0x5f, 0x60, 0x61 })
        {
            string name = MapSpriteDefinitions.Frames.ToArray().Single(frame => frame.NativeId == id).Name;
            document.Frames[name] = [new SpriteVisualPart { OffsetX = -2, OffsetY = -3,
                TileColumn = 1, TileRow = 0, Size = 8, Priority = 3, Palette = null,
                FlipX = false, FlipY = false }];
        }
        var sprites = MapSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)),
            new MemoryStream(artwork[MapSpriteFormat.PngFile]));
        var marker = new FileSelectStationMarker(bus, AreaId.Crateria, 0, layout);
        int[] expectedFrames = [0x60, 0x61, 0x60, 0x5f, 0x60, 0x61, 0x60, 0x5f];
        int[] durations = [4, 8, 4, 8, 4, 8, 4, 8];
        for (int phase = 0; phase < durations.Length; phase++)
        for (int tick = 0; tick < durations[phase]; tick++)
        {
            marker.Step();
            AssertEqual(expectedFrames[phase], marker.SpritemapId, "station marker exact animation cadence");
            bool backing = phase < 3 || phase >= 7;
            AssertEqual(backing, marker.ShowBacking, "station marker alternates backing at loop boundary");
            var oam = new OamBuffer();
            oam.BeginFrame();
            marker.Draw(bus, oam, 24, 16, sprites);
            oam.FinalizeFrame();
            AssertEqual(backing ? 2 : 1, oam.LastFinalizedSpriteCount, "marker backing OAM emission");
            AssertEqual(74, oam.LowTable[0], "marker subtracts horizontal scroll and sprite offset");
            AssertEqual(61, oam.LowTable[1], "marker subtracts vertical scroll and sprite offset");
            AssertEqual(0x3e, oam.LowTable[3], "marker retains sprite priority with palette seven");
            byte[] first = oam.LowTable.ToArray();
            oam.BeginFrame();
            marker.Draw(bus, oam, 24, 16, sprites);
            oam.FinalizeFrame();
            AssertTrue(first.SequenceEqual(oam.LowTable.ToArray()), "repainting station marker does not advance animation");
        }
        int unused = Enumerable.Range(0, 16).First(index => !MapSaveMarkerDefinitions.IsUsable(AreaId.Crateria, index));
        AssertThrows<InvalidDataException>(() => new FileSelectStationMarker(bus, AreaId.Crateria, unused, layout),
            "unused station map entry rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(bus, AreaId.Crateria, 16, layout),
            "station lookup cannot cross the sixteen-slot domain");
    }
}
