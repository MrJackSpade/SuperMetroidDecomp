using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

/// <summary>
/// ROM-backed proof that the host option bypasses only the story cinematic and rejoins the
/// ordinary Ceres new-game path. Keeping this out of Program.cs avoids expanding its already
/// large front-end capture dispatcher for a configuration-boundary regression.
/// </summary>
internal static class FrontendSkipIntroAudit
{
    public static int Run(string romPath, string outputPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        int saveRamChangeCount = 0;
        game.SaveRamChanged += () => saveRamChangeCount++;

        FrontendFrame frame = game.Step(0);
        frame = game.Step((ushort)SnesButton.Start);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.Phase == nameof(TitleSequencePhase.TitleScreen),
            maximumFrames: 120,
            "title montage skip did not reach the title screen");

        frame = game.Step((ushort)SnesButton.Start);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.FileSelectMenus,
            maximumFrames: 120,
            "title screen did not reach file select");
        for (int frameIndex = 0; frameIndex < 16; frameIndex++)
            frame = game.Step(0);

        frame = game.Step((ushort)SnesButton.A);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.GameOptionsMenu,
            maximumFrames: 180,
            "fresh file did not reach the game-options screen");
        for (int frameIndex = 0; frameIndex < 16; frameIndex++)
            frame = game.Step(0);

        // Accept the options menu exactly as the playable keyboard host does. Every frame
        // is inspected so an accidental one-frame construction of IntroCinematic cannot be
        // hidden by the eventual arrival at Ceres.
        frame = game.Step((ushort)SnesButton.A);
        for (int frameIndex = 0;
            frameIndex < 180 && frame.GameState == SuperMetroidGameState.GameOptionsMenu;
            frameIndex++)
        {
            frame = game.Step(0);
            if (frame.GameState == SuperMetroidGameState.IntroCinematic)
                throw new InvalidOperationException("Skip option entered IntroCinematic.");
        }
        if (frame.GameState != SuperMetroidGameState.SetUpNewGame)
        {
            throw new InvalidOperationException(
                $"Skip option reached {frame.GameState}, not SetUpNewGame.");
        }

        // State $1F performs the authentic room/load-station initialization on the next
        // dispatcher call. The existing bank-$86 elevator then owns the full wait, descent,
        // graphical concealer, landing, and input-unlock sequence.
        frame = game.Step(0);
        if (frame.GameState != SuperMetroidGameState.MadeItToCeresElevator)
            throw new InvalidOperationException("New-game setup did not enter the Ceres elevator.");
        SuperMetroidSaveSlot saved = new SuperMetroidSaveRam(bus).ReadSlot(0)
            ?? throw new InvalidDataException("Ceres setup did not produce a valid slot-A SRAM checkpoint.");
        if (saved.Area != 6 || saved.SaveStation != 0 || saved.Health != 99)
        {
            throw new InvalidDataException(
                $"Ceres checkpoint decoded as area {saved.Area}, station {saved.SaveStation}, " +
                $"energy {saved.Health}, not 6:0/99.");
        }
        if (saveRamChangeCount != 2)
        {
            throw new InvalidOperationException(
                $"Expected selected-slot and checkpoint SRAM publications, observed {saveRamChangeCount}.");
        }
        var reloadedMenu = new FileSelectMenuState(bus);
        if (!reloadedMenu.SelectedSlotContainsSave)
            throw new InvalidDataException("A restarted file-select menu did not recognize the Ceres checkpoint.");
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.MainGameplay,
            maximumFrames: 180,
            "Ceres elevator did not unlock normal gameplay");
        if (!game.GameplayMovementEnabled)
            throw new InvalidOperationException("Ceres gameplay was reached with movement disabled.");

        // A reconstructed menu reading ENERGY/TIME is not a save-load test. Build a wholly
        // new outer dispatcher over the same cartridge/SRAM image, enter slot A through the
        // same title, file-select, and options calls as the desktop host, and require the
        // existing-save branch to reconstruct the cartridge-authored Ceres station. This
        // deliberately does not call SaveSlot or a runtime initializer from the audit.
        var restartedGame = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        int restartedSaveRamChangeCount = 0;
        restartedGame.SaveRamChanged += () => restartedSaveRamChangeCount++;
        FrontendFrame restartedFrame = EnterSelectedSlot(restartedGame);
        if (restartedFrame.GameState != SuperMetroidGameState.MadeItToCeresElevator ||
            restartedGame.GameplayActiveRoomPointer != game.GameplayActiveRoomPointer)
        {
            throw new InvalidDataException(
                $"Fresh frontend loaded state {restartedFrame.GameState}/room " +
                $"${restartedGame.GameplayActiveRoomPointer.GetValueOrDefault():X4}, not " +
                $"the saved Ceres room ${game.GameplayActiveRoomPointer.GetValueOrDefault():X4}.");
        }
        if (restartedSaveRamChangeCount != 1)
        {
            throw new InvalidOperationException(
                $"Existing-save startup published {restartedSaveRamChangeCount} SRAM changes; " +
                "only the selected-slot word should be rewritten.");
        }
        restartedFrame = StepUntil(
            restartedGame,
            restartedFrame,
            candidate => candidate.GameState == SuperMetroidGameState.MainGameplay,
            maximumFrames: 180,
            "fresh frontend did not unlock the loaded Ceres checkpoint");
        if (!restartedGame.GameplayMovementEnabled ||
            restartedGame.GameplayCollectedItems != saved.CollectedItems ||
            restartedGame.GameplayEquippedItems != saved.EquippedItems)
        {
            throw new InvalidDataException(
                "Fresh frontend did not restore Ceres control and saved inventory words.");
        }

        for (int pixelIndex = 0; pixelIndex < frame.Pixels.Length; pixelIndex++)
        {
            if (frame.Pixels[pixelIndex].A != byte.MaxValue)
            {
                throw new InvalidDataException(
                    $"Skip-intro gameplay frame contains alpha at pixel {pixelIndex}.");
            }
        }

        PngWriter.WriteRgba(outputPath, FrontendFrame.Width, FrontendFrame.Height, frame.Pixels);
        Console.WriteLine(
            $"Skip-opening-cinematic audit reached {frame.GameState} on dispatcher frame " +
            $"{frame.FrameNumber}, Samus=(${game.GameplaySamusX:X4},${game.GameplaySamusY:X4}), " +
            "slot A saved as Ceres 6:0 and loaded through a fresh frontend instance.");
        Console.WriteLine($"Captured skip-intro Ceres frame to {Path.GetFullPath(outputPath)}.");
        return 0;
    }

    /// <summary>
    /// Drives a fresh outer dispatcher into its currently selected, existing save slot.
    /// Keeping every transition controller-authored catches stale menu flags and button-edge
    /// mistakes that a direct call to the runtime's saved-game initializer would conceal.
    /// </summary>
    private static FrontendFrame EnterSelectedSlot(SuperMetroidGame game)
    {
        FrontendFrame frame = game.Step(0);
        frame = game.Step((ushort)SnesButton.Start);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.Phase == nameof(TitleSequencePhase.TitleScreen),
            maximumFrames: 120,
            "restarted title montage skip did not reach the title screen");

        frame = game.Step((ushort)SnesButton.Start);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.FileSelectMenus,
            maximumFrames: 120,
            "restarted title screen did not reach file select");
        for (int frameIndex = 0; frameIndex < 16; frameIndex++)
            frame = game.Step(0);

        frame = game.Step((ushort)SnesButton.A);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.GameOptionsMenu,
            maximumFrames: 180,
            "restarted existing slot did not reach game options");
        for (int frameIndex = 0; frameIndex < 16; frameIndex++)
            frame = game.Step(0);

        frame = game.Step((ushort)SnesButton.A);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.SetUpNewGame,
            maximumFrames: 180,
            "restarted options did not request saved-game setup");
        return game.Step(0);
    }

    private static FrontendFrame StepUntil(
        SuperMetroidGame game,
        FrontendFrame initialFrame,
        Func<FrontendFrame, bool> finished,
        int maximumFrames,
        string failure)
    {
        FrontendFrame frame = initialFrame;
        for (int frameIndex = 0; frameIndex < maximumFrames && !finished(frame); frameIndex++)
            frame = game.Step(0);
        if (!finished(frame))
            throw new InvalidOperationException($"{failure} within {maximumFrames} frames.");
        return frame;
    }
}
