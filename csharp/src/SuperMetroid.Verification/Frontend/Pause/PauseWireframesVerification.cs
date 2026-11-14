using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks imported pause equipment wireframes and isolated artwork replacement across menu states.</summary>
internal static class PauseWireframesVerification
{
    /// <summary>Compares all four native patches, page transitions, equipment toggles, and restored-content rebinding.</summary>
    internal static void VerifyPauseWireframes(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        var guard = NewPauseWireframeReadGuard(bus);
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            // Every page cell starts as a sentinel tile, so the cells the patch leaves alone must keep it.
            var sentinel = new BackgroundTile(0x25a, 6, FlipHorizontally: true, FlipVertically: false, HighPriority: false);
            BackgroundTile[] expected = Enumerable.Repeat(sentinel, PauseWireframeDefinitions.PageTiles).ToArray();
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82b25f + (int)kind * 2);
            for (int row = 0; row < 17; row++)
                BackgroundTileWordBytes.Tiles(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0x820000 | (pointer + row * 16), 16))
                    .CopyTo(expected, PauseWireframeDefinitions.FirstPageTile + row * PauseWireframeDefinitions.PageColumns);
            BackgroundTile[] actual = Enumerable.Repeat(sentinel, PauseWireframeDefinitions.PageTiles).ToArray();
            InstalledFixtures.ImportOf(original).Pause.Wireframes.ApplyTo(actual, kind);
            AssertTrue(expected.SequenceEqual(actual), "all 136 native wireframe tiles and untouched surrounding cells match");
            ushort items = (ushort)(((int)kind & 1) * 0x100 | ((int)kind >> 1));
            foreach (bool gravity in new[] { false, true })
            {
                SamusEquipmentFlags equipped = SamusEquipmentWordCodec.FromEquipmentWord(items) | (gravity ? SamusEquipmentFlags.GravitySuit : SamusEquipmentFlags.None);
                var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { CollectedItems = equipped, EquippedItems = equipped, CollectedBeams = SamusBeamFlags.Charge };
                var native = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { CollectedItems = equipped, EquippedItems = equipped, CollectedBeams = samus.CollectedBeams }, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Maridia, RoomMapPosition = new(5, 5), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
                var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Maridia, RoomMapPosition = new(5, 5), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
                // Every tick compares the complete render inputs; the pixels are compared once each page change settles,
                // drawing the two frames only when their descriptions differ (#1532).
                void StepBoth(ControllerButton input)
                {
                    native.Step(input, input); pause.Step(input, input);
                    var nativeInputs = native.CaptureRenderSnapshot(); var portInputs = pause.CaptureRenderSnapshot();
                    AssertTrue(native.ShownVram(original).SequenceEqual(pause.ShownVram(original)), "every native wireframe variant retains exact VRAM through page transitions, including Gravity");
                    AssertTrue(PauseShownColours.Of(native, original).SequenceEqual(PauseShownColours.Of(pause, original)) && nativeInputs.SpriteParts().SequenceEqual(portInputs.SpriteParts()) &&
                        nativeInputs.Brightness == portInputs.Brightness, "every native wireframe variant retains exact colors, objects and brightness through page transitions");
                }
                void AssertPixels() =>
                    AssertTrue(ScenePixels.Same(native.CaptureRenderSnapshot(), pause.CaptureRenderSnapshot()), "wireframe migration retains actual native pixels with all artwork reads blocked");
                // To the equipment page and back, each change run to completion (#1266 cadence).
                ChangePausePage(pause, ControllerButton.R, StepBoth);
                AssertPixels();
                ChangePausePage(pause, ControllerButton.L, StepBoth);
                AssertPixels();
                AssertEqual(PauseScreenMode.Map, pause.ScreenMode, "wireframe fixture returns to the map page");
                EnterEquipment(pause);
                AssertPage(pause, original, kind);
            }
        }
        // Exercise selection through real menu actions, not only a direct bit fixture.
        var toggledSamus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { CollectedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.HiJumpBoots),
            EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.HiJumpBoots) };
        var toggles = new PauseMenuState(new PauseMenuEntry { Samus = toggledSamus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        EnterEquipment(toggles); AssertPage(toggles, original, PauseWireframeKind.VariaSuitHiJump);
        toggles.Step(0, ControllerButton.A); AssertPage(toggles, original, PauseWireframeKind.PowerSuitHiJump);
        toggles.Step(0, ControllerButton.Down);
        AssertEqual(new CursorBeforeRestore(PauseEquipmentCategory.Boots, 0), new CursorBeforeRestore(toggles.SelectedCategory, toggles.SelectedItem), "actual navigation reaches Hi-Jump");
        toggles.Step(0, ControllerButton.A); AssertPage(toggles, original, PauseWireframeKind.PowerSuit);
        toggles.Step(0, ControllerButton.Up);
        AssertEqual(new CursorBeforeRestore(PauseEquipmentCategory.Suits, 0), new CursorBeforeRestore(toggles.SelectedCategory, toggles.SelectedItem), "actual navigation returns to Varia");
        toggles.Step(0, ControllerButton.A); AssertPage(toggles, original, PauseWireframeKind.VariaSuit);

        AssertThrows<ArgumentOutOfRangeException>(() => InstalledFixtures.ImportOf(original).Pause.Wireframes.ApplyTo(new BackgroundTile[PauseWireframeDefinitions.PageTiles], (PauseWireframeKind)4), "invalid compiled selector rejected");
        AssertThrows<ArgumentException>(() => InstalledFixtures.ImportOf(original).Pause.Wireframes.ApplyTo(new BackgroundTile[16], PauseWireframeKind.PowerSuit), "short patch target rejected before partial write");
        Console.WriteLine("Pause wireframes: four exact patches/untouched surroundings, 576 guarded native transition frames and actual equipment toggles pass.");

        static void EnterEquipment(PauseMenuState pause)
        { EnterPauseEquipment(pause); AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "wireframe fixture enters equipment"); }
        static void AssertPage(PauseMenuState pause, AreaMapPresentation catalog, PauseWireframeKind kind)
        {
            BackgroundTile[] actual = BackgroundTileWordBytes.Tiles(pause.ShownVram(catalog).AsSpan(0x6000, 0x800));
            BackgroundTile[] expected = actual.ToArray(); InstalledFixtures.ImportOf(catalog).Pause.Wireframes.ApplyTo(expected, kind);
            AssertTrue(expected.SequenceEqual(actual), "actual equipment page contains the selected current-content wireframe patch");
        }
    }

    /// <summary>Blocks the four native wireframe bodies and their pointer table during installed pause rendering.</summary>
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
