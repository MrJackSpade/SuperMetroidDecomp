using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static CompiledEnemyVisualSelectorAssertionsVerification;
using static SuiteTimingVerification;

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
                ShaktoolInstructionDefinitions.ForOrientationBucket(bucket),
                $"Shaktool orientation list {bucket:X2}");
        }
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaadf13 + index * 2)),
                ShaktoolInstructionDefinitions.CollisionForSegment(index),
                $"Shaktool collision list {index}");
        }

        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(rom);
        RoomEnemySlot center = enemies.SlotTable.Slots[3];
        ShaktoolSegmentState centerState = enemies.ShaktoolSegments[3]!;
        ShaktoolSegmentState nextState = enemies.ShaktoolSegments[4]!;
        Action<RoomEnemySlot, ShaktoolSegmentState> orient = enemies.ShaktoolBehavior.AdvanceAndOrientShaktoolCenter;
        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            ushort midpoint = unchecked((ushort)(bucket << 8));
            center.Parameter1 = 0;
            centerState.OrbitAngle = unchecked((ushort)(midpoint ^ 0x8000));
            centerState.AngularVelocity = 0;
            centerState.OrientationAndAcceleration = 0;
            nextState.OrbitAngle = midpoint;
            orient(center, centerState);
            AssertEqual(bucket, unchecked((ushort)(
                    SnesLittleEndian.LowByte(centerState.OrientationAndAcceleration))),
                $"Shaktool production orientation bucket {bucket:X2}");
            AssertEqual(ShaktoolInstructionDefinitions.ForOrientationBucket(bucket),
                center.CurrentInstruction,
                $"Shaktool production orientation list {bucket:X2}");
        }

        RoomEnemySlot tail = enemies.SlotTable.Slots[6];
        ShaktoolSegmentState tailState = enemies.ShaktoolSegments[6]!;
        Action<RoomEnemySlot, ShaktoolSegmentState, Position16<RoomPixels>> reverse = enemies.ShaktoolBehavior.ReverseShaktoolAfterCollision;
        reverse(tail, tailState, tail.Position.Whole);
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(ShaktoolInstructionDefinitions.CollisionForSegment(index),
                enemies.SlotTable.Slots[index].CurrentInstruction,
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
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises steady and collision programs while rejecting compiled mechanics reads.</summary>
    internal static void VerifyShaktoolInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {

        for (int index = 0;
             index < ShaktoolInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ShaktoolInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaa0000 | definition.Address)),
                $"Shaktool instruction mechanics word $AA:{definition.Address:X4}");
        }

        var guard = NewShaktoolInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(guard);

        ushort[] steadyPrograms =
        [
            ShaktoolInstructionProgramDefinitions.SawHandPrimaryPiece,
            ShaktoolInstructionProgramDefinitions.SawHandFinalPiece,
            ShaktoolInstructionProgramDefinitions.ArmPieceNormal,
            ShaktoolInstructionProgramDefinitions.HeadAimingLeft,
            ShaktoolInstructionProgramDefinitions.HeadAimingUpLeft,
            ShaktoolInstructionProgramDefinitions.HeadAimingUp,
            ShaktoolInstructionProgramDefinitions.HeadAimingUpRight,
            ShaktoolInstructionProgramDefinitions.HeadAimingRight,
            ShaktoolInstructionProgramDefinitions.HeadAimingDownRight,
            ShaktoolInstructionProgramDefinitions.HeadAimingDown,
            ShaktoolInstructionProgramDefinitions.HeadAimingDownLeft,
        ];
        for (int index = 0; index < steadyPrograms.Length; index++)
        {
            RoomEnemySlot segment = enemies.SlotTable.Slots[Math.Min(index, 6)];
            segment.CurrentInstruction = steadyPrograms[index];
            RunForcedShaktoolInstructions(enemies, segment, 5);
        }

        RoomEnemySlot tail = enemies.SlotTable.Slots[6];
        ShaktoolSegmentState tailState = enemies.ShaktoolSegments[6]!;
        Action<RoomEnemySlot, ShaktoolSegmentState, Position16<RoomPixels>> reverse = enemies.ShaktoolBehavior.ReverseShaktoolAfterCollision;
        reverse(tail, tailState, tail.Position.Whole);
        for (int index = 0; index < 7; index++)
            RunForcedShaktoolInstructions(enemies, enemies.SlotTable.Slots[index], 8);

        AssertEqual(0, guard.ObservedWords.Count,
            "Shaktool execution uses compiled spritemap selectors");
        for (int index = 0;
             index < ShaktoolInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                ShaktoolInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionId.Shaktool,
                0xaa, address, $"Shaktool $AA:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => ShaktoolInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Shaktool spritemap $AA:{address:X4} is rejected as mechanics");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Shaktool mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionProgramDefinitions.ReadMechanicsWord(
                ShaktoolInstructionProgramDefinitions.FirstAdjacentCodeRoutine),
            "adjacent Shaktool code routine is rejected as instruction mechanics");

        _ = ProbeShaktoolInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeShaktoolInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Shaktool allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Shaktool mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Shaktool instruction mechanics: 110 compiled words, all 21 reachable " +
            "programs and 15 compiled spritemap selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>Forces successive instruction steps and reports whether they moved the segment.</summary>
    private static bool RunForcedShaktoolInstructions(
        RoomEnemySystem enemies,
        RoomEnemySlot segment,
        int steps)
    {
        bool moved = false;
        object?[] arguments =
            [segment, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int step = 0; step < steps; step++)
        {
            (ushort X, ushort Y) before = (segment.Position.X.Whole, segment.Position.Y.Whole);
            segment.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, arguments);
            moved |= segment.Position.X.Whole != before.X || segment.Position.Y.Whole != before.Y;
        }
        return moved;
    }

    /// <summary>Repeatedly resolves a compiled aiming program to detect warmed lookup allocations.</summary>
    private static int ProbeShaktoolInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ShaktoolInstructionProgramDefinitions.ReadMechanicsWord(
                ShaktoolInstructionProgramDefinitions.HeadAimingDown);
        }
        return checksum;
    }

    /// <summary>Creates and initializes all seven Shaktool segments behind an instruction read guard.</summary>
    private static RoomEnemySystem CreateCompiledShaktoolGroup(
        ISnesAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewShaktoolInstructionReadGuard(rom));
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

    /// <summary>Rejects migrated mechanics and selector-table reads while recording presentation operands.</summary>
    private static CartridgeReadGuard NewShaktoolInstructionReadGuard(ISnesAddressSpace source)
    {
        var guard = new CartridgeReadGuard(source).Forbid(ShaktoolInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            address => $"Production read compiled Shaktool mechanics byte ${address:X6}.")
            .Forbid(address => address is >= 0xaadd15 and < 0xaadd25 or >= 0xaadf13 and < 0xaadf2f,
                address => $"Shaktool attempted migrated instruction-selector read ${address:X6}.");
        // Every table word whose bytes are read is recorded, including repeated table entries.
        return guard.Observe(address => (SnesAddressMath.BankBase(address)) == 0xaa0000, address =>
        {
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0; index < ShaktoolInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
            {
                ushort presentation = ShaktoolInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                if (bankAddress == presentation || bankAddress == unchecked((ushort)(presentation + 1)))
                    guard.ObservedWords.Add(presentation);
            }
        });
    }

    /// <summary>Runs the standalone Shaktool instruction mechanics verification command.</summary>
    [VerificationCommand("--shaktool-instruction-mechanics")]
    internal static int ShaktoolInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions());
        return 0;
    }
}
