using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks quicksand displacement, suit and direction branches, sinking contact, and horizontal sandfall side effects.</summary>
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

    /// <summary>Verifies compiled special-air and quicksand tables and exercises their production collision and movement paths.</summary>
    /// <param name="rom">Retail address space used to compare authored table bytes and pointers.</param>
    private static void VerifyQuicksandDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifySpecialAirInsideHeaderMapping), () => VerifySpecialAirInsideHeaderMapping(rom));
        Suite(nameof(VerifySpecialAirInsideSetupMapping), () => VerifySpecialAirInsideSetupMapping(rom));
        Suite(nameof(VerifySpecialAirCollisionHeaderMapping), () => VerifySpecialAirCollisionHeaderMapping(rom));
        Suite(nameof(VerifySpecialAirCollisionSetupMapping), () => VerifySpecialAirCollisionSetupMapping(rom));

        Suite(nameof(VerifyQuicksandSetupMapping), () => VerifyQuicksandSetupMapping(rom));
        Suite(nameof(VerifyQuicksandInstructionMapping), () => VerifyQuicksandInstructionMapping(rom));
        Suite(nameof(VerifyQuicksandMovingDisplacement), () => VerifyQuicksandMovingDisplacement(rom));
        Suite(nameof(VerifyQuicksandStationaryDisplacement), () => VerifyQuicksandStationaryDisplacement(rom));
        Suite(nameof(VerifyQuicksandUpwardLimit), () => VerifyQuicksandUpwardLimit(rom));

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
            "Special-air/quicksand definitions: all 224 retail area dispatch records, eight quicksand setup/list pairs, and six physical words match the cartridge; real inside and collision paths run with every migrated source forbidden.");
    }

    /// <summary>Checks each native quicksand PLM header's reaction setup pointer.</summary>
    /// <param name="rom">Retail address space supplying the bank-$84 instruction words.</param>
    private static void VerifyQuicksandSetupMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandReactionField), () => VerifyQuicksandReactionField(rom, false));

    /// <summary>Checks each native quicksand PLM header's reaction instruction-list pointer.</summary>
    /// <param name="rom">Retail address space supplying the bank-$84 instruction words.</param>
    private static void VerifyQuicksandInstructionMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandReactionField), () => VerifyQuicksandReactionField(rom, true));

    /// <summary>Compares all possible reaction headers with catalog ownership and checks their setup or instruction word.</summary>
    /// <param name="rom">Retail address space containing the native reaction definitions.</param>
    /// <param name="instruction">Selects instruction-list comparison when true; otherwise checks the setup pointer.</param>
    private static void VerifyQuicksandReactionField(SuperMetroidAddressSpace rom, bool instruction)
    {
        // Original non-clone sand PLM headers, independent of the replacement's named cases.
        ushort[] headers = [0xb713, 0xb71f, 0xb723, 0xb727, 0xb72b, 0xb737, 0xb73b, 0xb73f];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort header = (ushort)raw;
            bool expected = headers.Contains(header);
            bool found = QuicksandDefinitions.TryGetReaction(header, out var actual);
            AssertEqual(expected, found, "quicksand header ownership");
            if (!expected)
            {
                AssertEqual(default(QuicksandReactionDefinition), actual, "missing quicksand clears result");
                AssertThrows<ArgumentOutOfRangeException>(() => QuicksandDefinitions.ResolveReaction(header),
                    "quicksand allocator rejects unsupported headers including clone gaps");
                continue;
            }
            AssertEqual(actual, QuicksandDefinitions.ResolveReaction(header), "quicksand allocator resolution");
            ushort native = ReadBotwoonInstructionWord(rom, 0x840000 | (header + (instruction ? 2 : 0)));
            AssertEqual(native, instruction ? actual.InstructionListPointer : actual.SetupPointer,
                $"quicksand header {header:X4} instruction {instruction}");
        }
    }

    /// <summary>Compares both suit-specific moving displacement values with their bank-$84 source words.</summary>
    /// <param name="rom">Retail address space used as the reference.</param>
    private static void VerifyQuicksandMovingDisplacement(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandPhysicsField), () => VerifyQuicksandPhysicsField(rom, 0x84b48b, physics => physics.MovingDisplacement));

    /// <summary>Compares both suit-specific stationary displacement values with their bank-$84 source words.</summary>
    /// <param name="rom">Retail address space used as the reference.</param>
    private static void VerifyQuicksandStationaryDisplacement(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandPhysicsField), () => VerifyQuicksandPhysicsField(rom, 0x84b48f, physics => physics.StationaryDisplacement));

    /// <summary>Compares both suit-specific upward speed limits with their bank-$84 source words.</summary>
    /// <param name="rom">Retail address space used as the reference.</param>
    private static void VerifyQuicksandUpwardLimit(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandPhysicsField), () => VerifyQuicksandPhysicsField(rom, 0x84b493, physics => physics.UpwardSpeedLimit));

    /// <summary>Reads one native quicksand physics word per suit and compares the selected catalog value.</summary>
    /// <param name="rom">Retail address space containing the two suit records.</param>
    /// <param name="address">Address of the non-Gravity-Suit word; the Gravity Suit value follows by one word.</param>
    /// <param name="field">Selector for the displacement or speed-limit value under verification.</param>
    private static void VerifyQuicksandPhysicsField(SuperMetroidAddressSpace rom, int address,
        Func<QuicksandSurfacePhysics, ushort> field)
    {
        for (int suit = 0; suit < 2; suit++)
            AssertEqual(ReadBotwoonInstructionWord(rom, address + suit * 2),
                field(QuicksandDefinitions.SurfacePhysics(suit != 0)),
                $"quicksand physics {address:X6} suit {suit}");
    }

    /// <summary>Checks area-indexed special-air inside-reaction headers against the native pointer table.</summary>
    /// <param name="rom">Retail address space used to traverse the area and reaction tables.</param>
    private static void VerifySpecialAirInsideHeaderMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpecialAirField), () => VerifySpecialAirField(rom, SpecialAirReactionDefinitions.ResolveInside, 0x949b06, false));

    /// <summary>Checks setup pointers selected by special-air inside-reaction headers.</summary>
    /// <param name="rom">Retail address space used to traverse the area and setup tables.</param>
    private static void VerifySpecialAirInsideSetupMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpecialAirField), () => VerifySpecialAirField(rom, SpecialAirReactionDefinitions.ResolveInside, 0x949b06, true));

    /// <summary>Checks area-indexed special-air collision headers against the native pointer table.</summary>
    /// <param name="rom">Retail address space used to traverse the area and reaction tables.</param>
    private static void VerifySpecialAirCollisionHeaderMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpecialAirField), () => VerifySpecialAirField(rom, SpecialAirReactionDefinitions.ResolveCollision, 0x9492d9, false));

    /// <summary>Checks setup pointers selected by special-air collision headers.</summary>
    /// <param name="rom">Retail address space used to traverse the area and setup tables.</param>
    private static void VerifySpecialAirCollisionSetupMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpecialAirField), () => VerifySpecialAirField(rom, SpecialAirReactionDefinitions.ResolveCollision, 0x9492d9, true));

    /// <summary>Checks selector bounds, area-table routing, and either the native reaction header or its setup pointer.</summary>
    /// <param name="rom">Retail address space containing the native area pointers and reaction data.</param>
    /// <param name="resolve">Catalog lookup for the inside or collision special-air reaction table.</param>
    /// <param name="nativePointers">Bank-$94 address of the per-area pointer table.</param>
    /// <param name="setup">When true, compares the setup word referenced by the selected header.</param>
    private static void VerifySpecialAirField(SuperMetroidAddressSpace rom,
        Func<AreaId, byte, SpecialAirReactionDefinition> resolve, int nativePointers, bool setup)
    {
        for (int rawArea = 0; rawArea <= byte.MaxValue; rawArea++)
        for (int index = 0; index <= byte.MaxValue; index++)
        {
            var area = (AreaId)rawArea;
            if (rawArea >= 7 || index >= 16)
            {
                try
                {
                    resolve(area, (byte)index);
                    throw new InvalidOperationException("Special-air selector accepted an unsupported area/index.");
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    AssertEqual(rawArea >= 7 ? "area" : "areaReactionIndex", exception.ParamName!,
                        "special-air bounds and validation order");
                }
                continue;
            }

            // Follow the original area pointer and header, independently of the production cases.
            ushort table = ReadBotwoonInstructionWord(rom, nativePointers + rawArea * 2);
            ushort header = ReadBotwoonInstructionWord(rom, 0x940000 | (table + index * 2));
            ushort expected = setup ? ReadBotwoonInstructionWord(rom, 0x840000 | header) : header;
            var bts = new RoomBlockBehavior((byte)(0x80 | index));
            AssertEqual(true, bts.UsesAreaReactionTable, "special-air BTS sign selects area dispatch");
            AssertEqual((byte)index, bts.AreaReactionIndex, "special-air BTS clears its sign bit");
            SpecialAirReactionDefinition actual = resolve(area, bts.AreaReactionIndex);
            AssertEqual(expected, setup ? actual.SetupPointer : actual.HeaderPointer,
                $"special-air {nativePointers:X6} area {rawArea} index {index} setup {setup}");
        }
    }

    /// <summary>Address-space guard that rejects retail reads for every migrated quicksand and special-air definition region.</summary>
    /// <param name="source">Underlying address space used for all reads and writes outside the forbidden definition ranges.</param>
    private sealed class QuicksandDefinitionReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the guard's migrated-definition checks.</summary>
        /// <param name="address">Bus address requested by the importer.</param>
        /// <returns>The byte supplied by the guarded address-space mapping.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from migrated table ranges and forwards all other addresses to the wrapped bus.</summary>
        /// <param name="address">Bus address requested by production code.</param>
        /// <returns>The byte read from the wrapped address space when the address remains allowed.</returns>
        /// <exception cref="InvalidOperationException">Production requests a definition byte that the catalog should supply.</exception>
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

        /// <summary>Forwards writes to the wrapped address space; the guard only restricts reads.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) =>
            source.WriteByte(address, value);
    }
}
