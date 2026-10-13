using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Runtime;

/// <summary>Compares the gunship escape event, fade, and ending handoff through captured rendering and audio.</summary>
internal static class RetailEndingHandoffTests
{
    /// <summary>Runs paired retail games through the native takeoff boundary and sixty ending frames.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = InstalledFixtures.CreateGame(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom), options);
        var captured = InstalledFixtures.CreateGame(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom), options);
        var audioAssets = DesktopAccess.LoadAudioAssets();
        legacy.BindAudio(audioAssets);
        captured.BindAudio(audioAssets);
        var leftAudio = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        var rightAudio = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        long sequence = 0;
        void Step(ushort input, bool pixels)
        {
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, ++sequence, 1);
            legacy.CopyAudioFrame(leftAudio);
            captured.CopyAudioFrame(rightAudio);
            if (actual.UsedLegacyRaster || expected.GameState != actual.Frame.GameState ||
                expected.Phase != actual.Frame.Phase || expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.AudioCommands.SequenceEqual(actual.Frame.AudioCommands) ||
                !leftAudio.AsSpan().SequenceEqual(rightAudio))
                throw new InvalidOperationException("Ending handoff changed frontend cadence/audio.");
            if (pixels) PixelComparison.Verify(actual.Snapshot!, expected.Pixels,
                renderer.RenderForReadback(actual.Snapshot!), $"{device.Kind}: ending handoff {sequence}, {expected.Phase}");
        }
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0, false);
        if (legacy.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Ending fixture startup failed.");
        foreach (var game in new[] { legacy, captured })
        {
            var runtime = game.RuntimeForVerification!;
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
            var top = runtime.Enemies.SlotTable.Slots.First(slot =>
                (((int)new SnesAddress(slot.Definition.Bank, slot.Definition.InitializationAiPointer))) == (int)EnemyAiRoutine.InitAI_ShipTop);
            // Stage the existing accelerating integrator near its native completion
            // threshold. It—not the fixture—publishes the frontend escape event.
            top.VariableF = (ushort)GunshipFunction.AcceleratingLiftoff;
            // The staged hull starts outside this fixture's camera. Explicitly keep
            // its AI active; this is handoff coverage, not full departure validation.
            top.Properties = top.Properties.With(EnemyProperties.ProcessOffScreen);
            runtime.Player.Samus!.InputLocked = true;
            runtime.Player.Samus.Position = runtime.Player.Samus.Position.WithWholeY(EndingHandoffFixtureDefinitions.TakeoffBoundaryY);
            runtime.NmiHandler.RunNmi(0, true);
        }
        bool sawFade = false;
        int endingFrames = 0, frames = 0;
        for (; frames < 256; frames++)
        {
            Step(0, true);
            sawFade |= legacy.GameState == SuperMetroidGameState.SamusEscapesFromZebes;
            if (legacy.GameState == SuperMetroidGameState.EndingAndCredits && ++endingFrames == 60) break;
        }
        if (!sawFade || endingFrames != 60) throw new InvalidOperationException($"Ending fixture missed native escape fade/credits handoff: state={legacy.GameState}, fade={sawFade}, ending={endingFrames}, health={legacy.RuntimeForVerification!.Player.Samus!.Health}, event={legacy.RuntimeForVerification!.Enemies.GunshipBehavior.LastGunshipEvent}.");
        Console.WriteLine($"{device.Kind}: {frames + 1} exact gunship-event/fade/ending frames with matching PCM and frontend state.");
    }
}

/// <summary>Fixture position that places the gunship top hull near its escape completion threshold.</summary>
internal static class EndingHandoffFixtureDefinitions
{
    /// <summary>Stage Samus near the gunship top-hull Y=256 escape threshold; the native attachment places the hull above her.</summary>
    internal const ushort TakeoffBoundaryY = 256;
}
