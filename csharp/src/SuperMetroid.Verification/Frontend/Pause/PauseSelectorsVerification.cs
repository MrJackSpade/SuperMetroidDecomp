using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorAnchorsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorCompositionsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorDurationsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause equipment selector placement, animation, and restored-state rebinding.</summary>
internal static class PauseSelectorsVerification
{
    /// <summary>Compares all sixteen selectors with guarded native rendering.</summary>
    internal static void VerifyPauseSelectors(ISnesAddressSpace bus, AreaMapPresentation catalog)
    {
        var guard = NewPauseSelectorReadGuard(bus);
        PaletteRowWords.AssertSelectsOnlyPaletteRow(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82b9c9), SuperMetroid.AssetExtraction.Frontend.Map.Artwork.MapSpriteColouringSets.SpriteSet(SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteColouring.Markers), "the map caller's colouring selects the native word's sprite set");
        Suite(nameof(VerifyPauseSelectorAnchors), () => VerifyPauseSelectorAnchors(bus));
        Suite(nameof(VerifyPauseSelectorNativeDurations), () => VerifyPauseSelectorNativeDurations(bus, catalog.PauseSelectors));
        for (int group = 0; group < 3; group++) VerifyPauseSelectorNativeCompositions(bus, catalog.PauseSelectors, group);
        foreach (var anchor in PauseSelectorAnchors.Anchors())
        {
            var native = Create(bus, catalog, anchor.Category, anchor.Item);
            var pause = Create(guard, catalog, anchor.Category, anchor.Item);
            AssertEqual(new CursorBeforeRestore(anchor.Category, anchor.Item), new CursorBeforeRestore(pause.SelectedCategory, pause.SelectedItem), "real menu fixture reaches the reported semantic selector");
            // Every tick compares the complete render inputs; pixels are rasterized once per
            // displayed selector frame, since identical inputs cannot render differently.
            int renderedFrame = -1;
            for (int tick = 0; tick < 100; tick++)
            {
                native.Step(0, 0); pause.Step(0, 0);
                AssertEqual(native.ItemSelectorAnimationState, pause.ItemSelectorAnimationState, "real selector phase and timer follow native decrement/advance order");
                var nativeInputs = native.CaptureRenderSnapshot(); var portInputs = pause.CaptureRenderSnapshot();
                var shownNative = native.ShownVram();
                var shownPause = pause.ShownVram();
                AssertTrue(nativeInputs.Paint.SpriteParts().SequenceEqual(portInputs.Paint.SpriteParts()) && PauseShownColours.Of(native).SequenceEqual(PauseShownColours.Of(pause)) &&
                    shownNative.SequenceEqual(shownPause) && nativeInputs.Brightness == portInputs.Brightness,
                    "all sixteen selectors match native render inputs with pointer/art/position/timing reads forbidden");
                if (pause.ItemSelectorAnimationState.Frame != renderedFrame)
                {
                    renderedFrame = pause.ItemSelectorAnimationState.Frame;
                    AssertTrue(native.Render().SequenceEqual(pause.Render()), "all sixteen selectors match native pixels with pointer/art/position/timing reads forbidden");
                }
                if (tick == 51)
                {
                    using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, pause); state.Position = 0;
                    pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state); pause.BindFixtureArtwork(catalog);
                }
            }
        }
        Console.WriteLine("Pause selectors: sixteen anchors/672 native OAM cases and 1600 guarded native frames pass.");
        static PauseMenuState Create(ISnesAddressSpace addressSpace, AreaMapPresentation content, PauseEquipmentCategory category, int item)
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence());
            if (category == PauseEquipmentCategory.Reserves) { samus.MaxReserveEnergy = 100; samus.ReserveEnergy = 50; samus.ReserveTankMode = ReserveTankMode.Manual; }
            else
            {
                PauseEquipmentBit bit = PauseEquipmentRules.Mask(category, item);
                samus.CollectedBeams = samus.EquippedBeams = bit.Beam;
                samus.CollectedItems = samus.EquippedItems = bit.Item;
            }
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = content, SpriteSheets = MenuSheetFixtures.For(content) });
            EnterPauseEquipment(pause);
            if (category == 0 && item == 1) pause.Step(0, ControllerButton.Down);
            return pause;
        }
    }
    /// <summary>Blocks runtime reads of native selector anchors, animation lists, and spritemaps after installation.</summary>
    private static CartridgeReadGuard NewPauseSelectorReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        void Add(int address, int count) { for (int i = 0; i < count; i++) blocked.Add(address + i); }
        Add(0x82c0da, 2); Add(0x82c0ec, 2); Add(0x82c100, 2); Add(0x82c10c, 1); Add(0x82c18e, 8); Add(0x82c1e8, 2);
        Add(0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c0ec), 43);
        Add(0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c1e8), 8);
        foreach (var anchor in PauseSelectorAnchors.Anchors())
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
