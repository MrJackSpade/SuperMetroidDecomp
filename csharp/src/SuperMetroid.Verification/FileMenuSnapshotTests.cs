using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyFileMenuRenderSnapshots()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var saveRam = new SuperMetroidSaveRam(bus);
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot { Health = 99, MaxHealth = 99 });
        int comparisons = 0;

        // Use private in-memory SRAM. Copy/clear validation must never edit player saves.
        foreach (bool clear in new[] { false, true })
        {
            var menu = new FileSelectMenuState(bus);
            void Tick(ushort input = 0) => Check(menu.Render, menu.CaptureRenderSnapshot, () => menu.Step(input));
            void Press(SnesButton input) { Tick(); Tick((ushort)input); }
            void Until(FileSelectPhase phase)
            {
                for (int i = 0; i < 90 && menu.Phase != phase; i++) Tick();
                AssertEqual(phase, menu.Phase, "file snapshot fixture phase");
            }
            Until(FileSelectPhase.Main);
            for (int i = 0; i < (clear ? 4 : 3); i++) Press(SnesButton.Down);
            Press(SnesButton.A);
            Until(clear ? FileSelectPhase.ClearSelectSlot : FileSelectPhase.CopySelectSource);
            Press(SnesButton.A);
            if (!clear) Press(SnesButton.A);
            AssertEqual(clear ? FileSelectPhase.ClearConfirm : FileSelectPhase.CopyConfirm,
                menu.Phase, "file snapshot confirmation");
            Press(SnesButton.A);
            AssertEqual(clear ? FileSelectPhase.ClearCompleted : FileSelectPhase.CopyCompleted,
                menu.Phase, "file snapshot completed operation");
            Tick();
        }

        var options = new GameOptionsMenuState(bus);
        void OptionTick(ushort input = 0) => Check(options.Render, options.CaptureRenderSnapshot,
            () => options.Step(input));
        void OptionPress(SnesButton input) { OptionTick(); OptionTick((ushort)input); }
        void OptionUntil(GameOptionsPhase phase)
        {
            for (int i = 0; i < 100 && options.Phase != phase; i++) OptionTick();
            AssertEqual(phase, options.Phase, "options snapshot fixture phase");
        }
        OptionUntil(GameOptionsPhase.Main);
        for (int i = 0; i < 3; i++) OptionPress(SnesButton.Down);
        OptionPress(SnesButton.A);
        OptionUntil(GameOptionsPhase.ControllerSettings);
        OptionPress(SnesButton.R);
        for (int i = 0; i < 7; i++)
        {
            OptionPress(SnesButton.Down);
            OptionUntil(GameOptionsPhase.ControllerSettings);
        }
        OptionPress(SnesButton.A);
        OptionUntil(GameOptionsPhase.Main);
        for (int i = 0; i < 4; i++) OptionPress(SnesButton.Down);
        OptionPress(SnesButton.A);
        OptionUntil(GameOptionsPhase.SpecialSettings);
        OptionPress(SnesButton.A);
        OptionPress(SnesButton.Down);
        OptionPress(SnesButton.Right);
        OptionTick();
        Console.WriteLine($"  File/options snapshots: {comparisons} retail frames match through COPY/CLEAR, controller scrolling, special settings and fades.");

        void Check(Func<Rgba32[]> legacy, Func<LayeredRenderSnapshot> capture, Action step)
        {
            Rgba32[] expected = legacy();
            LayeredRenderSnapshot snapshot = capture();
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(snapshot));
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(capture()));
            step();
            Match(expected, SoftwareLayeredSnapshotRenderer.Render(snapshot));
            comparisons++;
        }

        void Match(Rgba32[] expected, Rgba32[] actual)
        {
            AssertEqual(expected.Length, actual.Length, "menu snapshot geometry");
            for (int i = 0; i < expected.Length; i++)
                if (expected[i] != actual[i])
                    throw new InvalidOperationException($"File/options snapshot sample {comparisons}, pixel ({i % FrontendFrame.Width},{i / FrontendFrame.Width}): expected {expected[i]}, got {actual[i]}.");
        }
    }
}
