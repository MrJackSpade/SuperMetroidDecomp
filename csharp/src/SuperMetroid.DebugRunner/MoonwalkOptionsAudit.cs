using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Disposable SRAM and real menu inputs exercise the options-to-gameplay handoff.</summary>
internal static class MoonwalkOptionsAudit
{
    public static int Run(string rom)
    {
        foreach (bool initial in new[] { false, true })
        foreach (bool abandon in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var saves = new SuperMetroidSaveRam(bus);
            saves.SaveSlot(0, new SuperMetroidSaveSnapshot
            {
                Area = 0, SaveStation = 1, Health = 99, MaxHealth = 99,
                MoonwalkEnabled = initial, IconCancelEnabled = initial,
            });
            saves.SaveSlot(1, new SuperMetroidSaveSnapshot
            {
                Area = 0, SaveStation = 1, Health = 75, MaxHealth = 99,
                MoonwalkEnabled = !initial,
            });
            saves.SelectSlot(0);
            var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
            FrontendFrame frame = default;
            void Step(ushort input = 0) => frame = game.Step(input);
            void Press(SnesButton button) { Step(); Step((ushort)button); Step(); }
            void Until(Func<bool> condition, int limit = 240)
            {
                for (int tick = 0; tick < limit && !condition(); tick++) Step();
                if (!condition()) throw new InvalidDataException($"Options audit stalled at {frame.GameState}/{frame.Phase}.");
            }
            void MainOptions() => Until(() => frame.GameState == SuperMetroidGameState.GameOptionsMenu &&
                frame.Phase == nameof(GameOptionsPhase.Main));
            Step(); Press(SnesButton.Start);
            Until(() => frame.Phase == nameof(TitleSequencePhase.TitleScreen));
            Press(SnesButton.Start);
            Until(() => frame.GameState == SuperMetroidGameState.FileSelectMenus);
            for (int tick = 0; tick < 16; tick++) Step();
            Press(SnesButton.A); MainOptions();

            // Change a controller binding as well: all three settings share the same overwrite.
            for (int row = 0; row < 3; row++) Press(SnesButton.Down);
            Press(SnesButton.A);
            Until(() => frame.Phase == nameof(GameOptionsPhase.ControllerSettings));
            Press(SnesButton.R);
            for (int row = 0; row < 7; row++)
            {
                Press(SnesButton.Down);
                Until(() => frame.Phase == nameof(GameOptionsPhase.ControllerSettings));
            }
            Press(SnesButton.A); MainOptions();
            for (int row = 0; row < 4; row++) Press(SnesButton.Down);
            Press(SnesButton.A);
            Until(() => frame.Phase == nameof(GameOptionsPhase.SpecialSettings));
            Press(SnesButton.A); // Icon Cancel
            Press(SnesButton.Down); Press(SnesButton.Right); // Moonwalk
            Press(SnesButton.B); MainOptions();

            if (abandon)
            {
                Press(SnesButton.B);
                Until(() => frame.GameState == SuperMetroidGameState.FileSelectMenus);
                for (int tick = 0; tick < 16; tick++) Step();
                Press(SnesButton.A); MainOptions();
            }
            Press(SnesButton.A);
            Until(() => frame.GameState == SuperMetroidGameState.FileSelectMap && frame.Phase == "Area");
            Press(SnesButton.Start); Until(() => frame.Phase == "Room");
            Press(SnesButton.Start);
            Until(() => game.RuntimeForVerification is not null);
            var runtime = game.RuntimeForVerification!;
            bool expected = abandon ? initial : !initial;
            if (runtime.MoonwalkEnabled != expected || runtime.IconCancelEnabled != expected ||
                runtime.ControllerBindings.Shoot != (ushort)(abandon ? SnesButton.X : SnesButton.R))
                throw new InvalidDataException($"Options handoff initial={initial} abandon={abandon}: " +
                    $"moonwalk={runtime.MoonwalkEnabled}, icon={runtime.IconCancelEnabled}, shoot={runtime.ControllerBindings.Shoot:X4}.");
            if (saves.ReadSlot(0)!.MoonwalkEnabled != initial || saves.ReadSlot(1)!.MoonwalkEnabled != !initial)
                throw new InvalidDataException("Menu edits changed stored settings before a gameplay save.");
            saves.SaveSlot(0, SuperMetroidSaveSnapshot.Capture(runtime.Samus!, runtime.System,
                area: 0, saveStation: 1, controllerBindings: runtime.ControllerBindings,
                moonwalkEnabled: runtime.MoonwalkEnabled, iconCancelEnabled: runtime.IconCancelEnabled));
            var reloaded = saves.ReadSlot(0)!;
            if (reloaded.MoonwalkEnabled != expected || reloaded.IconCancelEnabled != expected ||
                reloaded.ControllerBindings != runtime.ControllerBindings || saves.ReadSlot(1)!.MoonwalkEnabled != !initial)
                throw new InvalidDataException("Live options did not survive save encoding or leaked into another slot.");
        }
        Console.WriteLine("Moonwalk options: enable/disable, controller and Icon Cancel handoff, abandoned edits, and slot isolation pass.");
        return 0;
    }
}
