using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

internal static class AttractDemoFrontendAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
        byte[] originalSave = bus.SaveRam.ToArray();
        var ports = new byte[4];
        FrontendFrame Step(ushort input = 0)
        {
            FrontendFrame frame = game.Step(input);
            foreach (var command in frame.AudioCommands)
                if (command.Kind == CartridgeAudioCommandKind.WritePort) ports[command.Port] = command.Value;
            game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
            return frame;
        }
        Step();
        Step((ushort)SnesButton.Start);
        int guard = 0;
        while (game.CurrentFrame.Phase != nameof(TitleSequencePhase.TitleScreen) && guard++ < 40) Step();
        if (guard >= 40) throw new InvalidDataException("Title skip failed before attract audit.");
        for (int idle = 1; idle < TitleSequenceRomData.Timing.TitleScreenNtscFrames; idle++)
        {
            Step();
            if (game.CurrentFrame.Phase != nameof(TitleSequencePhase.TitleScreen))
                throw new InvalidDataException($"Title left idle early at frame {idle}.");
        }
        Step();
        if (game.CurrentFrame.Phase != nameof(TitleSequencePhase.TitleScreenFadeOut))
            throw new InvalidDataException("Title did not fade to the demo on frame 900.");

        int completed = 0, activeFrames = 0, holdFrames = 0;
        ushort heldX = 0, heldY = 0;
        guard = 0;
        while (completed < 18 && guard++ < 25000)
        {
            bool wasPlaying = game.GameState == SuperMetroidGameState.PlayingDemo;
            bool wasHolding = game.AttractDemoHoldFramesRemaining != 0;
            int sceneIndex = game.AttractDemoSceneIndex;
            int setIndex = game.AttractDemoSet;
            if (wasHolding)
            {
                heldX = game.RuntimeForVerification!.Samus!.XPosition;
                heldY = game.RuntimeForVerification.Samus.YPosition;
                holdFrames++;
            }
            if (wasPlaying && !wasHolding) activeFrames++;
            Step();
            if (wasHolding && (game.RuntimeForVerification!.Samus!.XPosition != heldX ||
                game.RuntimeForVerification.Samus.YPosition != heldY))
                throw new InvalidDataException("Final demo hold advanced Samus instead of waiting for NMI.");
            if (wasPlaying && game.GameState == SuperMetroidGameState.TransitionFromDemoA)
            {
                AttractDemoScene expected = AttractDemoScene.Read(bus, setIndex, sceneIndex)!;
                if (activeFrames != expected.Duration || holdFrames != AttractDemoRomData.FinalImageHoldFrames)
                    throw new InvalidDataException($"Demo {sceneIndex} timing differs: gameplay={activeFrames}, hold={holdFrames}.");
                completed++;
                activeFrames = holdFrames = 0;
            }
        }
        if (completed != 18) throw new InvalidDataException("All three ordinary demo sets did not complete.");
        guard = 0;
        while (game.GameState != SuperMetroidGameState.OpeningCinematic && guard++ < 5) Step();
        if (game.AttractDemoSet != 0 || game.CurrentFrame.Phase != nameof(TitleSequencePhase.YearText))
            throw new InvalidDataException("Completed sets did not wrap to set zero and restart the title sequence.");

        // Re-enter attract mode and cancel using physical Start. Scripted input must not
        // hide the edge, and holding Start must not auto-select a file on title return.
        Step((ushort)SnesButton.Start);
        guard = 0;
        while (game.GameState != SuperMetroidGameState.PlayingDemo && guard++ < 1100) Step();
        if (game.GameState != SuperMetroidGameState.PlayingDemo)
            throw new InvalidDataException("Second attract entry failed.");
        Directory.CreateDirectory("csharp/test-temp/attract-demo");
        PngWriter.WriteRgba("csharp/test-temp/attract-demo/landing-site-start.png", FrontendFrame.Width,
            FrontendFrame.Height, SuperMetroidRuntimeFrameRenderer.Render(game.RuntimeForVerification!));
        for (int frame = 1; frame <= 360; frame++)
        {
            Step();
            if (frame is 2 or 120 or 360)
                PngWriter.WriteRgba($"csharp/test-temp/attract-demo/landing-site-{frame}.png", FrontendFrame.Width,
                    FrontendFrame.Height, SuperMetroidRuntimeFrameRenderer.Render(game.RuntimeForVerification!));
        }
        Step((ushort)SnesButton.Start);
        if (game.GameState != SuperMetroidGameState.TransitionFromDemoA)
            throw new InvalidDataException("Physical controller input did not cancel the demo.");
        for (int frame = 0; frame < 15; frame++) Step((ushort)SnesButton.Start);
        if (game.GameState != SuperMetroidGameState.OpeningCinematic ||
            game.CurrentFrame.Phase != nameof(TitleSequencePhase.TitleScreen))
            throw new InvalidDataException("Demo cancellation failed to return to the immediate title screen.");
        guard = 0;
        while (game.AttractDemoHoldFramesRemaining == 0 && guard++ < 2500) Step();
        if (game.AttractDemoHoldFramesRemaining == 0)
            throw new InvalidDataException("Did not reach the final-image hold for cancellation testing.");
        Step((ushort)SnesButton.R);
        if (game.GameState != SuperMetroidGameState.TransitionFromDemoA)
            throw new InvalidDataException("Physical input failed to cancel the final-image hold.");
        if (!bus.SaveRam.SequenceEqual(originalSave))
            throw new InvalidDataException("Attract-mode dispatcher altered SRAM.");
        Console.WriteLine("PASS: 900-frame title timeout, 18 scene durations, 90-frame frozen holds, three-set cycling, cancellation, title return, unchanged SRAM.");
        return 0;
    }
}
