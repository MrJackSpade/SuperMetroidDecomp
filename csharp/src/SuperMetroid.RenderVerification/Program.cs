using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.RenderVerification.Bosses;
using SuperMetroid.RenderVerification.Device;
using SuperMetroid.RenderVerification.Enemies;
using SuperMetroid.RenderVerification.Frontend;
using SuperMetroid.RenderVerification.Harness;
using SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.RenderVerification.Rooms;
using SuperMetroid.RenderVerification.Samus;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RenderFixtures;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.WindowsConsole;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// Direct3D 11 renderer verification against the WARP reference device.
/// </summary>
internal static class Program
{
    /// <summary>Dispatches focused D3D11 graphics-device/WARP parity checks and reports failures through the console process boundary.</summary>
    private static void Main(string[] args)
    {
        try
        {
            NativeConsoleErrors.DisableDialogs();
            if (args is ["--packet-input"])
            {
                DisplayPacketInputTests.Run();
                return;
            }
            if (args is ["--ending-shooting-stars"])
            {
                foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
                {
                    using var device = new D3D11RenderDevice(kind);
                    using var renderer = new D3D11FrameRenderer(device);
                    EndingShootingStarsTests.Run(device, renderer);
                }
                return;
            }
            if (args is ["--ending-wavy-samus"])
            {
                foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
                {
                    using var device = new D3D11RenderDevice(kind);
                    using var renderer = new D3D11FrameRenderer(device);
                    EndingWavySamusTests.Run(device, renderer);
                }
                return;
            }
            if (args is ["--mother-brain-ascent-mask"])
            {
                foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
                {
                    using var device = new D3D11RenderDevice(kind);
                    using var renderer = new D3D11FrameRenderer(device);
                    MotherBrainAscentMaskTests.Run(device, renderer);
                }
                return;
            }
            if (args is ["--waterfall"])
            {
                foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
                {
                    using var device = new D3D11RenderDevice(kind);
                    using var renderer = new D3D11FrameRenderer(device);
                    WaterfallTests.Run(device, renderer);
                }
                return;
            }
            // Installed-content checks read the repository ROM's shared extracted installation. The
            // spores check corrupts and restores stock files, so it runs on a private copy.
            if (args is ["--installed-spores"])
            {
                using var copy = InstalledFixtures.CreatePrivateCopy();
                InstalledSporesTests.Run(copy.Root, Path.GetFullPath("Super Metroid.smc"), Path.GetFullPath("standalone-assets/raw"));
                return;
            }
            if (args is ["--installed-power-bomb-isolation"])
            {
                InstalledPowerBombIsolationTests.Run(RepositoryInstallation.Installation.Root);
                return;
            }
            if (args is ["--installed-samus-file-contracts"])
            {
                InstalledSamusFileContractTests.Run(RepositoryInstallation.Installation.Root);
                return;
            }
            if (args is ["--installed-samus-isolation"])
            {
                string samusInstallationRoot = RepositoryInstallation.Installation.Root;
                using var fixture = new InstalledSamusArtworkFixture(samusInstallationRoot, editLayout: true);
                InstalledSamusIsolationTests.Run(fixture, samusInstallationRoot);
                return;
            }
            if (args is ["--installed-samus-artwork"])
            {
                using var fixture = new InstalledSamusArtworkFixture(RepositoryInstallation.Installation.Root);
                foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
                {
                    using var device = new D3D11RenderDevice(kind);
                    using var renderer = new D3D11FrameRenderer(device);
                    InstalledSamusArtworkTests.Run(fixture, device, renderer);
                }
                return;
            }
            if (args is ["--background-mosaic"])
            {
                foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
                {
                    using var device = new D3D11RenderDevice(kind);
                    using var renderer = new D3D11FrameRenderer(device);
                    BackgroundMosaicTests.Run(device, renderer);
                }
                return;
            }
            if (args is ["--minimap-blink"])
            {
                foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
                {
                    using var device = new D3D11RenderDevice(kind);
                    using var renderer = new D3D11FrameRenderer(device);
                    long sequence = 0;
                    MinimapBlinkAudit.Run(runtime =>
                    {
                        var packet = new DisplayPacket(new(++sequence, 1, runtime.Clock.Counter),
                            GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                        var pixels = renderer.RenderForReadback(packet);
                        PixelComparison.Verify(packet, FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame()), pixels,
                            $"{kind}: minimap blink frame {sequence}");
                        return pixels;
                    });
                    Console.WriteLine($"{kind}: {sequence} full frames match the WARP reference and preserve visible minimap flashing.");
                }
                return;
            }
            if (args is ["--reference-startup"] or ["--reference-title-pan"])
            {
                byte[] rom = File.ReadAllBytes("Super Metroid.smc");
                foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
                {
                    using var device = new D3D11RenderDevice(kind);
                    using var renderer = new D3D11FrameRenderer(device);
                    if (args[0] == "--reference-startup") StartupReferenceTests.Run(device, renderer);
                    else StartupReferenceTests.RunPan(device, renderer);
                }
                return;
            }
            if (args.Length == 1 && RetailSceneTests.TryRun(args[0])) return;
            if (args is ["--presentation-handoff"])
            {
                PresentationHandoffTests.Run();
                return;
            }
            if (args.Length == 1 && args[0] == "--profile-simulation")
            {
                SimulationProfile.Run();
                return;
            }
            // No argument runs the device contract suite: shader failure, readback, solid compute and threading.
            if (args.Length > 1 || args.Length == 1 && args[0] is not ("--solid-smoke" or "--tile-smoke" or "--obj-smoke" or "--sprite-draw-smoke" or "--mode7-smoke" or "--window-smoke" or "--ordinary-smoke" or "--scene-window-smoke" or "--retail-frontend" or "--retail-intro" or "--retail-transitions" or "--display-smoke" or "--swapchain-smoke" or "--slow-consumer-audio" or "--xray-smoke"))
                throw new ArgumentException("Usage: SuperMetroid.RenderVerification [--solid-smoke | --tile-smoke | --obj-smoke | --sprite-draw-smoke | --mode7-smoke | --window-smoke | --ordinary-smoke | --scene-window-smoke | --retail-frontend | --retail-intro | --retail-transitions | --display-smoke | --swapchain-smoke | --slow-consumer-audio | --xray-smoke]");
            foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
            {
                using var device = new D3D11RenderDevice(kind);
                using var renderer = new D3D11FrameRenderer(device);
                if (args is ["--slow-consumer-audio"]) { SwapchainTests.RunSlowConsumerAudio(device); continue; }
                if (args is ["--swapchain-smoke"]) { SwapchainTests.Run(device, renderer); SwapchainTests.RunWorker(device); continue; }
                if (args is ["--display-smoke"]) { DisplayPassTests.Run(device, renderer); continue; }
                if (args is ["--retail-transitions"]) { RetailTransitionTests.Run(device, renderer); continue; }
                if (args is ["--retail-intro"]) { RetailCinematicTests.Run(device, renderer); continue; }
                if (args is ["--retail-frontend"]) { RetailFrontendTests.Run(device, renderer); continue; }
                if (args is ["--scene-window-smoke"]) { WindowSceneSmokeTests.Run(device, renderer); continue; }
                if (args is ["--ordinary-smoke"]) { OrdinarySmokeTests.Run(device, renderer); continue; }
                if (args is ["--window-smoke"]) { ColorWindowSmokeTests.Run(device, renderer); continue; }
                if (args is ["--mode7-smoke"]) { Mode7SmokeTests.Run(device, renderer); continue; }
                if (args is ["--obj-smoke"]) { ObjectSmokeTests.Run(device, renderer); continue; }
                if (args is ["--sprite-draw-smoke"]) { SpriteDrawSmokeTests.Run(device, renderer); continue; }
                if (args is ["--tile-smoke"]) { TileSmokeTests.Run(device, renderer); continue; }
                if (args is ["--xray-smoke"]) { XrayGameplayTests.Run(device, renderer); continue; }
                bool rejected = false;
                ShaderFailureTests.Run(device);
                try { renderer.Readback(); } catch (InvalidOperationException) { rejected = true; }
                if (!rejected) throw new InvalidOperationException("Readback before submission was accepted.");
                ComparisonArtifactTests.Run(device, renderer);
                DisplayPacket? last = null;
                for (int i = 0; i < 64; i++)
                {
                    last = new(new(i + 1, 1, 0), new RgbaPixel((byte)i, 73, 129));
                    renderer.Render(last);
                }
                PixelComparison.Verify((last ?? throw new InvalidOperationException("Verification requires last.")), DisplayRendering.Render((last)), renderer.Readback(),
                    $"{kind}: 64 submissions with deferred readback");
                PixelComparison.Verify(last, DisplayRendering.Render(last), renderer.Readback(),
                    $"{kind}: repeat deferred readback");
                Console.WriteLine($"{kind}: deferred submission/readback and uninitialized readback guard passed.");
                int samples = 0;
                foreach (byte alpha in new byte[] { 0, 1, 128, 255 })
                for (byte brightness = 0; brightness <= 15; brightness++)
                {
                    var frame = new DisplayPacket(new(++samples, 1, 0), new RgbaPixel(231, 123, 47, alpha), [new FadeLevel(brightness), new FadeLevel(11)]);
                    RgbaPixel[] expected = DisplayRendering.Render(frame);
                    RgbaPixel[] actual = renderer.RenderForReadback(frame);
                    PixelComparison.Verify(frame, expected, actual, $"{kind}: solid compute sample {samples}");
                }
                // A wrong-thread call must fail before issuing any D3D context operation.
                Exception? wrongThread = Task.Run(() =>
                {
                    try { renderer.RenderForReadback(new(new(1, 1, 0), new RgbaPixel(0, 0, 0))); return null; }
                    catch (Exception exception) { return exception; }
                }).GetAwaiter().GetResult();
                if (wrongThread is not InvalidOperationException) throw new InvalidOperationException("Missing render-owner thread guard.");
                Console.WriteLine($"{kind}: {device.AdapterDescription}; {samples} exact compute/readback frames passed; thread guard passed.");
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            Environment.ExitCode = 1;
        }
    }
}
