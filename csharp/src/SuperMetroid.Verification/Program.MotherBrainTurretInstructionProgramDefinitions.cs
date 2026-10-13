using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks Mother Brain turret and bullet programs, directional selectors, and contact animation against ROM.</summary>
internal static class MotherBrainTurretInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the turret instruction suite.</summary>
    internal static void VerifyMotherBrainTurretInstructionProgramDefinitions() =>
        Suite(nameof(VerifyMotherBrainTurretInstructionProgramDefinitions), () => VerifyMotherBrainTurretInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks compiled words, real turret and bullet producers, eight directions, contact frames, and deletion paths.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain turret instruction program definitions")]
    internal static void VerifyMotherBrainTurretInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(49,
            MotherBrainTurretInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Mother Brain turret catalog contains every mechanics word");
        for (int index = 0;
             index < MotherBrainTurretInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MotherBrainTurretInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Mother Brain turret mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewMotherBrainTurretInstructionReadGuard(rom);

        for (ushort parameter = 0; parameter < 12; parameter++)
        {
            RoomEnemySystem system = NewSystem();
            system.MotherBrainBehavior.SpawnMotherBrainTurret(parameter);
            RoomEnemyProjectileSlot turret = system.Projectiles.EnemyProjectiles.Single(
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
            RoomEnemyProjectileSlot turretPose = turretSystem.Projectiles.EnemyProjectiles[0];
            turretPose.Kind = RoomEnemyProjectileKind.MotherBrainRoomTurret;
            turretPose.InstructionPointer = turretProgram;
            turretPose.InstructionTimer = NativeCountdown.Start(1);
            RunForcedTick(turretSystem, turretPose);
            VerifyPresentation(turretPose, (ushort)(0xc103 + 6 * directionValue));
            AssertEqual(1, turretPose.InstructionTimer.RawValue,
                $"Mother Brain turret direction {directionValue} pose duration");
            AssertEqual(unchecked((ushort)(turretProgram + 4)), turretPose.InstructionPointer,
                $"Mother Brain turret direction {directionValue} reaches sleep");
            RunForcedTick(turretSystem, turretPose);
            AssertEqual(unchecked((ushort)(turretProgram + 4)), turretPose.InstructionPointer,
                $"Mother Brain turret direction {directionValue} retains sleep opcode");

            RoomEnemySystem bulletSystem = NewSystem();
            var source = new RoomEnemyProjectileSlot(0)
            {
                Position = new(new(0x0400, 0), new(0x0200, directionValue)),
            };
            bulletSystem.MotherBrainBehavior.SpawnMotherBrainTurretBullet(source);
            RoomEnemyProjectileSlot bullet = bulletSystem.Projectiles.EnemyProjectiles.Single(
                projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet);
            AssertEqual(MotherBrainTurretInstructionProgramDefinitions.BulletSelector,
                bullet.InstructionPointer,
                $"real Mother Brain turret bullet producer selects direction {directionValue}");
            RunForcedTick(bulletSystem, bullet);
            ushort bulletProgram = MotherBrainTurretInstructionProgramDefinitions
                .BulletProgram(direction);
            VerifyPresentation(bullet, (ushort)(0xc145 + 6 * directionValue));
            AssertEqual(1, bullet.InstructionTimer.RawValue,
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
            Position = new(new(0x0400, 0), new(0x0200, (ushort)MotherBrainTurretDirection.Left)),
        };
        contactSystem.MotherBrainBehavior.SpawnMotherBrainTurretBullet(contactSource);
        RoomEnemyProjectileSlot contact = contactSystem.Projectiles.EnemyProjectiles.Single(
            projectile =>
                projectile.Kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet);
        contact.InstructionPointer =
            MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot;
        ushort[] contactDurations = [8, 8, 8, 8, 32];
        for (int frame = 0; frame < contactDurations.Length; frame++)
        {
            RunForcedTick(contactSystem, contact);
            VerifyPresentation(contact, (ushort)(0xc1a0 + 4 * frame));
            AssertEqual(contactDurations[frame], contact.InstructionTimer.RawValue,
                $"Mother Brain turret bullet contact frame {frame} duration");
            AssertEqual(unchecked((ushort)(
                    MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot +
                    8 + frame * 4)),
                contact.InstructionPointer,
                $"Mother Brain turret bullet contact frame {frame} advance");
            AssertEqual(0, contact.GraphicsIndex,
                $"Mother Brain turret bullet contact frame {frame} palette reset");
            AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, contact.PreInstruction,
                $"Mother Brain turret bullet contact frame {frame} movement cleared");
        }
        RunForcedTick(contactSystem, contact);
        AssertTrue(!contact.IsActive,
            "Mother Brain turret bullet contact reaches private compiled deletion");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shotTurret = shotSystem.Projectiles.EnemyProjectiles[0];
        shotTurret.Kind = RoomEnemyProjectileKind.MotherBrainRoomTurret;
        shotTurret.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shotTurret.InstructionTimer = NativeCountdown.Start(1);
        RunForcedTick(shotSystem, shotTurret);
        AssertTrue(!shotTurret.IsActive,
            "Mother Brain turret shot reaction reaches shared compiled deletion");

        AssertEqual(21,
            MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount,
            "Mother Brain turret catalog retains all presentation operands");
        AssertEqual(0, guard.ObservedWords.Count,
            "Mother Brain turret visual operands require no runtime cartridge reads");
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
            "and 21 native presentation operand identities pass without runtime cartridge reads.");

        void VerifyPresentation(RoomEnemyProjectileSlot projectile, ushort expectedOperand)
        {
            AssertEqual(expectedOperand, projectile.PresentationOperandAddress,
                "Mother Brain turret selects the exact native presentation operand");
            AssertTrue(EnemyProjectilePresentationFrameDefinitions.Contains(expectedOperand),
                "Mother Brain turret operand belongs to installed presentation definitions");
            AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, projectile.SpritemapPointer,
                "Mother Brain turret defers the exact visual identity to installed artwork");
        }
        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            system.BindFixtureBus(guard);
            system.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
            return system;
        }

        void RunForcedTick(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            system.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
        }
    }

    /// <summary>Consumes repeated turret and contact control lookups for the warmed allocation check.</summary>
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

    /// <summary>Forbids private and shared turret mechanics reads while observing visual operand addresses.</summary>
    private static CartridgeReadGuard NewMotherBrainTurretInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => MotherBrainTurretInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Mother Brain turret mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount, MotherBrainTurretInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Mother Brain turret suite through the verification command dispatcher.</summary>
    [VerificationCommand("--mother-brain-turret-instruction-mechanics")]
    internal static int MotherBrainTurretInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMotherBrainTurretInstructionProgramDefinitions), () => VerifyMotherBrainTurretInstructionProgramDefinitions());
        return 0;
    }
}
