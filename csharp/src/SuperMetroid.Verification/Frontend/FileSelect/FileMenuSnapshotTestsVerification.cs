using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileMapSnapshotTestsVerification;
using static SuperMetroid.Verification.Frontend.FrontendCompletionVerification;
using static SuperMetroid.Verification.Hud.MessageSnapshotTestsVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks retained file-select and options render packets through native menu flows.</summary>
internal static class FileMenuSnapshotTestsVerification
{
    /// <summary>Compares live rendering and snapshots across copy, clear, controller, special-setting, and fade phases.</summary>
    internal static void VerifyFileMenuRenderSnapshots()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot { Health = 99, MaxHealth = 99 });
        int comparisons = 0;

        // Use private in-memory saves. Copy/clear validation must never edit player saves.
        foreach (bool clear in new[] { false, true })
        {
            var menu = new FileSelectMenuState(saveRam, mapPresentation: RetailPresentationFixture(), spriteSheets: MenuSheetFixtures.For(RetailPresentationFixture()));
            void Tick(ControllerButton input = ControllerButton.None) => Check(menu.Render, menu.CaptureRenderSnapshot, () => menu.Step(input));
            void Press(ControllerButton input) { Tick(); Tick(input); }
            void Until(FileSelectPhase phase)
            {
                for (int i = 0; i < 90 && menu.Phase != phase; i++) Tick();
                AssertEqual(phase, menu.Phase, "file snapshot fixture phase");
            }
            Until(FileSelectPhase.Main);
            for (int i = 0; i < (clear ? 4 : 3); i++) Press(ControllerButton.Down);
            Press(ControllerButton.A);
            Until(clear ? FileSelectPhase.ClearSelectSlot : FileSelectPhase.CopySelectSource);
            Press(ControllerButton.A);
            if (!clear) Press(ControllerButton.A);
            AssertEqual(clear ? FileSelectPhase.ClearConfirm : FileSelectPhase.CopyConfirm,
                menu.Phase, "file snapshot confirmation");
            Press(ControllerButton.A);
            AssertEqual(clear ? FileSelectPhase.ClearCompleted : FileSelectPhase.CopyCompleted,
                menu.Phase, "file snapshot completed operation");
            Tick();
        }

        var options = new GameOptionsMenuState(mapPresentation: RetailPresentationFixture(), spriteSheets: MenuSheetFixtures.For(RetailPresentationFixture()));
        void OptionTick(ControllerButton input = 0) => Check(options.Render, options.CaptureRenderSnapshot,
            () => options.Step(input));
        void OptionPress(ControllerButton input) { OptionTick(); OptionTick(input); }
        void OptionUntil(GameOptionsPhase phase)
        {
            for (int i = 0; i < 100 && options.Phase != phase; i++) OptionTick();
            AssertEqual(phase, options.Phase, "options snapshot fixture phase");
        }
        OptionUntil(GameOptionsPhase.Main);
        for (int i = 0; i < 3; i++) OptionPress(ControllerButton.Down);
        OptionPress(ControllerButton.A);
        OptionUntil(GameOptionsPhase.ControllerSettings);
        OptionPress(ControllerButton.R);
        for (int i = 0; i < 7; i++)
        {
            OptionPress(ControllerButton.Down);
            OptionUntil(GameOptionsPhase.ControllerSettings);
        }
        OptionPress(ControllerButton.A);
        OptionUntil(GameOptionsPhase.Main);
        for (int i = 0; i < 4; i++) OptionPress(ControllerButton.Down);
        OptionPress(ControllerButton.A);
        OptionUntil(GameOptionsPhase.SpecialSettings);
        OptionPress(ControllerButton.A);
        OptionPress(ControllerButton.Down);
        OptionPress(ControllerButton.Right);
        OptionTick();
        Console.WriteLine($"  File/options snapshots: {comparisons} retail frames match through COPY/CLEAR, controller scrolling, special settings and fades.");

        void Check(Func<RgbaPixel[]> legacy, Func<LayeredScene> capture, Action step)
        {
            RgbaPixel[] expected = legacy();
            LayeredScene snapshot = capture();
            Match(expected, DisplayRendering.Render(
                new DisplayPacket(new(comparisons + 1, 1, (ushort)comparisons), snapshot)));
            Match(expected, DisplayRendering.Render(snapshot));
            Match(expected, DisplayRendering.Render(capture()));
            step();
            Match(expected, DisplayRendering.Render(snapshot));
            comparisons++;
        }

        void Match(RgbaPixel[] expected, RgbaPixel[] actual)
        {
            AssertEqual(expected.Length, actual.Length, "menu snapshot geometry");
            for (int i = 0; i < expected.Length; i++)
                if (expected[i] != actual[i])
                    throw new InvalidOperationException($"File/options snapshot sample {comparisons}, pixel ({i % FrontendFrame.Width},{i / FrontendFrame.Width}): expected {expected[i]}, got {actual[i]}.");
        }
    }

    /// <summary>Runs file, map, options, and message snapshot fixture checks.</summary>
    [VerificationCommand("--menu-fixtures")]
    internal static int MenuFixturesCommand()
    {
     VerifyFileMenuRenderSnapshots(); VerifyFileMapSnapshots(); VerifyControllerBindingsAndOptionsSubmenus(); VerifyMessageSnapshots(); return 0; 
    }
}
