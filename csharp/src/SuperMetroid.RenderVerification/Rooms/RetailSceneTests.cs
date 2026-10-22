using SuperMetroid.RenderVerification.Areas;
using SuperMetroid.RenderVerification.Bosses;
using SuperMetroid.RenderVerification.Frontend;
using SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.RenderVerification.Saves;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;

namespace SuperMetroid.RenderVerification.Rooms;

/// <summary>Ordered retail qualification inventory; aggregate execution reuses one GPU renderer.</summary>
internal static class RetailSceneTests
{
    /// <summary>Command-to-scene mapping in the order used by aggregate GPU qualification.</summary>
    private static readonly RetailScene[] scenes =
    [
        new RetailScene("--retail-frontend", RetailFrontendTests.Run),
        new RetailScene("--retail-intro", RetailCinematicTests.Run),
        new RetailScene("--retail-transitions", RetailTransitionTests.Run),
        new RetailScene("--retail-file-map", RetailFileMapTests.Run),
        new RetailScene("--retail-pause", RetailPauseTests.Run),
        new RetailScene("--retail-attract", RetailAttractTests.Run),
        new RetailScene("--retail-rooms", RetailRoomPublicationTests.Run),
        new RetailScene("--runtime-overlays", RuntimeOverlayTests.Run),
        new RetailScene("--retail-ridley", RetailRidleyCaptureTests.Run),
        new RetailScene("--retail-doors", RetailDoorCaptureTests.Run),
        new RetailScene("--retail-kraid", RetailKraidCaptureTests.Run),
        new RetailScene("--retail-crocomire", RetailCrocomireCaptureTests.Run),
        new RetailScene("--norfair-glow", NorfairGlowCaptureTests.Run),
        new RetailScene("--retail-eye", RetailEyeCaptureTests.Run),
        new RetailScene("--retail-elevator", RetailElevatorCaptureTests.Run),
        new RetailScene("--retail-elevator-return", RetailElevatorReturnTests.Run),
        new RetailScene("--retail-ceres-quakes", RetailCeresQuakeTests.Run),
        new RetailScene("--retail-save", RetailSaveCaptureTests.Run),
        new RetailScene("--retail-death", RetailDeathCaptureTests.Run),
        new RetailScene("--retail-reserves", RetailDeathCaptureTests.RunReserveRecovery),
        new RetailScene("--retail-ending", RetailEndingTests.Run),
        new RetailScene("--retail-ending-handoff", RetailEndingHandoffTests.Run),
    ];

    /// <summary>Runs the selected retail scenes on each GPU device kind with one renderer reused per device.</summary>
    internal static bool TryRun(string command)
    {
        bool all = command == "--retail-all";
        var selected = scenes.Where(scene => all || scene.Command == command).ToArray();
        if (selected.Length == 0) return false;
        foreach (var kind in Enum.GetValues<D3D11DeviceKind>())
        {
            using var device = new D3D11RenderDevice(kind);
            using var renderer = new D3D11FrameRenderer(device);
            foreach (var scene in selected)
            {
                Console.WriteLine($"{kind}: starting {scene.Command} on shared renderer.");
                scene.Run(device, renderer);
            }
            Console.WriteLine($"{kind}: {selected.Length} retail suites passed on shared renderer.");
        }
        return true;
    }
}
