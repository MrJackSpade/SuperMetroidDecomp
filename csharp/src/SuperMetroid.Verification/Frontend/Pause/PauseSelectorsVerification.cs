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
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause equipment selector placement, animation, and restored-state rebinding.</summary>
internal static class PauseSelectorsVerification
{
    /// <summary>
    /// Compares the selectors' anchors, durations and compositions with the cartridge's, and confirms every one of the sixteen
    /// selectors keeps its animation across a saved-state round trip.
    /// </summary>
    internal static void VerifyPauseSelectors(ISnesAddressSpace bus, AreaMapPresentation catalog)
    {
        PaletteRowWords.AssertSelectsOnlyPaletteRow(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82b9c9), SuperMetroid.AssetExtraction.Frontend.Map.Artwork.MapSpriteColouringSets.SpriteSet(SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteColouring.Markers), "the map caller's colouring selects the native word's sprite set");
        Suite(nameof(VerifyPauseSelectorAnchors), () => VerifyPauseSelectorAnchors(bus));
        Suite(nameof(VerifyPauseSelectorNativeDurations), () => VerifyPauseSelectorNativeDurations(bus, catalog.PauseSelectors));
        for (int group = 0; group < 3; group++) VerifyPauseSelectorNativeCompositions(bus, InstalledFixtures.ImportOf(catalog).PauseSelectors, group);
        foreach (PauseEquipmentControl control in PauseEquipmentControls.All)
        {
            PauseMenuState continuing = Create(catalog, control);
            PauseMenuState pause = Create(catalog, control);
            AssertEqual(control, pause.SelectedControl, "real menu fixture reaches the reported semantic selector");
            // The two menus are built and stepped alike, so only the restored menu's ticks, from its round trip on, are
            // compared with the menu that was never restored (#1680). Each compared tick compares the complete render
            // inputs; the pixels are compared once per displayed selector frame (#1532).
            int renderedFrame = -1;
            for (int tick = 0; tick < 100; tick++)
            {
                continuing.Step(0, 0); pause.Step(0, 0);
                if (tick == RestoredTick)
                {
                    // The capture names the installed content by path, as a host's store does, instead of copying it (#1532).
                    pause = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(pause); pause.BindFixtureArtwork(catalog);
                }
                if (tick < RestoredTick)
                    continue;
                AssertEqual(continuing.ItemSelectorAnimationState, pause.ItemSelectorAnimationState, "a restored selector keeps its phase and timer");
                var continuingInputs = continuing.CaptureRenderSnapshot(); var restoredInputs = pause.CaptureRenderSnapshot();
                AssertTrue(continuingInputs.SpriteParts().SequenceEqual(restoredInputs.SpriteParts()) && PauseShownColours.Of(continuing, catalog).SequenceEqual(PauseShownColours.Of(pause, catalog)) &&
                    PauseScreenMemory.SameImage(continuing, pause, catalog) && continuingInputs.Brightness == restoredInputs.Brightness,
                    "every restored selector keeps the render inputs of the menu never restored");
                if (pause.ItemSelectorAnimationState.Frame != renderedFrame)
                {
                    renderedFrame = pause.ItemSelectorAnimationState.Frame;
                    AssertTrue(ScenePixels.Same(continuingInputs, restoredInputs), "every restored selector keeps the pixels of the menu never restored");
                }
            }
        }
        Console.WriteLine("Pause selectors: sixteen anchors, 672 native OAM cases and the selectors' restored animation pass.");
        static PauseMenuState Create(AreaMapPresentation content, PauseEquipmentControl control)
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
            if (control is not PauseUpgradeControl upgrade) { samus.MaxReserveEnergy = 100; samus.ReserveEnergy = 50; samus.ReserveTankMode = ReserveTankMode.Manual; }
            else
            {
                PauseEquipmentBit bit = upgrade.Upgrade;
                samus.CollectedBeams = samus.EquippedBeams = bit.Beam;
                samus.CollectedItems = samus.EquippedItems = bit.Item;
            }
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = content, SpriteSheets = MenuSheetFixtures.For(content) });
            EnterPauseEquipment(pause);
            if (control == PauseEquipmentControls.ReserveTransfer) pause.Step(0, ControllerButton.Down);
            return pause;
        }
    }

    /// <summary>The tick after which a selector is saved and restored: midway through its animation.</summary>
    private const int RestoredTick = 51;
}