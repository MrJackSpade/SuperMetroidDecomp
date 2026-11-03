using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks file-select map navigation with installed presentation and guarded cartridge reads.</summary>
internal static class InstalledFileSelectMapVerification
{
    /// <summary>Compares stock frames through entry, scroll, debugger restore, and exit handoffs with map ROM blocked.</summary>
    internal static void VerifyInstalledFileSelectMenu(AreaMapPresentation original, bool verifyCapturedRendering = false)
    {
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        var snapshot = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 0, Health = 99, MaxHealth = 99 };
        snapshot.Progress.SetAreaMap(AreaId.Maridia, acquired: true);
        snapshot.Progress.UsedStations(AreaId.Maridia).Add(0);
        saves.SaveSlot(0, snapshot);
        var slot = saves.ReadSlot(0);
        var control = new FileSelectMapMenuState(new AudioQueues(), (slot ?? throw new InvalidOperationException("Verification requires slot.")), 0, original, MenuSheetFixtures.For(original));
        var installed = new FileSelectMapMenuState(new AudioQueues(), slot, 0, original, MenuSheetFixtures.For(original));

        // One raster per side: the two captured renders are compared with each other.
        void StepBoth(ControllerButton input)
        {
            control.Step(input);
            installed.Step(input);
            AssertEqual(control.Phase, installed.Phase, "installed map navigation phase matches cartridge path");
            AssertEqual(control.LoadRequested, installed.LoadRequested, "installed map load handoff timing");
            AssertEqual(control.OptionsRequested, installed.OptionsRequested, "installed map options handoff timing");
            AssertTrue(control.Render().SequenceEqual(installed.Render()), "installed map navigation renders exact stock frame");
            if (verifyCapturedRendering)
            {
                var nativePixels = DisplayRendering.Render(control.CaptureRenderSnapshot());
                var installedPixels = DisplayRendering.Render(installed.CaptureRenderSnapshot());
                AssertTrue(nativePixels.SequenceEqual(installedPixels), "installed map render captures retain native pixels through every transition");
                AssertTrue(installed.Render().SequenceEqual(installedPixels), "captured and direct map rendering agree");
            }
        }
        for (int i = 0; i < 48; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Area, installed.Phase, "installed menu finishes entry");
        StepBoth(ControllerButton.Start);
        for (int i = 0; i < 54; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Room, installed.Phase, "installed menu reaches room and recreates catalog scroll");
        ushort ScrollX(FileSelectMapMenuState menu) => (menu.Scroll).Scroll.X;
        ushort initialScrollX = ScrollX(installed);
        for (int i = 0; i < 16; i++) StepBoth(ControllerButton.Left);
        AssertTrue(initialScrollX != ScrollX(installed), "installed menu test actually scrolls before saving");

        // Preserve a live, scrolled menu; the stored state names the catalog it is bound to, and a restore draws from it.
        byte[] state = GraphDigestVerification.SerializeGraph(installed, original, MenuSheetFixtures.For(original));
        installed = GraphDigestVerification.RestoreGraph<FileSelectMapMenuState>(state, original, MenuSheetFixtures.For(original));
        AssertTrue(control.Render().SequenceEqual(installed.Render()), "restored room map retains scrolled frame");
        StepBoth(ControllerButton.B);
        for (int i = 0; i < 60; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Area, installed.Phase, "restored menu returns to area");
        StepBoth(ControllerButton.Start);
        for (int i = 0; i < 54; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Room, installed.Phase, "restored menu reenters room without map ROM access");
        if (verifyCapturedRendering)
        {
            StepBoth(ControllerButton.Start);
            for (int tick = 0; tick < 128 && !installed.LoadRequested; tick++) StepBoth(0);
            AssertTrue(installed.LoadRequested, "ROM-free map reaches gameplay-load handoff after full fade");
            // Exercise the other exit from a fresh area view as well. The test
            // ends at the handoff; gameplay/options retain separate dependencies.
            control = new FileSelectMapMenuState(new AudioQueues(), slot, 0, original, MenuSheetFixtures.For(original));
            installed = new FileSelectMapMenuState(new AudioQueues(), slot, 0, original, MenuSheetFixtures.For(original));
            for (int tick = 0; tick < 48; tick++) StepBoth(0);
            StepBoth(ControllerButton.B);
            for (int tick = 0; tick < 32 && !installed.OptionsRequested; tick++) StepBoth(0);
            AssertTrue(installed.OptionsRequested, "ROM-free map reaches options handoff after full fade");
        }
        Console.WriteLine("Installed file-select: stock frame parity through entry, scroll, debugger restore, return and reentry with map ROM reads forbidden.");
    }
}
