using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyGrappleFiringDefinitions(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        var refresh = typeof(SamusGrappleMovement).GetMethod("RefreshFiringDrawOrigins", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusGrappleState>>();
        var readOrigin = typeof(SamusGrappleMovement).GetMethod("ReadFiringOrigin", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, byte, bool, (short X, short Y)>>();
        var connect = typeof(SamusGrappleMovement).GetMethod("ConnectAcceptedFiring", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, SamusState, SamusGrappleState, ushort, ushort, bool, bool, GrappleMovementResult>>();
        var bus = new GrappleFiringReadGuard(rom);
        var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose };
        var grapple = samus.Grapple;
        for (byte direction = 0; direction < 10; direction++)
        {
            int offset = direction * 2;
            short vx = Word(0x9bc0db + offset), vy = Word(0x9bc0ef + offset);
            ushort angle = unchecked((ushort)Word(0x9bc104 + offset));
            AssertEqual((vx, vy, angle), GrappleFiringDefinitions.Launch(direction), "Native Grapple launch words");
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                // Covers every valid movement type and signed graphics-Y byte, and every X/Y
                // position word. Only native type one selects running hand offsets.
                bus.Movement = (byte)((raw >> 8) % 28);
                bus.GraphicsY = (byte)raw;
                bus.Direction = direction;
                bool running = bus.Movement == 1;
                short x = Word((running ? 0x9bc172 : 0x9bc122) + offset);
                short y = Word((running ? 0x9bc186 : 0x9bc136) + offset);
                int correctedY = y - unchecked((sbyte)bus.GraphicsY);
                samus.XPosition = (ushort)raw;
                samus.YPosition = unchecked((ushort)~raw);
                grapple.Phase = GrapplePhase.Inactive;
                samus.LiquidPhysics.BeginFrameSoundRequests();
                SamusGrappleMovement.BeginFiring(bus, samus);
                AssertEqual(vx, grapple.ExtensionXVelocity, "Actual launch X velocity");
                AssertEqual(vy, grapple.ExtensionYVelocity, "Actual launch Y velocity");
                AssertEqual(angle, grapple.Angle.RawValue, "Actual launch angle");
                AssertEqual(angle, grapple.MirroredAngle.RawValue, "Actual mirrored angle");
                AssertEqual(x, grapple.OriginXOffset, "Actual physical origin X");
                AssertEqual(unchecked((short)correctedY), grapple.OriginYOffset, "Signed graphics correction");
                AssertEqual(unchecked((ushort)(samus.XPosition + x)), grapple.AnchorX, "Launch anchor wraps X");
                AssertEqual(unchecked((ushort)(samus.YPosition + correctedY)), grapple.AnchorY, "Launch anchor wraps Y");
                ushort anchorX = grapple.AnchorX, anchorY = grapple.AnchorY;
                // Late drawing recomputes the physical hand after Samus moves; it must
                // not reuse stale offsets or move the independent collision endpoint.
                samus.XPosition = unchecked((ushort)(samus.XPosition + 7));
                samus.YPosition = unchecked((ushort)(samus.YPosition - 5));
                grapple.OriginXOffset = grapple.OriginYOffset = 1234;
                refresh(bus, samus, grapple);
                AssertEqual(unchecked((ushort)(samus.XPosition + x)), grapple.RopeStartX, "Late physical Start X");
                AssertEqual(unchecked((ushort)(samus.YPosition + correctedY)), grapple.RopeStartY, "Late physical Start Y");
                AssertEqual(anchorX, grapple.AnchorX, "Late draw retains endpoint X");
                AssertEqual(anchorY, grapple.AnchorY, "Late draw retains endpoint Y");
                short flareX = Word((running ? 0x9bc19a : 0x9bc14a) + offset);
                short flareY = Word((running ? 0x9bc1ae : 0x9bc15e) + offset);
                AssertEqual(unchecked((ushort)(samus.XPosition + flareX)), grapple.BeamStartX, "Flare X remains separate");
                AssertEqual(unchecked((ushort)(samus.YPosition + flareY - unchecked((sbyte)bus.GraphicsY))), grapple.BeamStartY, "Flare Y remains separate");
            }
        }
        // Run the real four-substep extension path in empty terrain, not just its setup.
        // Presentation overrides must change Flare without changing trajectory or Start.
        var empty = CreateRoom(64, 64, new ushort[4096], new byte[4096]);
        foreach (bool replaceFlare in new[] { false, true })
        for (byte direction = 0; direction < 10; direction++)
        {
            bus.ReplaceFlare = replaceFlare;
            bus.Movement = 0; bus.GraphicsY = 0; bus.Direction = direction;
            samus.XPosition = samus.YPosition = 512;
            grapple.Phase = GrapplePhase.Inactive;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.BeginFiring(bus, samus);
            if (replaceFlare)
                AssertEqual(0x55aa, grapple.FlareXOffset, "Presentation override is actually consumed");
            for (int frame = 1; frame <= 10; frame++)
            {
                SamusGrappleMovement.StepFiring(bus, empty, samus, (ushort)SnesButton.X);
                int dx = Word(0x9bc0db + direction * 2) * 256 * frame;
                int dy = Word(0x9bc0ef + direction * 2) * 256 * frame;
                AssertEqual(dx, grapple.EndpointXOffsetFixed, "Native extension X trajectory");
                AssertEqual(dy, grapple.EndpointYOffsetFixed, "Native extension Y trajectory");
                AssertEqual(unchecked((ushort)(512 + Word(0x9bc122 + direction * 2) + (dx >> 16))), grapple.AnchorX, "Native endpoint X");
                AssertEqual(unchecked((ushort)(512 + Word(0x9bc136 + direction * 2) + (dy >> 16))), grapple.AnchorY, "Native endpoint Y");
            }
        }
        bus.ReplaceFlare = false;
        // Moving Draygon-held poses use controller-derived direction and a fixed six-
        // pixel correction. Exercise every input word through the actual launch route.
        foreach (bool left in new[] { false, true })
        for (int input = 0; input <= ushort.MaxValue; input++)
        {
            samus.Pose = left ? SamusPoseIds.DraygonGrabbedMovingLeftPose : SamusPoseIds.DraygonGrabbedMovingRightPose;
            samus.XPosition = samus.YPosition = 512;
            bool outward = (input & (int)(left ? SnesButton.Left : SnesButton.Right)) != 0;
            int direction = outward && (input & (int)SnesButton.Down) != 0 ? (left ? 6 : 3) :
                outward && (input & (int)SnesButton.Up) != 0 ? (left ? 8 : 1) : (left ? 7 : 2);
            grapple.Phase = GrapplePhase.Inactive;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.BeginFiring(bus, samus, (ushort)input);
            AssertEqual(direction, grapple.FireDirection, "Held aim selection");
            AssertEqual(Word(0x9bc0db + direction * 2), grapple.ExtensionXVelocity, "Held launch X");
            AssertEqual(Word(0x9bc0ef + direction * 2), grapple.ExtensionYVelocity, "Held launch Y");
            AssertEqual(unchecked((ushort)Word(0x9bc104 + direction * 2)), grapple.Angle.RawValue, "Held launch angle");
            AssertEqual(512 + Word(0x9bc122 + direction * 2), grapple.AnchorX, "Held anchor X");
            AssertEqual(506 + Word(0x9bc136 + direction * 2), grapple.AnchorY, "Held anchor Y correction");
        }
        // Actual locked connection subtracts the raw no-run origin, without graphics-Y
        // correction. Native connection/pose/animation records remain on the real bus.
        int locked = 0;
        foreach (byte movement in new byte[] { 0, 1, 5 })
        for (byte direction = 0; direction < 10; direction++)
        foreach (ushort coordinate in new ushort[] { 0, 512, ushort.MaxValue })
        {
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            bus.Movement = movement; bus.Direction = direction; bus.GraphicsY = 127;
            samus.XPosition = coordinate; samus.YPosition = coordinate;
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            grapple.FireDirection = direction;
            grapple.AnchorX = unchecked((ushort)(coordinate + 24));
            grapple.AnchorY = unchecked((ushort)(coordinate + 8));
            grapple.RopeLength = 32;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            var result = connect(bus, samus, grapple, coordinate, coordinate, true, false);
            if (!result.LockedInPlace) continue;
            locked++;
            AssertEqual(unchecked((ushort)(grapple.RopeStartX - Word(0x9bc122 + direction * 2))), samus.XPosition, "Locked connection body X");
            AssertEqual(unchecked((ushort)(grapple.RopeStartY - Word(0x9bc136 + direction * 2))), samus.YPosition, "Locked connection body Y ignores graphics correction");
        }
        AssertEqual(54, locked, "All six locked directions in three source movement families at three boundaries");
        foreach (bool running in new[] { false, true })
        for (int direction = 0; direction <= byte.MaxValue; direction++)
        {
            bus.ForbidMechanics = direction < 10;
            AssertEqual(direction < 10, GrappleFiringDefinitions.TryGetOrigin((byte)direction, running, out _), "Authored origin boundary");
            AssertEqual((Word((running ? 0x9bc172 : 0x9bc122) + direction * 2), Word((running ? 0x9bc186 : 0x9bc136) + direction * 2)),
                readOrigin(bus, (byte)direction, running), "Restored direction preserves non-catalog read");
        }
        Console.WriteLine("Grapple firing definitions: 70 native words, 655360 launch/late-origin cases, 131072 held launches, 200 trajectory frames with flare overrides, 54 locked snaps and all 512 origin selections pass; authored mechanics reads forbidden.");
    }

    private sealed class GrappleFiringReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte Movement, GraphicsY, Direction;
        public bool ForbidMechanics = true;
        public bool ReplaceFlare;
        public byte ReadByte(int address)
        {
            int pose = SamusMovementRomData.Poses.Definitions + SamusPoseIds.FacingRightNormalPose * 8;
            if (address == pose + 1) return Movement;
            if (address == pose + 3) return Direction;
            if (address == pose + 4) return GraphicsY;
            if (ReplaceFlare && (address is >= 0x9bc14a and < 0x9bc172 or >= 0x9bc19a and < 0x9bc1c2))
                return (address & 1) == 0 ? (byte)0xaa : (byte)0x55;
            if (ForbidMechanics && (address is >= 0x9bc0db and < 0x9bc103 or >= 0x9bc104 and < 0x9bc118 or
                >= 0x9bc122 and < 0x9bc14a or >= 0x9bc172 and < 0x9bc19a))
                throw new InvalidOperationException($"Compiled Grapple mechanics read ROM ${address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected Grapple definition write.");
    }
}
