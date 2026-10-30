using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks installed pause backdrops, labels, and button tiles through live page transitions.</summary>
internal static class PauseBackdropsVerification
{
    /// <summary>Checks native VRAM and pixels, edited content rebinds, and preserved equipment state across fades.</summary>
    internal static void VerifyPauseBackdrops(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentation original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseBackdropDefinitions.FileName);
        string path = Path.Combine(overrides, PauseBackdropDefinitions.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        var document = RequiredJson.Deserialize<PauseBackdropDocument>(stockBytes, ArtworkJson.Options);
        var guard = NewPauseBackdropReadGuard(bus);
        var editedAreas = document.Areas.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        foreach (var cells in editedAreas.Values)
        {
            // Change the lettering region itself, not a hidden edge of the page.
            // This proves return-to-map cannot secretly reinstall ROM lettering.
            for (int i = 0; i < 12; i++) cells[170 + i] = cells[170 + i] with
                { Atlas = PauseAtlas.Map, TileColumn = 4, TileRow = 0, Palette = 3, Priority = true, FlipX = true, FlipY = true };
        }
        var buttons = document.Buttons.ToArray();
        for (int i = 0; i < buttons.Length; i++) buttons[i] = buttons[i] with
            { TileColumn = (buttons[i].TileColumn + 1) % 32, Palette = 7, FlipX = !buttons[i].FlipX };
        using (var output = File.Create(path)) PauseBackdropPresentation.Write(output, document with { Areas = editedAreas, Buttons = buttons });
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "backdrop/button-only edits change catalog identity");
        AssertTrue(InstalledAreaMapPresentation.PauseOf(original).Backdrops.CreateButtonTilemap().SequenceEqual(BackgroundTileWordBytes.Tiles(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0xb6e400, 0x400))), "all native mutable button tiles survive extraction");
        AreaId everyTickArea = Enum.GetValues<AreaId>()[0];
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            byte[] expected = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0xb6e000, 0x800);
            ushort label = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82965f + (int)area * 2);
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0x820000 | label, 24).CopyTo(expected, 170 * 2);
            var tiles = new BackgroundTile[PauseBackdropDefinitions.Cells]; InstalledAreaMapPresentation.PauseOf(original).Backdrops.LoadTo(tiles, area);
            AssertTrue(BackgroundTileWordBytes.Tiles(expected).SequenceEqual(tiles), "all 1024 backdrop tiles match native frame plus unmasked area lettering");
            var native = Create(bus, area, original);
            var pause = Create(guard, area, original);
            var persistentEdit = Create(guard, area, edited);
            // Rebind at every page/fade position, including the instant R/L updates
            // buttons before ScreenMode changes. Compare the following transition
            // too, so a hidden timing/selection reset cannot pass a single-frame test.
            // Each page button lands on the first tick after the previous change completes.
            int toMap = 2 + PausePageTransitionNativeLength + 1, unpause = toMap + PausePageTransitionNativeLength + 1;
            for (int tick = 0; tick < unpause + 2; tick++)
            {
                ControllerButton input = tick == 2 ? ControllerButton.R : tick == toMap ? ControllerButton.L :
                    tick == unpause ? ControllerButton.Start : 0;
                AssertEqual(native.Step(input, input), pause.Step(input, input), "backdrop migration preserves Start acceptance and page controls");
                persistentEdit.Step(input, input);
                // Each area's complete backdrop is compared word-for-word above, and the fade and
                // page logic is area-independent: one area compares every tick, the others the
                // page-change, mid-fade, settled and unpause ticks.
                bool checkpoint = tick is 0 or 2 or 3 || tick == 2 + PausePageTransitionNativeLength / 2 ||
                    tick == toMap || tick == toMap + 1 || tick == unpause || tick == unpause + 1;
                if (area != everyTickArea && !checkpoint) continue;
                var snapshot = pause.CaptureRenderSnapshot();
                byte[] snapshotVram = pause.ShownVram();
                var shownNative = native.ShownVram();
                AssertTrue(shownNative.SequenceEqual(snapshotVram), "every stock transition frame retains exact native VRAM");
                var pixels = pause.Render();
                AssertTrue(native.Render().SequenceEqual(pixels), "seven-area stock pixels match native path with frame, buttons and label ROM reads blocked");
                var selection = new PauseBackdropsSelection(pause.ScreenMode, pause.SelectedCategory, pause.SelectedItem, pause.MapScroll);
                pause.BindFixtureArtwork(edited);
                var changed = pause.CaptureRenderSnapshot();
                byte[] changedVram = pause.ShownVram();
                var shownPersistentEdit = persistentEdit.ShownVram();
                AssertTrue(changedVram.SequenceEqual(shownPersistentEdit),
                    "fresh edited session and live rebind retain the same artwork through equipment return and Start");
                AssertTrue(snapshotVram.AsSpan(0x6000, 0x1000).SequenceEqual(changedVram.AsSpan(0x6000, 0x1000)), "backdrop rebind leaves shared map/equipment BG1 untouched");
                AssertEqual(selection, new PauseBackdropsSelection(pause.ScreenMode, pause.SelectedCategory, pause.SelectedItem, pause.MapScroll), "backdrop rebind leaves navigation and scroll untouched");
                foreach (var span in PauseMenuButtonLabels.ButtonLabelSpans)
                for (int word = 0; word < span.Count; word++)
                {
                    int offset = (PauseMenuLayoutConstants.Bg2TilemapWord + span.Word + word) * 2;
                    var before = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(snapshotVram.AsSpan(offset)));
                    var after = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(changedVram.AsSpan(offset)));
                    AssertEqual(before.PaletteIndex, after.PaletteIndex, "live button highlight wins over replacement palette at every fade position");
                    AssertTrue(before.CharacterIndex != after.CharacterIndex, "replacement button characters reach live highlighted words");
                }
                // Map page, mid page change, and the settled equipment page.
                if (tick == 0 || tick == 2 + PausePageTransitionNativeLength / 2 || tick == toMap)
                {
                    var changedPixels = pause.Render();
                    AssertTrue(!pixels.SequenceEqual(changedPixels), "authored frame/button changes are visible on map and equipment pages");
                    AssertTrue(changedPixels.SequenceEqual(RenderForComparison(changed)), "edited backdrop agrees between direct and captured renderers");
                    using var capture = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(capture, pause); capture.Position = 0;
                    pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(capture);
                }
                pause.BindFixtureArtwork(original);
                AssertTrue(pixels.SequenceEqual(pause.Render()), "current-content restore preserves complete pixels and animation phase");
            }
        }
        // The native invalid-beam selection deliberately leaves an overlong label
        // in equipment VRAM. A full inventory rebuild during content rebind loses it.
        var samus = new SamusState { CollectedItems = (SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), EquippedItems = (SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), CollectedBeams = (SamusBeamFlags)0x100f, EquippedBeams = (SamusBeamFlags)4 };
        var glitch = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        EnterPauseEquipment(glitch);
        SelectPauseBoots(glitch);
        AssertEqual(0, glitch.SelectedItem, "fixture selects Hi-Jump Boots");
        glitch.Step(0, (ControllerButton.Left | ControllerButton.A));
        AssertEqual((ushort)12, samus.EquippedBeams.ToEquipmentWord(), "fixture actually performs native Boots-to-Plasma simultaneous-input glitch");
        byte[] glitchPage = glitch.ShownVram().AsSpan(0x6000, 0x800).ToArray();
        AssertEqual((ushort)0x0900, BinaryPrimitives.ReadUInt16LittleEndian(glitchPage.AsSpan(0x514)), "fixture retains native VAR overrun beyond ordinary Plasma label");
        glitch.BindFixtureArtwork(edited);
        AssertTrue(glitchPage.SequenceEqual(glitch.ShownVram().AsSpan(0x6000, 0x800)), "replacing backdrop does not rebuild away live native equipment glitch");

        void Reject(PauseBackdropDocument invalid, string message) => AssertThrows<InvalidDataException>(() =>
            PauseBackdropPresentation.Write(new MemoryStream(), invalid), message);
        Reject(document with { Version = 2 }, "unsupported backdrop version rejected");
        Reject(document with { Areas = [] }, "missing areas rejected");
        Reject(document with { Buttons = [] }, "incomplete button image rejected");
        var invalidAreas = document.Areas.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        invalidAreas["Crateria"] = [];
        Reject(document with { Areas = invalidAreas }, "wrong backdrop dimensions rejected");
        invalidAreas["Crateria"] = document.Areas["Crateria"].ToArray();
        invalidAreas["Crateria"][0] = new() { Atlas = PauseAtlas.Interface, TileColumn = 31, TileRow = 7, Palette = 7, Priority = true, FlipX = true, FlipY = true };
        using (var authored = new MemoryStream())
        {
            PauseBackdropPresentation.Write(authored, document with { Areas = invalidAreas }); authored.Position = 0;
            var compiled = PauseBackdropPresentation.Load(authored); var tiles = new BackgroundTile[PauseBackdropDefinitions.Cells]; compiled.LoadTo(tiles, AreaId.Crateria);
            // The Interface atlas's last cell (column 31, row 7) is tile $1FF; every visual attribute carries through.
            AssertEqual(new BackgroundTile(0x1ff, 7, FlipHorizontally: true, FlipVertically: true, HighPriority: true), tiles[0],
                "named Interface atlas resolves its own tile coordinates and every visual attribute");
        }
        foreach (var invalid in new[] { document.Buttons[0] with { Atlas = (PauseAtlas)byte.MaxValue }, document.Buttons[0] with { TileColumn = 32 },
            document.Buttons[0] with { TileRow = 8 }, document.Buttons[0] with { Palette = -1 }, document.Buttons[0] with { Palette = 8 } })
        {
            invalidAreas["Crateria"][0] = invalid;
            Reject(document with { Areas = invalidAreas }, "invalid atlas, coordinates or palette rejected");
        }
        File.WriteAllText(path, "{\"version\":1,\"gameplay\":true}");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "unknown fields or missing required content fail loudly");
        File.WriteAllText(path, "broken JSON");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "corrupt override cannot silently fall back");
        File.WriteAllBytes(path, stockBytes);
        try
        {
            File.Delete(stockPath);
            AssertThrows<IOException>(() => MapPresentationFiles.Load(stock, overrides), "override cannot conceal missing stock backdrop");
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "override cannot conceal failed stock hash");
        }
        finally { File.WriteAllBytes(stockPath, stockBytes); }
        Console.WriteLine("Pause backdrops: seven native frames/labels, button artwork, 532 guarded transition/rebind frames, visible edits, restore, native equipment overrun preservation and strict resource failures pass.");

        static PauseMenuState Create(ISnesAddressSpace addressSpace, AreaId area, AreaMapPresentation catalog)
        {
            // Ceres has no map station, so its map byte is never set natively; since #1252 the
            // pause map draws elevator destinations for acquired maps, which Ceres does not have.
            var system = new SharedGameState();
            if (area != AreaId.Ceres) system.SetAreaMapAcquired(area);
            return new(new PauseMenuEntry { Samus = new SamusState { CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge }, System = system, AreaIndex = area, RoomMapPosition = new(10, 10), MapPresentation = catalog, SpriteSheets = MenuSheetFixtures.For(catalog) });
        }
    }

    /// <summary>Forbids native backdrop, button, and area-label reads after installed art is bound.</summary>
    private static CartridgeReadGuard NewPauseBackdropReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        void Add(int address, int size) { for (int i = 0; i < size; i++) blocked.Add(address + i); }
        Add(0xb6e000, 0x800); Add(0x82965f, 14);
        for (int area = 0; area < 7; area++) Add(0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82965f + area * 2), 24);
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed pause read backdrop/button/label at {address:X6}.");
    }
}
