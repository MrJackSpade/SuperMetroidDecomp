using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Maridia quicksand movement and special-air dispatch against compiled cartridge definitions.</summary>
internal static class QuicksandVerification
{
    /// <summary>Checks suit- and direction-dependent sinking, surface contact, and sandfall side effects.</summary>
    internal static void VerifyQuicksand()
    {
        RoomLevelData Room(byte bts) => new RoomLevelData(new Extent16<RoomBlocks>(4, 4), new RoomLevelPlanes(Enumerable.Repeat((ushort)0x3000, 16).ToArray(), Enumerable.Repeat(bts, 16).ToArray(), new ushort[16]), new byte[8]);
        var level = Room(0x80);
        for (int suit = 0; suit < 2; suit++)
        foreach (SamusVerticalDirection direction in Enum.GetValues<SamusVerticalDirection>())
        {
            var samus = new SamusState { EquippedItems = suit == 0 ? 0 : SamusEquipmentFlags.GravitySuit };
            var body = samus.Kinematics;
            body.Position = body.Position.WithWhole(new(24, 24));
            body.Radius = body.Radius.WithY(8);
            body.YDirection = direction;
            body.YSpeed = body.YSpeed.WithWhole(5);
            samus.HorizontalSpeed.BaseSpeed = new SnesUnsignedSixteenSixteen(4, 0xffff);
            samus.HorizontalSpeed.HasRunningMomentum = true;
            SamusInsideBlockReactions.PrepareFrame(level, samus, AreaId.Maridia);
            AssertEqual(direction == SamusVerticalDirection.None ? (suit == 0 ? 0x12000 : 0x10000) : 0x20000,
                body.ExtraDisplacement.Y.RawValue, "quicksand per-direction/suit displacement");
            AssertEqual(direction == SamusVerticalDirection.None ? 0u : direction == SamusVerticalDirection.Up ? (suit == 0 ? 0x28000u : 0x38000u) : 0x50000u,
                body.VerticalSpeedFixed, "quicksand speed cancellation and jump cap");
            AssertEqual(0x7fff, samus.HorizontalSpeed.BaseSpeed.Fraction, "quicksand retains low fractional X");
            AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Whole, "quicksand clears whole base X");
            AssertEqual(false, samus.HorizontalSpeed.HasRunningMomentum, "quicksand cancels running momentum");
            var move = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level }, body, samus, new VerticalBlockMove { Displacement = 0x28000, ScanLeftToRight = true });
            AssertEqual(direction == SamusVerticalDirection.None ? 0x3000 : 0x28000, move.AcceptedDisplacement, "surface probe accepts native sinking distance");
            AssertEqual(direction != SamusVerticalDirection.Up, move.Collided, "surface publishes grounded contact separately from carry");
            body.Position = body.Position.WithWholeY(24);
            var poseProbe = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level }, body, samus, new VerticalBlockMove { Displacement = 0x28000, ScanLeftToRight = true, PublishQuicksandGrounding = false });
            AssertEqual(false, poseProbe.Collided, "pose clearance does not treat sand contact as solid carry");
        }
        for (int kind = 0; kind < 3; kind++)
        {
            var samus = new SamusState();
            samus.Kinematics.Position = samus.Kinematics.Position.WithWhole(new(24, 24));
            samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(8);
            samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithWhole(4);
            samus.Kinematics.YAcceleration = samus.Kinematics.YAcceleration.WithFraction(0x1c00);
            var sand = Room((byte)(0x83 + kind));
            SamusInsideBlockReactions.PrepareFrame(sand, samus, AreaId.Maridia);
            AssertEqual(kind == 0 ? 0x12000 : kind == 1 ? 0x14000 : 0x1c000,
                samus.Kinematics.ExtraDisplacement.Y.RawValue, "submerging and sandfall forces");
            SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = sand }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = 1 << 16 });
            AssertEqual(kind == 0 ? (ushort)0 : (ushort)4, samus.Kinematics.YSpeed.Whole, "submerging horizontal collision speed side effect");
            AssertEqual(kind == 0 ? (ushort)0 : (ushort)0x1c00, samus.Kinematics.YAcceleration.Fraction, "submerging horizontal collision gravity side effect");
        }
        Console.WriteLine("  Quicksand: suit/direction branches, sinking contact, pose clearance, and sandfall forces agree.");
    }

    /// <summary>Compares quicksand and special-air definition fields with the retail cartridge.</summary>
    internal static void VerifyQuicksandDefinitions(SuperMetroidAddressSpace rom)
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

        var guarded = NewQuicksandDefinitionReadGuard(rom);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(4, 4), new RoomLevelPlanes(Enumerable.Repeat((ushort)0x3000, 16).ToArray(), Enumerable.Repeat((byte)0x80, 16).ToArray(), new ushort[16]), new byte[8]);
        for (int suit = 0; suit < 2; suit++)
        foreach (SamusVerticalDirection direction in Enum.GetValues<SamusVerticalDirection>())
        {
            var samus = new SamusState
            {
                EquippedItems = suit == 0
                    ? 0
                    : SamusEquipmentFlags.GravitySuit,
            };
            SamusKinematicsState body = samus.Kinematics;
            body.Position = body.Position.WithWhole(new(24, 24));
            body.Radius = body.Radius.WithY(8);
            body.YDirection = direction;
            body.YSpeed = body.YSpeed.WithWhole(5);
            SamusInsideBlockReactions.PrepareFrame(
                level, samus, AreaId.Maridia);
            QuicksandSurfacePhysics physics =
                QuicksandDefinitions.SurfacePhysics(suit != 0);
            AssertEqual(
                direction == SamusVerticalDirection.None
                    ? physics.StationaryDisplacement
                    : physics.MovingDisplacement,
                body.ExtraDisplacement.Y,
                $"production quicksand suit {suit} direction {(int)direction} displacement");
            if (direction == SamusVerticalDirection.Up)
            {
                // A whole upward speed of five pixels exceeds both caps, so the speed becomes the limit exactly.
                AssertEqual(physics.UpwardSpeedLimit,
                    SnesSignedSixteenSixteen.FromParts(unchecked((short)body.YSpeed.Whole), body.YSpeed.Fraction),
                    $"production quicksand suit {suit} clamps upward speed to its limit");
            }
        }
        VerifyQuicksandUpwardLimitComparesMiddleWords(level);

        for (byte index = 0; index < 16; index++)
        {
            var body = new SamusKinematicsState
            {
                SandCollisionArea = AreaId.Maridia,
                YDirection = SamusVerticalDirection.Down,
                YSpeed = new SnesUnsignedSixteenSixteen(4, 0),
                YAcceleration = new SnesUnsignedSixteenSixteen(0, 0x1c00),
            };
            int displacement = 0x28000;
            bool collided = SamusInsideBlockReactions.ReactCollision(
                new(body, body.CollisionContactDamageIndex(null)),
                new RoomCollisionBlock(0, 0x3000, new RoomBlockBehavior(0x80 + index)),
                axis: Axis.Vertical,
                ref displacement,
                out bool surfaceContact);
            AssertEqual(false, collided,
                $"production quicksand collision row {index} remains non-solid");
            AssertEqual(index < 3, surfaceContact,
                $"production quicksand collision row {index} surface contact");
            AssertEqual(index == 3 ? (ushort)0 : (ushort)4, body.YSpeed.Whole,
                $"production quicksand collision row {index} speed side effect");
            AssertEqual(index == 3 ? (ushort)0 : (ushort)0x1c00,
                body.YAcceleration.Fraction,
                $"production quicksand collision row {index} gravity side effect");
        }

        Console.WriteLine(
            "Special-air/quicksand definitions: all 224 retail area dispatch records, eight quicksand setup/list pairs, and six physical words match the cartridge; real inside and collision paths run with every migrated source forbidden.");
    }

    /// <summary>Checks each quicksand PLM header's setup pointer.</summary>
    private static void VerifyQuicksandSetupMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandReactionField), () => VerifyQuicksandReactionField(rom, false));

    /// <summary>Checks each quicksand PLM header's instruction-list pointer.</summary>
    private static void VerifyQuicksandInstructionMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandReactionField), () => VerifyQuicksandReactionField(rom, true));

    /// <summary>Checks the supported sand headers and rejects every other 16-bit PLM header.</summary>
    private static void VerifyQuicksandReactionField(SuperMetroidAddressSpace rom, bool instruction)
    {
        // Original non-clone sand PLM headers, independent of the replacement's named cases.
        ushort[] headers = [0xb713, 0xb71f, 0xb723, 0xb727, 0xb72b, 0xb737, 0xb73b, 0xb73f];
        // PlmHeaderId's values are arbitrary (#1414): the domain is every member, each translated to its cartridge word.
        foreach (PlmHeaderId candidate in Enum.GetValues<PlmHeaderId>())
        {
            ushort header = PlmNativeWords.Word(candidate);
            bool expected = headers.Contains(header);
            bool found = QuicksandDefinitions.TryGetReaction(candidate, out var actual);
            AssertEqual(expected, found, "quicksand header ownership");
            if (!expected)
            {
                AssertEqual(default(QuicksandReactionDefinition), actual, "missing quicksand clears result");
                AssertThrows<ArgumentOutOfRangeException>(() => QuicksandDefinitions.ResolveReaction(candidate),
                    "quicksand allocator rejects unsupported headers including clone gaps");
                continue;
            }
            AssertEqual(actual, QuicksandDefinitions.ResolveReaction(candidate), "quicksand allocator resolution");
            ushort native = ReadRomWord(rom, 0x840000 | (header + (instruction ? 2 : 0)));
            if (instruction)
                AssertEqual(PlmProgramStartOracle.ForNativeList(native), actual.Program, $"quicksand header {(int)header:X4} program");
            else
                AssertEqual(native, FxNativeWords.SpecialAirReactionSetups.Word(actual.Setup), $"quicksand header {(int)header:X4} setup");
        }
    }

    /// <summary>Checks the two native suit-dependent moving displacement words.</summary>
    private static void VerifyQuicksandMovingDisplacement(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandPhysicsField), () => VerifyQuicksandPhysicsField(rom, 0x84b48b, physics => physics.MovingDisplacement));

    /// <summary>Checks the two native suit-dependent stationary sinking words.</summary>
    private static void VerifyQuicksandStationaryDisplacement(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandPhysicsField), () => VerifyQuicksandPhysicsField(rom, 0x84b48f, physics => physics.StationaryDisplacement));

    /// <summary>Checks the two native quicksand upward-speed limits.</summary>
    private static void VerifyQuicksandUpwardLimit(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyQuicksandPhysicsField), () => VerifyQuicksandPhysicsField(rom, 0x84b493, physics => physics.UpwardSpeedLimit));

    /// <summary>
    /// Compares a selected surface-physics field for both Gravity and ordinary suits. The cartridge stores each as
    /// an unsigned 8.8 word; the oracle widens it to 16.16 exactly as the surface reaction consumes it.
    /// </summary>
    private static void VerifyQuicksandPhysicsField(SuperMetroidAddressSpace rom, int address,
        Func<QuicksandSurfacePhysics, SnesSignedSixteenSixteen> field)
    {
        for (int suit = 0; suit < 2; suit++)
            AssertEqual(SnesSignedSixteenSixteen.FromRaw(SnesSixteenSixteen.FromEightEight(ReadRomWord(rom, address + suit * 2))),
                field(QuicksandDefinitions.SurfacePhysics(suit != 0)),
                $"quicksand physics {address:X6} suit {suit}");
    }

    /// <summary>
    /// The upward cap compares only the middle words of the 16.16 speed and limit: a speed above the 2.5-pixel
    /// limit by less than one 256th of a pixel shares the limit's middle word and is left unclamped.
    /// </summary>
    private static void VerifyQuicksandUpwardLimitComparesMiddleWords(RoomLevelData level)
    {
        var samus = new SamusState();
        SamusKinematicsState body = samus.Kinematics;
        body.Position = body.Position.WithWhole(new(24, 24));
        body.Radius = body.Radius.WithY(8);
        body.YDirection = SamusVerticalDirection.Up;
        SnesSignedSixteenSixteen limit = QuicksandDefinitions.SurfacePhysics(gravitySuit: false).UpwardSpeedLimit;
        // 255/65536 of a pixel above the limit: below the resolution of the middle-word comparison.
        SnesSignedSixteenSixteen justAbove = limit + SnesSignedSixteenSixteen.FromParts(wholePixels: 0, fraction: 255);
        body.YSpeed = new SnesUnsignedSixteenSixteen(justAbove.WholeWord, justAbove.Fraction);
        SamusInsideBlockReactions.PrepareFrame(level, samus, AreaId.Maridia);
        AssertEqual(justAbove, SnesSignedSixteenSixteen.FromParts(unchecked((short)body.YSpeed.Whole), body.YSpeed.Fraction),
            "quicksand upward cap ignores the speed's lowest fraction byte");
    }

    /// <summary>Checks the special-air inside-reaction header for each area and BTS index.</summary>
    private static void VerifySpecialAirInsideHeaderMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpecialAirField), () => VerifySpecialAirField(rom, SpecialAirReactionDefinitions.ResolveInside, 0x949b06, false));

    /// <summary>Checks the setup pointer selected by each special-air inside reaction.</summary>
    private static void VerifySpecialAirInsideSetupMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpecialAirField), () => VerifySpecialAirField(rom, SpecialAirReactionDefinitions.ResolveInside, 0x949b06, true));

    /// <summary>Checks the special-air collision header for each area and BTS index.</summary>
    private static void VerifySpecialAirCollisionHeaderMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpecialAirField), () => VerifySpecialAirField(rom, SpecialAirReactionDefinitions.ResolveCollision, 0x9492d9, false));

    /// <summary>Checks the setup pointer selected by each special-air collision reaction.</summary>
    private static void VerifySpecialAirCollisionSetupMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySpecialAirField), () => VerifySpecialAirField(rom, SpecialAirReactionDefinitions.ResolveCollision, 0x9492d9, true));

    /// <summary>Follows native area tables while checking valid reactions and out-of-range selector behavior.</summary>
    private static void VerifySpecialAirField(SuperMetroidAddressSpace rom,
        Func<AreaId, int, SpecialAirReactionDefinition> resolve, int nativePointers, bool setup)
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
                    AssertEqual(rawArea >= 7 ? "area" : "areaReactionIndex", exception.ParamName,
                        "special-air bounds and validation order");
                }
                continue;
            }

            // Follow the original area pointer and header, independently of the production cases.
            ushort table = ReadRomWord(rom, nativePointers + rawArea * 2);
            ushort header = ReadRomWord(rom, 0x940000 | (table + index * 2));
            ushort expected = setup ? ReadRomWord(rom, 0x840000 | header) : header;
            var bts = new RoomBlockBehavior((byte)(0x80 | index));
            AssertEqual(true, bts.UsesAreaReactionTable, "special-air BTS sign selects area dispatch");
            AssertEqual((byte)index, bts.AreaReactionIndex, "special-air BTS clears its sign bit");
            SpecialAirReactionDefinition actual = resolve(area, bts.AreaReactionIndex);
            AssertEqual(expected, setup ? FxNativeWords.SpecialAirReactionSetups.Word(actual.Setup) : PlmNativeWords.Word(actual.Header),
                $"special-air {nativePointers:X6} area {rawArea} index {index} setup {setup}");
        }
    }

    /// <summary>Rejects production reads of the migrated special-air, quicksand, and PLM definition bytes.</summary>
    private static CartridgeReadGuard NewQuicksandDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x9491d9 and < 0x9492e7 or
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
                0x84d6f2 or 0x84d6f3,
            address => $"Quicksand attempted migrated definition read ${address:X6}.");

    /// <summary>Runs both the quicksand definition and movement verification suites.</summary>
    [VerificationCommand("--quicksand-definitions")]
    internal static int QuicksandDefinitionsCommand()
    {
        Suite(nameof(VerifyQuicksandDefinitions), () => VerifyQuicksandDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyQuicksand), () => VerifyQuicksand());
        return 0;
    }
}
