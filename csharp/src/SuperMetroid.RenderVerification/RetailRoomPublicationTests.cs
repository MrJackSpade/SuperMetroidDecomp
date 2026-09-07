using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailRoomPublicationTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        ushort[] rooms = [RoomHeaderPointers.LandingSite, RoomHeaderPointers.ParlorAndAlcatraz,
            RoomHeaderPointers.BlueBrinstarElevatorRoom, RoomHeaderPointers.GreenBrinstarMainShaft,
            RoomHeaderPointers.MorphBallRoom, RoomHeaderPointers.CeresDeadScientistRoom];
        long sequence = 0;
        foreach (ushort room in rooms)
        {
            var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            // Room-local setup, explicitly not an incoming door/elevator transition.
            runtime.LoadCartridgeRoomForDebug(room, 0, 0);
            runtime.RunNmi(0, true);
            for (int tick = 0; tick < 13; tick++)
            {
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(++sequence, 1, (ushort)tick),
                    GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: room {room:X4} publication {tick}");
                runtime.StepFrame(0, allowCeresElevatorDeparture: false);
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: room {room:X4} retained publication {tick}");
            }
        }
        Console.WriteLine($"{device.Kind}: {sequence} room publications plus retained checks across {rooms.Length} room-local fixtures match exactly.");
    }
}
