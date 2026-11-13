using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Samus.Projectiles.InvalidBeamSelectionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.FrontendVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseMenuFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core;
using SuperMetroid.Core.Map;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core.Samus.Palettes;

/// <summary>Checks pause equipment, map reveal, sprite placement, and unpause asset transfers.</summary>
internal static class PauseMenuVerification
{
    /// <summary>
    /// Exercises pause transitions and live equipment changes with constructed
    /// map, palette and sprite assets. Immutable selection rules remain retail;
    /// the tiny visual fixture makes layer priority, OAM placement and unpause
    /// transfers observable without depending on retail pixel colors.
    /// </summary>
    internal static void VerifyPauseMenuEquipmentInteraction()
    {
        var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];

        // Area zero's pause map is a literal 64x32 tilemap at $B5:9000, and its area label
        // uses a harmless zero-filled bank-$82 source. The visual bytes may remain zero for
        // this state/bit test; the private audit verifies nonempty retail rendering.
        WriteRomLong(rom, 0x82964a, 0xb59000);
        WriteRomWord(rom, 0x82965f, 0x8000);

        // Make every cartridge map cell exist as a room walled all round, character 32 in the
        // unexplored row as the retail maps store it, so the import classifies every cell
        // (#1513). Put a high-priority BG2 holder tile at screen tile (10,10).
        WriteRomWord(rom, 0x829717, 0x8500);
        for (int mapByte = 0; mapByte < 0x0100; mapByte++)
            WriteRomByte(rom, 0x828500 + mapByte, 0xff);
        for (int mapWord = 0; mapWord < 0x0800; mapWord++)
            WriteRomWord(rom, 0xb59000 + mapWord * 2, 0x0c20);
        const int secretMapX = 8;
        const int publicMapX = 9;
        const int exploredSecretMapX = 10;
        const int revealTestMapY = 3;
        int secretMapByte = AreaMapPages.GetBitByteIndex(new(secretMapX, revealTestMapY));
        byte secretAndExploredSecretMasks = (byte)(
            AreaMapPages.GetBitMask(secretMapX) |
            AreaMapPages.GetBitMask(exploredSecretMapX));
        WriteRomByte(
            rom,
            0x828500 + secretMapByte,
            unchecked((byte)~secretAndExploredSecretMasks));
        // The reveal-test cells are rooms open to the right, character 33 in the unexplored row.
        foreach (int mapX in new[] { secretMapX, publicMapX, exploredSecretMapX })
        {
            WriteRomWord(
                rom,
                0xb59000 + AreaMapPages.GetTilemapWordIndex(new(checked((ushort)mapX), revealTestMapY)) * 2,
                0x0c21);
        }
        var revealCell = new MapCell(MapCellKind.Room, new(MapCellEdge.Wall, MapCellEdge.Wall, MapCellEdge.Wall, MapCellEdge.Open));

        // The three reveal-test cells share exported Crateria byte $0D, so the SRAM
        // round trip exercises the real compiled cartridge codec without patching its
        // immutable table definitions into this otherwise synthetic visual fixture.
        WriteRomWord(rom, 0xb6e000 + (10 * 32 + 10) * 2, 0x2002);
        for (int row = 0; row < 8; row++)
        {
            WriteRomByte(rom, 0xb68000 + 1 * 32 + row * 2, 0xff);
            WriteRomByte(rom, 0xb68000 + 2 * 32 + row * 2 + 1, 0xff);
        }

        // Pause keeps the existing BG3 HUD tilemap but replaces its character sheet with
        // $9A:B200. Give character one one visible pixel and palette color one a white
        // value, then publish that character in the retained gameplay VRAM image.
        // Character zero is deliberately visible. Before the native FX-tilemap clear was
        // ported, zero-filled BG3 words below the retained HUD selected this character and
        // tiled it over empty pause-map space (the retail character is the orange `1`).
        // Character one remains the retained HUD sentinel used by the assertion below.
        WriteRomByte(rom, 0x9ab200, 0x80);
        WriteRomByte(rom, 0x9ab210, 0x80);
        WriteRomWord(rom, 0xb6f002, 0x7fff);
        WriteRomWord(rom, 0xb6f004, 0x03e0);

