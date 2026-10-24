using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Hud;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Areas;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Escape;

/// <summary>Native AI-driven escape after staging either retail termination condition at hover.</summary>
internal static class RetailRidleyEscapeTests
{
    /// <summary>Checks both hover-exit triggers through Mode 7 retreat, ejection, and escape-timer handoff.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        foreach (bool hitThreshold in new[] { false, true })
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = RepositoryInstallation.CreateRuntime(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
            runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            var ridley = runtime.Enemies.RidleyEncounter.CeresRidley;
            bool triggered = false, finished = false;
            int samples = 0, mixedFrames = 0;
            var matrices = new HashSet<Mode7Matrix>();
            var phases = new HashSet<RidleyAiFunction>();
            for (int tick = 0; tick < 6000; tick++)
            {
                if (!triggered && (ridley ?? throw new InvalidOperationException("Verification requires ridley.")).Function == RidleyAiFunction.CeresHovering)
                {
                    if (hitThreshold) ridley.HitCounter = RidleyEscapeFixtureDefinitions.HitThreshold;
                    else samus.Health = RidleyEscapeFixtureDefinitions.LowEnergy;
                    triggered = true;
                }
                runtime.StepFrame(0);
                bool newPhase = phases.Add((ridley ?? throw new InvalidOperationException("Verification requires ridley.")).Function);
                if (ridley.Mode7Active)
                {
                    mixedFrames++;
                    matrices.Add(new Mode7Matrix(ridley.Mode7MatrixA, ridley.Mode7MatrixB, ridley.Mode7MatrixC, ridley.Mode7MatrixD));
                }
                finished = triggered && runtime.Enemies.RidleyEncounter.CeresStatus == SuperMetroid.Core.Enemies.CeresEscapeStatus.SelfDestructStarted && ridley.Mode7Finished &&
                    !ridley.Mode7Active && !samus.CeresRidleyEjection.IsActive;
                // Capture every mixed-mode frame, every AI boundary and periodic startup
                // frames. Completion is captured before leaving the room-local fixture.
                if (tick % 8 == 0 || newPhase || ridley.Mode7Active || finished)
                {
                    GameplayDisplayFrame frame = runtime.PublishDisplayFrame();
                    var expected = FrontendScenePixelsAccess.Pixels(frame);
                    var scene = GameplayDisplayCapture.Capture(frame)
                        ?? throw new InvalidOperationException("Ridley escape omitted capture.");
                    // The getaway's floor reaches the screen at the accepted frame after the main loop that computed it
                    // (#1307), so the published frame, not the live getaway, decides whether this picture shows it.
                    if (frame.Latched.PerspectiveFloor is { Below: BelowPerspectiveFloor.Layer2 } && scene.Layers[0] is not Mode7GameplayRenderLayer { Floor: not null })
                        throw new InvalidOperationException("Native Mode-7 escape omitted its preserved floor layer.");
                    var packet = new DisplayPacket(new(++samples, 1, (ushort)tick), scene);
                    PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                        $"{device.Kind}: Ridley native escape hits={hitThreshold}, tick={tick}, phase={ridley.Function}");
                }
                if (finished) break;
            }
            if (!finished || mixedFrames == 0 || matrices.Count < 2 || runtime.EscapeTimer.State == EscapeTimerState.Inactive)
                throw new InvalidOperationException($"Native escape coverage incomplete: triggered={triggered}, finished={finished}, " +
                    $"phase={(ridley ?? throw new InvalidOperationException("Verification requires ridley.")).Function}, mode7={mixedFrames}, matrices={matrices.Count}.");
            Console.WriteLine($"{device.Kind}: native Ridley escape hits={hitThreshold}: {samples} exact samples, " +
                $"{mixedFrames} Mode-7 frames, {matrices.Count} matrices, {phases.Count} phases; timer/ejection handoff complete.");
        }
    }
}
