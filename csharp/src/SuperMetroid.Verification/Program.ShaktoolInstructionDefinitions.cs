using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            AssertEqual(Word(0xaadd15 + (bucket >> 5) * 2),
                ShaktoolInstructionDefinitions.ForOrientationBucket(bucket),
                $"Shaktool orientation list {bucket:X2}");
        }
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(Word(0xaadf13 + index * 2),
                ShaktoolInstructionDefinitions.CollisionForSegment(index),
                $"Shaktool collision list {index}");
            AssertEqual(Word(0xaadf21 + index * 2),
                ShaktoolInstructionDefinitions.AttackForSegment(index),
                $"Shaktool attack list {index}");
        }

        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(rom, flags);
        RoomEnemySlot center = enemies.Slots[3];
        ShaktoolSegmentState centerState = enemies.ShaktoolSegments[3]!;
        ShaktoolSegmentState nextState = enemies.ShaktoolSegments[4]!;
        var orient = typeof(RoomEnemySystem).GetMethod(
                "AdvanceAndOrientShaktoolCenter", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState>>(enemies);
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
                    centerState.OrientationAndAcceleration & 0x00ff)),
                $"Shaktool production orientation bucket {bucket:X2}");
            AssertEqual(ShaktoolInstructionDefinitions.ForOrientationBucket(bucket),
                center.CurrentInstruction,
                $"Shaktool production orientation list {bucket:X2}");
        }

        RoomEnemySlot tail = enemies.Slots[6];
        ShaktoolSegmentState tailState = enemies.ShaktoolSegments[6]!;
        var reverse = typeof(RoomEnemySystem).GetMethod(
                "ReverseShaktoolAfterCollision", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState, ushort, ushort>>(
                enemies);
        reverse(tail, tailState, tail.XPosition, tail.YPosition);
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(ShaktoolInstructionDefinitions.CollisionForSegment(index),
                enemies.Slots[index].CurrentInstruction,
                $"Shaktool production collision list {index}");
        }

        enemies.StartUnusedShaktoolAttack(enemies.Slots[0]);
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(ShaktoolInstructionDefinitions.AttackForSegment(index),
                enemies.Slots[index].CurrentInstruction,
                $"Shaktool production dormant attack list {index}");
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
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.AttackForSegment(7),
            "Shaktool attack segment past table");
        Console.WriteLine(
            "Shaktool instruction definitions: 22 native selectors and all orientation, collision-reversal and dormant-attack production handoffs pass with source tables forbidden.");
    }

    private static void VerifyShaktoolInstructionProgramDefinitions()
    {
        VerifyShaktoolInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyShaktoolInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < ShaktoolInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ShaktoolInstructionMechanicsWord definition =
                ShaktoolInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadShaktoolInstructionWord(rom, 0xaa0000 | definition.Address),
                $"Shaktool instruction mechanics word $AA:{definition.Address:X4}");
        }

        var guard = new ShaktoolInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(guard, flags);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

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
            RoomEnemySlot segment = enemies.Slots[Math.Min(index, 6)];
            segment.CurrentInstruction = steadyPrograms[index];
            RunForcedShaktoolInstructions(process, enemies, segment, 5);
        }

        RoomEnemySlot tail = enemies.Slots[6];
        ShaktoolSegmentState tailState = enemies.ShaktoolSegments[6]!;
        var reverse = typeof(RoomEnemySystem).GetMethod(
                "ReverseShaktoolAfterCollision", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState, ushort, ushort>>(
                enemies);
        reverse(tail, tailState, tail.XPosition, tail.YPosition);
        for (int index = 0; index < 7; index++)
            RunForcedShaktoolInstructions(process, enemies, enemies.Slots[index], 8);

        enemies.StartUnusedShaktoolAttack(enemies.Slots[0]);
        bool observedAttackMovement = false;
        for (int index = 0; index < 7; index++)
        {
            observedAttackMovement |= RunForcedShaktoolInstructions(
                process,
                enemies,
                enemies.Slots[index],
                14);
        }
        AssertTrue(observedAttackMovement,
            "Shaktool dormant attack programs execute their movement callbacks");
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(ShaktoolSegmentDefinitions.ForIndex(index).PreInstruction,
                enemies.ShaktoolSegments[index]!.PreInstruction,
                $"Shaktool dormant attack restores segment {index} pre-instruction");
        }

        AssertEqual(ShaktoolInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Shaktool spritemap words remain cartridge reads");
        for (int index = 0;
             index < ShaktoolInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                ShaktoolInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Shaktool presentation word $AA:{address:X4}");
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
            "programs and 15 live spritemap reads pass with mechanics bytes forbidden.");
    }

    private static bool RunForcedShaktoolInstructions(
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot segment,
        int steps)
    {
        bool moved = false;
        object?[] arguments =
            [segment, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int step = 0; step < steps; step++)
        {
            (ushort X, ushort Y) before = (segment.XPosition, segment.YPosition);
            segment.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            moved |= segment.XPosition != before.X || segment.YPosition != before.Y;
        }
        return moved;
    }

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

    private static ushort ReadShaktoolInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static RoomEnemySystem CreateCompiledShaktoolGroup(
        ISnesAddressSpace rom,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new ShaktoolInstructionReadGuard(rom));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeShaktool", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        for (int index = 0; index < 7; index++)
        {
            RoomEnemySlot slot = enemies.Slots[index];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ShaktoolDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xaa };
            slot.Parameter2 = unchecked((ushort)(index * 2));
            slot.XPosition = unchecked((ushort)(0x0100 + index * 16));
            slot.YPosition = 0x0200;
            initialize(slot);
        }
        return enemies;
    }

    private sealed class ShaktoolInstructionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ShaktoolInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Shaktool mechanics byte ${address:X6}.");
            }
            if (address is >= 0xaadd15 and < 0xaadd25 or >= 0xaadf13 and < 0xaadf2f)
            {
                throw new InvalidOperationException(
                    $"Shaktool attempted migrated instruction-selector read ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xaa0000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ShaktoolInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ShaktoolInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
