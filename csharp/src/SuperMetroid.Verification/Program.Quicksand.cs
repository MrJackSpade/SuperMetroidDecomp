using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    static void VerifyQuicksand()
    {
        var bus = new TestAddressSpace();
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

        var distinctDefinitions = new HashSet<SpecialAirReactionDefinition>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            int areaOffset = AreaIds.ToIndex(area) * sizeof(ushort);
            ushort insideTable = Word(
                rom,
                QuicksandRomData.InsideAreaTables + areaOffset);
            ushort collisionTable = Word(
                rom,
                QuicksandRomData.CollisionAreaTables + areaOffset);
            for (byte index = 0;
                 index < SpecialAirReactionDefinitions.EntriesPerArea;
                 index++)
            {
                SpecialAirReactionDefinition inside =
                    SpecialAirReactionDefinitions.ResolveInside(area, index);
                AssertEqual(
                    Word(rom, QuicksandRomData.CollisionBank |
                        unchecked((ushort)(insideTable + index * sizeof(ushort)))),
                    inside.HeaderPointer,
                    $"{area} inside header {index}");
                AssertEqual(
                    Word(rom, QuicksandRomData.PlmBank | inside.HeaderPointer),
                    inside.SetupPointer,
                    $"{area} inside setup {index}");
                distinctDefinitions.Add(inside);

                SpecialAirReactionDefinition collision =
                    SpecialAirReactionDefinitions.ResolveCollision(area, index);
                AssertEqual(
                    Word(rom, QuicksandRomData.CollisionBank |
                        unchecked((ushort)(collisionTable + index * sizeof(ushort)))),
                    collision.HeaderPointer,
                    $"{area} collision header {index}");
                AssertEqual(
                    Word(rom, QuicksandRomData.PlmBank | collision.HeaderPointer),
                    collision.SetupPointer,
                    $"{area} collision setup {index}");
                distinctDefinitions.Add(collision);
            }
        }

        AssertEqual(22, distinctDefinitions.Count,
            "special-air dispatch distinct header/setup definition count");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SpecialAirReactionDefinitions.ResolveInside(AreaId.Maridia, 16),
            "out-of-range inside dispatch rejects adjacent code");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SpecialAirReactionDefinitions.ResolveCollision(AreaId.Maridia, 16),
            "out-of-range collision dispatch rejects adjacent code");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SpecialAirReactionDefinitions.ResolveInside((AreaId)7, 0),
            "debug-area inside dispatch is outside the retail area domain");

        AssertEqual(8, QuicksandDefinitions.Reactions.Length,
            "quicksand reaction definition count");
        foreach (QuicksandReactionDefinition definition in QuicksandDefinitions.Reactions)
        {
            AssertEqual(
                Word(rom, QuicksandRomData.PlmBank | definition.HeaderPointer),
                definition.SetupPointer,
                $"quicksand header ${definition.HeaderPointer:X4} setup");
            AssertEqual(
                Word(rom, QuicksandRomData.PlmBank |
                    unchecked((ushort)(definition.HeaderPointer + 2))),
                definition.InstructionListPointer,
                $"quicksand header ${definition.HeaderPointer:X4} initial list");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => QuicksandDefinitions.ResolveReaction(0xb62f),
            "ordinary no-op header cannot enter quicksand allocation domain");

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
            "Special-air/quicksand definitions: all 224 retail area dispatch records, 22 distinct setup identities, eight quicksand setup/list pairs, and six physical words match the cartridge; real inside and collision paths run with every migrated source forbidden.");
    }

    private sealed class QuicksandDefinitionReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x9491d9 and < 0x9492e7 or
                >= 0x949a06 and < 0x949b14 or
                >= 0x84b48b and < 0x84b497 or
                >= 0x84b713 and < 0x84b743 or
                0x84b62f or 0x84b630 or
                0x84b633 or 0x84b634 or
                0x84b653 or 0x84b654 or
                0x84b657 or 0x84b658 or
                0x84b65b or 0x84b65c or
                0x84b6cb or 0x84b6cc or
                0x84b6cf or 0x84b6d0 or
                0x84b70f or 0x84b710 or
                0x84d030 or 0x84d031 or
                0x84d034 or 0x84d035 or
                0x84d03c or 0x84d03d or
                0x84d040 or 0x84d041 or
                0x84d6da or 0x84d6db or
                0x84d6f2 or 0x84d6f3
                ? throw new InvalidOperationException(
                    $"Quicksand attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) =>
            source.WriteByte(address, value);
    }
}
