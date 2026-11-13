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
using SuperMetroid.TestSupport.FrameRendering;

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
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            var ridley = runtime.Enemies.RidleyEncounter.CeresRidley;
            bool triggered = false, finished = false;
            int samples = 0, mixedFrames = 0;
            var floorViews = new HashSet<FloorView>();
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
                if (ridley.GetawayActive)
                {
                    mixedFrames++;
                    floorViews.Add(CeresRidleyGetaway.FloorTransform(ridley) ?? throw new InvalidOperationException("An active getaway shows its floor."));
                }
                finished = triggered && runtime.Enemies.RidleyEncounter.CeresStatus == SuperMetroid.Core.Enemies.CeresEscapeStatus.SelfDestructStarted && ridley.GetawayFinished &&
                    !ridley.GetawayActive && !samus.CeresRidleyEjection.IsActive;
                // Capture every mixed-mode frame, every AI boundary and periodic startup
                // frames. Completion is captured before leaving the room-local fixture.
                if (tick % 8 == 0 || newPhase || ridley.GetawayActive || finished)
                {
                    GameplayDisplayFrame frame = runtime.PublishDisplayFrame();
                    var expected = FrontendScenePixelsAccess.Pixels(frame);
                    var scene = GameplayDisplayCapture.Capture(frame)
                        ?? throw new InvalidOperationException("Ridley escape omitted capture.");
                    // The getaway's floor reaches the screen at the accepted frame after the main loop that computed it
                    // (#1307), so the published frame, not the live getaway, decides whether this picture shows it.
                    if (frame.Latched.PerspectiveFloor is { Below: BelowPerspectiveFloor.RoomBackground } && scene.Layers[0] is not TurningRoomLayer { Floor: not null })
                        throw new InvalidOperationException("Native Mode-7 escape omitted its preserved floor layer.");
                    samples++;
                    var packet = new DisplayPacket((ushort)tick, scene);
                    PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                        $"{device.Kind}: Ridley native escape hits={hitThreshold}, tick={tick}, phase={ridley.Function}");
                }
                if (finished) break;
            }
            if (!finished || mixedFrames == 0 || floorViews.Count < 2 || runtime.EscapeTimer.Phase == EscapeTimerPhase.Inactive)
                throw new InvalidOperationException($"Native escape coverage incomplete: triggered={triggered}, finished={finished}, " +
                    $"phase={(ridley ?? throw new InvalidOperationException("Verification requires ridley.")).Function}, mode7={mixedFrames}, floor views={floorViews.Count}.");
            Console.WriteLine($"{device.Kind}: native Ridley escape hits={hitThreshold}: {samples} exact samples, " +
                $"{mixedFrames} Mode-7 frames, {floorViews.Count} floor views, {phases.Count} phases; timer/ejection handoff complete.");
        }
    }
}
