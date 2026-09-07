using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Native AI-driven escape after staging either retail termination condition at hover.</summary>
internal static class RetailRidleyEscapeTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        foreach (bool hitThreshold in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom, 0, 0);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            var ridley = runtime.Enemies.CeresRidley!;
            bool triggered = false, finished = false;
            int samples = 0, mixedFrames = 0;
            var matrices = new HashSet<(ushort, ushort, ushort, ushort)>();
            var phases = new HashSet<RidleyAiFunction>();
            for (int tick = 0; tick < 6000; tick++)
            {
                if (!triggered && ridley.Function == RidleyAiFunction.CeresHovering)
                {
                    if (hitThreshold) ridley.HitCounter = RidleyEscapeFixtureDefinitions.HitThreshold;
                    else samus.Health = RidleyEscapeFixtureDefinitions.LowEnergy;
                    triggered = true;
                }
                runtime.StepFrame(0);
                bool newPhase = phases.Add(ridley.Function);
                if (ridley.Mode7Active)
                {
                    mixedFrames++;
                    matrices.Add((ridley.Mode7MatrixA, ridley.Mode7MatrixB, ridley.Mode7MatrixC, ridley.Mode7MatrixD));
                }
                finished = triggered && runtime.Enemies.CeresStatus == 2 && ridley.Mode7Finished &&
                    !ridley.Mode7Active && !samus.CeresRidleyEjection.IsActive;
                // Capture every mixed-mode frame, every AI boundary and periodic startup
                // frames. Completion is captured before leaving the room-local fixture.
                if (tick % 8 == 0 || newPhase || ridley.Mode7Active || finished)
                {
                    var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                    var scene = GameplayDisplayCapture.TryCaptureFrame(runtime)
                        ?? throw new InvalidOperationException("Ridley escape omitted capture.");
                    if (ridley.Mode7Active && scene.Layers[0] is not Mode7GameplayRenderLayer { Floor: not null })
                        throw new InvalidOperationException("Native Mode-7 escape omitted its preserved floor layer.");
                    var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick), scene);
                    packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                    PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                        $"{device.Kind}: Ridley native escape hits={hitThreshold}, tick={tick}, phase={ridley.Function}");
                }
                if (finished) break;
            }
            if (!finished || mixedFrames == 0 || matrices.Count < 2 || runtime.EscapeTimer.State == EscapeTimerState.Inactive)
                throw new InvalidOperationException($"Native escape coverage incomplete: triggered={triggered}, finished={finished}, " +
                    $"phase={ridley.Function}, mode7={mixedFrames}, matrices={matrices.Count}.");
            Console.WriteLine($"{device.Kind}: native Ridley escape hits={hitThreshold}: {samples} exact samples, " +
                $"{mixedFrames} Mode-7 frames, {matrices.Count} matrices, {phases.Count} phases; timer/ejection handoff complete.");
        }
    }
}

internal static class RidleyEscapeFixtureDefinitions
{
    /// <summary>Ceres hover AI branches to the fake retreat at 100 registered projectile contacts.</summary>
    internal const ushort HitThreshold = 100;
    /// <summary>Energy below the native 30 threshold selects the direct retreat; keep the player alive.</summary>
    internal const ushort LowEnergy = 29;
}
