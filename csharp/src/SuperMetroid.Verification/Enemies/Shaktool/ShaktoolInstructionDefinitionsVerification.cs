using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Shaktool orientation and collision selectors alongside compiled segment instruction programs.</summary>
internal static class ShaktoolInstructionDefinitionsVerification
{
    /// <summary>Compares orientation and collision lists with the cartridge and checks live Shaktool handoffs.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Shaktool instruction definitions")]
    internal static void VerifyShaktoolInstructionDefinitions(SuperMetroidAddressSpace rom)
    {

        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaadd15 + (bucket >> 5) * 2)),
                NativeEnemyProgramOracle.EntryOf(ShaktoolNativeListAddresses.Entries, ShaktoolInstructionDefinitions.ForOrientationBucket(bucket)),
                $"Shaktool orientation list {bucket:X2}");
        }
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaadf13 + index * 2)),
                NativeEnemyProgramOracle.EntryOf(ShaktoolNativeListAddresses.Entries, ShaktoolInstructionDefinitions.CollisionForSegment(index)),
                $"Shaktool collision list {index}");
        }

        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(rom);
        RoomEnemySlot center = enemies.SlotTable.Slots[3];
        ShaktoolSegmentState centerState = (enemies.ShaktoolSegments[3] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[3]."));
        ShaktoolSegmentState nextState = (enemies.ShaktoolSegments[4] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[4]."));
        Action<RoomEnemySlot, ShaktoolSegmentState> orient = enemies.ShaktoolBehavior.AdvanceAndOrientShaktoolCenter;
        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            ushort midpoint = unchecked((ushort)(bucket << 8));
            center.Parameter1 = 0;
            (centerState ?? throw new InvalidOperationException("Verification requires centerState.")).OrbitAngle = unchecked((ushort)(midpoint ^ 0x8000));
            centerState.AngularVelocity = 0;
            centerState.OrientationAndAcceleration = 0;
            (nextState ?? throw new InvalidOperationException("Verification requires nextState.")).OrbitAngle = midpoint;
            orient(center, centerState);
            AssertEqual(bucket, unchecked((ushort)(
                    SnesLittleEndian.LowByte(centerState.OrientationAndAcceleration))),
                $"Shaktool production orientation bucket {bucket:X2}");
            AssertEqual(AnimationProgramCursor<ShaktoolProgram>.Start(ShaktoolInstructionDefinitions.ForOrientationBucket(bucket)),
                centerState.Program,
                $"Shaktool production orientation list {bucket:X2}");
        }

        RoomEnemySlot tail = enemies.SlotTable.Slots[6];
        ShaktoolSegmentState tailState = (enemies.ShaktoolSegments[6] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[6]."));
        Action<RoomEnemySlot, ShaktoolSegmentState, Position16<RoomPixels>> reverse = enemies.ShaktoolBehavior.ReverseShaktoolAfterCollision;
        reverse(tail, (tailState ?? throw new InvalidOperationException("Verification requires tailState.")), tail.Position.Whole);
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(AnimationProgramCursor<ShaktoolProgram>.Start(ShaktoolInstructionDefinitions.CollisionForSegment(index)),
                (enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException("Verification requires a Shaktool segment.")).Program,
                $"Shaktool production collision list {index}");
        }

        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.ForOrientationBucket(1),
            "unaligned Shaktool orientation bucket");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.ForOrientationBucket(0x0100),
            "Shaktool orientation bucket past table");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.CollisionForSegment(-1),
            "negative Shaktool collision segment");
        Console.WriteLine(
            "Shaktool instruction definitions: 22 native selectors and all orientation and collision-reversal production handoffs pass with source tables forbidden.");
    }

    /// <summary>Loads the retail cartridge and runs Shaktool instruction program checks.</summary>
    internal static void VerifyShaktoolInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs every steady and collision program on the real seven-segment group; the program oracle matches bank $AA.</summary>
    internal static void VerifyShaktoolInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(rom);
        ShaktoolProgram[] steadyPrograms =
        [
            ShaktoolProgram.SawHandPrimaryPiece,
            ShaktoolProgram.SawHandFinalPiece,
            ShaktoolProgram.ArmPieceNormal,
            ShaktoolProgram.HeadAimingLeft,
            ShaktoolProgram.HeadAimingUpLeft,
            ShaktoolProgram.HeadAimingUp,
            ShaktoolProgram.HeadAimingUpRight,
            ShaktoolProgram.HeadAimingRight,
            ShaktoolProgram.HeadAimingDownRight,
            ShaktoolProgram.HeadAimingDown,
            ShaktoolProgram.HeadAimingDownLeft,
        ];
        for (int index = 0; index < steadyPrograms.Length; index++)
        {
            int slotIndex = Math.Min(index, 6);
            (enemies.ShaktoolSegments[slotIndex] ?? throw new InvalidOperationException("Verification requires a Shaktool segment.")).Program =
                AnimationProgramCursor<ShaktoolProgram>.Start(steadyPrograms[index]);
            RunForcedShaktoolInstructions(enemies, enemies.SlotTable.Slots[slotIndex], 5);
        }

        RoomEnemySlot tail = enemies.SlotTable.Slots[6];
        ShaktoolSegmentState tailState = enemies.ShaktoolSegments[6] ?? throw new InvalidOperationException("Verification requires enemies.ShaktoolSegments[6].");
        enemies.ShaktoolBehavior.ReverseShaktoolAfterCollision(tail, tailState, tail.Position.Whole);
        for (int index = 0; index < 7; index++)
            RunForcedShaktoolInstructions(enemies, enemies.SlotTable.Slots[index], 8);

        Suite(nameof(ShaktoolAnimationsVerification.VerifyShaktoolAnimations), () => ShaktoolAnimationsVerification.VerifyShaktoolAnimations(rom));
        Console.WriteLine("Shaktool: every steady and collision program runs on the real seven-segment group.");
    }

    /// <summary>Forces successive instruction steps and reports whether they moved the segment.</summary>
    private static bool RunForcedShaktoolInstructions(
        RoomEnemySystem enemies,
        RoomEnemySlot segment,
        int steps)
    {
        bool moved = false;
        object?[] arguments =
            [segment, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
        for (int step = 0; step < steps; step++)
        {
            (ushort X, ushort Y) before = (segment.Position.X.Whole, segment.Position.Y.Whole);
            segment.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
            moved |= segment.Position.X.Whole != before.X || segment.Position.Y.Whole != before.Y;
        }
        return moved;
    }

    /// <summary>Creates and initializes all seven Shaktool segments with the selector tables forbidden.</summary>
    private static RoomEnemySystem CreateCompiledShaktoolGroup(
        ISnesAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(new CartridgeReadGuard(rom).Forbid(address => address is >= 0xaadd15 and < 0xaadd25 or >= 0xaadf13 and < 0xaadf2f,
            address => $"Shaktool attempted migrated animation-selector read ${address:X6}."));
        Action<RoomEnemySlot> initialize = enemies.ShaktoolBehavior.InitializeShaktool;
        for (int index = 0; index < 7; index++)
        {
            RoomEnemySlot slot = enemies.SlotTable.Slots[index];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Shaktool;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xaa };
            slot.Parameter2 = unchecked((ushort)(index * 2));
            slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
            initialize(slot);
        }
        return enemies;
    }

    /// <summary>Runs the Shaktool orientation and collision animation-selection check.</summary>
    [VerificationCommand("--shaktool-instruction-definitions")]
    internal static int ShaktoolInstructionDefinitionsCommand()
    {
        Suite(nameof(VerifyShaktoolInstructionDefinitions), () => VerifyShaktoolInstructionDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the standalone Shaktool instruction mechanics verification command.</summary>
    [VerificationCommand("--shaktool-instruction-mechanics")]
    internal static int ShaktoolInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions());
        return 0;
    }
}
