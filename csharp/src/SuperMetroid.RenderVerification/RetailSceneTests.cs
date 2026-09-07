using SuperMetroid.Rendering.Direct3D11;

/// <summary>Ordered retail qualification inventory; aggregate execution reuses one GPU renderer.</summary>
internal static class RetailSceneTests
{
    private static readonly (string Command, Action<D3D11RenderDevice, D3D11FrameRenderer> Run)[] scenes =
    [
        ("--retail-frontend", RetailFrontendTests.Run),
        ("--retail-intro", RetailCinematicTests.Run),
        ("--retail-transitions", RetailTransitionTests.Run),
        ("--retail-file-map", RetailFileMapTests.Run),
        ("--retail-attract", RetailAttractTests.Run),
        ("--retail-rooms", RetailRoomPublicationTests.Run),
        ("--runtime-overlays", RuntimeOverlayTests.Run),
        ("--retail-ridley", RetailRidleyCaptureTests.Run),
        ("--retail-doors", RetailDoorCaptureTests.Run),
        ("--retail-kraid", RetailKraidCaptureTests.Run),
        ("--retail-crocomire", RetailCrocomireCaptureTests.Run),
        ("--norfair-glow", NorfairGlowCaptureTests.Run),
        ("--retail-eye", RetailEyeCaptureTests.Run),
        ("--retail-elevator", RetailElevatorCaptureTests.Run),
        ("--retail-elevator-return", RetailElevatorReturnTests.Run),
        ("--retail-ceres-quakes", RetailCeresQuakeTests.Run),
        ("--retail-save", RetailSaveCaptureTests.Run),
        ("--retail-death", RetailDeathCaptureTests.Run),
        ("--retail-reserves", RetailDeathCaptureTests.RunReserveRecovery),
        ("--retail-ending", RetailEndingTests.Run),
        ("--retail-ending-handoff", RetailEndingHandoffTests.Run),
    ];

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
