using System.Buffers.Binary;
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
    private static void VerifyPauseBackdrops(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseBackdropDefinitions.FileName);
        string path = Path.Combine(overrides, PauseBackdropDefinitions.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        var document = JsonSerializer.Deserialize<PauseBackdropDocument>(stockBytes, MapPresentationFormat.JsonOptions)!;
        var guard = new PauseBackdropReadGuard(bus);
        var editedAreas = document.Areas.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        foreach (var cells in editedAreas.Values)
        {
            // Change the lettering region itself, not a hidden edge of the page.
            // This proves return-to-map cannot secretly reinstall ROM lettering.
            for (int i = 0; i < 12; i++) cells[170 + i] = cells[170 + i] with
                { Atlas = "Map", TileColumn = 4, TileRow = 0, Palette = 3, Priority = true, FlipX = true, FlipY = true };
        }
        var buttons = document.Buttons.ToArray();
        for (int i = 0; i < buttons.Length; i++) buttons[i] = buttons[i] with
            { TileColumn = (buttons[i].TileColumn + 1) % 32, Palette = 7, FlipX = !buttons[i].FlipX };
        using (var output = File.Create(path)) PauseBackdropPresentation.Write(output, document with { Areas = editedAreas, Buttons = buttons });
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "backdrop/button-only edits change catalog identity");
        AssertTrue(original.PauseBackdrops.CreateButtonTilemap().AsSpan().SequenceEqual(
            RomDataReader.ReadFixedBank(bus, 0xb6e400, 0x400)), "all native mutable button words survive extraction");
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            byte[] expected = RomDataReader.ReadFixedBank(bus, 0xb6e000, 0x800);
            ushort label = RomDataReader.ReadWordFixedBank(bus, 0x82965f + (int)area * 2);
            RomDataReader.ReadFixedBank(bus, 0x820000 | label, 24).CopyTo(expected, 170 * 2);
            var vram = new SnesVram(); original.PauseBackdrops.LoadTo(vram, 0, area);
            AssertTrue(expected.AsSpan().SequenceEqual(vram.Bytes[..0x800]), "all 1024 backdrop words match native frame plus unmasked area lettering");
            var native = Create(bus, area, null);
            var pause = Create(guard, area, original);
            var persistentEdit = Create(guard, area, edited);
            // Rebind at every page/fade position, including the instant R/L updates
            // buttons before ScreenMode changes. Compare the following transition
            // too, so a hidden timing/selection reset cannot pass a single-frame test.
            for (int tick = 0; tick < 76; tick++)
            {
                ushort input = tick == 2 ? (ushort)SnesButton.R : tick == 39 ? (ushort)SnesButton.L :
                    tick == 74 ? (ushort)SnesButton.Start : (ushort)0;
                AssertEqual(native.Step(input, input), pause.Step(input, input), "backdrop migration preserves Start acceptance and page controls");
                persistentEdit.Step(input, input);
                var snapshot = pause.CaptureRenderSnapshot();
                AssertTrue(native.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(snapshot.Memory.Vram), "every stock transition frame retains exact native VRAM");
                var pixels = pause.Render();
                AssertTrue(native.Render().AsSpan().SequenceEqual(pixels), "seven-area stock pixels match native path with frame, buttons and label ROM reads blocked");
                var selection = (pause.ScreenMode, pause.SelectedCategory, pause.SelectedItem, pause.MapHorizontalScroll, pause.MapVerticalScroll);
                pause.BindMapPresentation(edited);
                var changed = pause.CaptureRenderSnapshot();
                AssertTrue(changed.Memory.Vram.SequenceEqual(persistentEdit.CaptureRenderSnapshot().Memory.Vram),
                    "fresh edited session and live rebind retain the same artwork through equipment return and Start");
                AssertTrue(snapshot.Memory.Vram.Slice(0x6000, 0x1000).SequenceEqual(changed.Memory.Vram.Slice(0x6000, 0x1000)), "backdrop rebind leaves shared map/equipment BG1 untouched");
                AssertEqual(selection, (pause.ScreenMode, pause.SelectedCategory, pause.SelectedItem, pause.MapHorizontalScroll, pause.MapVerticalScroll), "backdrop rebind leaves navigation and scroll untouched");
                foreach (var span in PauseMenuLayout.ButtonLabelSpans)
                for (int word = 0; word < span.Count; word++)
                {
                    int offset = (PauseMenuLayout.Bg2TilemapWord + span.Word + word) * 2;
                    var before = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(snapshot.Memory.Vram.Slice(offset)));
                    var after = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(changed.Memory.Vram.Slice(offset)));
                    AssertEqual(before.PaletteIndex, after.PaletteIndex, "live button highlight wins over replacement palette at every fade position");
                    AssertTrue(before.CharacterIndex != after.CharacterIndex, "replacement button characters reach live highlighted words");
                }
                if (tick is 0 or 36 or 73)
                {
                    var changedPixels = pause.Render();
                    AssertTrue(!pixels.AsSpan().SequenceEqual(changedPixels), "authored frame/button changes are visible on map and equipment pages");
                    AssertTrue(changedPixels.AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(changed)), "edited backdrop agrees between direct and captured renderers");
                    using var capture = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(capture, pause); capture.Position = 0;
                    pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(capture);
                }
                pause.BindMapPresentation(original);
                AssertTrue(pixels.AsSpan().SequenceEqual(pause.Render()), "current-content restore preserves complete pixels and animation phase");
            }
        }
        // The native invalid-beam selection deliberately leaves an overlong label
        // in equipment VRAM. A full inventory rebuild during content rebind loses it.
        var samus = new SamusState { CollectedItems = (ushort)SamusEquipmentFlags.HiJumpBoots, EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots };
        var glitch = new PauseMenuState(guard, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: original);
        samus.CollectedBeams = 0x100f; samus.EquippedBeams = 4; samus.CollectedItems = samus.EquippedItems = 0x3300;
        glitch.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int tick = 0; tick < 32; tick++) glitch.Step(0, 0);
        glitch.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        AssertEqual((ushort)12, samus.EquippedBeams, "fixture actually performs native Boots-to-Plasma simultaneous-input glitch");
        byte[] glitchPage = glitch.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x800).ToArray();
        AssertEqual((ushort)0x0900, BinaryPrimitives.ReadUInt16LittleEndian(glitchPage.AsSpan(0x514)), "fixture retains native VAR overrun beyond ordinary Plasma label");
        glitch.BindMapPresentation(edited);
        AssertTrue(glitchPage.AsSpan().SequenceEqual(glitch.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x800)), "replacing backdrop does not rebuild away live native equipment glitch");

        void Reject(PauseBackdropDocument invalid, string message) => AssertThrows<InvalidDataException>(() =>
            PauseBackdropPresentation.Write(new MemoryStream(), invalid), message);
        Reject(document with { Version = 2 }, "unsupported backdrop version rejected");
        Reject(document with { Areas = [] }, "missing areas rejected");
        Reject(document with { Buttons = [] }, "incomplete button image rejected");
        var invalidAreas = document.Areas.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        invalidAreas["Crateria"] = [];
        Reject(document with { Areas = invalidAreas }, "wrong backdrop dimensions rejected");
        invalidAreas["Crateria"] = document.Areas["Crateria"].ToArray();
        invalidAreas["Crateria"][0] = new() { Atlas = "Interface", TileColumn = 31, TileRow = 7, Palette = 7, Priority = true, FlipX = true, FlipY = true };
        using (var authored = new MemoryStream())
        {
            PauseBackdropPresentation.Write(authored, document with { Areas = invalidAreas }); authored.Position = 0;
            var compiled = PauseBackdropPresentation.Load(authored); var memory = new SnesVram(); compiled.LoadTo(memory, 0, AreaId.Crateria);
            AssertEqual((ushort)0xfdff, BinaryPrimitives.ReadUInt16LittleEndian(memory.Bytes), "named Interface atlas resolves its own tile coordinates and every visual attribute");
        }
        foreach (var invalid in new[] { document.Buttons[0] with { Atlas = "Other" }, document.Buttons[0] with { TileColumn = 32 },
            document.Buttons[0] with { TileRow = 8 }, document.Buttons[0] with { Palette = -1 }, document.Buttons[0] with { Palette = 8 } })
        {
            invalidAreas["Crateria"][0] = invalid;
            Reject(document with { Areas = invalidAreas }, "invalid atlas, coordinates or palette rejected");
        }
        File.WriteAllText(path, "{\"version\":1,\"gameplay\":true}");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "unknown fields or missing required content fail loudly");
        File.WriteAllText(path, "broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt override cannot silently fall back");
        File.WriteAllBytes(path, stockBytes);
        try
        {
            File.Delete(stockPath);
            AssertThrows<IOException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override cannot conceal missing stock backdrop");
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "override cannot conceal failed stock hash");
        }
        finally { File.WriteAllBytes(stockPath, stockBytes); }
        Console.WriteLine("Pause backdrops: seven native frames/labels, button artwork, 532 guarded transition/rebind frames, visible edits, restore, native equipment overrun preservation and strict resource failures pass.");

        static PauseMenuState Create(ISnesAddressSpace addressSpace, AreaId area, AreaMapPresentationCatalog? catalog)
        {
            var system = new Bank80SystemState(); system.SetAreaMapAcquired(area);
            return new(addressSpace, new SamusState { CollectedBeams = (ushort)SamusBeamFlags.Charge, EquippedBeams = (ushort)SamusBeamFlags.Charge },
                system, area, 10, 10, mapPresentation: catalog);
        }
    }

    private sealed class PauseBackdropReadGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> blocked = [];
        public PauseBackdropReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            Add(0xb6e000, 0x800); Add(0x82965f, 14);
            for (int area = 0; area < 7; area++) Add(0x820000 | RomDataReader.ReadWordFixedBank(source, 0x82965f + area * 2), 24);
        }
        private void Add(int address, int size) { for (int i = 0; i < size; i++) blocked.Add(address + i); }
        public byte ReadByte(int address) => blocked.Contains(address)
            ? throw new InvalidOperationException($"Installed pause read backdrop/button/label at {address:X6}.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
