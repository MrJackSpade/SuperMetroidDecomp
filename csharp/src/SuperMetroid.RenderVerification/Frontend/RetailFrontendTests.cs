using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.RenderVerification.Harness;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Frontend;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares captured title and frontend frames with the WARP reference device and checks their state transitions.</summary>
internal static class RetailFrontendTests
{
    /// <summary>Drives the title into file select, then compares frontend capture through the intro handoff.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // Required fixture, not an optional skip. SRAM is private to these address
        // spaces; no player save files are loaded or modified.
        StartupReferenceTests.Run(device, renderer);
        var title = RenderRetailFixture.CreateTitle();
        var phases = new HashSet<TitleSequencePhase>();
        int titleCount = 0;
        for (int tick = 0; tick < 2500 && !title.FileSelectRequested; tick++)
        {
            bool newPhase = phases.Add(title.Phase);
            if (newPhase || tick % 17 == 0 || title.Phase == TitleSequencePhase.TitleScreenFadeOut)
            {
                var expected = title.Render();
                var packet = new DisplayPacket(new(tick + 1, 1, (ushort)tick), title.CaptureRenderSnapshot());
                if (title.Phase == TitleSequencePhase.SceneThreeZoom && (packet.Floor ?? throw new InvalidOperationException("Verification requires packet.Floor.")).Gradient.IsEmpty)
                    throw new InvalidOperationException("Native scene-three gradient was not spawned.");
                if (title.Phase == TitleSequencePhase.YearText && !(packet.Floor ?? throw new InvalidOperationException("Verification requires packet.Floor.")).Gradient.IsEmpty)
                    throw new InvalidOperationException("Title gradient spawned before its native scene command.");
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: retail title {title.Phase}, tick {tick}");
                titleCount++;
                title.Step(title.Phase == TitleSequencePhase.TitleScreen ? ControllerButton.Start : 0);
            }
            else title.Step(0);
        }
        if (!title.FileSelectRequested || !phases.Contains(TitleSequencePhase.SceneThreeZoom))
            throw new InvalidOperationException("Retail title did not cover natural zoom and reach file select.");

        var legacy = RenderRetailFixture.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        var captured = RenderRetailFixture.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        var states = new HashSet<SuperMetroidGameState>();
        for (int tick = 0; tick < 500; tick++)
        {
            ControllerButton input = tick % 47 == 0 ? ControllerButton.Start : ControllerButton.None;
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, tick + 1, 2);
            if (expected.GameState != actual.Frame.GameState || expected.Phase != actual.Frame.Phase ||
                expected.FrameNumber != actual.Frame.FrameNumber ||
                !legacy.AudioEventsForVerification.SequenceEqual(captured.AudioEventsForVerification))
                throw new InvalidOperationException($"Frontend state/audio diverged at tick {tick}.");
            states.Add(expected.GameState);
            var packet = actual.Snapshot;
            PixelComparison.Verify(packet, legacy.PublishedPixels(), renderer.RenderForReadback(packet),
                $"{device.Kind}: retail frontend {expected.Phase}, tick {tick}");
            if (expected.GameState == SuperMetroidGameState.IntroCinematic)
            {
                if (!states.Contains(SuperMetroidGameState.FileSelectMenus) || !states.Contains(SuperMetroidGameState.GameOptionsMenu))
                    throw new InvalidOperationException("Retail frontend did not visit file/options menus.");
                Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {titleCount} natural title and {tick + 1} frontend frames match the WARP reference device; frontend state/audio commands agree.");
                return;
            }
        }
        throw new InvalidOperationException("Retail frontend did not reach intro handoff.");
    }
}
