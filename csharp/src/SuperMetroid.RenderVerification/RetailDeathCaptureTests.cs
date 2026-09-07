using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailDeathCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
        => Run(device, renderer, reserveRecovery: false);

    internal static void RunReserveRecovery(D3D11RenderDevice device, D3D11FrameRenderer renderer)
        => Run(device, renderer, reserveRecovery: true);

    private static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer, bool reserveRecovery)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        var captured = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        using var legacyAudio = new SpcAudioEngine();
        using var capturedAudio = new SpcAudioEngine();
        long sequence = 0;
        void Step(ushort input, bool comparePixels)
        {
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, ++sequence, 1);
            if (actual.UsedLegacyRaster || expected.GameState != actual.Frame.GameState ||
                expected.Phase != actual.Frame.Phase || expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.AudioCommands.SequenceEqual(actual.Frame.AudioCommands))
                throw new InvalidOperationException($"Death capture cadence/commands diverged at {sequence}.");
            if (!legacyAudio.RenderFrame(expected.AudioCommands).SequenceEqual(capturedAudio.RenderFrame(actual.Frame.AudioCommands)))
                throw new InvalidOperationException($"Death PCM diverged at {sequence}.");
            legacy.SetAudioAcknowledgements(legacyAudio.ReadAcknowledgements());
            captured.SetAudioAcknowledgements(capturedAudio.ReadAcknowledgements());
            if (comparePixels) PixelComparison.Verify(actual.Snapshot!, expected.Pixels,
                renderer.RenderForReadback(actual.Snapshot!), $"{device.Kind}: death frame {sequence}, {expected.GameState}");
        }
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0, false);
        if (legacy.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Death fixture startup failed.");
        foreach (var game in new[] { legacy, captured })
        {
            game.RuntimeForVerification!.LoadCartridgeRoomForDebug(SlowConsumerFixture.AlphaPowerBombRoom, 0, 0);
            game.RuntimeForVerification.RunNmi(0, true);
            game.RuntimeForVerification.Samus!.Health = 0;
            game.RuntimeForVerification.Samus.MaxReserveEnergy = ReserveCaptureFixtureDefinitions.Capacity;
            game.RuntimeForVerification.Samus.ReserveEnergy = reserveRecovery ? ReserveCaptureFixtureDefinitions.Capacity : (ushort)0;
            game.RuntimeForVerification.Samus.ReserveTankMode = ReserveCaptureFixtureDefinitions.AutomaticMode;
        }
        var states = new HashSet<SuperMetroidGameState>();
        int frames = 0;
        for (; frames < 2000; frames++)
        {
            Step(0, true);
            states.Add(legacy.GameState);
            var expectedSamus = legacy.RuntimeForVerification!.Samus!;
            var actualSamus = captured.RuntimeForVerification!.Samus!;
            if (expectedSamus.Health != actualSamus.Health || expectedSamus.ReserveEnergy != actualSamus.ReserveEnergy ||
                expectedSamus.InputLocked != actualSamus.InputLocked ||
                legacy.RuntimeForVerification.GameplayTimeFrozen != captured.RuntimeForVerification.GameplayTimeFrozen)
                throw new InvalidOperationException("Death/recovery state diverged between captured and legacy owners.");
            if (reserveRecovery && states.Contains(SuperMetroidGameState.ReserveTanksAuto) &&
                legacy.GameState == SuperMetroidGameState.MainGameplay) break;
            if (legacy.GameState == SuperMetroidGameState.GameOverMenu) break;
        }
        if (reserveRecovery)
        {
            var samus = legacy.RuntimeForVerification!.Samus!;
            if (frames == 2000 || !states.Contains(SuperMetroidGameState.ReserveTanksAuto) ||
                legacy.GameState != SuperMetroidGameState.MainGameplay || samus.Health == 0 ||
                samus.ReserveEnergy != 0 || samus.InputLocked || legacy.RuntimeForVerification.GameplayTimeFrozen)
                throw new InvalidOperationException("Reserve fixture missed recovery, depletion or release.");
            Console.WriteLine($"{device.Kind}: {frames + 1} exact automatic reserve recovery frames, matching PCM, energy/lock/freeze state and gameplay handoff.");
            return;
        }
        if (frames == 2000 || !states.Contains(SuperMetroidGameState.DeathBlackOutSurroundings) ||
            !states.Contains(SuperMetroidGameState.DeathExplosionWhiteOut) || !states.Contains(SuperMetroidGameState.DeathFinalBlackOut))
            throw new InvalidOperationException("Death fixture missed blackout, explosion or game-over handoff.");
        Console.WriteLine($"{device.Kind}: {frames + 1} exact death frames with matching PCM, blackout/explosion/fade and game-over handoff.");
    }
}

internal static class ReserveCaptureFixtureDefinitions
{
    /// <summary>One acquired reserve tank supplies 100 energy units for the focused recovery fixture.</summary>
    internal const ushort Capacity = 100;
    /// <summary>WRAM reserve mode one selects the native automatic-recovery path.</summary>
    internal const ushort AutomaticMode = 1;
}
