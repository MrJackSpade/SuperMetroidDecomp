using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailLoadAppearanceTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var left = new SuperMetroidAddressSpace(rom);
        var right = new SuperMetroidAddressSpace(rom);
        foreach (var bus in new[] { left, right })
        {
            // In-memory SRAM only, matching the portable saved-file fixture.
            var save = new SuperMetroidSaveSnapshot { Area = 4, SaveStation = 0, Health = 99, MaxHealth = 99 };
            save.MapStationBytes[4] = 1; save.UsedSaveStationBytes[8] = 1;
            new SuperMetroidSaveRam(bus).SaveSlot(0, save);
        }
        var legacy = new SuperMetroidGame(left);
        var captured = new SuperMetroidGame(right);
        bool sawMap = false, sawAppearance = false, completed = false;
        int samples = 0;
        for (int tick = 0; tick < 2400; tick++)
        {
            ushort input = legacy.GameState != SuperMetroidGameState.MainGameplay && tick % 47 == 0
                ? (ushort)SnesButton.Start : (ushort)0;
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, tick + 1, 1);
            if (actual.UsedLegacyRaster || expected.GameState != actual.Frame.GameState ||
                expected.FrameNumber != actual.Frame.FrameNumber || expected.Phase != actual.Frame.Phase ||
                !expected.AudioCommands.SequenceEqual(actual.Frame.AudioCommands))
                throw new InvalidOperationException($"Saved-file load capture diverged at {tick}.");
            sawMap |= legacy.GameState == SuperMetroidGameState.FileSelectMap;
            sawAppearance |= legacy.RuntimeForVerification?.SamusLoadAppearanceActive == true;
            if (legacy.RuntimeForVerification?.SamusLoadAppearanceFramesRemaining !=
                captured.RuntimeForVerification?.SamusLoadAppearanceFramesRemaining)
                throw new InvalidOperationException("Capture changed load appearance timing.");
            if (sawMap)
            {
                PixelComparison.Verify(actual.Snapshot!, expected.Pixels, renderer.RenderForReadback(actual.Snapshot!),
                    $"{device.Kind}: saved-file load frame {tick}, {expected.GameState}");
                samples++;
            }
            if (sawAppearance && legacy.RuntimeForVerification is { SamusLoadAppearanceActive: false } runtime &&
                legacy.GameState == SuperMetroidGameState.MainGameplay)
            {
                if (runtime.Samus!.InputLocked || captured.RuntimeForVerification!.SamusLoadAppearanceActive ||
                    captured.RuntimeForVerification.Samus!.InputLocked)
                    throw new InvalidOperationException("Load appearance did not release both owners.");
                completed = true; break;
            }
        }
        if (!sawMap || !sawAppearance || !completed) throw new InvalidOperationException("Saved-file load missed map, appearance or control handoff.");
        Console.WriteLine($"{device.Kind}: {samples} exact saved-file map/load/appearance frames through control release.");
    }
}
