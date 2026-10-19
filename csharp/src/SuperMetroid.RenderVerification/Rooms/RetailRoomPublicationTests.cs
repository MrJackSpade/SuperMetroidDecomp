using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.RenderVerification.Rooms;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Checks captured and retained gameplay frames across retail room-local publication fixtures.</summary>
internal static class RetailRoomPublicationTests
{
    /// <summary>Compares serialized frame packets with legacy pixels, including water and heat FX publication.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        (RoomHeaderDefinition Room, PublishedFx Fx)[] rooms =
        [
            (RoomHeaderDefinitions.LandingSite, PublishedFx.Unchecked),
            (RoomHeaderDefinitions.Parlor, PublishedFx.Unchecked),
            (RoomHeaderDefinitions.ElevToBlueBrinstar, PublishedFx.Unchecked),
            (RoomHeaderDefinitions.GreenBrinstarMainShaft, PublishedFx.Unchecked),
            (RoomHeaderDefinitions.MorphBall, PublishedFx.Unchecked),
            (RoomHeaderDefinitions.DeadScientist, PublishedFx.Unchecked),
            (RoomPublicationFixtureDefinitions.BrinstarWater, PublishedFx.Water),
            (RoomPublicationFixtureDefinitions.NorfairBusinessCenter, PublishedFx.Heat),
        ];
        long sequence = 0;
        foreach ((RoomHeaderDefinition room, PublishedFx fx) in rooms)
        {
            var runtime = RepositoryInstallation.CreateRuntime();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            // Room-local setup, explicitly not an incoming door/elevator transition.
            runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            switch (fx)
            {
                case PublishedFx.Unchecked:
                    break;
                case PublishedFx.Water:
                    if (runtime.Display.DisplayedRoomLayer3Fx?.Type != RoomFxType.Water)
                        throw new InvalidOperationException("Retail water fixture did not publish water FX.");
                    break;
                case PublishedFx.Heat:
                    if (runtime.Display.DisplayedRoomLayer3Fx is not { Type: RoomFxType.Lava } lava ||
                        lava.Surface.Y != RoomPublicationFixtureDefinitions.BusinessCenterSurface)
                        throw new InvalidOperationException("Retail heat fixture did not publish its authored lava surface.");
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PublishedFx)} {(int)fx}.");
            }
            for (int tick = 0; tick < 13; tick++)
            {
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                var packet = new RenderFrameSnapshot(new(++sequence, 1, (ushort)tick),
                    GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
                if (fx == PublishedFx.Heat &&
                    ((packet.Layers ?? throw new InvalidOperationException("Verification requires packet.Layers.")).Layers[0] is not OrdinaryGameplayRenderLayer heat ||
                    heat.Bg2LineScrolls.Vertical.CountDistinct() != 3))
                    throw new InvalidOperationException("Heat packet lost the three-valued vertical distortion table.");
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: room {room.Name} publication {tick}");
                runtime.StepFrame(0, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AllowCeresElevatorDeparture = false } });
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: room {room.Name} retained publication {tick}");
            }
        }
        Console.WriteLine($"{device.Kind}: {sequence} room publications plus retained checks across {rooms.Length} room-local fixtures match exactly.");
    }

    /// <summary>The layer-3 FX publication a room-local fixture additionally asserts.</summary>
    private enum PublishedFx
    {
        /// <summary>Only the frame publication itself is compared.</summary>
        Unchecked,
        /// <summary>Brinstar water FX is published.</summary>
        Water,
        /// <summary>Business Center lava publishes its authored surface and three-valued heat distortion.</summary>
        Heat,
    }
}
