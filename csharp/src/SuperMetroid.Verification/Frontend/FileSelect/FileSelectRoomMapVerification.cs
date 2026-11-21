using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Rooms;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Progression;

/// <summary>Checks file-select room-map rendering, controls, animation, markers, and save loading against native behavior.</summary>
internal static class FileSelectRoomMapVerification
{
    /// <summary>Checks every area map projection and frame word against ROM, then verifies the visible viewport and icon capacity.</summary>
    internal static void VerifyFileSelectRoomMapGraphics()
    {
        Suite(nameof(VerifyFileSelectStationMarker), () => VerifyFileSelectStationMarker());
        Suite(nameof(VerifyFileSelectMapScroll), () => VerifyFileSelectMapScroll());
        Suite(nameof(VerifyFileSelectMapNavigation), () => VerifyFileSelectMapNavigation());
        Suite(nameof(VerifySavedGameMapFrontend), () => VerifySavedGameMapFrontend());
        Suite(nameof(VerifyFileSelectMapIcons), () => VerifyFileSelectMapIcons());
        Suite(nameof(VerifyFileSelectMapAnimations), () => VerifyFileSelectMapAnimations());
        Suite(nameof(VerifyMapCancelPresentation), () => VerifyMapCancelPresentation());
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (AreaId area in AreaIds.Zebes)
        foreach (bool downloaded in new[] { false, true })
        {
            var system = new SharedGameState(new SharedRandom());
            ImportedAreaMap map = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, area);
            byte[] original = map.RawTilemapBytes.ToArray();
            // Deliberately cross both native pages and explore cells outside the map
            // station's mask: explored cells win, even for secret rooms.
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 64; x++)
                if ((x + y) % 7 == 0) system.MarkExploredMapTile(area, new(checked((ushort)x), checked((ushort)y)));
            if (downloaded) system.SetAreaMapAcquired(area);
            var graphics = new FileSelectRoomMapGraphics(system, SuperMetroid.Verification.Harness.VerificationZebesAreas.Of(area), screenContent: RetailPresentationFixture(), sheets: MenuSheetFixtures.For(RetailPresentationFixture()));
            ShownAreaMap pause = AreaMapProjection.Project(map, system);
            // The room map's image is built once and every frame word read from it.
            var shownRoomMap = MenuScreenFixtures.Vram(graphics, RetailPresentationFixture());
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 64; x++)
            {
                var at = new Position16<MapTiles>(checked((ushort)x), checked((ushort)y));
                int offset = AreaMapPages.GetTilemapWordIndex(at) * 2;
                MapCell stockCell = MapCellCharacters.Decode(new MapTileWord(BinaryPrimitives.ReadUInt16LittleEndian(original.AsSpan(offset))));
                bool explored = (x + y) % 7 == 0;
                // Independent transcription of $82:9517's two branches, in the cells they show (#1513): an explored cell
                // explored, a station-revealed one unexplored, and any other the empty cell, which the cartridge draws
                // with an empty character whether or not the area's map is downloaded.
                ShownMapCell expected = explored ? ShownMapCell.Explored(stockCell)
                    : downloaded && map.IsRevealedByMapStation(at) ? ShownMapCell.Unexplored(stockCell)
                    : ShownMapCell.Unexplored(MapCell.Empty);
                AssertEqual(expected, graphics.ShownCells.Cells[y * 64 + x],
                    $"room-select area {area} downloaded={downloaded} cell {x},{y}");
                AssertEqual(expected, pause.Cells[y * 64 + x], "shared builder shows the pause map's hidden cells empty");
                AssertEqual(explored, system.IsMapTileExplored(area, new(checked((ushort)x), checked((ushort)y))), "map projection does not mark exploration");
            }
            AssertTrue(original.SequenceEqual(map.RawTilemapBytes), "map projection preserves cartridge map");
            ushort label = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.FileSelectMap.AreaLabelPointers + SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area) * 2);
            for (int word = 0; word < 1024; word++)
            {
                ushort expected = word < 800
                    ? RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.FileSelectMap.RoomFrame + word * 2)
                    : word < 960
                        ? RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.FileSelectMap.RoomFrameFooter + (word - 799) * 2)
                        : (ushort)0x2801;
                if (word is >= 170 and < 182)
                    expected = (ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x820000 | (label + (word - 170) * 2)) & 0xefff);
                AssertEqual(expected, shownRoomMap.ReadWord((ushort)(0x5800 + word)), "native room-select frame and area label");
            }
            RgbaPixel[] pixels = graphics.RenderBackgrounds(new Offset16<MapPixels>(0, -40));
            AssertEqual(256 * 224, pixels.Length, "room map visible viewport");
            AssertTrue(pixels.All(pixel => pixel.A == 255), "room map has opaque backdrop");
            var retailIcons = new FileSelectMapIcons(system, area, RetailPresentationFixture().Maps.Stations, RetailPresentationFixture().Maps.Landmarks,
                MenuSheetFixtures.MapSprites(RetailImportPresentationFixture().Sprites));
            var retailOam = ScreenSpritesFixtures.Blank();
            retailOam.BeginFrame();
            retailIcons.DrawBeforeMarker(retailOam, new(0, 0));
            retailIcons.DrawAfterMarker(retailOam, new(0, 0));
            retailOam.FinalizeFrame();
            AssertTrue(retailOam.LastFinalizedSpriteCount < OamBuffer.SpriteCount,
                $"area {area} retail map-icon lists fit OAM");

        }
    }

    /// <summary>Checks the retained room frame and native center-window exposure during cancellation back to the area map.</summary>
    internal static void VerifyMapCancelPresentation()
    {
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        var snapshot = SaveSnapshotFixtures.NewGame with { Checkpoint = SuperMetroid.Core.Progression.LoadStationDefinitions.GlassTunnelSave };
        snapshot.Progress.SetAreaMap(SuperMetroid.Core.Progression.AreaId.Maridia, acquired: true);
        snapshot.Progress.UsedStations.Add(SuperMetroid.Core.Progression.LoadStationDefinitions.GlassTunnelSave);
        saves.SaveSlot(SaveSlot.A, snapshot);
        var slot = saves.ReadSlot(SaveSlot.A);
        var cancel = new FileSelectMapMenuState(SuperMetroid.Core.Audio.AudioQueues.Standalone(), new SharedRandom(), (slot ?? throw new InvalidOperationException("Verification requires slot.")), 0, RetailPresentationFixture(), MenuSheetFixtures.For(RetailPresentationFixture()));
        var control = new FileSelectMapMenuState(SuperMetroid.Core.Audio.AudioQueues.Standalone(), new SharedRandom(), slot, 0, RetailPresentationFixture(), MenuSheetFixtures.For(RetailPresentationFixture()));
        foreach (var menu in new[] { cancel, control })
        {
            for (int i = 0; i < 48; i++) menu.Step(0);
            AssertEqual(FileSelectMapNavigationPhase.Area, menu.Phase, "cancel fixture finishes area reveal");
            menu.Step(ControllerButton.Start);
            for (int i = 0; i < 54; i++) menu.Step(0);
            AssertEqual(FileSelectMapNavigationPhase.Room, menu.Phase, "cancel fixture reaches live room map");
        }
        cancel.Step(ControllerButton.B);
        control.Step(0);
        AssertTrue(cancel.Render().SequenceEqual(control.Render()),
            "room cancel must retain the map/icons drawn before input until next coroutine call");
        var system = new SharedGameState(new SharedRandom());
        system.SetAreaMapAcquired(AreaId.Maridia);
        AssertTrue(slot.Progression.HasAreaMap(AreaId.Maridia), "the saved slot holds the Maridia map");
        RgbaPixel[] roomFrame = new FileSelectRoomMapGraphics(system, ZebesArea.Maridia, screenContent: RetailPresentationFixture(), sheets: MenuSheetFixtures.For(RetailPresentationFixture())).RenderFrameOnly();
        var maps = RetailPresentationFixture();
        var areaGraphics = new FileSelectAreaMapGraphics(AreaId.Maridia, maps, MenuSheetFixtures.For(maps));
        RgbaPixel[] area = areaGraphics.Render(new ushort[] { 0, 0, 0, 0, 1, 0 });
        for (int tick = 1; tick <= 4; tick++)
        {
            cancel.Step(0);
            AssertTrue(cancel.Render().SequenceEqual(roomFrame), "return transfer stages retain only BG2 frame");
        }
        cancel.Step(0);
        RgbaPixel[] setup = cancel.Render();
        for (int y = 0; y < 224; y++)
        for (int x = 0; x < 256; x++)
            AssertEqual(x >= 127 && x <= 129 && y >= 111 && y < 113 ? area[y * 256 + x] : roomFrame[y * 256 + x],
                setup[y * 256 + x], "return setup temporarily exposes native centered entry window");
        cancel.Step(0);
        AssertTrue(cancel.Render().SequenceEqual(FileSelectMapWindowCompositor.Composite(area, roomFrame,
                FileSelectMapWindow.CreateReturn(ZebesArea.Maridia, maps.Maps.Labels))),
            "return contraction uses normal area backdrop addition, unlike forward transition");
    }

    /// <summary>Checks map palette cycling and the arrows each showing only while the map can scroll its way.</summary>
    internal static void VerifyFileSelectMapAnimations()
    {
        var bus = new TestAddressSpace();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var arrows = new Dictionary<string, MapArrowEntry>();
        // The fixture's arrows stand 24 pixels apart from the left one at 32, in the order their directions are listed.
        MapScrollDirection[] arrowOrder = [MapScrollDirection.Left, MapScrollDirection.Right, MapScrollDirection.Up, MapScrollDirection.Down];
        for (int arrow = 0; arrow < arrowOrder.Length; arrow++)
            arrows[arrowOrder[arrow].ToString()] = new() { X = 32 + arrow * 24, Y = 79 };
        using var arrowJson = new MemoryStream();
        MapArrowFile.Write(arrowJson, new() { Version = MapArrowFormat.Version, Arrows = arrows });
        arrowJson.Position = 0;
        var paletteFrames = Enumerable.Range(0, 2).Select(frame => new MapPaletteCycleFrame
        {
            DurationTicks = frame == 0 ? 3 : 2,
            Highlight = new PaletteRgb5 { Red = (100 + frame * 16) & 31, Green = ((100 + frame * 16) >> 5) & 31, Blue = 0 },
        }).ToArray();
        using var paletteJson = new MemoryStream();
        MapPaletteCycleFile.Write(paletteJson, new() { Version = MapPaletteCycleFile.Version, Frames = paletteFrames });
        paletteJson.Position = 0;
        var animations = new FileSelectMapAnimations(MapArrowFile.Read(arrowJson), MapPaletteCycleFile.Read(paletteJson).Timing);
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var artwork = SuperMetroid.AssetExtraction.Frontend.Map.MapSpriteExtractor.Extract(retail);
        var spriteDocument = RequiredJson.Deserialize<MapSpriteDocument>(artwork[MapSpriteFormat.JsonFile], options);
        foreach (MapScrollDirection direction in new[] { MapScrollDirection.Left, MapScrollDirection.Right, MapScrollDirection.Up, MapScrollDirection.Down })
        {
            MapSpriteId id = MapArrowDefinitions.ArrowPictures[direction];
            string name = MapSpriteArtwork.FrameNames.Single(frame => frame.Id == id).Name;
            spriteDocument.Frames[name] = [new SpriteVisualPart { OffsetX = 0, OffsetY = 0,
                TileColumn = 0, TileRow = 1, Size = 8, Depth = SuperMetroid.Core.Assets.ActorDepth.InFront, Palette = null, FlipX = false, FlipY = false }];
        }
        var sprites = MapSpriteArtwork.Read(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(spriteDocument, global::SuperMetroid.Verification.VerificationJson.Info<global::SuperMetroid.AssetExtraction.Frontend.Map.MapSpriteDocument>(options))),
            new MemoryStream(artwork[MapSpriteFormat.PngFile]));
        // The highlighted sprites name the shade of the cycle's current frame, which the import pre-colours from the cycle's
        // colours (#1512, #1597).
        AssertEqual(PictureShading.Shaded(MapSpriteShades.Highlight(MapHighlightFrameConstants.First)),
            animations.HighlightShading, "before the first cycle step the map highlight shows frame zero's shade");
        AssertTrue(!animations.StepHighlight(), "initial map highlight tick advances to frame one without loop sound");
        AssertEqual(PictureShading.Shaded(MapSpriteShades.Highlight(MapHighlightFrame.At(1))), animations.HighlightShading,
            "map highlight shows frame one's shade");
        AssertTrue(!animations.StepHighlight(), "highlight delay does not queue a sound");
        AssertTrue(animations.StepHighlight(), "highlight sentinel queues exactly one loop sound");
        AssertEqual(PictureShading.Shaded(MapSpriteShades.Highlight(MapHighlightFrameConstants.First)), animations.HighlightShading,
            "highlight sentinel returns the highlight to frame zero's shade");
        for (int tick = 0; tick < 3; tick++)
            AssertTrue(!animations.StepHighlight(), "frame zero's hold queues no sound");
        AssertEqual(PictureShading.Shaded(MapSpriteShades.Highlight(MapHighlightFrame.At(1))), animations.HighlightShading,
            "frame zero's three-tick hold ends on frame one before the reset");
        animations.ResetHighlight();
        AssertEqual(PictureShading.Shaded(MapSpriteShades.Highlight(MapHighlightFrameConstants.First)), animations.HighlightShading,
            "resetting the cycle shows frame zero's shade at once");
        animations.StepArrows(direction => direction == MapScrollDirection.Left);
        ScreenSprites Draw()
        {
            var oam = ScreenSpritesFixtures.Blank(); oam.BeginFrame(); animations.DrawArrows(oam, animations.HighlightShading, MenuSheetFixtures.MapSprites(sprites)); oam.FinalizeFrame(); return oam;
        }
        ScreenSprites first = Draw();
        AssertEqual(1, first.LastFinalizedSpriteCount, "only available arrows draw");
        AssertEqual(32, first.LowTable[0], "arrow uses cartridge X");
        AssertEqual(79, first.LowTable[1], "arrow subtracts native one-pixel Y offset");
        AssertEqual(0x10, first.LowTable[2], "installed arrow shape reaches OAM");
        AssertTrue(first.LowTable.SequenceEqual(Draw().LowTable), "repaint does not change the arrows");
        animations.StepArrows(_ => false);
        AssertEqual(0, Draw().LastFinalizedSpriteCount, "unavailable arrows disappear");
        animations.StepArrows(direction => direction is MapScrollDirection.Left or MapScrollDirection.Right);
        ScreenSprites resumed = Draw();
        AssertEqual(2, resumed.LastFinalizedSpriteCount, "both available direction shapes draw");
    }

    /// <summary>Checks exploration, map download, and boss flags against exact icon ordering and scrolled OAM coordinates.</summary>
    internal static void VerifyFileSelectMapIcons()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var system = new SharedGameState(new SharedRandom());
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var landmarks = MapLandmarkFile.Ids().ToDictionary(id => id, _ => new Position16<MapPixels>(511, 255));
        landmarks[MapLandmarks.BossMarkers(AreaId.Brinstar).First().Id] = new(32, 40);
        var destination = MapLandmarks.Elevators(ZebesArea.Brinstar).First(label =>
            label.Destination == MapElevatorDestination.Crateria);
        landmarks[destination.Id] = new(96, 112);
        using var landmarkJson = new MemoryStream();
        MapLandmarkFile.Write(landmarkJson, new() { Version = MapLandmarkFormat.Version, Markers = landmarks });
        landmarkJson.Position = 0;
        var stations = MapStationLayoutFile.Markers.ToDictionary(rule => rule.Id, _ => new Position16<MapPixels>(511, 255));
        var station = MapStationDiscoveryRules.Get(AreaId.Brinstar, MapStationKind.Missile).First();
        stations[station.Id] = new(64, 80);
        using var stationJson = new MemoryStream();
        MapStationLayoutFile.Write(stationJson, new() { Version = 1, Markers = stations });
        stationJson.Position = 0;
        var artwork = SuperMetroid.AssetExtraction.Frontend.Map.MapSpriteExtractor.Extract(bus);
        var document = RequiredJson.Deserialize<MapSpriteDocument>(artwork[MapSpriteFormat.JsonFile], options);
        foreach (ushort id in new ushort[] { 9, 0x62, 0x0b, 0x59, 0x5b, 0x5d })
        {
            string name = MapSpriteArtwork.FrameNames.Single(frame => frame.Id == MapSpriteOrdinals.Spritemaps.Decode(id, "map spritemap ordinal")).Name;
            document.Frames[name] = [new SpriteVisualPart { OffsetX = 0, OffsetY = 0,
                TileColumn = (id == 0x59 ? 0x50 : id) % 16, TileRow = (id == 0x59 ? 0x50 : id) / 16, Size = 8, Depth = SuperMetroid.Core.Assets.ActorDepth.InFront,
                Palette = null, FlipX = false, FlipY = false }];
        }
        var sprites = MapSpriteArtwork.Read(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.Verification.VerificationJson.Info<global::SuperMetroid.AssetExtraction.Frontend.Map.MapSpriteDocument>(options))),
            new MemoryStream(artwork[MapSpriteFormat.PngFile]));
        var icons = new FileSelectMapIcons(system, AreaId.Brinstar, MapStationLayoutFile.Read(stationJson), MapLandmarkFile.Read(landmarkJson),
            MenuSheetFixtures.MapSprites(sprites));
        ScreenSprites Draw()
        {
            var oam = ScreenSpritesFixtures.Blank();
            oam.BeginFrame();
            icons.DrawBeforeMarker(oam, new(8, 16));
            icons.DrawAfterMarker(oam, new(8, 16));
            oam.FinalizeFrame();
            return oam;
        }
        AssertEqual(0, Draw().LastFinalizedSpriteCount, "unexplored map hides station, live boss and destination icons");
        system.SetAreaMapAcquired(AreaId.Brinstar);
        ScreenSprites downloaded = Draw();
        AssertEqual(6, downloaded.LastFinalizedSpriteCount, "download emits one boss and all five Brinstar destinations, without unvisited refill");
        AssertEqual(9, downloaded.LowTable[2], "live boss uses native marker");
        AssertEqual(0x50, downloaded.LowTable[6], "destination selects its authored spritemap");
        AssertEqual(88, downloaded.LowTable[4], "destination subtracts map X scroll");
        AssertEqual(96, downloaded.LowTable[5], "destination subtracts map Y scroll");
        AssertEqual(0x30, downloaded.LowTable[7], "destination uses palette zero and retained priority");
        system.MarkExploredMapTile(AreaId.Brinstar, station.Cell);
        ScreenSprites explored = Draw();
        AssertEqual(7, explored.LastFinalizedSpriteCount, "visited refill adds one icon to the boss and five destinations");
        AssertEqual(0x0b, explored.LowTable[6], "refill draws between boss and destination");
        AssertEqual(56, explored.LowTable[4], "refill exact scrolled X");
        AssertEqual(64, explored.LowTable[5], "refill exact scrolled Y");
        system.RecordBossesDefeated(AreaId.Brinstar, DefeatedBosses.AreaBoss);
        ScreenSprites defeated = Draw();
        AssertEqual(8, defeated.LastFinalizedSpriteCount, "defeated boss adds its overlay ahead of the dim marker, refill and five destinations");
        AssertEqual(0x62, defeated.LowTable[2], "defeated overlay precedes boss marker");
        AssertEqual(9, defeated.LowTable[6], "defeated boss retains marker identity");
        AssertEqual(0x3c, defeated.LowTable[7], "defeated boss changes to palette six");
    }

    /// <summary>Checks that a Zebes save visits the map before gameplay while a Ceres checkpoint resumes its elevator arrival.</summary>
    internal static void VerifySavedGameMapFrontend()
    {
        Suite(nameof(VerifySavedGameMapFrontend), () => VerifySavedGameMapFrontend(AreaId.Maridia));
        Suite(nameof(VerifySavedGameMapFrontend), () => VerifySavedGameMapFrontend(AreaId.Ceres));
    }

    /// <summary>Drives a saved slot through frontend map confirmation or the Ceres direct-load path.</summary>
    private static void VerifySavedGameMapFrontend(AreaId savedArea)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        var snapshot = SaveSnapshotFixtures.NewGame with { Checkpoint = SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Decode(savedArea, 0, "room map fixture"),
            StartupRoute = savedArea == AreaId.Ceres ? SaveStartupRoute.CeresElevatorArrival : SaveStartupRoute.MainGame ,
            Inventory = SaveSnapshotFixtures.NewGame.Inventory with { Health = 99, MaxHealth = 99 },
        };
        snapshot.Progress.UsedStations.Add(snapshot.Checkpoint);
        if (savedArea != AreaId.Ceres)
        {
            var station = SuperMetroid.TestSupport.VerificationImporters.LoadStationEntryImporter.Load(bus, savedArea, 0);
            var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, (station.Room ?? throw new InvalidDataException("Station 0 has no room.")).Pointer);
            var cell = room.MapPosition + new Offset16<MapTiles>(checked(LittleEndianBytes.HighByte(station.SamusPosition.X)), checked(LittleEndianBytes.HighByte(station.SamusPosition.Y)));
            snapshot.Progress.SetExplored(savedArea, cell, explored: true);
        }
        saves.SaveSlot(SaveSlot.A, snapshot);
        saves.SelectSlot(SaveSlot.A);
        var game = CreateRetailGameFixture(saves, assists: null, renderGameplayFrames: false);
        game.BindArtwork(game.Artwork with
        {
            ScreenContent = RetailPresentationFixture(),
        });
        FrontendFrame frame = game.Step(0);
        frame = game.Step(ControllerButton.Start);
        Until(() => frame.Phase == nameof(TitleSequencePhase.TitleScreen), 150);
        frame = game.Step(ControllerButton.Start);
        Until(() => game.IsAt(FileSelectStep.SaveSlots), 150);
        // Native entry dispatches and fade-in end on the 35th update.
        Until(() => frame.Phase == nameof(FileSelectPhase.Main), 40);
        frame = game.Step(ControllerButton.A);
        Until(() => game.IsAt(FileSelectStep.Options), 200);
        // Options indexes zero and one, fifteen fade-in levels, then the interactive index.
        for (int i = 0; i < 18; i++) frame = game.Step(0);
        frame = game.Step(ControllerButton.A);
        if (savedArea == AreaId.Ceres)
        {
            Until(() => game.RuntimeForVerification is not null, 200);
            AssertEqual(AreaId.Ceres, game.GameplayRuntimeForVerification.Room.LoadedRoom.Area,
                "Ceres checkpoint bypasses Zebes map and retains elevator arrival");
            AssertTrue(game.GameplayRuntimeForVerification.Room.CeresElevatorArrival is not null,
                "Ceres checkpoint still owns its arrival sequence");
            return;
        }
        Until(() => game.IsAt(FileSelectStep.AreaMap), 200);
        for (int i = 0; i < 70; i++)
        {
            frame = game.Step(ControllerButton.Start);
        }
        AssertEqual("Area", frame.Phase, "holding confirm during initial reveal cannot skip area selection");
        AssertTrue(game.RuntimeForVerification is null, "saved-game map does not construct gameplay before confirmation");
        frame = game.Step(ControllerButton.B);
        Until(() => game.IsAt(FileSelectStep.Options), 30);
        // Options indexes zero and one, fifteen fade-in levels, then the interactive index.
        for (int i = 0; i < 18; i++) frame = game.Step(0);
        frame = game.Step(ControllerButton.A);
        Until(() => game.IsAt(FileSelectStep.AreaMap) && frame.Phase == "Area", 200);
        for (int i = 0; i < 10; i++) frame = game.Step(0);
        frame = game.Step(ControllerButton.Start);
        Until(() => frame.Phase == "Room", 80);
        for (int i = 0; i < 15; i++) frame = game.Step(0);

        AssertTrue(game.RuntimeForVerification is null, "room map does not implicitly load the save");
        frame = game.Step(ControllerButton.B);
        Until(() => frame.Phase == "Area", 80);
        frame = game.Step(ControllerButton.Start);
        Until(() => frame.Phase == "Room", 80);
        frame = game.Step(ControllerButton.Start);
        Until(() => game.RuntimeForVerification is not null, 100);
        AssertEqual(SuperMetroid.TestSupport.VerificationImporters.LoadStationEntryImporter.Load(bus, AreaId.Maridia, 0).Room,
            game.GameplayRuntimeForVerification.Room.LoadedRoom.Definition, "second map confirmation loads selected SRAM station");
        void Until(Func<bool> predicate, int limit)
        {
            for (int tick = 0; tick < limit && !predicate(); tick++)
            {
                frame = game.Step(0);
            }
            AssertTrue(predicate(), $"saved map frontend reached requested boundary; current {frame.GameState}/{frame.Phase}");
        }

    }

    /// <summary>Checks area and room confirmation edges, cancel precedence, and the animated return boundary.</summary>
    private static void VerifyFileSelectMapNavigation()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ControllerButton confirm in new ControllerButton[] { ControllerButton.Start, ControllerButton.A })
        {
            var navigation = new FileSelectMapNavigation(ZebesArea.Maridia, RetailPresentationFixture().Maps.Labels, confirm);
            navigation.Step(confirm);
            AssertEqual(FileSelectMapNavigationPhase.Area, navigation.Phase,
                "options confirmation carried into map must not immediately confirm the area");
            navigation.Step((ControllerButton.Up | ControllerButton.Down | ControllerButton.Left | ControllerButton.Right | ControllerButton.L));
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
            navigation.Step(ControllerButton.B);
            AssertEqual(FileSelectMapNavigationPhase.LoadRequested, navigation.Phase, "load request remains pending until frontend handles it");
        }
        var cancel = new FileSelectMapNavigation(ZebesArea.Maridia, RetailPresentationFixture().Maps.Labels, initialHeldInput: SuperMetroid.Core.Input.ControllerButton.None);
        cancel.Step((ControllerButton.B | ControllerButton.Start | ControllerButton.Right | ControllerButton.A));
        AssertEqual(FileSelectMapNavigationPhase.Area, cancel.Phase,
            "native non-debug direction branch suppresses simultaneous cancel and confirm");
        cancel.Step(0);
        cancel.Step((ControllerButton.B | ControllerButton.Start | ControllerButton.A));
        AssertEqual(FileSelectMapNavigationPhase.OptionsRequested, cancel.Phase, "B takes precedence over confirm on area map");
        var back = new FileSelectMapNavigation(ZebesArea.Maridia, RetailPresentationFixture().Maps.Labels, initialHeldInput: SuperMetroid.Core.Input.ControllerButton.None);
        back.Step(ControllerButton.Start);
        for (int frame = 0; frame < 54; frame++) back.Step(0);
        back.Step(ControllerButton.B);
        AssertEqual(FileSelectMapNavigationPhase.AreaReturnRequested, back.Phase, "room cancel requests animated return");
        back.Step(ControllerButton.Start);
        back.CompleteAreaReturn();
        back.Step(ControllerButton.Start);
        AssertEqual(FileSelectMapNavigationPhase.Area, back.Phase, "return transition consumes held input without deferred confirmation");
        AssertThrows<InvalidOperationException>(() => back.CompleteAreaReturn(), "unsolicited area-return completion fails");
    }

    /// <summary>Checks native map centering, eight-frame arrow pulses, input precedence, and scroll limits.</summary>
    internal static void VerifyFileSelectMapScroll()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        ImportedAreaMap map = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria);
        var system = new SharedGameState(new SharedRandom());
        system.MarkExploredMapTile(AreaId.Crateria, new(0, 0));
        system.MarkExploredMapTile(AreaId.Crateria, new(63, 31));
        ControllerButton[] inputs = [ControllerButton.Left, ControllerButton.Right, ControllerButton.Up, ControllerButton.Down];
        for (int direction = 1; direction <= 4; direction++)
        {
            var scroll = MapScroll.Centred(MapFramingDefinitions.FileSelect, MapScrollLimits.OfRevealedPlane(map, system), new(240, 128));
            AssertEqual(124, scroll.Scroll.X, "room map centers native horizontal bounds");
            AssertEqual(32, scroll.Scroll.Y, "room map rounds the 112px center offset down to a tile boundary");
            AssertEqual(0, scroll.Limits.Top, "the revealed reach's top is the top row's pixel");
            AssertEqual(40, scroll.Framing.TopEdge, "room-select up arrow stops 40 pixels down the screen, 64 less its delayed 24");
            for (int frame = 1; frame <= 8; frame++)
            {
                bool sound = scroll.Step(frame == 1 ? inputs[direction - 1] : ControllerButton.None);
                int delta = frame >= 4 ? 8 : 0;
                AssertEqual(124 + (direction == 1 ? -delta : direction == 2 ? delta : 0), scroll.Scroll.X,
                    "map scroll horizontal exact pulse frame");
                AssertEqual(32 + (direction == 3 ? -delta : direction == 4 ? delta : 0), scroll.Scroll.Y,
                    "map scroll vertical exact pulse frame");
                AssertEqual(frame == 8, sound, "map scroll sound occurs at completion, not displacement");
            }
            AssertEqual<MapScrollDirection?>(null, scroll.StepUnderWay?.Direction, "released direction completes its accepted step");
        }
        var precedence = MapScroll.Centred(MapFramingDefinitions.FileSelect, MapScrollLimits.OfRevealedPlane(map, system), new(240, 128));
        precedence.Step((ControllerButton.Up | ControllerButton.Down | ControllerButton.Left | ControllerButton.Right));
        AssertEqual(MapScrollDirection.Left, precedence.StepUnderWay?.Direction, "simultaneous map input uses native arrow order");
        var empty = MapScroll.Centred(MapFramingDefinitions.FileSelect, MapScrollLimits.OfRevealedPlane(map, new SharedGameState(new SharedRandom())), new(216, 48));
        AssertEqual(208, empty.Limits.Left, "empty map uses retail left default");
        AssertEqual(224, empty.Limits.Right, "empty map uses retail right default");
        AssertEqual(8, empty.Limits.Top, "empty map uses retail top default");
        AssertEqual(88, empty.Limits.Bottom, "empty map uses retail bottom default");
        // Repeated stepping must stop at the native inequality, not run past the edge.
        for (int frame = 0; frame < 1000; frame++) precedence.Step(ControllerButton.Left);
        AssertTrue(!precedence.CanScroll(MapScrollDirection.Left), "held left stops at map boundary");
        short stoppedX = precedence.Scroll.X;
        for (int frame = 0; frame < 16; frame++) precedence.Step(ControllerButton.Left);
        AssertEqual(stoppedX, precedence.Scroll.X, "held input cannot scroll beyond unavailable arrow");
    }

    /// <summary>Checks save-station marker frame cadence, backing visibility, OAM placement, and invalid station rejection.</summary>
    internal static void VerifyFileSelectStationMarker()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var positions = RequiredJson.Deserialize<MapSaveMarkerDocument>(
            SuperMetroid.AssetExtraction.Frontend.Map.MapSaveMarkerExtractor.Extract(bus), options);
        positions.Markers[MapSaveMarkers.Id(LoadStationDefinitions.Gunship)] = new(100, 80);
        using var positionJson = new MemoryStream();
        MapSaveMarkerFile.Write(positionJson, positions);
        positionJson.Position = 0;
        var layout = MapSaveMarkerFile.Read(positionJson);
        var artwork = SuperMetroid.AssetExtraction.Frontend.Map.MapSpriteExtractor.Extract(bus);
        var document = RequiredJson.Deserialize<MapSpriteDocument>(artwork[MapSpriteFormat.JsonFile], options);
        // Preserve the original fixture's one-part (-2,-3) sprite and priority three.
        foreach (ushort id in new ushort[] { 0x12, 0x5f, 0x60, 0x61 })
        {
            string name = MapSpriteArtwork.FrameNames.Single(frame => frame.Id == MapSpriteOrdinals.Spritemaps.Decode(id, "map spritemap ordinal")).Name;
            document.Frames[name] = [new SpriteVisualPart { OffsetX = -2, OffsetY = -3,
                TileColumn = 1, TileRow = 0, Size = 8, Depth = SuperMetroid.Core.Assets.ActorDepth.InFront, Palette = null,
                FlipX = false, FlipY = false }];
        }
        var sprites = MapSpriteArtwork.Read(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.Verification.VerificationJson.Info<global::SuperMetroid.AssetExtraction.Frontend.Map.MapSpriteDocument>(options))),
            new MemoryStream(artwork[MapSpriteFormat.PngFile]));
        var marker = new FileSelectStationMarker(LoadStationDefinitions.Gunship, layout);
        int[] expectedFrames = [0x60, 0x61, 0x60, 0x5f, 0x60, 0x61, 0x60, 0x5f];
        int[] durations = [4, 8, 4, 8, 4, 8, 4, 8];
        for (int phase = 0; phase < durations.Length; phase++)
        for (int tick = 0; tick < durations[phase]; tick++)
        {
            marker.Step();
            AssertEqual(expectedFrames[phase], MapSpriteOrdinals.Spritemaps.Word(marker.SpriteId), "station marker exact animation cadence");
            bool backing = phase is < 3 or >= 7;
            AssertEqual(backing, marker.ShowBacking, "station marker alternates backing at loop boundary");
            var oam = ScreenSpritesFixtures.Blank();
            oam.BeginFrame();
            marker.Draw(oam, new(24, 16), MenuSheetFixtures.MapSprites(sprites));
            oam.FinalizeFrame();
            AssertEqual(backing ? 2 : 1, oam.LastFinalizedSpriteCount, "marker backing OAM emission");
            AssertEqual(74, oam.LowTable[0], "marker subtracts horizontal scroll and sprite offset");
            AssertEqual(61, oam.LowTable[1], "marker subtracts vertical scroll and sprite offset");
            AssertEqual(0x3e, oam.LowTable[3], "marker retains sprite priority with palette seven");
            byte[] first = oam.LowTable.ToArray();
            oam.BeginFrame();
            marker.Draw(oam, new(24, 16), MenuSheetFixtures.MapSprites(sprites));
            oam.FinalizeFrame();
            AssertTrue(first.SequenceEqual(oam.LowTable.ToArray()), "repainting station marker does not advance animation");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(LoadStationDefinitions.ZebesArrival, layout),
            "a station with no map marker is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectStationMarker(LoadStationDefinitions.CeresElevator, layout),
            "Ceres has no map markers");
    }

    /// <summary>Runs the file-select map icon visibility and OAM fixture.</summary>
    [VerificationCommand("--map-icons-fixture")]
    internal static int MapIconsFixtureCommand()
    {
     VerifyFileSelectMapIcons(); return 0; 
    }

    /// <summary>Runs the file-select palette and scroll-arrow animation fixture.</summary>
    [VerificationCommand("--map-animation-fixture")]
    internal static int MapAnimationFixtureCommand()
    {
     VerifyFileSelectMapAnimations(); return 0; 
    }

    /// <summary>Runs the saved-slot frontend map flow fixture.</summary>
    [VerificationCommand("--saved-map-fixture")]
    internal static int SavedMapFixtureCommand()
    {
     VerifySavedGameMapFrontend(); return 0; 
    }

    /// <summary>Runs the room-to-area map cancellation presentation fixture.</summary>
    [VerificationCommand("--map-cancel-fixture")]
    internal static int MapCancelFixtureCommand()
    {
     VerifyMapCancelPresentation(); return 0; 
    }

    /// <summary>Runs the native save-station marker cadence and OAM fixture.</summary>
    [VerificationCommand("--station-marker-fixture")]
    internal static int StationMarkerFixtureCommand()
    {
     VerifyFileSelectStationMarker(); return 0; 
    }
}
