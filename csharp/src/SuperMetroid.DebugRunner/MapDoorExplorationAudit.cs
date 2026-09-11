using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

/// <summary>Traces newly explored cells across the reported Hellway/Caterpillar door.</summary>
internal static class MapDoorExplorationAudit
{
    public static int Run(string romPath)
    {
        foreach (bool mapDownloaded in new[] { false, true })
            RunCase(romPath, mapDownloaded);
        return 0;
    }

    private static void RunCase(string romPath, bool mapDownloaded)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Hellway, 512, 0);
        runtime.InitializeDebugGroundedSamus(700, desiredScreenY: 139, minimumFloorBlockY: 8);
        if (mapDownloaded)
            runtime.System.SetAreaMapAcquired(AreaId.Brinstar);
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        var visited = new HashSet<(int X, int Y)>();
        int destinationFrames = 0;
        bool entered = false;
        int leg = 0;
        for (int frame = 0; frame < 1800; frame++)
        {
            ushort destination = RoomHeaderPointers.Caterpillar;
            string phase = transition.IsActive ? transition.Phase.ToString() : "Gameplay";
            ushort input = entered ? (ushort)0 :
                (ushort)(SnesButton.Right | (frame % 12 == 0 ? SnesButton.X : 0));
            if (transition.IsActive)
                transition.Step(runtime, audio, input);
            else
            {
                runtime.StepFrame(input);
                runtime.RunNmi(input, mainLoopRequestedNmi: true);
                if (runtime.HasPendingDoorTransition)
                {
                    if (runtime.PendingDoorTransition!.DestinationRoomPointer != destination)
                        throw new InvalidDataException("Ordinary input selected the wrong door.");
                    transition.Begin(runtime);
                    entered = true;
                }
                else if (entered)
                    destinationFrames++;
            }

            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 64; x++)
            {
                if (!runtime.System.IsMapTileExplored(AreaId.Brinstar, x, y) || !visited.Add((x, y)))
                    continue;
                Console.WriteLine($"map={mapDownloaded} leg={leg} frame={frame} phase={phase} room={runtime.ActiveRoom!.Pointer:X4} " +
                    $"samus={runtime.Samus!.XPosition},{runtime.Samus.YPosition} " +
                    $"new-cell={x},{y} hud-center={runtime.Hud.MinimapCenterX},{runtime.Hud.MinimapCenterY}");
            }
            if (destinationFrames == 20)
            {
                Console.WriteLine($"Reached destination normally; {visited.Count} visited cells: " +
                    string.Join("; ", visited.OrderBy(cell => cell.Y).ThenBy(cell => cell.X)));
                const string directory = "csharp/test-temp/map-door-571";
                Directory.CreateDirectory(directory);
                PngWriter.WriteRgba(Path.Combine(directory, $"map-{mapDownloaded}-leg-{leg}.png"),
                    FrontendFrame.Width, FrontendFrame.Height, SuperMetroidRuntimeFrameRenderer.Render(runtime));
                for (int row = 0; row < 3; row++)
                    Console.WriteLine("HUD " + string.Join(" ", runtime.Hud.Tiles.Slice(26 + row * 32, 5)
                        .ToArray().Select(word => word.ToString("X4"))));
                if (!visited.SetEquals(new[] { (36, 10), (37, 10) }))
                    throw new InvalidDataException("This doorway sequence explored cells outside its two occupied map screens.");
                if (++leg == 2)
                    return;
                // Isolate repeated entry with the exact accumulated exploration/map
                // state. Re-seed only the source placement; traversing back would add
                // an unrelated Power Bomb puzzle at Caterpillar's yellow return door.
                runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Hellway, 512, 0);
                runtime.InitializeDebugGroundedSamus(700, desiredScreenY: 139, minimumFloorBlockY: 8);
                destinationFrames = 0;
                entered = false;
            }
        }
        throw new InvalidDataException($"Door exploration trace timed out: room={runtime.ActiveRoom!.Pointer:X4}, " +
            $"Samus={runtime.Samus!.XPosition},{runtime.Samus.YPosition}, phase={transition.Phase}.");
    }
}
