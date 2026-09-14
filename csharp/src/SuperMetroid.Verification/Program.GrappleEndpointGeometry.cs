using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyGrappleEndpointGeometry(ISnesAddressSpace bus)
    {
        // $94:B093 chooses by pose, not grapple connection phase. Keep the
        // rope origin off-screen so its first visited slot is culled and only
        // the endpoint can emit OAM. Probe both sides of the camera subtraction
        // as well as the pre-centering vertical visibility boundary.
        foreach (byte pose in new byte[] { 1, 2, 0xb2, 0xb3 })
        foreach (ushort camera in new ushort[] { 0, 128, 65535 })
        foreach (int offsetX in new[] { -1, 0, 4, 255, 256 })
        foreach (int offsetY in new[] { -1, 0, 3, 4, 255, 256 })
        {
            ushort endpointX = unchecked((ushort)(camera + offsetX));
            ushort endpointY = unchecked((ushort)(camera + offsetY));
            var grapple = new SamusGrappleState
            {
                Phase = GrapplePhase.Firing, RopeLength = 8,
                BeamStartX = camera, BeamStartY = camera,
                AnchorX = endpointX, AnchorY = endpointY, PointAnimationTimer = 5,
            };
            grapple.SegmentAnimationTimers[15] = 1;
            var oam = new OamBuffer();
            SamusGrappleMovement.DrawConnectedBeam(bus, grapple, oam, new VramWriteQueue(), camera, camera, samusPose: pose);
            bool swinging = pose is 0xb2 or 0xb3;
            ushort relativeY = unchecked((ushort)(endpointY - camera));
            bool visible = swinging || (relativeY & 0xff00) == 0;
            string context = $"pose {pose:X2}, camera {camera}, offsets {offsetX}/{offsetY}";
            AssertEqual(visible ? 4 : 0, oam.NextByteOffset, "Native endpoint vertical gate: " + context);
            if (!visible) continue;
            // Connected code omits the second SEC. Preserve the borrow from the
            // camera subtraction, including when unsigned world coordinates wrap.
            ushort x = unchecked((ushort)(endpointX - camera - 4 - (swinging && endpointX < camera ? 1 : 0)));
            ushort y = unchecked((ushort)(endpointY - camera - 4 - (swinging && endpointY < camera ? 1 : 0)));
            AssertEqual((byte)x, oam.LowTable[0], "Native endpoint X: " + context);
            AssertEqual((byte)y, oam.LowTable[1], "Native endpoint Y: " + context);
            AssertEqual((byte)((x >> 8) & 1), oam.HighTable[0], "Native endpoint high-X bit: " + context);
            AssertEqual((byte)0x20, oam.LowTable[2], "Native endpoint character");
            AssertEqual((byte)0x3a, oam.LowTable[3], "Native endpoint palette/priority");
        }
        VerifyGrappleEndpointActor((SuperMetroidAddressSpace)bus);
        Console.WriteLine("Grapple endpoint geometry: 360 pose/camera/wrapped-coordinate cases and actual actor pose handoff match native clipping, borrow and OAM attributes.");
    }

    private static void VerifyGrappleEndpointActor(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.RunNmi(0, true);
        var samus = runtime.Samus!;
        foreach (byte pose in new byte[] { 1, 0xb2, 0xb3 })
        {
            samus.Pose = pose;
            samus.XPosition = (ushort)(runtime.Camera!.XPosition + 100);
            samus.YPosition = (ushort)(runtime.Camera.YPosition + 100);
            samus.InitializeAnimation(bus); samus.PrimeGraphics(bus);
            samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
            samus.Grapple.RopeLength = 8;
            samus.Grapple.AnchorX = samus.XPosition;
            samus.Grapple.AnchorY = unchecked((ushort)(runtime.Camera.YPosition - 1));
            samus.Grapple.BeamStartX = samus.XPosition;
            samus.Grapple.BeamStartY = samus.YPosition;
            samus.Grapple.SegmentAnimationTimers[15] = 1;
            runtime.Oam.BeginFrame();
            typeof(SuperMetroidRuntime).GetMethod("DrawGameplayActors", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(runtime, new object?[] { false, null, null, false });
            int endpoints = 0;
            for (int offset = 0; offset < runtime.Oam.NextByteOffset; offset += 4)
                if (runtime.Oam.LowTable[offset + 2] == 0x20 && runtime.Oam.LowTable[offset + 3] == 0x3a)
                    endpoints++;
            AssertEqual(pose == 1 ? 0 : 1, endpoints, $"Actual gameplay actor passes endpoint pose {pose:X2}");
        }
    }
}
