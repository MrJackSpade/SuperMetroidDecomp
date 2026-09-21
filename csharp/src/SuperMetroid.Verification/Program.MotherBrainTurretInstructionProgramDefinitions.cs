using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainTurretInstructionProgramDefinitions() =>
        VerifyMotherBrainTurretInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyMotherBrainTurretInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(49,
            MotherBrainTurretInstructionProgramDefinitions.MechanicsWordCount,
            "Mother Brain turret catalog contains every mechanics word");
        for (int index = 0;
             index < MotherBrainTurretInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MotherBrainTurretInstructionMechanicsWord definition =
                MotherBrainTurretInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Mother Brain turret mechanics word $86:{definition.Address:X4}");
        }

        var guard = new MotherBrainTurretInstructionReadGuard(rom);
        MethodInfo spawnTurret = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainTurret", flags)!;
        MethodInfo spawnBullet = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainTurretBullet", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        for (ushort parameter = 0; parameter < 12; parameter++)
        {
            RoomEnemySystem system = NewSystem();
            spawnTurret.Invoke(system, [parameter]);
            RoomEnemyProjectileSlot turret = system.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainRoomTurret);
            MotherBrainTurretDirection direction = MotherBrainTurretDefinitions
                .ForTurret(parameter).InitialDirection;
            AssertEqual(MotherBrainTurretInstructionProgramDefinitions.TurretProgram(direction),
                turret.InstructionPointer,
                $"real Mother Brain turret producer selects parameter {parameter} pose");
        }

        for (byte directionValue = 0;
             directionValue < MotherBrainTurretInstructionProgramDefinitions.DirectionCount;
             directionValue++)
        {
            MotherBrainTurretDirection direction =
                (MotherBrainTurretDirection)directionValue;
            ushort turretProgram = MotherBrainTurretInstructionProgramDefinitions
                .TurretProgram(direction);
            RoomEnemySystem turretSystem = NewSystem();
            RoomEnemyProjectileSlot turretPose = turretSystem.EnemyProjectiles[0];
            turretPose.Kind = RoomEnemyProjectileKind.MotherBrainRoomTurret;
            turretPose.InstructionPointer = turretProgram;
            turretPose.InstructionTimer = 1;
            RunForcedTick(turretSystem, turretPose);
            AssertEqual(1, turretPose.InstructionTimer,
                $"Mother Brain turret direction {directionValue} pose duration");
            AssertEqual(unchecked((ushort)(turretProgram + 4)), turretPose.InstructionPointer,
                $"Mother Brain turret direction {directionValue} reaches sleep");
            RunForcedTick(turretSystem, turretPose);
            AssertEqual(unchecked((ushort)(turretProgram + 4)), turretPose.InstructionPointer,
                $"Mother Brain turret direction {directionValue} retains sleep opcode");

            RoomEnemySystem bulletSystem = NewSystem();
            var source = new RoomEnemyProjectileSlot(0)
            {
                XPosition = 0x0400,
                YPosition = 0x0200,
                YSubposition = directionValue,
            };
            spawnBullet.Invoke(bulletSystem, [source]);
            RoomEnemyProjectileSlot bullet = bulletSystem.EnemyProjectiles.Single(
                projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet);
            AssertEqual(MotherBrainTurretInstructionProgramDefinitions.BulletSelector,
                bullet.InstructionPointer,
                $"real Mother Brain turret bullet producer selects direction {directionValue}");
            RunForcedTick(bulletSystem, bullet);
            ushort bulletProgram = MotherBrainTurretInstructionProgramDefinitions
                .BulletProgram(direction);
            AssertEqual(1, bullet.InstructionTimer,
                $"Mother Brain turret bullet direction {directionValue} pose duration");
            AssertEqual(unchecked((ushort)(bulletProgram + 4)), bullet.InstructionPointer,
                $"Mother Brain turret bullet direction {directionValue} selector target");
            RunForcedTick(bulletSystem, bullet);
            AssertEqual(unchecked((ushort)(bulletProgram + 4)), bullet.InstructionPointer,
                $"Mother Brain turret bullet direction {directionValue} retains sleep opcode");
        }

        RoomEnemySystem contactSystem = NewSystem();
        var contactSource = new RoomEnemyProjectileSlot(0)
        {
            XPosition = 0x0400,
            YPosition = 0x0200,
            YSubposition = (ushort)MotherBrainTurretDirection.Left,
        };
        spawnBullet.Invoke(contactSystem, [contactSource]);
        RoomEnemyProjectileSlot contact = contactSystem.EnemyProjectiles.Single(
            projectile =>
                projectile.Kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet);
        contact.InstructionPointer =
            MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot;
        ushort[] contactDurations = [8, 8, 8, 8, 32];
        for (int frame = 0; frame < contactDurations.Length; frame++)
        {
            RunForcedTick(contactSystem, contact);
            AssertEqual(contactDurations[frame], contact.InstructionTimer,
                $"Mother Brain turret bullet contact frame {frame} duration");
            AssertEqual(unchecked((ushort)(
                    MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot +
                    8 + frame * 4)),
                contact.InstructionPointer,
                $"Mother Brain turret bullet contact frame {frame} advance");
            AssertEqual(0, contact.GraphicsIndex,
                $"Mother Brain turret bullet contact frame {frame} palette reset");
            AssertEqual(EnemyProjectileCodePointers.RTS_868170, contact.PreInstruction,
                $"Mother Brain turret bullet contact frame {frame} movement cleared");
        }
        RunForcedTick(contactSystem, contact);
        AssertTrue(!contact.IsActive,
            "Mother Brain turret bullet contact reaches private compiled deletion");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shotTurret = shotSystem.EnemyProjectiles[0];
        shotTurret.Kind = RoomEnemyProjectileKind.MotherBrainRoomTurret;
        shotTurret.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shotTurret.InstructionTimer = 1;
        RunForcedTick(shotSystem, shotTurret);
        AssertTrue(!shotTurret.IsActive,
            "Mother Brain turret shot reaction reaches shared compiled deletion");

        AssertEqual(21,
            MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount,
            "Mother Brain turret catalog retains all presentation operands");
        AssertEqual(MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Mother Brain turret spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared Mother Brain turret mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MotherBrainTurretInstructionProgramDefinitions.ReadMechanicsWord(0xc103),
            "Mother Brain turret spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainTurretInstructionProgramDefinitions.ReadMechanicsWord(0xc173),
            "adjacent Mother Brain turret callback body is rejected as mechanics");

        _ = ProbeMotherBrainTurretInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMotherBrainTurretInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Mother Brain turret allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Mother Brain turret mechanics lookups allocate no storage");

        Console.WriteLine(
            "Mother Brain turret instruction mechanics: 49 compiled words, twelve real " +
            "turrets, all eight real bullet selectors, touch/shot smoke, shared deletion, " +
            "and 21 live spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!
                .SetValue(system, (Func<ushort>)(() => 0));
            return system;
        }

        void RunForcedTick(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(system, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    private static int ProbeMotherBrainTurretInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MotherBrainTurretInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MotherBrainTurretInstructionProgramDefinitions.TurretLeft
                    : MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot);
        }
        return checksum;
    }

    private sealed class MotherBrainTurretInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (MotherBrainTurretInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Mother Brain turret mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = MotherBrainTurretInstructionProgramDefinitions
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
