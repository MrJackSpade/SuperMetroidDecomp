using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDraygonProjectileInstructionProgramDefinitions() =>
        VerifyDraygonProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyDraygonProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < DraygonProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DraygonProjectileInstructionMechanicsWord definition =
                DraygonProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value, ReadWord(rom, definition.Address),
                $"Draygon projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new DraygonProjectileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawnGoop = typeof(RoomEnemySystem).GetMethod(
            "SpawnDraygonGoop", flags)!;
        MethodInfo spawnTurret = typeof(RoomEnemySystem).GetMethod(
            "SpawnDraygonWallTurret", flags)!;

        RoomEnemySystem goopSystem = NewSystem();
        DraygonEnemyState goopState = NewState(goopSystem);
        spawnGoop.Invoke(goopSystem, [goopState, true]);
        RoomEnemyProjectileSlot goop = goopSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.DraygonGoop);
        AssertEqual(DraygonProjectileInstructionProgramDefinitions.Goop,
            goop.InstructionPointer, "real Draygon goop producer selects named loop");
        RunForcedTicks(goopSystem, goop, 7, new SamusState());
        AssertEqual((ushort)0x8c3e, goop.InstructionPointer,
            "Draygon goop completes all six frames and loops");

        RoomEnemySystem touchSystem = NewSystem();
        RoomEnemyProjectileSlot touch = SpawnGoop(touchSystem);
        touch.InstructionPointer = DraygonProjectileInstructionProgramDefinitions.GoopTouch;
        var attachedSamus = new SamusState { XSpeedDivisor = 0 };
        RunForcedTicks(touchSystem, touch, 1, attachedSamus);
        AssertEqual((ushort)1, attachedSamus.XSpeedDivisor,
            "Draygon goop touch callback applies the first native speed divisor");
        AssertEqual(EnemyProjectileCodePointers.PreInstruction_DraygonGoop_StuckToSamus,
            touch.PreInstruction, "Draygon goop touch installs attached movement");
        AssertEqual((ushort)0x8c3e, touch.InstructionPointer,
            "Draygon goop touch falls through into its first animation frame");

        RoomEnemySystem sleepSystem = NewSystem();
        RoomEnemyProjectileSlot sleeping = SpawnGoop(sleepSystem);
        sleeping.InstructionPointer = 0x8c56;
        RunForcedTicks(sleepSystem, sleeping, 1, new SamusState());
        AssertEqual((ushort)0x8c56, sleeping.InstructionPointer,
            "Draygon goop's terminal sleep retains its own opcode");
        AssertEqual((ushort)0, sleeping.InstructionTimer,
            "Draygon goop's terminal sleep stores the native zero timer");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = SpawnGoop(shotSystem);
        shot.InstructionPointer = DraygonProjectileInstructionProgramDefinitions.GoopShot;
        RunForcedTicks(shotSystem, shot, 3, new SamusState { Health = 10, MaxHealth = 99 });
        AssertTrue(!shot.IsActive,
            "Draygon goop shot displays both frames, requests a drop, and deletes");
        AssertTrue(shotSystem.EnemyProjectiles.Any(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup),
            "Draygon goop shot allocates its production pickup before deletion");

        RoomEnemySystem privateDeleteSystem = NewSystem();
        RoomEnemyProjectileSlot privateDelete = SpawnGoop(privateDeleteSystem);
        privateDelete.InstructionPointer = 0x8c66;
        RunForcedTicks(privateDeleteSystem, privateDelete, 1, new SamusState());
        AssertTrue(!privateDelete.IsActive,
            "Draygon's private trailing delete word remains executable");

        RoomEnemySystem turretSystem = NewSystem();
        DraygonEnemyState turretState = NewState(turretSystem);
        var targetSamus = new SamusState { XPosition = 128, YPosition = 128 };
        spawnTurret.Invoke(turretSystem, [turretState, targetSamus, (ushort)0]);
        RoomEnemyProjectileSlot turret = turretSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.DraygonWallTurret);
        AssertEqual(DraygonProjectileInstructionProgramDefinitions.WallTurretBloom,
            turret.InstructionPointer, "real Draygon turret producer selects named bloom");
        RunForcedTicks(turretSystem, turret, 20, targetSamus);
        AssertEqual((ushort)0x8cea, turret.InstructionPointer,
            "Draygon turret completes sixteen bloom frames and its three-frame flight loop");
        AssertEqual(
            EnemyProjectileCodePointers.PreInstruction_EnemyProj_DraygonsWallTurretProjectile_Fired,
            turret.PreInstruction, "Draygon turret callback enables projectile flight");

        AssertEqual(DraygonProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Draygon spritemap operands remain cartridge reads");
        for (int index = 0;
             index < DraygonProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = DraygonProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads Draygon presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Draygon and shared-delete mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => DraygonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x8c3c),
            "Draygon spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DraygonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x8c68),
            "Draygon drop callback body is rejected as mechanics");

        _ = ProbeDraygonProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDraygonProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Draygon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Draygon mechanics lookups allocate no storage");

        Console.WriteLine(
            "Draygon projectile instruction mechanics: thirty-eight compiled words, " +
            "real goop/turret producers, touch/drop callbacks, all private programs, " +
            "and twenty-seven live spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies, (Func<ushort>)(() => 0x0080));
            typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
                enemies, new SamusState { Health = 10, MaxHealth = 99 });
            return enemies;
        }

        static DraygonEnemyState NewState(RoomEnemySystem enemies)
        {
            RoomEnemySlot body = enemies.Slots[0];
            body.XPosition = 128;
            body.YPosition = 128;
            return new DraygonEnemyState(body);
        }

        RoomEnemyProjectileSlot SpawnGoop(RoomEnemySystem enemies)
        {
            DraygonEnemyState state = NewState(enemies);
            spawnGoop.Invoke(enemies, [state, true]);
            return enemies.EnemyProjectiles.Single(projectile => projectile.Kind ==
                RoomEnemyProjectileKind.DraygonGoop);
        }

        void RunForcedTicks(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile,
            int count,
            SamusState samus)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, samus, (ushort)0, (ushort)0]);
            }
        }

        static ushort ReadWord(SuperMetroidAddressSpace source, ushort address) =>
            unchecked((ushort)(source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
                source.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(address + 1))) << 8));
    }

    private static int ProbeDraygonProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DraygonProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DraygonProjectileInstructionProgramDefinitions.Goop
                    : DraygonProjectileInstructionProgramDefinitions.WallTurretBloom);
        }
        return checksum;
    }

    private sealed class DraygonProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (DraygonProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Draygon mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DraygonProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = DraygonProjectileInstructionProgramDefinitions
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
