using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.RenderVerification.Frontend;

/// <summary>Compares the pause screen's frames on this device with the WARP reference device's (#1392).</summary>
internal static class RetailPauseTests
{
    /// <summary>The pause map and equipment pages, drawn from their screen tiles, match the WARP reference device exactly.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var system = new SharedGameState();
        system.Progress.ExploreEveryCell();
        var pause = new PauseMenuState(new PauseMenuEntry
        {
            Samus = new SamusState(InstalledFixtures.AnimationContinuation), System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0),
            MapPresentation = InstalledFixtures.Maps, SpriteSheets = InstalledFixtures.SpriteSheets,
        });
        int samples = 0;
        void Compare(string page)
        {
            var packet = new DisplayPacket(new(++samples, 1, 0), pause.CaptureRenderSnapshot());
            PixelComparison.Verify(packet, DisplayRendering.Render(packet), renderer.RenderForReadback(packet), $"{device.Kind}: pause {page}");
        }
        Compare("map");
        pause.Step(ControllerButton.R, ControllerButton.R);
        do
            pause.Step(0, 0);
        while (pause.IsPageTransitionActive);
        if (pause.ScreenMode != PauseScreenMode.Equipment)
            throw new InvalidOperationException($"Pressing R left pause on its {pause.ScreenMode} page, not the equipment page.");
        Compare("equipment");
        Console.WriteLine($"{device.Kind}: the pause map and equipment pages match the WARP reference device.");
    }
}
