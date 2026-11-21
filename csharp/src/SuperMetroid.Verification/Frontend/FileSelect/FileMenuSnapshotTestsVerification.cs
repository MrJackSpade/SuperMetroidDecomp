using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileMapSnapshotTestsVerification;
using static SuperMetroid.Verification.Frontend.FrontendCompletionVerification;
using static SuperMetroid.Verification.Hud.MessageSnapshotTestsVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Saves;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;

/// <summary>Checks retained file-select and options render packets through native menu flows.</summary>
internal static class FileMenuSnapshotTestsVerification
{
    /// <summary>Compares live rendering and snapshots across copy, clear, controller, special-setting, and fade phases.</summary>
    internal static void VerifyFileMenuRenderSnapshots()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        saveRam.SaveSlot(SaveSlot.A, SaveSnapshotFixtures.NewGame);
        int comparisons = 0;

        // Use private in-memory saves. Copy/clear validation must never edit player saves.
        foreach (bool clear in new[] { false, true })
        {
            var menu = new FileSelectMenuState(saveRam, AudioQueues.Standalone(), screenContent: RetailPresentationFixture(), spriteSheets: MenuSheetFixtures.For(RetailPresentationFixture()));
            void Tick(ControllerButton input = ControllerButton.None) => Check(menu.ComposeScene, () => menu.Step(input));
            void Press(ControllerButton input) { Tick(); Tick(input); }
            void Until(FileSelectPhase phase)
            {
                for (int i = 0; i < 90 && menu.Phase != phase; i++) Tick();
                AssertEqual(phase, menu.Phase, "file snapshot fixture phase");
            }
            Until(new FileSelectPhase.Main());
            for (int i = 0; i < (clear ? 4 : 3); i++) Press(ControllerButton.Down);
            Press(ControllerButton.A);
            Until(clear ? new FileSelectPhase.DataManagement.ClearSelectSlot() : new FileSelectPhase.DataManagement.CopySelectSource());
            Press(ControllerButton.A);
            if (!clear) Press(ControllerButton.A);
            AssertEqual(true, clear ? menu.Phase is FileSelectPhase.DataManagement.ClearConfirm : menu.Phase is FileSelectPhase.DataManagement.CopyConfirm,
                "file snapshot confirmation");
            Press(ControllerButton.A);
            AssertEqual(clear ? new FileSelectPhase.DataManagement.ClearCompleted() : new FileSelectPhase.DataManagement.CopyCompleted(),
                menu.Phase, "file snapshot completed operation");
            Tick();
        }

        var options = new GameOptionsMenuState(AudioQueues.Standalone(), screenContent: RetailPresentationFixture(), spriteSheets: MenuSheetFixtures.For(RetailPresentationFixture()), controllerBindings: SuperMetroid.Core.Input.ControllerBindings.Default, iconCancelEnabled: false, moonwalkEnabled: false, language: TextLanguage.English);
        void OptionTick(ControllerButton input = 0) => Check(options.ComposeScene, () => options.Step(input));
        void OptionPress(ControllerButton input) { OptionTick(); OptionTick(input); }
        void OptionUntil(GameOptionsPage page)
        {
            for (int i = 0; i < 100 && !(options.Phase == GameOptionsPhase.Browsing && options.Page == page); i++) OptionTick();
            AssertTrue(options.Phase == GameOptionsPhase.Browsing && options.Page == page, $"options snapshot fixture browses the {page} page");
        }
        OptionUntil(GameOptionsPage.Primary);
        for (int i = 0; i < 3; i++) OptionPress(ControllerButton.Down);
        OptionPress(ControllerButton.A);
        OptionUntil(GameOptionsPage.Controller);
        OptionPress(ControllerButton.R);
        for (int i = 0; i < 7; i++)
        {
            OptionPress(ControllerButton.Down);
            OptionUntil(GameOptionsPage.Controller);
        }
        OptionPress(ControllerButton.A);
        OptionUntil(GameOptionsPage.Primary);
        for (int i = 0; i < 4; i++) OptionPress(ControllerButton.Down);
        OptionPress(ControllerButton.A);
        OptionUntil(GameOptionsPage.Special);
        OptionPress(ControllerButton.A);
        OptionPress(ControllerButton.Down);
        OptionPress(ControllerButton.Right);
        OptionTick();
        Console.WriteLine($"  File/options snapshots: {comparisons} retail frames match through COPY/CLEAR, controller scrolling, special settings and fades.");

        // The live menu's own frame and a repeated capture are compared with the held snapshot for the pixels they show,
        // which draws them only when their descriptions differ; the held snapshot's whole description, every value it
        // reaches through owned or shared state, is digested before the menu advances and again after (#1532).
        void Check(Func<LayeredScene> capture, Action step)
        {
            LayeredScene snapshot = capture();
            byte[] held = GraphDigest(snapshot);
            AssertTrue(ScenePixels.Same(new DisplayPacket(0, capture()), new DisplayPacket((ushort)comparisons, snapshot)),
                $"File/options snapshot sample {comparisons} shows the live menu's pixels");
            AssertTrue(ScenePixels.Same(capture(), snapshot), $"File/options snapshot sample {comparisons} repeats its capture");
            step();
            AssertTrue(held.AsSpan().SequenceEqual(GraphDigest(snapshot)), $"File/options snapshot sample {comparisons} survives the menu update unchanged");
            comparisons++;
        }
    }

    /// <summary>Runs file, map, options, and message snapshot fixture checks.</summary>
    [VerificationCommand("--menu-fixtures")]
    internal static int MenuFixturesCommand()
    {
     VerifyFileMenuRenderSnapshots(); VerifyFileMapSnapshots(); VerifyControllerBindingsAndOptionsSubmenus(); VerifyMessageSnapshots(); return 0; 
    }
}
