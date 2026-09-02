using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Reproduces Start being pressed throughout a cartridge-authored permanent-item message.
/// The audit uses the outer frontend because the defect was in state-$08 pause admission,
/// not in the isolated bank-$85 message coroutine.
/// </summary>
internal static class PauseMessageGateAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        SeedCrateriaSave(bus);
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        FrontendFrame frame = FrontendAuditDriver.EnterSelectedSlot(game);
        frame = FrontendAuditDriver.StepUntil(
            game,
            frame,
            candidate => game.GameplayMovementEnabled,
            maximumFrames: 420,
            "saved-game appearance did not unlock movement");
        SuperMetroid.Core.Runtime.SuperMetroidRuntime runtime = game.RuntimeForVerification
            ?? throw new InvalidOperationException("Pause/message audit lost its gameplay runtime.");

        runtime.MessageBox.Begin(bus, GameplayMessageIds.MorphBall);
        int messageOwnedFrames = 0;
        bool sawMinimumDisplay = false;
        bool sawClosing = false;
        while (runtime.MessageBox.IsActive && messageOwnedFrames < 440)
        {
            GameplayMessageBoxPhase phaseAtEntry = runtime.MessageBox.Phase;
            frame = game.Step((ushort)SnesButton.Start);
            messageOwnedFrames++;
            sawMinimumDisplay |= phaseAtEntry == GameplayMessageBoxPhase.MinimumDisplay;
            sawClosing |= phaseAtEntry == GameplayMessageBoxPhase.Closing;
            if (game.GameState != SuperMetroidGameState.MainGameplay)
            {
                throw new InvalidDataException(
                    $"Start entered frontend state {game.GameState} while bank-$85 owned " +
                    $"message phase {phaseAtEntry} on message frame {messageOwnedFrames}.");
            }
        }
        if (runtime.MessageBox.IsActive || !sawMinimumDisplay || !sawClosing)
        {
            throw new InvalidDataException(
                $"Morph Ball message did not traverse the complete opening/display/closing " +
                $"route in {messageOwnedFrames} frames.");
        }

        // The held Start that dismissed the message must not leak through its final closing
        // frame. Once released, a fresh rising edge on the next ordinary gameplay frame must
        // still retain its normal pause meaning.
        frame = game.Step(0);
        if (game.GameState != SuperMetroidGameState.MainGameplay)
            throw new InvalidDataException("Releasing Start after the message unexpectedly changed state.");
        frame = game.Step((ushort)SnesButton.Start);
        if (game.GameState != SuperMetroidGameState.PausingDarkening)
        {
            throw new InvalidDataException(
                $"The first clean Start edge after the message selected {game.GameState}, " +
                "not pause darkening.");
        }

        Console.WriteLine(
            $"Pause/message gate passed {messageOwnedFrames} bank-$85-owned frames, " +
            "blocked Start through the final close frame, and admitted the next clean edge.");
        return 0;
    }

    private static void SeedCrateriaSave(SuperMetroidAddressSpace bus)
    {
        var system = new Bank80SystemState();
        system.SetBossBits(0, BossBits.AreaTorizo);
        system.MarkSaveStationUsed(areaIndex: 0, stationBitIndex: 0);
        var samus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
            EquippedItems = (ushort)SamusEquipmentFlags.MorphBall,
            CollectedItems = (ushort)SamusEquipmentFlags.MorphBall,
        };
        new SuperMetroidSaveRam(bus).SaveSlot(
            0,
            SuperMetroidSaveSnapshot.Capture(samus, system, area: 0, saveStation: 0));
    }
}
