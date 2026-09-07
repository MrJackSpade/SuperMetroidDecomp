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
            RoomHeaderPointers.MorphBallRoom, RoomHeaderPointers.CeresDeadScientistRoom,
            RoomPublicationFixtureDefinitions.BrinstarWater, RoomPublicationFixtureDefinitions.NorfairBusinessCenter];
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
            if (room == RoomPublicationFixtureDefinitions.BrinstarWater && runtime.DisplayedRoomLayer3Fx?.Type != RoomFxType.Water)
                throw new InvalidOperationException("Retail water fixture did not publish water FX.");
            if (room == RoomPublicationFixtureDefinitions.NorfairBusinessCenter &&
                (runtime.DisplayedRoomLayer3Fx is not { Type: RoomFxType.Lava } lava ||
                lava.CurrentYPosition != RoomPublicationFixtureDefinitions.BusinessCenterSurface))
                throw new InvalidOperationException("Retail heat fixture did not publish its authored lava surface.");
            for (int tick = 0; tick < 13; tick++)
            {
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(++sequence, 1, (ushort)tick),
                    GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                if (room == RoomPublicationFixtureDefinitions.NorfairBusinessCenter &&
                    (packet.Layers!.Layers[0] is not OrdinaryGameplayRenderLayer heat ||
                    heat.VerticalScrolls.ToArray().Distinct().Count() != 3))
                    throw new InvalidOperationException("Heat packet lost the three-valued vertical distortion table.");
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

internal static class RoomPublicationFixtureDefinitions
{
    /// <summary>Retail Brinstar water room $01/$27, header $8F:A3DD.</summary>
    internal const ushort BrinstarWater = 0xa3dd;
    /// <summary>Retail Norfair Business Center $8F:A75D, used by the existing heat audit.</summary>
    internal const ushort NorfairBusinessCenter = 0xa75d;
    /// <summary>Authored liquid surface Y from Business Center's default FX record.</summary>
    internal const ushort BusinessCenterSurface = 0x00b8;
}
