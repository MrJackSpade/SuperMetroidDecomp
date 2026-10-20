using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorAnchorsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorCompositionsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorDurationsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause equipment selector placement, animation, editable art, and restored-state rebinding.</summary>
internal static class PauseSelectorsVerification
{
    /// <summary>Compares all sixteen selectors with guarded native rendering and exercises edited animation and OAM parts.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause selector anchor tables")]
    internal static void VerifyPauseSelectors(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog catalog)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseSelectorDefinitions.FileName), path = Path.Combine(overrides, PauseSelectorDefinitions.FileName);
        byte[] bytes = File.ReadAllBytes(stockPath);
        var document = RequiredJson.Deserialize<PauseSelectorDocument>(bytes, ArtworkJson.Options);
        var guard = NewPauseSelectorReadGuard(bus);
        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82b9c9), PauseMenuLayout.MapMarkerPaletteBits, "compiled map caller palette matches native word");
        Suite(nameof(VerifyPauseSelectorAnchors), () => VerifyPauseSelectorAnchors(bus));
        Suite(nameof(VerifyPauseSelectorNativeDurations), () => VerifyPauseSelectorNativeDurations(bus, catalog.PauseSelectors));
        for (int group = 0; group < 3; group++) VerifyPauseSelectorNativeCompositions(bus, catalog.PauseSelectors, group);
        foreach (var anchor in PauseSelectorDefinitions.Anchors())
        {
            var native = Create(bus, catalog, anchor.Category, anchor.Item);
            var pause = Create(guard, catalog, anchor.Category, anchor.Item);
            AssertEqual((anchor.Category, anchor.Item), (pause.SelectedCategory, pause.SelectedItem), "real menu fixture reaches the reported semantic selector");
            // Every tick compares the complete render inputs; pixels are rasterized once per
            // displayed selector frame, since identical inputs cannot render differently.
            int renderedFrame = -1;
            for (int tick = 0; tick < 100; tick++)
            {
                native.Step(0, 0); pause.Step(0, 0);
                AssertEqual(native.ItemSelectorAnimationState, pause.ItemSelectorAnimationState, "real selector phase and timer follow native decrement/advance order");
                var nativeInputs = native.CaptureRenderSnapshot(); var portInputs = pause.CaptureRenderSnapshot();
                AssertTrue(nativeInputs.Memory.SpriteParts().SequenceEqual(portInputs.Memory.SpriteParts()) && nativeInputs.Memory.Cgram.SequenceEqual(portInputs.Memory.Cgram) &&
                    nativeInputs.Memory.Vram.SequenceEqual(portInputs.Memory.Vram) && nativeInputs.Brightness == portInputs.Brightness,
                    "all sixteen selectors match native render inputs with pointer/art/position/timing reads forbidden");
                if (pause.ItemSelectorAnimationState.Frame != renderedFrame)
                {
                    renderedFrame = pause.ItemSelectorAnimationState.Frame;
                    AssertTrue(native.Render().SequenceEqual(pause.Render()), "all sixteen selectors match native pixels with pointer/art/position/timing reads forbidden");
                }
                if (tick == 51)
                {
                    using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, pause); state.Position = 0;
                    pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state); pause.BindMapPresentation(catalog);
                }
            }
        }
        var anchors = new Dictionary<string, Position16<OamPixels>>(document.Anchors) { ["Beam.Charge"] = new(100, 90) };
        var parts = new SpriteVisualPart[]
        {
            new() { OffsetX = 12, OffsetY = -4, TileColumn = 2, TileRow = 3, Size = 16, Priority = 3, Palette = 5, FlipX = true, FlipY = true },
            new() { OffsetX = 0, OffsetY = 0, TileColumn = 1, TileRow = 1, Size = 8, Priority = 2, Palette = null, FlipX = false, FlipY = false },
        };
        var custom = document with { Anchors = anchors, InitialDurationTicks = 4, Palette = 4,
            Frames = new() { ["Hidden"] = [], ["Visible"] = parts }, Animation =
            [new() { DurationTicks = 2, Reserve = "Hidden", Beam = "Hidden", Equipment = "Hidden" },
             new() { DurationTicks = 3, Reserve = "Visible", Beam = "Visible", Equipment = "Visible" }] };
        using (var output = File.Create(path)) PauseSelectorPresentation.Write(output, custom);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(catalog.ContentIdentity != edited.ContentIdentity, "selector-only edit changes catalog identity");
        var customSamus = new SamusState { CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge };
        var customMenu = new PauseMenuState(new PauseMenuEntry { Samus = customSamus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = edited });
        customMenu.Step(SnesButton.R, SnesButton.R);
        while (customMenu.ScreenMode == 0) customMenu.Step(0, 0);
        AssertEqual((0, 4), customMenu.ItemSelectorAnimationState, "custom initial duration applies exactly at page setup");
        int expectedFrame = 0, expectedTimer = 4;
        for (int tick = 0; tick < 80; tick++)
        {
            if (--expectedTimer <= 0) { expectedFrame = (expectedFrame + 1) % 2; expectedTimer = expectedFrame == 0 ? 2 : 3; }
            customMenu.Step(0, 0);
            AssertEqual((expectedFrame, expectedTimer), customMenu.ItemSelectorAnimationState, "authored animation obeys exact initial and loop timing");
            var snapshot = customMenu.CaptureRenderSnapshot();
            var oam = new ScreenSprites(edited.Sprites.Page, ObjectSelectionCodec.Sizes(PauseMenuLayout.ObjectSelection)); oam.BeginFrame(); edited.PauseSelectors.Draw(oam, PauseEquipmentCategory.Beams, 0, expectedFrame); oam.FinishFrame();
            AssertEqual(expectedFrame == 0, snapshot.Memory.SpriteParts().Length == 0, "explicit hidden and visible phases reach the frame");
            AssertTrue(PlacedSpritePart.Of(oam.Draws).SequenceEqual(snapshot.Memory.SpriteParts()),
                "the frame shows exactly the authored selector phase");
            if (expectedFrame == 1)
            {
                AssertEqual((byte)112, oam.LowTable[0], "authored selector anchor plus X offset");
                AssertEqual((byte)86, oam.LowTable[1], "authored selector anchor plus Y offset");
                var attributes = new SnesObjAttributeWord(SnesLittleEndian.Word(oam.LowTable[2], oam.LowTable[3]));
                AssertEqual(SnesObjAttributeWord.Create(50, 5, 3, SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical), attributes, "authored selector tile, explicit palette, priority and flips");
                AssertTrue((oam.HighTable[0] & 2) != 0, "authored large selector sprite size");
                AssertEqual(4, new SnesObjAttributeWord(SnesLittleEndian.Word(oam.LowTable[6], oam.LowTable[7])).PaletteIndex, "null part palette inherits authored selector palette");
            }
            AssertTrue(customMenu.Render().SequenceEqual(RenderForComparison(snapshot)), "custom selector animation reaches direct and captured rendering consistently");
        }
        var before = customMenu.Render(); var timing = customMenu.ItemSelectorAnimationState;
        using (var state = new MemoryStream())
        {
            DebuggerObjectGraphSerializer.Serialize(state, new object[] { customMenu, customSamus }); state.Position = 0;
            var restored = DebuggerObjectGraphSerializer.Deserialize<object[]>(state);
            customMenu = (PauseMenuState)restored[0]; customSamus = (SamusState)restored[1];
        }
        customMenu.BindMapPresentation(catalog);
        AssertEqual(timing, customMenu.ItemSelectorAnimationState, "rebind does not reset saved visual timing");
        customMenu.Render(); AssertEqual((ushort)47, customMenu.LastIndicatorOrigin.X, "restored stock anchor replaces edited anchor");
        customMenu.BindMapPresentation(edited); AssertTrue(before.SequenceEqual(customMenu.Render()), "restored edit returns exact pixels without advancing animation");
        customMenu.Step(0, SnesButton.A);
        AssertEqual(SamusBeamFlags.None, customSamus.EquippedBeams, "custom hidden/visible selector cannot prevent actual equipment toggle");
        AssertEqual(SamusBeamFlags.Charge, customSamus.CollectedBeams, "selector edit cannot change collection");
        var shortened = Create(guard, catalog, PauseEquipmentCategory.Beams, 0);
        for (int tick = 0; shortened.ItemSelectorAnimationState.Frame != 12 && tick < 100; tick++) shortened.Step(0, 0);
        var oldTiming = shortened.ItemSelectorAnimationState;
        AssertEqual(12, oldTiming.Frame, "short-cycle fixture reaches a stock-only phase");
        shortened.BindMapPresentation(edited);
        AssertEqual(oldTiming, shortened.ItemSelectorAnimationState, "shorter content does not rewrite saved timer/phase at bind");
        AssertEqual(0, shortened.CaptureRenderSnapshot().Memory.SpriteParts().Length, "short cycle safely projects saved phase modulo its new length");
        for (int tick = 0; tick < oldTiming.Timer; tick++) shortened.Step(0, 0);
        AssertEqual((1, 3), shortened.ItemSelectorAnimationState, "next expiry advances into the authored short cycle");
        var empty = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(), System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = edited });
        EnterPauseEquipment(empty);
        var emptyTiming = empty.ItemSelectorAnimationState;
        for (int tick = 0; tick < 40; tick++) empty.Step(0, 0);
        AssertEqual(emptyTiming, empty.ItemSelectorAnimationState, "empty inventory retains native animation gating");
        AssertEqual(0, empty.CaptureRenderSnapshot().Memory.SpriteParts().Length, "custom selector cannot appear without any available equipment");
        void Reject(PauseSelectorDocument invalid) => AssertThrows<InvalidDataException>(() => PauseSelectorPresentation.Write(new MemoryStream(), invalid), "invalid selector document rejected");
        Reject(document with { Version = 2 }); Reject(document with { InitialDurationTicks = 0 }); Reject(document with { Palette = 8 });
        Reject(document with { Anchors = [] }); Reject(document with { Frames = [] }); Reject(document with { Animation = [] });
        Reject(document with { Anchors = new(document.Anchors) { ["Beam.Charge"] = new(256, 90) } });
        Reject(custom with { Animation = [custom.Animation[0] with { Beam = "missing" }] });
        Reject(custom with { Animation = [custom.Animation[0] with { DurationTicks = 0 }] });
        Reject(custom with { Frames = new() { ["Hidden"] = [], ["Visible"] = [parts[0] with { TileColumn = 15 }] } });
        File.WriteAllText(path, "bad JSON"); AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "invalid selector override never falls back");
        File.WriteAllBytes(path, bytes);
        try
        {
            File.Delete(stockPath); AssertThrows<IOException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override cannot hide missing stock selectors");
            File.WriteAllText(stockPath, "corrupt stock"); AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override cannot hide invalid selector provenance");
        }
        finally { File.WriteAllBytes(stockPath, bytes); }
        Console.WriteLine("Pause selectors: sixteen anchors/672 native OAM cases, 1600 guarded native frames, authored timing/parts/palettes, actual toggle, current-content restore and strict failures pass.");
        static PauseMenuState Create(ISnesAddressSpace addressSpace, AreaMapPresentationCatalog content, PauseEquipmentCategory category, int item)
        {
            var samus = new SamusState();
            if (category == PauseEquipmentCategory.Reserves) { samus.MaxReserveEnergy = 100; samus.ReserveEnergy = 50; samus.ReserveTankMode = ReserveTankMode.Manual; }
            else
            {
                PauseEquipmentBit bit = PauseEquipmentRules.Mask(category, item);
                samus.CollectedBeams = samus.EquippedBeams = bit.Beam;
                samus.CollectedItems = samus.EquippedItems = bit.Item;
            }
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = content });
            EnterPauseEquipment(pause);
            if (category == 0 && item == 1) pause.Step(0, SnesButton.Down);
            return pause;
        }
    }
    /// <summary>Blocks runtime reads of native selector anchors, animation lists, and spritemaps after installation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause selector anchor and spritemap ranges")]
    private static CartridgeReadGuard NewPauseSelectorReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        void Add(int address, int count) { for (int i = 0; i < count; i++) blocked.Add(address + i); }
        Add(0x82c0da, 2); Add(0x82c0ec, 2); Add(0x82c100, 2); Add(0x82c10c, 1); Add(0x82c18e, 8); Add(0x82c1e8, 2);
        Add(0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c0ec), 43);
        Add(0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c1e8), 8);
        foreach (var anchor in PauseSelectorDefinitions.Anchors())
            Add((0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c18e + (int)anchor.Category * 2)) + anchor.Item * 4, 4);
        foreach (int id in new[] { 0x14, 0x15, 0x16 })
        {
            Add(0x82c569 + id * 2, 2);
            int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c569 + id * 2);
            Add(pointer, 2 + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), pointer) * 5);
        }
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed pause read selector data at {address:X6}.");
    }
}
