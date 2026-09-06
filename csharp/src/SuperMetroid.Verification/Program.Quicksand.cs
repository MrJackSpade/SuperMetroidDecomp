using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    static void VerifyQuicksand()
    {
        var bus = new TestAddressSpace();
        WriteTestWord(bus, QuicksandRomData.InsideAreaTables + 8, 0xa000);
        WriteTestWord(bus, QuicksandRomData.CollisionAreaTables + 8, 0xa100);
        ushort[] inside = [QuicksandRomData.SurfaceSetup, QuicksandRomData.SubmergingSetup,
            QuicksandRomData.SlowFallsSetup, QuicksandRomData.FastFallsSetup];
        ushort[] collision = [QuicksandRomData.SurfaceCollision, QuicksandRomData.SubmergingCollision, 0xb54f, 0xb54f];
        for (int i = 0; i < inside.Length; i++)
        {
            WriteTestWord(bus, 0x94a000 + i * 2, (ushort)(0xc000 + i * 4));
            WriteTestWord(bus, 0x94a100 + i * 2, (ushort)(0xc002 + i * 4));
            WriteTestWord(bus, 0x84c000 + i * 4, inside[i]);
            WriteTestWord(bus, 0x84c002 + i * 4, collision[i]);
        }
        WriteTestWords(bus, QuicksandRomData.StationarySurfaceDisplacement, 0x120, 0x100);
        WriteTestWords(bus, QuicksandRomData.MovingSurfaceDisplacement, 0x200, 0x200);
        WriteTestWords(bus, QuicksandRomData.SurfaceJumpLimit, 0x280, 0x380);

        RoomLevelData Room(byte bts) => new(4, 4,
            Enumerable.Repeat((ushort)0x3000, 16).ToArray(), Enumerable.Repeat(bts, 16).ToArray(),
            new ushort[16], new byte[8]);
        var level = Room(0x80);
        for (int suit = 0; suit < 2; suit++)
        for (ushort direction = 0; direction < 4; direction++)
        {
            var samus = new SamusState { EquippedItems = suit == 0 ? (ushort)0 : (ushort)SamusEquipmentFlags.GravitySuit };
            var body = samus.Kinematics;
            body.XPosition = body.YPosition = 24;
            body.YRadius = 8;
            body.YDirection = direction;
            body.YSpeed = 5;
            samus.HorizontalSpeed.BaseSpeed = 4;
            samus.HorizontalSpeed.BaseSubspeed = 0xffff;
            samus.HorizontalSpeed.HasRunningMomentum = true;
            SamusQuicksandPhysics.PrepareFrame(bus, level, samus, AreaId.Maridia);
            AssertEqual(direction is 0 or 3 ? (suit == 0 ? 0x12000 : 0x10000) : 0x20000,
                body.ExtraYFixed, "quicksand per-direction/suit displacement");
            AssertEqual(direction is 0 or 3 ? 0u : direction == 1 ? (suit == 0 ? 0x28000u : 0x38000u) : 0x50000u,
                body.VerticalSpeedFixed, "quicksand speed cancellation and jump cap");
            AssertEqual((ushort)0x7fff, samus.HorizontalSpeed.BaseSubspeed, "quicksand retains low fractional X");
            AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, "quicksand clears whole base X");
            AssertEqual(false, samus.HorizontalSpeed.HasRunningMomentum, "quicksand cancels running momentum");
            var move = SamusBlockCollision.MoveVertical(bus, level, body, 0x28000, true);
            AssertEqual(direction is 0 or 3 ? 0x3000 : 0x28000, move.AcceptedDisplacement, "surface probe accepts native sinking distance");
            AssertEqual(direction != 1, move.Collided, "surface publishes grounded contact separately from carry");
            body.YPosition = 24;
            var poseProbe = SamusBlockCollision.MoveVertical(bus, level, body, 0x28000, true, publishQuicksandGrounding: false);
            AssertEqual(false, poseProbe.Collided, "pose clearance does not treat sand contact as solid carry");
        }
        for (int kind = 1; kind < 4; kind++)
        {
            var samus = new SamusState();
            samus.Kinematics.XPosition = samus.Kinematics.YPosition = 24;
            samus.Kinematics.YRadius = 8;
            samus.Kinematics.YSpeed = 4;
            samus.Kinematics.YSubacceleration = 0x1c00;
            var sand = Room((byte)(0x80 + kind));
            SamusQuicksandPhysics.PrepareFrame(bus, sand, samus, AreaId.Maridia);
            AssertEqual(kind == 1 ? 0x12000 : kind == 2 ? 0x14000 : 0x1c000,
                samus.Kinematics.ExtraYFixed, "submerging and sandfall forces");
            SamusBlockCollision.MoveHorizontal(bus, sand, samus.Kinematics, 1 << 16);
            AssertEqual(kind == 1 ? (ushort)0 : (ushort)4, samus.Kinematics.YSpeed, "submerging horizontal collision speed side effect");
            AssertEqual(kind == 1 ? (ushort)0 : (ushort)0x1c00, samus.Kinematics.YSubacceleration, "submerging horizontal collision gravity side effect");
        }
        Console.WriteLine("  Quicksand: suit/direction branches, sinking contact, pose clearance, and sandfall forces agree.");
    }
}
