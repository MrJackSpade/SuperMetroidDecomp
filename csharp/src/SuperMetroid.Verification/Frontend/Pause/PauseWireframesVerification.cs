using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks imported pause equipment wireframes and isolated artwork replacement across menu states.</summary>
internal static class PauseWireframesVerification
{
    /// <summary>Compares all four native patches, page transitions, equipment toggles, and restored-content rebinding.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause wireframe artwork")]
    internal static void VerifyPauseWireframes(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseWireframeDefinitions.FileName), path = Path.Combine(overrides, PauseWireframeDefinitions.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        var document = RequiredJson.Deserialize<PauseWireframeDocument>(stockBytes, ArtworkJson.Options);
        var guard = NewPauseWireframeReadGuard(bus);
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            byte[] expected = Enumerable.Repeat((byte)0x5a, 2048).ToArray();
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82b25f + (int)kind * 2);
            for (int row = 0; row < 17; row++)
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0x820000 | (pointer + row * 16), 16).CopyTo(expected, 472 + row * 64);
            byte[] actual = Enumerable.Repeat((byte)0x5a, 2048).ToArray();
            original.PauseWireframes.ApplyTo(actual, kind);
            AssertTrue(expected.SequenceEqual(actual), "all 136 native wireframe words and untouched surrounding bytes match");
            // One replacement at a time proves semantic variants do not alias.
            var frames = document.Frames.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
            frames[kind.ToString()] = frames[kind.ToString()].Select(cell => cell with
                { Atlas = PauseAtlas.Map, TileColumn = 4, TileRow = 0, Palette = 3, Priority = true, FlipX = true, FlipY = true }).ToArray();
            using (var output = File.Create(path)) PauseWireframePresentation.Write(output, document with { Frames = frames });
            var edited = AreaMapPresentationCatalog.Load(stock, overrides);
            AssertTrue(original.ContentIdentity != edited.ContentIdentity, "independent wireframe edit changes selected identity");
            foreach (PauseWireframeKind other in Enum.GetValues<PauseWireframeKind>())
            {
                var baseline = new byte[2048]; var changed = new byte[2048];
                original.PauseWireframes.ApplyTo(baseline, other); edited.PauseWireframes.ApplyTo(changed, other);
                AssertEqual(kind != other, baseline.SequenceEqual(changed), "wireframe override affects only its named variant");
            }
            ushort items = (ushort)(((int)kind & 1) * 0x100 | ((int)kind >> 1));
            foreach (bool gravity in new[] { false, true })
            {
                SamusEquipmentFlags equipped = (SamusEquipmentFlags)items | (gravity ? SamusEquipmentFlags.GravitySuit : SamusEquipmentFlags.None);
                var samus = new SamusState { CollectedItems = equipped, EquippedItems = equipped, CollectedBeams = SamusBeamFlags.Charge };
                var native = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState { CollectedItems = equipped, EquippedItems = equipped, CollectedBeams = samus.CollectedBeams }, System = new Bank80SystemState(), AreaIndex = AreaId.Maridia, RoomMapPosition = new(5, 5), MapPresentation = original });
                var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Maridia, RoomMapPosition = new(5, 5), MapPresentation = original });
                // Every tick compares the complete render inputs; identical inputs cannot render
                // differently, so pixels are rasterized once each page change settles.
                void StepBoth(SnesButton input)
                {
                    native.Step(input, input); pause.Step(input, input);
                    var nativeInputs = native.CaptureRenderSnapshot(); var portInputs = pause.CaptureRenderSnapshot();
                    AssertTrue(nativeInputs.Memory.Vram.SequenceEqual(portInputs.Memory.Vram), "every native wireframe variant retains exact VRAM through page transitions, including Gravity");
                    AssertTrue(nativeInputs.Memory.Cgram.SequenceEqual(portInputs.Memory.Cgram) && nativeInputs.Memory.SpriteParts().SequenceEqual(portInputs.Memory.SpriteParts()) &&
                        nativeInputs.Brightness == portInputs.Brightness, "every native wireframe variant retains exact colors, objects and brightness through page transitions");
                }
                void AssertPixels() =>
                    AssertTrue(native.Render().SequenceEqual(pause.Render()), "wireframe migration retains actual native pixels with all artwork reads blocked");
                // To the equipment page and back, each change run to completion (#1266 cadence).
                ChangePausePage(pause, SnesButton.R, StepBoth);
                AssertPixels();
                ChangePausePage(pause, SnesButton.L, StepBoth);
                AssertPixels();
                AssertEqual(PauseScreenMode.Map, pause.ScreenMode, "wireframe fixture returns to the map page");
                // Rebind on the map must not replace the map's shared BG1 page.
                byte[] map = pause.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x1000).ToArray();
                pause.BindMapPresentation(edited);
                AssertTrue(map.SequenceEqual(pause.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x1000)), "wireframe edit while viewing map does not overwrite BG1 map");
                EnterEquipment(pause);
                AssertPage(pause, edited, kind);
                var editedPixels = pause.Render();
                AssertTrue(editedPixels.SequenceEqual(RenderForComparison(pause.CaptureRenderSnapshot())), "edited wireframe agrees between direct and captured rendering");
                var inventory = new Inventory(samus.EquippedItems, samus.CollectedItems, samus.EquippedBeams, samus.CollectedBeams);
                var cursor = new CursorBeforeRestore(pause.SelectedCategory, pause.SelectedItem);
                using var captured = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(captured, new object[] { pause, samus }); captured.Position = 0;
                var restored = DebuggerObjectGraphSerializer.Deserialize<object[]>(captured);
                pause = (PauseMenuState)restored[0]; samus = (SamusState)restored[1];
                pause.BindMapPresentation(original);
                AssertPage(pause, original, kind);
                AssertTrue(!editedPixels.SequenceEqual(pause.Render()), "restored current wireframe changes visible equipment pixels");
                AssertEqual(cursor, new CursorBeforeRestore(pause.SelectedCategory, pause.SelectedItem), "wireframe replacement preserves cursor");
                AssertEqual(inventory, new Inventory(samus.EquippedItems, samus.CollectedItems, samus.EquippedBeams, samus.CollectedBeams), "wireframe replacement cannot equip or collect upgrades");
                pause.BindMapPresentation(edited);
                AssertTrue(editedPixels.SequenceEqual(pause.Render()), "repeated restored-content rebind preserves exact animation phase");
            }
        }
        // Exercise selection through real menu actions, not only a direct bit fixture.
        var toggledSamus = new SamusState { CollectedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.HiJumpBoots),
            EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.HiJumpBoots) };
        var toggles = new PauseMenuState(new PauseMenuEntry { Samus = toggledSamus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original });
        EnterEquipment(toggles); AssertPage(toggles, original, PauseWireframeKind.VariaSuitHiJump);
        toggles.Step(0, SnesButton.A); AssertPage(toggles, original, PauseWireframeKind.PowerSuitHiJump);
        toggles.Step(0, SnesButton.Down);
        AssertEqual(new CursorBeforeRestore(PauseEquipmentCategory.Boots, 0), new CursorBeforeRestore(toggles.SelectedCategory, toggles.SelectedItem), "actual navigation reaches Hi-Jump");
        toggles.Step(0, SnesButton.A); AssertPage(toggles, original, PauseWireframeKind.PowerSuit);
        toggles.Step(0, SnesButton.Up);
        AssertEqual(new CursorBeforeRestore(PauseEquipmentCategory.Suits, 0), new CursorBeforeRestore(toggles.SelectedCategory, toggles.SelectedItem), "actual navigation returns to Varia");
        toggles.Step(0, SnesButton.A); AssertPage(toggles, original, PauseWireframeKind.VariaSuit);

        void Reject(PauseWireframeDocument invalid) => AssertThrows<InvalidDataException>(() =>
            PauseWireframePresentation.Write(new MemoryStream(), invalid), "invalid wireframe schema/shape/atlas reference rejected");
        Reject(document with { Version = 2 }); Reject(document with { Frames = [] });
        var invalidFrames = document.Frames.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        invalidFrames[nameof(PauseWireframeKind.PowerSuit)] = []; Reject(document with { Frames = invalidFrames });
        invalidFrames[nameof(PauseWireframeKind.PowerSuit)] = document.Frames[nameof(PauseWireframeKind.PowerSuit)].ToArray();
        foreach (var invalid in new[] { document.Frames[nameof(PauseWireframeKind.PowerSuit)][0] with { Atlas = (PauseAtlas)byte.MaxValue },
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
        { EnterPauseEquipment(pause); AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "wireframe fixture enters equipment"); }
        static void AssertPage(PauseMenuState pause, AreaMapPresentationCatalog catalog, PauseWireframeKind kind)
        {
            byte[] actual = pause.CaptureRenderSnapshot().Memory.Vram.Slice(0x6000, 0x800).ToArray();
            byte[] expected = actual.ToArray(); catalog.PauseWireframes.ApplyTo(expected, kind);
            AssertTrue(expected.SequenceEqual(actual), "actual equipment page contains the selected current-content wireframe patch");
        }
    }

    /// <summary>Blocks the four native wireframe bodies and their pointer table during installed pause rendering.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause wireframe pointer table and artwork ranges")]
    private static CartridgeReadGuard NewPauseWireframeReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        for (int kind = 0; kind < 4; kind++)
        {
            int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82b25f + kind * 2);
            for (int i = 0; i < 272; i++) blocked.Add(pointer + i);
        }
        for (int i = 0; i < 8; i++) blocked.Add(0x82b25f + i);
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed pause read wireframe art at {address:X6}.");
    }
}
