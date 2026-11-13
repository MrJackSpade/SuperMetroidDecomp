using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Saves;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Rooms;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.SaveMapFixtures;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares legacy and captured rendering through saved-file map, Samus appearance, and control release.</summary>
internal static class RetailLoadAppearanceTests
{
    /// <summary>Runs two independent in-memory save sessions and compares game state, audio, timing, and pixels through load.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // In-memory saves only, matching the portable saved-file fixture.
        SuperMetroid.Core.Saves.SaveSlotStore Saves()
        {
            var save = new SuperMetroidSaveSnapshot { Checkpoint = SuperMetroid.Core.Progression.LoadStationDefinitions.GlassTunnelSave, Health = 99, MaxHealth = 99 };
            save.Progress.SetAreaMap(SuperMetroid.Core.Progression.AreaId.Maridia, acquired: true); save.Progress.UsedStations.Add(SuperMetroid.Core.Progression.LoadStationDefinitions.GlassTunnelSave);
            var saves = new SuperMetroid.Core.Saves.SaveSlotStore(SaveMapPresentationFixture.Create());
            saves.SaveSlot(0, save);
            return saves;
        }
        var legacy = InstalledFixtures.CreateGame(Saves());
        var captured = InstalledFixtures.CreateGame(Saves());
        bool sawMap = false, sawAppearance = false, completed = false;
        int samples = 0;
        for (int tick = 0; tick < 2400; tick++)
        {
            ControllerButton input = legacy.GameState != SuperMetroidGameState.MainGameplay && tick % 47 == 0
                ? ControllerButton.Start : 0;
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input);
            if (expected.GameState != actual.Frame.GameState ||
                expected.FrameNumber != actual.Frame.FrameNumber || expected.Phase != actual.Frame.Phase ||
                !expected.Audio.Events.SequenceEqual(actual.Frame.Audio.Events))
                throw new InvalidOperationException($"Saved-file load capture diverged at {tick}.");
            sawMap |= legacy.GameState == SuperMetroidGameState.FileSelectMap;
            sawAppearance |= legacy.RuntimeForVerification?.SamusLoadAppearanceActive == true;
            if (legacy.RuntimeForVerification?.SamusLoadAppearanceFramesRemaining !=
                captured.RuntimeForVerification?.SamusLoadAppearanceFramesRemaining)
                throw new InvalidOperationException("Capture changed load appearance timing.");
            if (sawMap)
            {
                PixelComparison.Verify(actual.Snapshot, legacy.PublishedPixels(), DisplayRendering.Render(actual.Snapshot),
                    $"saved-file load WARP capture frame {tick}");
                PixelComparison.Verify(actual.Snapshot, legacy.PublishedPixels(), renderer.RenderForReadback(actual.Snapshot),
                    $"{device.Kind}: saved-file load frame {tick}, {expected.GameState}");
                samples++;
            }
            if (sawAppearance && legacy.RuntimeForVerification is { } runtime && !runtime.SamusLoadAppearanceActive &&
                legacy.GameState == SuperMetroidGameState.MainGameplay)
            {
                if (runtime.Player.IntroducedSamus.Control.AlphaLocked || captured.GameplayRuntimeForVerification.SamusLoadAppearanceActive ||
                    captured.GameplayRuntimeForVerification.Player.IntroducedSamus.Control.AlphaLocked)
                    throw new InvalidOperationException("Load appearance did not release both owners.");
                completed = true; break;
            }
        }
        if (!sawMap || !sawAppearance || !completed) throw new InvalidOperationException("Saved-file load missed map, appearance or control handoff.");
        Console.WriteLine($"{device.Kind}: {samples} exact saved-file map/load/appearance frames through control release.");
    }
}
