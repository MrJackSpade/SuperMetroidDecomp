using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyDownwardGateProjectileInstructionProgramDefinitions() =>
        VerifyDownwardGateProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyDownwardGateProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.NonPublic;
        for (int index = 0;
             index < DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DownwardGateProjectileInstructionMechanicsWord definition =
                DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"downward-gate projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new DownwardGateProjectileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo move = typeof(RoomEnemySystem).GetMethod(
            "RunDownwardGateProjectileMovement", flags)!;

        RoomEnemySystem closingSystem = NewSystem();
        RoomEnemyProjectileSlot closing = Spawn(
            closingSystem,
            RoomEnemyProjectileKind.DownwardGateMoving);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.Moving,
            closing.InstructionPointer,
            "real downward-moving gate producer selects the named program");
        AssertEqual((ushort)32, closing.YPosition,
            "downward-moving gate begins at its PLM row");
        Process(closingSystem, closing);
        AssertEqual((ushort)0x0100, closing.YVelocity,
            "downward-moving gate installs positive one-pixel velocity");
        AssertEqual(DownwardGateEnemyProjectileRomData.MovementPreInstruction,
            closing.PreInstruction,
            "downward-moving gate installs its translated movement callback");
        RunMovementFrames(closingSystem, closing, 64);
        AssertTrue(closing.IsActive,
            "downward-moving gate remains resident after reaching the closed position");
        AssertEqual((ushort)96, closing.YPosition,
            "downward-moving gate traverses exactly four sixteen-pixel stages");
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep,
            closing.InstructionPointer,
            "downward-moving gate hands off to the closed sleep");
        AssertEqual(EnemyProjectileCodePointers.RTS_868170, closing.PreInstruction,
            "downward-moving gate clears movement when it parks");

        RoomEnemySystem openingSystem = NewSystem();
        RoomEnemyProjectileSlot opening = Spawn(
            openingSystem,
            RoomEnemyProjectileKind.DownwardGateClosed);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.Closed,
            opening.InstructionPointer,
            "real closed-gate producer selects the named program");
        AssertEqual((ushort)96, opening.YPosition,
            "closed gate begins four blocks below its PLM row");
        Process(openingSystem, opening);
        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep,
            opening.InstructionPointer,
            "closed gate displays its parked frame and sleeps");
        openingSystem.ApplyDownwardGateProjectileRequest(
            new DownwardGateProjectileRequest(
                DownwardGateProjectileOperation.Wake,
                0,
                42),
            roomWidthInBlocks: 16);
        Process(openingSystem, opening);
        AssertEqual(unchecked((ushort)-0x0100), opening.YVelocity,
            "woken gate retains its authored upward velocity");
        AssertEqual(DownwardGateEnemyProjectileRomData.MovementPreInstruction,
            opening.PreInstruction,
            "woken gate installs its translated movement callback");
        RunMovementFrames(openingSystem, opening, 63);
        AssertTrue(opening.IsActive,
            "opening gate remains active one pixel before its fourth stage completes");
        AssertEqual((ushort)33, opening.YPosition,
            "opening gate reaches the final pixel before deletion");
        RunMovementFrames(openingSystem, opening, 1);
        AssertTrue(!opening.IsActive,
            "opening gate deletes after exactly four sixteen-pixel stages");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = Spawn(
            shotSystem,
            RoomEnemyProjectileKind.DownwardGateClosed);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shot.InstructionTimer = 1;
        Process(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "downward-gate shot reaction reaches shared compiled deletion");

        AssertEqual(DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all downward-gate spritemap operands remain cartridge reads");
        for (int index = 0;
             index < DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = DownwardGateProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads downward-gate presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared downward-gate mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe546),
            "downward-gate spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xe533),
            "downward-gate callback body is rejected as mechanics");

        _ = ProbeDownwardGateProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDownwardGateProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "downward-gate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed downward-gate mechanics lookups allocate no storage");

        Console.WriteLine(
            "Downward-gate projectile instruction mechanics: twenty-eight compiled words, " +
            "both real producers, four-stage close/open lifecycles, shared deletion, and " +
            "nine live spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, guard);
            return system;
        }

        RoomEnemyProjectileSlot Spawn(
            RoomEnemySystem system,
            RoomEnemyProjectileKind kind)
        {
            system.ApplyDownwardGateProjectileRequest(
                new DownwardGateProjectileRequest(
                    DownwardGateProjectileOperation.Spawn,
                    (ushort)kind,
                    42),
                roomWidthInBlocks: 16);
            return system.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            process.Invoke(system, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }

        void RunMovementFrames(
            RoomEnemySystem system,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int frame = 0; frame < count; frame++)
            {
                move.Invoke(null, [projectile]);
                if (projectile.IsActive)
                    Process(system, projectile);
            }
        }
    }

    private static int ProbeDownwardGateProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DownwardGateProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DownwardGateProjectileInstructionProgramDefinitions.Moving
                    : DownwardGateProjectileInstructionProgramDefinitions.Closed);
        }
        return checksum;
    }

    private sealed class DownwardGateProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (DownwardGateProjectileInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled downward-gate mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DownwardGateProjectileInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = DownwardGateProjectileInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
