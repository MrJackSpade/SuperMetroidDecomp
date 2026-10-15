using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks grapple endpoint clipping, wrapped subtraction, and OAM across poses and cameras.</summary>
internal static class GrappleEndpointGeometryVerification
{
    /// <summary>Checks endpoint visibility and attributes across pose, camera, and coordinate boundaries.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyGrappleEndpointGeometry(ISnesAddressSpace bus)
    {
        // $94:B093 chooses by pose, not grapple connection phase. Keep the
        // rope origin off-screen so its first visited slot is culled and only
        // the endpoint can emit OAM. Probe both sides of the camera subtraction
        // as well as the pre-centering vertical visibility boundary.
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.GrappleSwingRightPose, SamusPoseId.GrappleSwingLeftPose
        })
        foreach (ushort camera in new ushort[] { 0, 128, 65535 })
        foreach (int offsetX in new[] { -1, 0, 4, 255, 256 })
        foreach (int offsetY in new[] { -1, 0, 3, 4, 255, 256 })
        {
            ushort endpointX = unchecked((ushort)(camera + offsetX));
            ushort endpointY = unchecked((ushort)(camera + offsetY));
            var grapple = new SamusGrappleState
            {
                Phase = GrapplePhase.Firing, RopeLength = 8,
                BeamStart = new(camera, camera),
                Anchor = new(endpointX, endpointY), PointAnimationTimer = NativeCountdown.Start(5),
            };
            grapple.SegmentAnimationTimers[15] = NativeCountdown.Start(1);
            var oam = new OamBuffer();
            SamusGrappleMovement.DrawConnectedBeam(bus, grapple, oam, new VramWriteQueue(), new(camera, camera), samusPose: pose);
            bool swinging = pose is SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose;
            ushort relativeY = unchecked((ushort)(endpointY - camera));
            bool visible = swinging || (SnesLittleEndian.HighByteInPlace(relativeY)) == 0;
            string context = $"pose {(int)pose:X2}, camera {camera}, offsets {offsetX}/{offsetY}";
            AssertEqual(visible ? 4 : 0, oam.NextByteOffset, "Native endpoint vertical gate: " + context);
            if (!visible) continue;
            // Connected code omits the second SEC. Preserve the borrow from the
            // camera subtraction, including when unsigned world coordinates wrap.
            ushort x = unchecked((ushort)(endpointX - camera - 4 - (swinging && endpointX < camera ? 1 : 0)));
            ushort y = unchecked((ushort)(endpointY - camera - 4 - (swinging && endpointY < camera ? 1 : 0)));
            AssertEqual((byte)x, oam.LowTable[0], "Native endpoint X: " + context);
            AssertEqual((byte)y, oam.LowTable[1], "Native endpoint Y: " + context);
            AssertEqual((byte)((SnesLittleEndian.HighByte(x)) & 1), oam.HighTable[0], "Native endpoint high-X bit: " + context);
            AssertEqual((byte)0x20, oam.LowTable[2], "Native endpoint character");
            AssertEqual((byte)0x3a, oam.LowTable[3], "Native endpoint palette/priority");
        }
        Suite(nameof(VerifyGrappleEndpointActor), () => VerifyGrappleEndpointActor((SuperMetroidAddressSpace)bus));
        Console.WriteLine("Grapple endpoint geometry: 360 pose/camera/wrapped-coordinate cases and actual actor pose handoff match native clipping, borrow and OAM attributes.");
    }

    /// <summary>Checks the endpoint pose gate through the full gameplay actor drawing pass.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyGrappleEndpointActor(SuperMetroidAddressSpace bus)
    {
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.NmiHandler.RunNmi(0, true);
        var samus = runtime.Player.IntroducedSamus;
        foreach (SamusPoseId pose in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.GrappleSwingRightPose, SamusPoseId.GrappleSwingLeftPose })
        {
            samus.Pose = pose;
            samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
            samus.Presentation.InitializeAnimation(bus); samus.Presentation.PrimeGraphics(bus);
            samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
            samus.Grapple.RopeLength = 8;
            samus.Grapple.Anchor = samus.Grapple.Anchor.WithX(samus.Position.X.Whole);
            samus.Grapple.Anchor = samus.Grapple.Anchor.WithY(unchecked((ushort)(runtime.Room.LoadedCamera.PixelPosition.Y - 1)));
            samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(samus.Position.X.Whole);
            samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(samus.Position.Y.Whole);
            samus.Grapple.SegmentAnimationTimers[15] = NativeCountdown.Start(1);
            runtime.Oam.BeginFrame();
            runtime.ActorDrawing.DrawGameplayActors(false, false);
            int endpoints = 0;
            for (int offset = 0; offset < runtime.Oam.NextByteOffset; offset += 4)
                if (runtime.Oam.LowTable[offset + 2] == 0x20 && runtime.Oam.LowTable[offset + 3] == 0x3a)
                    endpoints++;
            AssertEqual((int)pose == 1 ? 0 : 1, endpoints, $"Actual gameplay actor passes endpoint pose {(int)pose:X2}");
        }
    }
}
