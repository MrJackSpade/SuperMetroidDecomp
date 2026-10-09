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
    /// <summary>
    /// Verifies native pause wireframes, isolated visual overrides, equipment interactions,
    /// rendering parity, and restore behavior while blocking runtime reads of wireframe art.
    /// </summary>
    /// <param name="bus">Retail cartridge address space used to compare native assets and seed the read guard.</param>
    /// <param name="stock">Directory containing the installed stock presentation documents.</param>
    /// <param name="overrides">Directory used for temporary pause-wireframe overrides.</param>
    /// <param name="original">Unmodified catalog used as the baseline for edits and content restoration.</param>
    private static void VerifyPauseWireframes(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseWireframeDefinitions.FileName), path = Path.Combine(overrides, PauseWireframeDefinitions.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        var document = JsonSerializer.Deserialize<PauseWireframeDocument>(stockBytes, MapPresentationFormat.JsonOptions)!;
        var guard = new PauseWireframeReadGuard(bus);
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            byte[] expected = Enumerable.Repeat((byte)0x5a, 2048).ToArray();
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82b25f + (int)kind * 2);
            for (int row = 0; row < 17; row++)
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0x820000 | (pointer + row * 16), 16).CopyTo(expected, 472 + row * 64);
            byte[] actual = Enumerable.Repeat((byte)0x5a, 2048).ToArray();
            original.PauseWireframes.ApplyTo(actual, kind);
            AssertTrue(expected.AsSpan().SequenceEqual(actual), "all 136 native wireframe words and untouched surrounding bytes match");
            // One replacement at a time proves semantic variants do not alias.
            var frames = document.Frames.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
            frames[kind.ToString()] = frames[kind.ToString()].Select(cell => cell with
                { Atlas = "Map", TileColumn = 4, TileRow = 0, Palette = 3, Priority = true, FlipX = true, FlipY = true }).ToArray();
            using (var output = File.Create(path)) PauseWireframePresentation.Write(output, document with { Frames = frames });
            var edited = AreaMapPresentationCatalog.Load(stock, overrides);
            AssertTrue(original.ContentIdentity != edited.ContentIdentity, "independent wireframe edit changes selected identity");
            foreach (PauseWireframeKind other in Enum.GetValues<PauseWireframeKind>())
            {
                var baseline = new byte[2048]; var changed = new byte[2048];
                original.PauseWireframes.ApplyTo(baseline, other); edited.PauseWireframes.ApplyTo(changed, other);
                AssertEqual(kind != other, baseline.AsSpan().SequenceEqual(changed), "wireframe override affects only its named variant");
            }
            ushort items = (ushort)(((int)kind & 1) * 0x100 | ((int)kind >> 1));
            foreach (bool gravity in new[] { false, true })
            {
                ushort equipped = (ushort)(items | (gravity ? (ushort)SamusEquipmentFlags.GravitySuit : 0));
                var samus = new SamusState { CollectedItems = equipped, EquippedItems = equipped, CollectedBeams = (ushort)SamusBeamFlags.Charge };
                var native = new PauseMenuState(bus, new SamusState { CollectedItems = equipped, EquippedItems = equipped, CollectedBeams = samus.CollectedBeams },
                    new Bank80SystemState(), AreaId.Maridia, 5, 5,
                    mapPresentation: original);
                var pause = new PauseMenuState(guard, samus, new Bank80SystemState(), AreaId.Maridia, 5, 5, mapPresentation: original);
                // Every tick compares the complete render inputs; identical inputs cannot render
                // differently, so pixels are rasterized once each page change settles.
                void StepBoth(ushort input)
                {
                    native.Step(input, input); pause.Step(input, input);
                    var nativeInputs = native.CaptureRenderSnapshot(); var portInputs = pause.CaptureRenderSnapshot();
                    AssertTrue(nativeInputs.Memory.Vram.SequenceEqual(portInputs.Memory.Vram), "every native wireframe variant retains exact VRAM through page transitions, including Gravity");
                    AssertTrue(nativeInputs.Memory.Cgram.SequenceEqual(portInputs.Memory.Cgram) && nativeInputs.Memory.Oam.SequenceEqual(portInputs.Memory.Oam) &&
                        nativeInputs.Brightness == portInputs.Brightness, "every native wireframe variant retains exact colors, objects and brightness through page transitions");
                }
                void AssertPixels() =>
                    AssertTrue(native.Render().AsSpan().SequenceEqual(pause.Render()), "wireframe migration retains actual native pixels with all artwork reads blocked");
                // To the equipment page and back, each change run to completion (#1266 cadence).
                ChangePausePage(pause, SnesButton.R, StepBoth);
                AssertPixels();
                ChangePausePage(pause, SnesButton.L, StepBoth);
                AssertPixels();
                AssertEqual(0, pause.ScreenMode, "wireframe fixture returns to the map page");
                // Rebind on the map must not replace the map's shared BG1 page.
                byte[] map = pause.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x1000).ToArray();
                pause.BindMapPresentation(edited);
                AssertTrue(map.AsSpan().SequenceEqual(pause.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x1000)), "wireframe edit while viewing map does not overwrite BG1 map");
                EnterEquipment(pause);
                AssertPage(pause, edited, kind);
                var editedPixels = pause.Render();
                AssertTrue(editedPixels.AsSpan().SequenceEqual(RenderForComparison(pause.CaptureRenderSnapshot())), "edited wireframe agrees between direct and captured rendering");
                var inventory = (samus.EquippedItems, samus.CollectedItems, samus.EquippedBeams, samus.CollectedBeams);
                var cursor = (pause.SelectedCategory, pause.SelectedItem);
                using var captured = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(captured, new object[] { pause, samus }); captured.Position = 0;
                var restored = DebuggerObjectGraphSerializer.Deserialize<object[]>(captured);
                pause = (PauseMenuState)restored[0]; samus = (SamusState)restored[1];
                pause.BindMapPresentation(original);
                AssertPage(pause, original, kind);
                AssertTrue(!editedPixels.AsSpan().SequenceEqual(pause.Render()), "restored current wireframe changes visible equipment pixels");
                AssertEqual(cursor, (pause.SelectedCategory, pause.SelectedItem), "wireframe replacement preserves cursor");
                AssertEqual(inventory, (samus.EquippedItems, samus.CollectedItems, samus.EquippedBeams, samus.CollectedBeams), "wireframe replacement cannot equip or collect upgrades");
                pause.BindMapPresentation(edited);
                AssertTrue(editedPixels.AsSpan().SequenceEqual(pause.Render()), "repeated restored-content rebind preserves exact animation phase");
            }
        }
        // Exercise selection through real menu actions, not only a direct bit fixture.
        var toggledSamus = new SamusState { CollectedItems = (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.HiJumpBoots),
            EquippedItems = (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.HiJumpBoots) };
        var toggles = new PauseMenuState(guard, toggledSamus, new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: original);
        EnterEquipment(toggles); AssertPage(toggles, original, PauseWireframeKind.VariaSuitHiJump);
        toggles.Step(0, (ushort)SnesButton.A); AssertPage(toggles, original, PauseWireframeKind.PowerSuitHiJump);
        toggles.Step(0, (ushort)SnesButton.Down);
        AssertEqual((3, 0), (toggles.SelectedCategory, toggles.SelectedItem), "actual navigation reaches Hi-Jump");
        toggles.Step(0, (ushort)SnesButton.A); AssertPage(toggles, original, PauseWireframeKind.PowerSuit);
        toggles.Step(0, (ushort)SnesButton.Up);
        AssertEqual((2, 0), (toggles.SelectedCategory, toggles.SelectedItem), "actual navigation returns to Varia");
        toggles.Step(0, (ushort)SnesButton.A); AssertPage(toggles, original, PauseWireframeKind.VariaSuit);

        void Reject(PauseWireframeDocument invalid) => AssertThrows<InvalidDataException>(() =>
            PauseWireframePresentation.Write(new MemoryStream(), invalid), "invalid wireframe schema/shape/atlas reference rejected");
        Reject(document with { Version = 2 }); Reject(document with { Frames = [] });
        var invalidFrames = document.Frames.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        invalidFrames[nameof(PauseWireframeKind.PowerSuit)] = []; Reject(document with { Frames = invalidFrames });
        invalidFrames[nameof(PauseWireframeKind.PowerSuit)] = document.Frames[nameof(PauseWireframeKind.PowerSuit)].ToArray();
        foreach (var invalid in new[] { document.Frames[nameof(PauseWireframeKind.PowerSuit)][0] with { Atlas = "Other" },
            document.Frames[nameof(PauseWireframeKind.PowerSuit)][0] with { TileRow = 8 },
            document.Frames[nameof(PauseWireframeKind.PowerSuit)][0] with { Palette = 8 } })
        { invalidFrames[nameof(PauseWireframeKind.PowerSuit)][0] = invalid; Reject(document with { Frames = invalidFrames }); }
        AssertThrows<ArgumentOutOfRangeException>(() => original.PauseWireframes.ApplyTo(new byte[2048], (PauseWireframeKind)4), "invalid compiled selector rejected");
        AssertThrows<ArgumentException>(() => original.PauseWireframes.ApplyTo(new byte[32], PauseWireframeKind.PowerSuit), "short patch target rejected before partial write");
        File.WriteAllText(path, "broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "invalid wireframe override never silently replaced");
        File.WriteAllBytes(path, stockBytes);
        try
        {
            File.Delete(stockPath);
            AssertThrows<IOException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "wireframe override cannot conceal missing stock");
            File.WriteAllText(stockPath, "corrupt stock");
            AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "wireframe override cannot conceal failed provenance");
        }
        finally { File.WriteAllBytes(stockPath, stockBytes); }
        Console.WriteLine("Pause wireframes: four exact patches/untouched surroundings, 576 guarded native transition frames, isolated visual edits, actual equipment toggles, current-content restore and strict failures pass.");

        static void EnterEquipment(PauseMenuState pause)
        { EnterPauseEquipment(pause); AssertEqual(1, pause.ScreenMode, "wireframe fixture enters equipment"); }
        static void AssertPage(PauseMenuState pause, AreaMapPresentationCatalog catalog, PauseWireframeKind kind)
        {
            byte[] actual = pause.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x800).ToArray();
            byte[] expected = actual.ToArray(); catalog.PauseWireframes.ApplyTo(expected, kind);
            AssertTrue(expected.AsSpan().SequenceEqual(actual), "actual equipment page contains the selected current-content wireframe patch");
        }
    }

    /// <summary>
    /// Wraps a cartridge bus and rejects runtime access to the native pause-wireframe
    /// selector table and all four referenced art payloads.
    /// </summary>
    private sealed class PauseWireframeReadGuard : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Underlying cartridge bus for reads outside the guarded wireframe ranges.</summary>
        private readonly ISnesAddressSpace source;

        /// <summary>Addresses of native selector and wireframe bytes that installed rendering must not reread.</summary>
        private readonly HashSet<int> blocked = [];

        /// <summary>Builds the blocked address set from the cartridge's four wireframe pointers.</summary>
        /// <param name="source">Address space used to read the selector table and later serve permitted requests.</param>
        public PauseWireframeReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            for (int kind = 0; kind < 4; kind++)
            {
                int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82b25f + kind * 2);
                for (int i = 0; i < 272; i++) blocked.Add(pointer + i);
            }
            for (int i = 0; i < 8; i++) blocked.Add(0x82b25f + i);
        }
        /// <summary>Routes importer reads through the same blocked-address check as runtime reads.</summary>
        /// <param name="address">Cartridge byte address requested by the importer.</param>
        /// <returns>The underlying byte when the address is not a guarded selector or art byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from guarded wireframe data and forwards other byte reads to the wrapped bus.</summary>
        /// <param name="address">Cartridge byte address requested by the caller.</param>
        /// <returns>The wrapped bus value for an allowed address.</returns>
        public byte ReadByte(int address) => blocked.Contains(address)
            ? throw new InvalidOperationException($"Installed pause read wireframe art at {address:X6}.") : source.ReadByte(address);

        /// <summary>Forwards a byte write unchanged to the wrapped cartridge bus.</summary>
        /// <param name="address">Cartridge byte address to update.</param>
        /// <param name="value">Byte value to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
