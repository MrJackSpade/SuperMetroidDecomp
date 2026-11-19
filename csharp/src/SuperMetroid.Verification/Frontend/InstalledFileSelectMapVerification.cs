using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.Core.Rendering;

/// <summary>Checks file-select map navigation with installed presentation and guarded cartridge reads.</summary>
internal static class InstalledFileSelectMapVerification
{
    /// <summary>Compares stock frames through entry, scroll, debugger restore, and exit handoffs with map ROM blocked.</summary>
    internal static void VerifyInstalledFileSelectMenu(InstalledScreenContent original, bool verifyCapturedRendering = false)
    {
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        var snapshot = SaveSnapshotFixtures.NewGame with { Checkpoint = SuperMetroid.Core.Progression.LoadStationDefinitions.GlassTunnelSave };
        snapshot.Progress.SetAreaMap(AreaId.Maridia, acquired: true);
        snapshot.Progress.UsedStations.Add(LoadStationDefinitions.GlassTunnelSave);
        saves.SaveSlot(SaveSlot.A, snapshot);
        var slot = saves.ReadSlot(SaveSlot.A);
        var control = new FileSelectMapMenuState(AudioQueues.Standalone(), new SharedRandom(), (slot ?? throw new InvalidOperationException("Verification requires slot.")), 0, original, MenuSheetFixtures.For(original));
        var installed = new FileSelectMapMenuState(AudioQueues.Standalone(), new SharedRandom(), slot, 0, original, MenuSheetFixtures.For(original));

        // Each side's frame is captured once per update and the two are compared for the pixels they show (#1532): equal
        // descriptions show equal pixels without a draw, and only differing ones are drawn and compared.
        void StepBoth(ControllerButton input)
        {
            control.Step(input);
            installed.Step(input);
            AssertEqual(control.Phase, installed.Phase, "installed map navigation phase matches cartridge path");
            AssertEqual((control.Outcome == FileSelectMapOutcome.Load), (installed.Outcome == FileSelectMapOutcome.Load), "installed map load handoff timing");
            AssertEqual((control.Outcome == FileSelectMapOutcome.Options), (installed.Outcome == FileSelectMapOutcome.Options), "installed map options handoff timing");
            LayeredScene installedScene = installed.ComposeScene();
            AssertTrue(ScenePixels.Same(control.ComposeScene(), installedScene),
                verifyCapturedRendering ? "installed map render captures retain native pixels through every transition"
                    : "installed map navigation renders exact stock frame");
            if (verifyCapturedRendering)
                AssertTrue(ScenePixels.Same(installed.ComposeScene(), installedScene), "captured and direct map rendering agree");
        }
        for (int i = 0; i < 48; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Area, installed.Phase, "installed menu finishes entry");
        StepBoth(ControllerButton.Start);
        for (int i = 0; i < 54; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Room, installed.Phase, "installed menu reaches room and recreates catalog scroll");
        short ScrollX(FileSelectMapMenuState menu) => menu.Scroll.Scroll.X;
        short initialScrollX = ScrollX(installed);
        for (int i = 0; i < 16; i++) StepBoth(ControllerButton.Left);
        AssertTrue(initialScrollX != ScrollX(installed), "installed menu test actually scrolls before saving");

        // Preserve a live, scrolled menu; the stored state names the catalog it is bound to, and a restore draws from it.
        // The installation's catalog and sheets are named through the process's one index of the installed content, which
        // every capture shares, instead of indexing them again as a list of their own (#1532).
        object[] content = ReferenceEquals(original, InstalledFixtures.Maps) ? [] : [original, MenuSheetFixtures.For(original)];
        byte[] state = GraphDigestVerification.SerializeGraph(installed, content);
        installed = GraphDigestVerification.RestoreGraph<FileSelectMapMenuState>(state, content);
        AssertTrue(ScenePixels.Same(control.ComposeScene(), installed.ComposeScene()), "restored room map retains scrolled frame");
        StepBoth(ControllerButton.B);
        for (int i = 0; i < 60; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Area, installed.Phase, "restored menu returns to area");
        StepBoth(ControllerButton.Start);
        for (int i = 0; i < 54; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Room, installed.Phase, "restored menu reenters room without map ROM access");
        if (verifyCapturedRendering)
        {
            StepBoth(ControllerButton.Start);
            for (int tick = 0; tick < 128 && installed.Outcome != FileSelectMapOutcome.Load; tick++) StepBoth(0);
            AssertTrue((installed.Outcome == FileSelectMapOutcome.Load), "ROM-free map reaches gameplay-load handoff after full fade");
            // Exercise the other exit from a fresh area view as well. The test
            // ends at the handoff; gameplay/options retain separate dependencies.
            control = new FileSelectMapMenuState(AudioQueues.Standalone(), new SharedRandom(), slot, 0, original, MenuSheetFixtures.For(original));
            installed = new FileSelectMapMenuState(AudioQueues.Standalone(), new SharedRandom(), slot, 0, original, MenuSheetFixtures.For(original));
            for (int tick = 0; tick < 48; tick++) StepBoth(0);
            StepBoth(ControllerButton.B);
            for (int tick = 0; tick < 32 && installed.Outcome != FileSelectMapOutcome.Options; tick++) StepBoth(0);
            AssertTrue((installed.Outcome == FileSelectMapOutcome.Options), "ROM-free map reaches options handoff after full fade");
        }
        Console.WriteLine("Installed file-select: stock frame parity through entry, scroll, debugger restore, return and reentry with map ROM reads forbidden.");
    }
}
