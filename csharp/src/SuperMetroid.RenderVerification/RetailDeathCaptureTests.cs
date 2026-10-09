using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Runtime;

/// <summary>Runs focused retail-ROM frame-capture parity checks through Samus's death and game-over sequence.</summary>
internal static class RetailDeathCaptureTests
{
    /// <summary>Compares captured rendering and audio against legacy rendering through an ordinary death.</summary>
    /// <param name="device">Render device used for pixel readback.</param>
    /// <param name="renderer">Frame renderer used to produce captured-frame pixels.</param>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
        => Run(device, renderer, reserveRecovery: false);

    /// <summary>Runs the same captured-frame comparison through automatic reserve recovery and gameplay handoff.</summary>
    /// <param name="device">Render device used for pixel readback.</param>
    /// <param name="renderer">Frame renderer used to produce captured-frame pixels.</param>
    internal static void RunReserveRecovery(D3D11RenderDevice device, D3D11FrameRenderer renderer)
        => Run(device, renderer, reserveRecovery: true);

    /// <summary>Replays the selected death scenario in legacy and captured games, checking frame state, PCM, and pixels.</summary>
    /// <param name="device">Render device used to verify each captured frame.</param>
    /// <param name="renderer">Renderer used to compare captured snapshots with legacy pixels.</param>
    /// <param name="reserveRecovery">Whether zero health starts with automatic reserve energy and must return to gameplay.</param>
    private static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer, bool reserveRecovery)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = RepositoryInstallation.CreateGame(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom), options);
        var captured = RepositoryInstallation.CreateGame(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom), options);
        using var legacyAudio = DesktopAccess.CreateAudioEngine();
        using var capturedAudio = DesktopAccess.CreateAudioEngine();
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
            game.RuntimeForVerification!.RunNmi(0, true);
            game.RuntimeForVerification!.Samus!.Health = 0;
            game.RuntimeForVerification!.Samus!.MaxReserveEnergy = ReserveCaptureFixtureDefinitions.Capacity;
            game.RuntimeForVerification!.Samus!.ReserveEnergy = reserveRecovery ? ReserveCaptureFixtureDefinitions.Capacity : (ushort)0;
            game.RuntimeForVerification!.Samus!.ReserveTankMode = ReserveCaptureFixtureDefinitions.AutomaticMode;
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
                legacy.RuntimeForVerification!.GameplayTimeFrozen != captured.RuntimeForVerification!.GameplayTimeFrozen)
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
                samus.ReserveEnergy != 0 || samus.InputLocked || legacy.RuntimeForVerification!.GameplayTimeFrozen)
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

/// <summary>Shared reserve-tank values used to construct the retail automatic-recovery capture scenario.</summary>
internal static class ReserveCaptureFixtureDefinitions
{
    /// <summary>One acquired reserve tank supplies 100 energy units for the focused recovery fixture.</summary>
    internal const ushort Capacity = 100;
    /// <summary>WRAM reserve mode one selects the native automatic-recovery path.</summary>
    internal const ushort AutomaticMode = 1;
}
