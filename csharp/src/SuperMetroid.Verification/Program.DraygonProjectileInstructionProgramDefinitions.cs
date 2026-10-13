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
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static EnemyProjectileProgramOracleVerification;
using static SuiteTimingVerification;

/// <summary>Checks Draygon goop and turret programs against retail control words and executed projectile behavior.</summary>
internal static class DraygonProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data for the Draygon projectile instruction check.</summary>
    internal static void VerifyDraygonProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyDraygonProjectileInstructionProgramDefinitions), () => VerifyDraygonProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Exercises goop attachment, sleep, drops, deletion, and turret bloom with compiled source reads forbidden.</summary>
    internal static void VerifyDraygonProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < DraygonProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DraygonProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value, ReadNativeEnemyProjectileProgramWord(rom, definition.Address),
                $"Draygon projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewDraygonProjectileInstructionReadGuard(rom);
        var selectedPresentation = new HashSet<ushort>();

        RoomEnemySystem goopSystem = NewSystem();
        DraygonEnemyState goopState = NewState(goopSystem);
        goopSystem.DraygonBehavior.SpawnDraygonGoop(goopState, true);
        RoomEnemyProjectileSlot goop = goopSystem.Projectiles.EnemyProjectiles.Single(
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
        AssertEqual((ushort)EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus,
            touch.PreInstruction, "Draygon goop touch installs attached movement");
        AssertEqual((ushort)0x8c3e, touch.InstructionPointer,
            "Draygon goop touch falls through into its first animation frame");

        RoomEnemySystem sleepSystem = NewSystem();
        RoomEnemyProjectileSlot sleeping = SpawnGoop(sleepSystem);
        sleeping.InstructionPointer = 0x8c56;
        RunForcedTicks(sleepSystem, sleeping, 1, new SamusState());
        AssertEqual((ushort)0x8c56, sleeping.InstructionPointer,
            "Draygon goop's terminal sleep retains its own opcode");
        AssertEqual((ushort)0, sleeping.InstructionTimer.RawValue,
            "Draygon goop's terminal sleep stores the native zero timer");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = SpawnGoop(shotSystem);
        shot.InstructionPointer = DraygonProjectileInstructionProgramDefinitions.GoopShot;
        RunForcedTicks(shotSystem, shot, 3, new SamusState { Health = 10, MaxHealth = 99 });
        AssertTrue(!shot.IsActive,
            "Draygon goop shot displays both frames, requests a drop, and deletes");
        AssertTrue(shotSystem.Projectiles.EnemyProjectiles.Any(projectile =>
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
        var targetSamus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
        turretSystem.DraygonBehavior.SpawnDraygonWallTurret(turretState, targetSamus, (ushort)0);
        RoomEnemyProjectileSlot turret = turretSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.DraygonWallTurret);
        AssertEqual(DraygonProjectileInstructionProgramDefinitions.WallTurretBloom,
            turret.InstructionPointer, "real Draygon turret producer selects named bloom");
        RunForcedTicks(turretSystem, turret, 20, targetSamus);
        AssertEqual((ushort)0x8cea, turret.InstructionPointer,
            "Draygon turret completes sixteen bloom frames and its three-frame flight loop");
        AssertEqual(
            (ushort)EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired,
            turret.PreInstruction, "Draygon turret callback enables projectile flight");

        ushort[] nativeOperands = [0x8c3c, 0x8c40, 0x8c44, 0x8c48, 0x8c4c, 0x8c50,
            0x8c5a, 0x8c5e, 0x8ca6, 0x8caa, 0x8cae, 0x8cb2, 0x8cb6, 0x8cba,
            0x8cbe, 0x8cc2, 0x8cc6, 0x8cca, 0x8cce, 0x8cd2, 0x8cd6, 0x8cda,
            0x8cde, 0x8ce2, 0x8ce8, 0x8cec, 0x8cf0];
        AssertEqual(nativeOperands.Length, DraygonProjectileInstructionProgramDefinitions.PresentationWordCount,
            "Draygon native presentation operand count");
        AssertEqual(nativeOperands.Length, selectedPresentation.Count,
            "actual Draygon programs select all installed presentation frames");
        AssertEqual(0, guard.ObservedWords.Count,
            "installed Draygon spritemap selections require zero cartridge reads");
        for (int index = 0; index < nativeOperands.Length; index++)
        {
            ushort address = DraygonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(nativeOperands[index], address, "Draygon exact ordered native visual operand identity");
            AssertTrue(selectedPresentation.Contains(address),
                $"production selects installed Draygon presentation $86:{address:X4}");
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
            "and twenty-seven installed spritemap selections pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x0080);
            enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState { Health = 10, MaxHealth = 99 };
            return enemies;
        }

        static DraygonEnemyState NewState(RoomEnemySystem enemies)
        {
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            body.Position = body.Position.WithWhole(new(128, 128));
            return new DraygonEnemyState(body);
        }

        RoomEnemyProjectileSlot SpawnGoop(RoomEnemySystem enemies)
        {
            DraygonEnemyState state = NewState(enemies);
            enemies.DraygonBehavior.SpawnDraygonGoop(state, true);
            return enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind ==
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
                projectile.InstructionTimer = NativeCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, samus, Position16<RoomPixels>.Zero);
                if (projectile.PresentationOperandAddress != 0)
                    selectedPresentation.Add(projectile.PresentationOperandAddress);
            }
        }
    }

    /// <summary>Alternates warmed goop and turret control lookups for allocation accounting.</summary>
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

    /// <summary>Rejects migrated mechanics reads and observes native spritemap operands during production execution.</summary>
    private static CartridgeReadGuard NewDraygonProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    DraygonProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Draygon mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                DraygonProjectileInstructionProgramDefinitions.PresentationWordCount,
                DraygonProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the retail-backed Draygon projectile instruction migration command.</summary>
    [VerificationCommand("--draygon-projectile-instruction-mechanics")]
    internal static int DraygonProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDraygonProjectileInstructionProgramDefinitions), () => VerifyDraygonProjectileInstructionProgramDefinitions());
        return 0;
    }
}
