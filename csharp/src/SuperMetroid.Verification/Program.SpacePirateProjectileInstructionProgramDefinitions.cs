using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySpacePirateProjectileInstructionProgramDefinitions() =>
        VerifySpacePirateProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifySpacePirateProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < SpacePirateProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            SpacePirateProjectileInstructionMechanicsWord definition =
                SpacePirateProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Space Pirate projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new SpacePirateProjectileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawnLaser = typeof(RoomEnemySystem).GetMethod(
            "SpawnPirateMotherBrainLaser", flags)!;
        MethodInfo spawnClaw = typeof(RoomEnemySystem).GetMethod(
            "SpawnNinjaPirateClaw", flags)!;

        foreach ((bool movingRight, ushort program, ushort loop,
                  ushort movement, ushort expectedX) in new[]
                 {
                     (false,
                         SpacePirateProjectileInstructionProgramDefinitions.LaserLeft,
                         SpacePirateProjectileInstructionProgramDefinitions.LaserLeftLoop,
                         EnemyProjectileCodePointers
                             .PreInstruction_EnemyProjectile_Pirate_MotherBrain_Laser_Left,
                         (ushort)124),
                     (true,
                         SpacePirateProjectileInstructionProgramDefinitions.LaserRight,
                         SpacePirateProjectileInstructionProgramDefinitions.LaserRightLoop,
                         EnemyProjectileCodePointers
                             .PreInst_EnemyProjectile_Pirate_MotherBrain_Laser_Right,
                         (ushort)132),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot source = enemies.Slots[0];
            source.XPosition = 128;
            source.YPosition = 96;
            source.Parameter1 = 0;
            RoomEnemyProjectileSlot laser = (RoomEnemyProjectileSlot)spawnLaser.Invoke(
                enemies,
                [source, (ushort)128, (ushort)96, movingRight])!;
            AssertEqual(program, laser.InstructionPointer,
                "real Pirate/Mother Brain laser producer selects its facing program");
            AssertEqual(EnemyProjectileCodePointers.RTS_86A05B, laser.PreInstruction,
                "laser initializer keeps muzzle frames stationary");
            RunForcedTicks(enemies, laser, 3);
            AssertEqual((ushort)128, laser.XPosition,
                "three laser muzzle frames remain stationary");
            RunForcedTick(enemies, laser);
            AssertEqual(movement, laser.PreInstruction,
                "laser program installs its facing movement callback");
            AssertEqual(expectedX, laser.XPosition,
                "laser callback executes immediately on installation");
            RunForcedTicks(enemies, laser, 10);
            AssertEqual(unchecked((ushort)(loop + 4)), laser.InstructionPointer,
                "laser completes its authored frames and loops");
        }

        foreach ((ushort direction, ushort program, ushort loop, ushort movement) in new[]
                 {
                     ((ushort)0,
                         SpacePirateProjectileInstructionProgramDefinitions.ClawLeft,
                         SpacePirateProjectileInstructionProgramDefinitions.ClawLeftLoop,
                         EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Left),
                     ((ushort)1,
                         SpacePirateProjectileInstructionProgramDefinitions.ClawRight,
                         SpacePirateProjectileInstructionProgramDefinitions.ClawRightLoop,
                         EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Right),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot source = enemies.Slots[0];
            source.XPosition = 128;
            source.YPosition = 96;
            var state = new NinjaSpacePirateEnemyState(source);
            spawnClaw.Invoke(
                enemies,
                [source, state, direction, (ushort)0, (ushort)0]);
            RoomEnemyProjectileSlot claw = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.PirateClaw);
            AssertEqual(program, claw.InstructionPointer,
                "real Ninja Pirate claw producer selects its facing program");
            RunForcedTick(enemies, claw);
            AssertEqual(movement, claw.PreInstruction,
                "claw program installs its facing movement callback");
            RunForcedTicks(enemies, claw, 8);
            AssertEqual(unchecked((ushort)(loop + 4)), claw.InstructionPointer,
                "claw completes all eight authored frames and loops");
        }

        RoomEnemySystem deleteSystem = NewSystem();
        RoomEnemySlot deleteSource = deleteSystem.Slots[0];
        var deleteState = new NinjaSpacePirateEnemyState(deleteSource);
        spawnClaw.Invoke(
            deleteSystem,
            [deleteSource, deleteState, (ushort)0, (ushort)0, (ushort)0]);
        RoomEnemyProjectileSlot deleting = deleteSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PirateClaw);
        deleting.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(deleteSystem, deleting);
        AssertTrue(!deleting.IsActive,
            "Space Pirate projectile shot reaction reaches shared compiled deletion");

        AssertEqual(SpacePirateProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Space Pirate projectile spritemap operands remain cartridge reads");
        for (int index = 0;
             index < SpacePirateProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = SpacePirateProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads Space Pirate presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared Space Pirate mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => SpacePirateProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9f43),
            "Space Pirate spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SpacePirateProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xa009),
            "Space Pirate initializer body is rejected as mechanics");

        _ = ProbeSpacePirateProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSpacePirateProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Space Pirate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Space Pirate mechanics lookups allocate no storage");

        Console.WriteLine(
            "Space Pirate projectile instruction mechanics: fifty-eight compiled words, " +
            "both real producers/facings, immediate laser motion, claw loops, shared " +
            "deletion, and forty-two live spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            return enemies;
        }

        void RunForcedTick(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }

        void RunForcedTicks(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int tick = 0; tick < count; tick++)
                RunForcedTick(enemies, projectile);
        }
    }

    private static int ProbeSpacePirateProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SpacePirateProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SpacePirateProjectileInstructionProgramDefinitions.LaserLeft
                    : SpacePirateProjectileInstructionProgramDefinitions.ClawRight);
        }
        return checksum;
    }

    private sealed class SpacePirateProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (SpacePirateProjectileInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Space Pirate mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SpacePirateProjectileInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = SpacePirateProjectileInstructionProgramDefinitions
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
