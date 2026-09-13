using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyPauseSelectors(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog catalog)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseSelectorDefinitions.FileName), path = Path.Combine(overrides, PauseSelectorDefinitions.FileName);
        byte[] bytes = File.ReadAllBytes(stockPath);
        var document = JsonSerializer.Deserialize<PauseSelectorDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var guard = new PauseSelectorReadGuard(bus);
        AssertEqual(RomDataReader.ReadWordFixedBank(bus, 0x82c100), PauseMenuLayout.MapMarkerPaletteBits, "compiled map caller palette matches native word");
        AssertEqual((int)bus.ReadByte(0x82c10c), catalog.PauseSelectors.InitialDurationTicks, "selector initial delay matches native initialization");
        int animation = 0x820000 | RomDataReader.ReadWordFixedBank(bus, 0x82c0ec);
        for (int phase = 0; phase < 14; phase++)
            AssertEqual((int)bus.ReadByte(animation + phase * 3), catalog.PauseSelectors.Duration(phase), "all native selector phase durations preserved");
        AssertEqual(14, catalog.PauseSelectors.PhaseCount, "native selector terminator resolves exactly fourteen phases");
        foreach (var anchor in PauseSelectorDefinitions.Anchors())
        {
            int positions = 0x820000 | RomDataReader.ReadWordFixedBank(bus, 0x82c18e + anchor.Category * 2);
            ushort x = (ushort)(RomDataReader.ReadWordFixedBank(bus, positions + anchor.Item * 4) - 1);
            ushort y = (ushort)(RomDataReader.ReadWordFixedBank(bus, positions + anchor.Item * 4 + 2) - 1);
            AssertEqual(new MapLabelPoint(x, y), catalog.PauseSelectors.Anchor(anchor.Category, anchor.Item), "authored anchor retains both native minus-one offsets");
            ushort id = (ushort)(anchor.Category == 0 ? 0x14 : anchor.Category == 1 ? 0x15 : 0x16);
            int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(bus, 0x82c569 + id * 2);
            foreach (int occupied in new[] { 0, 127, 128 })
            for (int phase = 0; phase < 14; phase++)
            {
                var nativeOam = new OamBuffer(); var actualOam = new OamBuffer(); nativeOam.BeginFrame(); actualOam.BeginFrame();
                for (int i = 0; i < occupied; i++) { nativeOam.AddRawSmallSprite(12, 34, 56); actualOam.AddRawSmallSprite(12, 34, 56); }
                nativeOam.AddOnScreenSpritemap(bus, pointer, x, y, 0x600);
                catalog.PauseSelectors.Draw(actualOam, anchor.Category, anchor.Item, phase);
                nativeOam.FinalizeFrame(); actualOam.FinalizeFrame();
                AssertTrue(nativeOam.LowTable.SequenceEqual(actualOam.LowTable) && nativeOam.HighTable.SequenceEqual(actualOam.HighTable), "selector parts/order/attributes/capacity match native");
            }
            var native = Create(bus, null, anchor.Category, anchor.Item);
            var pause = Create(guard, catalog, anchor.Category, anchor.Item);
            AssertEqual((anchor.Category, anchor.Item), (pause.SelectedCategory, pause.SelectedItem), "real menu fixture reaches the reported semantic selector");
            for (int tick = 0; tick < 100; tick++)
            {
                native.Step(0, 0); pause.Step(0, 0);
                AssertEqual(native.ItemSelectorAnimationState, pause.ItemSelectorAnimationState, "real selector phase and timer follow native decrement/advance order");
                AssertTrue(native.Render().AsSpan().SequenceEqual(pause.Render()), "all sixteen selectors match native pixels with pointer/art/position/timing reads forbidden");
                if (tick == 51)
                {
                    using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, pause); state.Position = 0;
                    pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state); pause.BindMapPresentation(catalog);
                }
            }
        }
        var anchors = new Dictionary<string, MapLabelPoint>(document.Anchors) { ["Beam.Charge"] = new(100, 90) };
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
        var customSamus = new SamusState { CollectedBeams = (ushort)SamusBeamFlags.Charge, EquippedBeams = (ushort)SamusBeamFlags.Charge };
        var customMenu = new PauseMenuState(guard, customSamus, new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: edited);
        customMenu.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        while (customMenu.ScreenMode == 0) customMenu.Step(0, 0);
        AssertEqual((0, 4), customMenu.ItemSelectorAnimationState, "custom initial duration applies exactly at page setup");
        int expectedFrame = 0, expectedTimer = 4;
        for (int tick = 0; tick < 80; tick++)
        {
            if (--expectedTimer <= 0) { expectedFrame = (expectedFrame + 1) % 2; expectedTimer = expectedFrame == 0 ? 2 : 3; }
            customMenu.Step(0, 0);
            AssertEqual((expectedFrame, expectedTimer), customMenu.ItemSelectorAnimationState, "authored animation obeys exact initial and loop timing");
            var snapshot = customMenu.CaptureRenderSnapshot();
            AssertEqual(expectedFrame == 0 ? 0 : 2, snapshot.Memory.ModeledSpriteCount, "explicit hidden and visible phases reach real OAM");
            if (expectedFrame == 1)
            {
                var oam = new OamBuffer(); oam.BeginFrame(); edited.PauseSelectors.Draw(oam, 1, 0, expectedFrame);
                AssertEqual((byte)112, oam.LowTable[0], "authored selector anchor plus X offset");
                AssertEqual((byte)86, oam.LowTable[1], "authored selector anchor plus Y offset");
                var attributes = new SnesObjAttributeWord((ushort)(oam.LowTable[2] | oam.LowTable[3] << 8));
                AssertEqual(SnesObjAttributeWord.Create(50, 5, 3, SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical), attributes, "authored selector tile, explicit palette, priority and flips");
                AssertTrue((oam.HighTable[0] & 2) != 0, "authored large selector sprite size");
                AssertEqual(4, new SnesObjAttributeWord((ushort)(oam.LowTable[6] | oam.LowTable[7] << 8)).PaletteIndex, "null part palette inherits authored selector palette");
            }
            AssertTrue(customMenu.Render().AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(snapshot)), "custom selector animation reaches direct and captured rendering consistently");
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
        customMenu.Render(); AssertEqual((ushort)47, customMenu.LastIndicatorOriginX, "restored stock anchor replaces edited anchor");
        customMenu.BindMapPresentation(edited); AssertTrue(before.AsSpan().SequenceEqual(customMenu.Render()), "restored edit returns exact pixels without advancing animation");
        customMenu.Step(0, (ushort)SnesButton.A);
        AssertEqual((ushort)0, customSamus.EquippedBeams, "custom hidden/visible selector cannot prevent actual equipment toggle");
        AssertEqual((ushort)SamusBeamFlags.Charge, customSamus.CollectedBeams, "selector edit cannot change collection");
        var shortened = Create(guard, catalog, 1, 0);
        for (int tick = 0; shortened.ItemSelectorAnimationState.Frame != 12 && tick < 100; tick++) shortened.Step(0, 0);
        var oldTiming = shortened.ItemSelectorAnimationState;
        AssertEqual(12, oldTiming.Frame, "short-cycle fixture reaches a stock-only phase");
        shortened.BindMapPresentation(edited);
        AssertEqual(oldTiming, shortened.ItemSelectorAnimationState, "shorter content does not rewrite saved timer/phase at bind");
        AssertEqual(0, shortened.CaptureRenderSnapshot().Memory.ModeledSpriteCount, "short cycle safely projects saved phase modulo its new length");
        for (int tick = 0; tick < oldTiming.Timer; tick++) shortened.Step(0, 0);
        AssertEqual((1, 3), shortened.ItemSelectorAnimationState, "next expiry advances into the authored short cycle");
        var empty = new PauseMenuState(guard, new SamusState(), new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: edited);
        empty.Step((ushort)SnesButton.R, (ushort)SnesButton.R); for (int tick = 0; tick < 32; tick++) empty.Step(0, 0);
        var emptyTiming = empty.ItemSelectorAnimationState;
        for (int tick = 0; tick < 40; tick++) empty.Step(0, 0);
        AssertEqual(emptyTiming, empty.ItemSelectorAnimationState, "empty inventory retains native animation gating");
        AssertEqual(0, empty.CaptureRenderSnapshot().Memory.ModeledSpriteCount, "custom selector cannot appear without any available equipment");
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
        static PauseMenuState Create(ISnesAddressSpace addressSpace, AreaMapPresentationCatalog? content, int category, int item)
        {
            var samus = new SamusState();
            if (category == 0) { samus.MaxReserveEnergy = 100; samus.ReserveEnergy = 50; samus.ReserveTankMode = 2; }
            else if (category == 1) samus.CollectedBeams = samus.EquippedBeams = PauseEquipmentRules.Mask(category, item);
            else samus.CollectedItems = samus.EquippedItems = PauseEquipmentRules.Mask(category, item);
            var pause = new PauseMenuState(addressSpace, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: content);
            pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R); for (int i = 0; i < 32; i++) pause.Step(0, 0);
            if (category == 0 && item == 1) pause.Step(0, (ushort)SnesButton.Down);
            return pause;
        }
    }
    private sealed class PauseSelectorReadGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> blocked = [];
        public PauseSelectorReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            Add(0x82c0da, 2); Add(0x82c0ec, 2); Add(0x82c100, 2); Add(0x82c10c, 1); Add(0x82c18e, 8); Add(0x82c1e8, 2);
            Add(0x820000 | RomDataReader.ReadWordFixedBank(source, 0x82c0ec), 43);
            Add(0x820000 | RomDataReader.ReadWordFixedBank(source, 0x82c1e8), 8);
            foreach (var anchor in PauseSelectorDefinitions.Anchors())
                Add((0x820000 | RomDataReader.ReadWordFixedBank(source, 0x82c18e + anchor.Category * 2)) + anchor.Item * 4, 4);
            foreach (int id in new[] { 0x14, 0x15, 0x16 })
            {
                Add(0x82c569 + id * 2, 2);
                int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(source, 0x82c569 + id * 2);
                Add(pointer, 2 + RomDataReader.ReadWordFixedBank(source, pointer) * 5);
            }
        }
        private void Add(int address, int count) { for (int i = 0; i < count; i++) blocked.Add(address + i); }
        public byte ReadByte(int address) => blocked.Contains(address) ? throw new InvalidOperationException($"Installed pause read selector data at {address:X6}.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
