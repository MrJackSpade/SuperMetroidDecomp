using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    static void VerifyQuicksand()
    {
        var bus = new TestAddressSpace();
        WriteTestWord(bus, QuicksandRomData.PlmBank |
            QuicksandRomData.SurfaceInsideHeader, QuicksandRomData.SurfaceSetup);
        WriteTestWord(bus, QuicksandRomData.PlmBank |
            QuicksandRomData.SubmergingInsideHeader, QuicksandRomData.SubmergingSetup);
        WriteTestWord(bus, QuicksandRomData.PlmBank |
            QuicksandRomData.SlowFallsInsideHeader, QuicksandRomData.SlowFallsSetup);
        WriteTestWord(bus, QuicksandRomData.PlmBank |
            QuicksandRomData.FastFallsInsideHeader, QuicksandRomData.FastFallsSetup);
        WriteTestWord(bus, QuicksandRomData.PlmBank |
            QuicksandRomData.SurfaceCollisionHeader, QuicksandRomData.SurfaceCollision);
        WriteTestWord(bus, QuicksandRomData.PlmBank |
            QuicksandRomData.SubmergingCollisionHeader, QuicksandRomData.SubmergingCollision);
        WriteTestWord(bus, QuicksandRomData.PlmBank |
            QuicksandRomData.SlowFallsCollisionHeader, QuicksandRomData.SandFallsCollision);
        WriteTestWord(bus, QuicksandRomData.PlmBank |
            QuicksandRomData.FastFallsCollisionHeader, QuicksandRomData.SandFallsCollision);

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
            SamusInsideBlockReactions.PrepareFrame(bus, level, samus, AreaId.Maridia);
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
        for (int kind = 0; kind < 3; kind++)
        {
            var samus = new SamusState();
            samus.Kinematics.XPosition = samus.Kinematics.YPosition = 24;
            samus.Kinematics.YRadius = 8;
            samus.Kinematics.YSpeed = 4;
            samus.Kinematics.YSubacceleration = 0x1c00;
            var sand = Room((byte)(0x83 + kind));
            SamusInsideBlockReactions.PrepareFrame(bus, sand, samus, AreaId.Maridia);
            AssertEqual(kind == 0 ? 0x12000 : kind == 1 ? 0x14000 : 0x1c000,
                samus.Kinematics.ExtraYFixed, "submerging and sandfall forces");
            SamusBlockCollision.MoveHorizontal(bus, sand, samus.Kinematics, 1 << 16);
            AssertEqual(kind == 0 ? (ushort)0 : (ushort)4, samus.Kinematics.YSpeed, "submerging horizontal collision speed side effect");
            AssertEqual(kind == 0 ? (ushort)0 : (ushort)0x1c00, samus.Kinematics.YSubacceleration, "submerging horizontal collision gravity side effect");
        }
        Console.WriteLine("  Quicksand: suit/direction branches, sinking contact, pose clearance, and sandfall forces agree.");
    }

    private static void VerifyQuicksandDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort Word(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        ushort insideTable = Word(
            rom,
            QuicksandRomData.InsideAreaTables +
                AreaIds.ToIndex(AreaId.Maridia) * sizeof(ushort));
        ushort collisionTable = Word(
            rom,
            QuicksandRomData.CollisionAreaTables +
                AreaIds.ToIndex(AreaId.Maridia) * sizeof(ushort));
        for (byte index = 0; index < 16; index++)
        {
            AssertTrue(QuicksandDefinitions.TryGetInsideHeader(
                AreaId.Maridia, index, out ushort insideHeader),
                $"Maridia inside header {index} is compiled");
            AssertEqual(
                Word(rom, QuicksandRomData.CollisionBank |
                    unchecked((ushort)(insideTable + index * 2))),
                insideHeader,
                $"Maridia inside header {index}");

            AssertTrue(QuicksandDefinitions.TryGetCollisionHeader(
                AreaId.Maridia, index, out ushort collisionHeader),
                $"Maridia collision header {index} is compiled");
            AssertEqual(
                Word(rom, QuicksandRomData.CollisionBank |
                    unchecked((ushort)(collisionTable + index * 2))),
                collisionHeader,
                $"Maridia collision header {index}");
        }

        AssertTrue(!QuicksandDefinitions.TryGetInsideHeader(
            AreaId.Brinstar, 0, out _), "non-Maridia inside dispatch remains explicit");
        AssertTrue(!QuicksandDefinitions.TryGetCollisionHeader(
            AreaId.Maridia, 16, out _), "out-of-range collision dispatch remains explicit");

        for (int suit = 0; suit < 2; suit++)
        {
            int sourceOffset = suit * sizeof(ushort);
            QuicksandSurfacePhysics physics =
                QuicksandDefinitions.SurfacePhysics(suit != 0);
            AssertEqual(
                Word(rom, QuicksandRomData.MovingSurfaceDisplacement + sourceOffset),
                physics.MovingDisplacement,
                $"quicksand suit {suit} moving displacement");
            AssertEqual(
                Word(rom, QuicksandRomData.StationarySurfaceDisplacement + sourceOffset),
                physics.StationaryDisplacement,
                $"quicksand suit {suit} stationary displacement");
            AssertEqual(
                Word(rom, QuicksandRomData.SurfaceJumpLimit + sourceOffset),
                physics.UpwardSpeedLimit,
                $"quicksand suit {suit} upward speed limit");
        }

        var guarded = new QuicksandDefinitionReadGuard(rom);
        var level = new RoomLevelData(
            4,
            4,
            Enumerable.Repeat((ushort)0x3000, 16).ToArray(),
            Enumerable.Repeat((byte)0x80, 16).ToArray(),
            new ushort[16],
            new byte[8]);
        for (int suit = 0; suit < 2; suit++)
        for (ushort direction = 0; direction < 4; direction++)
        {
            var samus = new SamusState
            {
                EquippedItems = suit == 0
                    ? (ushort)0
                    : (ushort)SamusEquipmentFlags.GravitySuit,
            };
            SamusKinematicsState body = samus.Kinematics;
            body.XPosition = body.YPosition = 24;
            body.YRadius = 8;
            body.YDirection = direction;
            body.YSpeed = 5;
            SamusInsideBlockReactions.PrepareFrame(
                guarded, level, samus, AreaId.Maridia);
            QuicksandSurfacePhysics physics =
                QuicksandDefinitions.SurfacePhysics(suit != 0);
            AssertEqual(
                direction is 0 or 3
                    ? physics.StationaryDisplacement << 8
                    : physics.MovingDisplacement << 8,
                body.ExtraYFixed,
                $"production quicksand suit {suit} direction {direction} displacement");
        }

        for (byte index = 0; index < 16; index++)
        {
            var body = new SamusKinematicsState
            {
                SandCollisionArea = AreaId.Maridia,
                YDirection = 2,
                YSpeed = 4,
                YSubacceleration = 0x1c00,
            };
            int displacement = 0x28000;
            bool collided = SamusInsideBlockReactions.ReactCollision(
                guarded,
                body,
                new RoomCollisionBlock(0, 0x3000, unchecked((byte)(0x80 + index))),
                vertical: true,
                ref displacement,
                out bool surfaceContact);
            AssertEqual(false, collided,
                $"production quicksand collision row {index} remains non-solid");
            AssertEqual(index < 3, surfaceContact,
                $"production quicksand collision row {index} surface contact");
            AssertEqual(index == 3 ? (ushort)0 : (ushort)4, body.YSpeed,
                $"production quicksand collision row {index} speed side effect");
            AssertEqual(index == 3 ? (ushort)0 : (ushort)0x1c00,
                body.YSubacceleration,
                $"production quicksand collision row {index} gravity side effect");
        }

        Console.WriteLine(
            "Quicksand definitions: all 32 Maridia dispatch headers and six physical words match the cartridge; real inside and collision paths run with every migrated source forbidden.");
    }

    private sealed class QuicksandDefinitionReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x949259 and < 0x949279 or
                >= 0x949a86 and < 0x949aa6 or
                >= 0x84b48b and < 0x84b497 or
                >= 0x9492e1 and < 0x9492e3 or
                >= 0x949b0e and < 0x949b10
                ? throw new InvalidOperationException(
                    $"Quicksand attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) =>
            source.WriteByte(address, value);
    }
}