        var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        var presentation = CreateConstructedPausePresentation(bus);
        // The HUD gameplay latched, which pause keeps resident (#1466).
        var gameplayHud = new HudState();
        gameplayHud.BindPresentation(presentation.Game.GameplayHud);
        gameplayHud.Initialize(HudSnapshot.CeresDebug);
        ShownHud retainedHud = gameplayHud.Shown();
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            MaxReserveEnergy = 100,
            ReserveEnergy = 47,
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0300)),
        };
        var system = new SharedGameState(new SharedRandom());
        system.SetAreaMapAcquired(0);
        system.MarkExploredMapTile(0, new(30, 5));
        var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(28, 1), GameplayHud = retainedHud, MapPresentation = presentation.Game, SpriteSheets = MenuSheetFixtures.For(presentation) });

        AssertEqual(PauseScreenMode.Map, pause.ScreenMode, "pause begins on map page");
        AssertEqual(0x0804, pause.ReadReserveSupplyDigit(presentation.Game, 0).Raw,
            "pause equipment reserve-supply hundreds digit renders current supply");
        AssertEqual(0x0808, pause.ReadReserveSupplyDigit(presentation.Game, 1).Raw,
            "pause equipment reserve-supply tens digit renders current supply");
        AssertEqual(0x080b, pause.ReadReserveSupplyDigit(presentation.Game, 2).Raw,
            "pause equipment reserve-supply ones digit renders current supply");
        HudLayer drawnHud = pause.CaptureRenderSnapshot().Layers.ToArray().OfType<HudLayer>().First();
        AssertTrue(ReferenceEquals(MenuSheetFixtures.For(presentation).Get(SpriteSheetId.PauseHud).Frame(HudTopRowImage.Instance.FrameName),
            drawnHud.Hud.Pictures[0].Picture), "pause retains and draws the gameplay HUD's top row on the HUD's layer");
        AssertEqual(124, pause.MapScroll.X,
            "downloaded pause map horizontal centering");
        AssertEqual(unchecked((ushort)-24), pause.MapScroll.Y,
            "downloaded pause map vertical centering");
        // The display packet shows the marker's first pulse frame where Samus's map cell lies on the scrolled map.
        SpriteSheetFrame markerFrame = MenuSheetFixtures.For(presentation).Get(SpriteSheetId.MapSprites)
            .Frame(SuperMetroid.Core.Frontend.Map.MapSpriteNames.Name(SuperMetroid.Core.Frontend.Map.MapSpriteId.IndicatorFrame0));
        AssertEqual(new Position16<ScreenPixels>(116, 64),
            ShownSpriteDraws(pause).Single(draw => ReferenceEquals(draw.Image, markerFrame)).Origin, "pause map marker shows its initial frame at its origin");
        // The fixture retains the five retail Crateria destination labels (22 OBJ parts).
        AssertEqual(23, pause.LastRenderedSpriteCount, "pause marker plus downloaded destination labels OAM count");
        AssertEqual(0x1400, pause.ReadPauseButtonLabelWord(presentation.Game, 805) & 0x1c00,
            "pause map label bright palette reached BG2");
        AssertEqual(0x0800, pause.ReadPauseButtonLabelWord(presentation.Game, 822) & 0x1c00,
            "pause equipment label dim palette reached BG2");
        EnterPauseEquipment(pause);
        AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "pause R transition reaches equipment page");
        // $82:ABAD-$ABB5 selects the reserve mode control whenever reserve capacity exists (#1266).
        AssertEqual(new CursorBeforeRestore(PauseEquipmentCategory.Reserves, PauseReserveEnergyDefinitions.ModeItem),
            new CursorBeforeRestore(pause.SelectedCategory, pause.SelectedItem), "pause entry selects the reserve mode control");
        pause.Step(0, ControllerButton.Right);
        AssertEqual(PauseEquipmentCategory.Suits, pause.SelectedCategory, "pause Right moves to suits/misc category");
        AssertEqual(2, pause.SelectedItem, "pause Right selects first collected Morph Ball item");
        pause.Render();
        AssertEqual(new Position16<ScreenPixels>(0x90, 0x70), ShownEquipmentSelector(pause, MenuSheetFixtures.For(presentation).Get(SpriteSheetId.PauseSprites), presentation.Game.PauseSelectors).Origin,
            "pause shows the suits category's selector frame at the Morph Ball's origin");
        AssertEqual(1, pause.LastRenderedSpriteCount, "pause equipment selector OAM count");
        AssertEqual(0x0800, pause.ReadPauseButtonLabelWord(presentation.Game, 805) & 0x1c00,
            "pause map label dims on equipment page");
        AssertEqual(0x1400, pause.ReadPauseButtonLabelWord(presentation.Game, 822) & 0x1c00,
            "pause equipment label brightens on equipment page");

        // Rebuilding after a live supply change must replace the displayed tile words;
        // this catches the original frozen-ROM-template failure rather than merely proving
        // that Samus's underlying reserve field changed.
        samus.ReserveEnergy = 100;
        pause.Step(0, ControllerButton.A);
        AssertEqual(0x0805, pause.ReadReserveSupplyDigit(presentation.Game, 0).Raw,
            "pause equipment reserve-supply hundreds digit refreshes after rebuild");
        AssertEqual(0x0804, pause.ReadReserveSupplyDigit(presentation.Game, 1).Raw,
            "pause equipment reserve-supply tens digit refreshes after rebuild");
        AssertEqual(0x0804, pause.ReadReserveSupplyDigit(presentation.Game, 2).Raw,
            "pause equipment reserve-supply ones digit refreshes after rebuild");

        // D-pad and A use joypad1_newkeys, not the delayed-held word used by L/R/Start.
        pause.Step(0, ControllerButton.Down);
        AssertEqual(3, pause.SelectedItem, "pause Down selects collected Bombs");
        pause.Render();
        AssertEqual(new Position16<ScreenPixels>(0xa0, 0x80), ShownEquipmentSelector(pause, MenuSheetFixtures.For(presentation).Get(SpriteSheetId.PauseSprites), presentation.Game.PauseSelectors).Origin,
            "pause shows the equipment selector at the Bombs' origin");
        pause.Step(0, ControllerButton.A);
        AssertTrue(!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs),
            "pause A unequips live Bombs bit");
        pause.Step(0, 0);
        pause.Step(0, ControllerButton.A);
        AssertTrue(samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs),
            "pause A re-equips live Bombs bit");
        AssertTrue(pause.Step(ControllerButton.Start, 0),
            "pause delayed Start requests outer unpause state");
        AssertEqual(0x0800, pause.ReadPauseButtonLabelWord(presentation.Game, 812) & 0x1c00,
            "pause Start label switches to the native unpause palette");

        // Re-enter the equipment page with only Wave collected so the pause code itself
        // performs the mutation under test. The runtime handoff then consumes that same
        // Samus word and must show the Wave beam's colours immediately.
        samus.CollectedBeams = SamusBeamFlags.Wave;
        samus.EquippedBeams = 0;
        var beamPause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(28, 1), GameplayHud = retainedHud, MapPresentation = presentation.Game, SpriteSheets = MenuSheetFixtures.For(presentation) });
        EnterPauseEquipment(beamPause);
        AssertEqual(PauseEquipmentCategory.Reserves, beamPause.SelectedCategory,
            "Wave-only pause entry selects the reserve mode control while capacity exists");
        SelectPauseBeams(beamPause);
        beamPause.Step(0, ControllerButton.A);
        AssertEqual(SamusBeamFlags.Wave, samus.EquippedBeams,
            "pause toggle immediately mutates live Wave equipment word");

        var resumeRuntime = CreateRetailRuntimeFixture();
        resumeRuntime.SamusPhases.ShowEquippedBeamColours(samus.EquippedBeams);
        resumeRuntime.FrameBoundary.BeginAcceptedFrame(controller1Input: 0);
        // Her beams' colours are a shade the import pre-colours her sheets in (#1512): unpausing names the Wave shade.
        AssertEqual(SamusShades.Beam(SamusBeamCombinations.Of(SamusBeamFlags.Wave)), resumeRuntime.Player.Colours.Beams,
            "unpause immediately shows the newly equipped Wave beam's shade");

        // Use a second page with no map acquisition or explored cells so BG1 is blank at
        // the sampled point. BG2 is also transparent there. The pixel must therefore be
        // backdrop black, proving that `$80:A211`'s $184E fill prevented visible BG3
        // character zero from leaking below the four-row HUD.
        var blankMapPause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Crateria, RoomMapPosition = new(28, 1), GameplayHud = retainedHud, MapPresentation = presentation.Game, SpriteSheets = MenuSheetFixtures.For(presentation) });
        RgbaPixel[] blankMapFrame = blankMapPause.Render();
        AssertEqual(new RgbaPixel(0, 0, 0, 255), blankMapFrame[40 * 256],
            "pause clears unused BG3 rows instead of tiling character zero over empty map space");

        // The same persistent state is projected through all three host modes. One public
        // cell, one secret-only cell, and one entered secret cell prove visibility and the
        // explored-palette distinction independently. Reconstructing None afterward proves
        // the earlier override never wrote either progression plane.
        var revealSystem = new SharedGameState(new SharedRandom());
        revealSystem.MarkExploredMapTile(
            AreaId.Crateria,
            new(exploredSecretMapX, revealTestMapY));
        PauseMenuState CreateRevealPause(MapRevealMode mode) => new(new PauseMenuEntry { Samus = samus, System = revealSystem, AreaIndex = AreaId.Crateria, RoomMapPosition = new(4, 3), GameplayHud = retainedHud, MapRevealMode = mode, MapPresentation = presentation.Game, SpriteSheets = MenuSheetFixtures.For(presentation) });

        PauseMenuState nonePause = CreateRevealPause(MapRevealMode.None);
        AssertEqual(ShownMapCell.Unexplored(MapCell.Empty), nonePause.ReadDisplayedMapCell(new(publicMapX, revealTestMapY)),
            "None pause map hides unentered public cell without acquired map");
        AssertEqual(ShownMapCell.Unexplored(MapCell.Empty), nonePause.ReadDisplayedMapCell(new(secretMapX, revealTestMapY)),
            "None pause map hides unentered secret cell");
        AssertEqual(ShownMapCell.Explored(revealCell),
            nonePause.ReadDisplayedMapCell(new(exploredSecretMapX, revealTestMapY)),
            "None pause map retains entered secret cell in the explored shade");

        PauseMenuState publicPause = CreateRevealPause(MapRevealMode.Public);
        AssertEqual(ShownMapCell.Unexplored(revealCell),
            publicPause.ReadDisplayedMapCell(new(publicMapX, revealTestMapY)),
            "Public pause map reveals cartridge station cell as unentered");
        AssertEqual(ShownMapCell.Unexplored(MapCell.Empty),
            publicPause.ReadDisplayedMapCell(new(secretMapX, revealTestMapY)),
            "Public pause map excludes cartridge secret-only cell");

        PauseMenuState secretPause = CreateRevealPause(MapRevealMode.Secret);
        AssertEqual(ShownMapCell.Unexplored(revealCell),
            secretPause.ReadDisplayedMapCell(new(secretMapX, revealTestMapY)),
            "Secret pause map reveals nonblank secret-only cell as unentered");
        AssertEqual(ShownMapCell.Explored(revealCell),
            secretPause.ReadDisplayedMapCell(new(exploredSecretMapX, revealTestMapY)),
            "Secret pause map keeps entered secret cell visually distinct");

        SuperMetroidSaveSnapshot revealSnapshot = SuperMetroidSaveSnapshot.Capture(samus, revealSystem, new SaveSnapshotContext { Checkpoint = SuperMetroid.Core.Progression.LoadStationDefinitions.Gunship });
        var revealSaveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        revealSaveRam.SaveSlot(0, revealSnapshot);
        SuperMetroidSaveSlot restoredRevealSlot = revealSaveRam.ReadSlot(0) ??
            throw new InvalidOperationException("Map-reveal SRAM fixture did not round trip.");
        var restoredRevealSystem = new SharedGameState(new SharedRandom());
        restoredRevealSlot.ApplyTo(new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), restoredRevealSystem);
        var restoredNonePause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = restoredRevealSystem, AreaIndex = AreaId.Crateria, RoomMapPosition = new(4, 3), GameplayHud = retainedHud, MapRevealMode = MapRevealMode.None, MapPresentation = presentation.Game, SpriteSheets = MenuSheetFixtures.For(presentation) });
        AssertTrue(!restoredRevealSystem.HasAreaMap(AreaId.Crateria),
            "map reveal mode does not persist a map-station flag through SRAM snapshot");
        AssertEqual(ShownMapCell.Unexplored(MapCell.Empty),
            restoredNonePause.ReadDisplayedMapCell(new(secretMapX, revealTestMapY)),
            "map reveal mode does not persist secret-only visibility through SRAM snapshot");
        AssertEqual(ShownMapCell.Explored(revealCell),
            restoredNonePause.ReadDisplayedMapCell(new(exploredSecretMapX, revealTestMapY)),
            "SRAM round trip preserves legitimately explored secret cell");

        Console.WriteLine(
            "  Pause menu: constructed assets, map reveal modes, native centering, OAM indicators, equipment toggles, unpause transfers, and Start agree.");
    }

    /// <summary>The sprite draws the pause screen's display packet shows, frontmost first.</summary>
    private static ImageSpriteDraw[] ShownSpriteDraws(PauseMenuState pause) =>
        [.. pause.CaptureRenderSnapshot().Layers.ToArray().OfType<SpritesLayer>().Single().Draws.Cast<ImageSpriteDraw>()];

    /// <summary>
    /// The one draw of the display packet showing the suits and boots categories' selector frame for the selector's
    /// current phase, which a beam or reserve selector does not show.
    /// </summary>
    private static ImageSpriteDraw ShownEquipmentSelector(PauseMenuState pause, SpriteSheet pauseSheet, PauseSelectorPresentation selectors)
    {
        SpriteSheetFrame frame = pauseSheet.Frame(PauseSelectorAnchors.SheetFrameName(
            selectors.equipment[selectors.NormalizePhase(pause.ItemSelectorAnimationState.Frame)]));
        return ShownSpriteDraws(pause).Single(draw => ReferenceEquals(draw.Image, frame));
    }

    /// <summary>Seeds a three-byte SNES pointer in the constructed pause-menu cartridge fixture.</summary>
    private static void WriteRomLong(byte[] rom, int snesAddress, int value)
    {
        int offset = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(snesAddress);
        rom[offset] = unchecked((byte)value);
        rom[offset + 1] = LittleEndianBytes.HighByte(unchecked((ushort)value));
        rom[offset + 2] = SnesLittleEndianConstants.BankByte(value);
    }

    /// <summary>Seeds one cartridge byte at a mapped SNES address in the pause fixture.</summary>
    private static void WriteRomByte(byte[] rom, int snesAddress, byte value) =>
        rom[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(snesAddress)] = value;

    /// <summary>Runs the constructed pause equipment and map interaction fixture.</summary>
    [VerificationCommand("--pause-equipment-interaction-fixture")]
    internal static int PauseEquipmentInteractionFixtureCommand()
    {
        Suite(nameof(VerifyPauseMenuEquipmentInteraction), () => VerifyPauseMenuEquipmentInteraction());
        return 0;
    }

    /// <summary>Runs pause equipment interaction and invalid beam-selection checks.</summary>
    [VerificationCommand("--invalid-beam-selection")]
    internal static int InvalidBeamSelectionCommand()
    {
        Suite(nameof(VerifyPauseMenuEquipmentInteraction), () => VerifyPauseMenuEquipmentInteraction());
        Suite(nameof(VerifyInvalidBeamSelection), () => VerifyInvalidBeamSelection());
        return 0;
    }
}
