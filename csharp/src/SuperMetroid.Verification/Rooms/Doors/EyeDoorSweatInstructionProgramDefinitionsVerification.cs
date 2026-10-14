using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Eye Door sweat falling loop, floor impact, and projectile deletion against bank 86.</summary>
internal static class EyeDoorSweatInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge to compare Eye Door sweat mechanics and visual operands.</summary>
    internal static void VerifyEyeDoorSweatInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEyeDoorSweatInstructionProgramDefinitions), () => VerifyEyeDoorSweatInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises production spawning, floor-impact handoff, three impact frames, and shared shot deletion.</summary>
    internal static void VerifyEyeDoorSweatInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Eye Door sweat mechanics word $86:{definition.Address:X4}");
        }

        var guard = new CartridgeReadGuard(rom)
            .Forbid(address => EyeDoorSweatInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Eye Door sweat mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount,
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress);
        var observedOperands = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        Action<RoomEnemyProjectileSlot, RoomLevelData> runPreInstruction = EyeDoorBehavior.RunEyeDoorSweatPreInstruction;

        const int roomWidth = 16;
        var foreground = new ushort[roomWidth * 16];
        foreground[5 * roomWidth + 6] =
            (ushort)((int)RoomCollisionType.SolidBlock << 12);
        RoomLevelData level = CreateRoom(new(roomWidth, 16),
            foreground,
            new byte[foreground.Length],
            blockDefinitions: new byte[0x400 * 8]);
        var samus = new SamusState();
        var system = new Bank80SystemState();
        var request = new EyeDoorProjectileRequest(
            EyeDoorProjectileDefinition.Sweat,
            Parameter: 4,
            PlmBlockIndex: 4 * roomWidth + 7,
            DoorBit: 0);

        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(request, roomWidth, system);
        RoomEnemyProjectileSlot sweat = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Initial,
            sweat.InstructionPointer,
            "Eye Door sweat definition selects the named initial program");

        RunForcedTicks(sweat, 2);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Initial + 4,
            sweat.InstructionPointer,
            "Eye Door sweat falling loop returns and schedules its frame again");

        runPreInstruction(sweat, level);
        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.Impact,
            sweat.InstructionPointer,
            "real floor collision selects the named Eye Door sweat impact program");
        AssertEqual((ushort)EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction,
            sweat.PreInstruction,
            "Eye Door sweat impact disables movement with the native inert pre-instruction");
        AssertEqual((ushort)1, sweat.InstructionTimer.RawValue,
            "Eye Door sweat impact requests a same-pass animation tick");
        AssertEqual((ushort)76, sweat.Position.Y.Whole,
            "Eye Door sweat applies the native four-pixel floor-impact correction");

        RunForcedTicks(sweat, 4);
        AssertTrue(!sweat.IsActive,
            "Eye Door sweat impact program deletes after all three presentation frames");

        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(request, roomWidth, system);
        RoomEnemyProjectileSlot shot = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat &&
                projectile.IsActive);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(shot, 1);
        AssertTrue(!shot.IsActive,
            "Eye Door sweat shot reaction reaches the compiled shared delete program");

        AssertEqual(
            EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount,
            observedOperands.Count,
            "all live Eye Door sweat spritemap operands are selected from installed artwork");
        for (int index = 0;
             index < EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(observedOperands.Contains(address),
                $"production execution selects Eye Door sweat presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "projectile visuals do not read cartridge bytes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Eye Door sweat and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(0xb617),
            "Eye Door sweat spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(0xb62d),
            "adjacent Eye Door origin table is rejected as sweat mechanics");

        _ = ProbeEyeDoorSweatInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEyeDoorSweatInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Eye Door sweat allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Eye Door sweat mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Eye Door sweat instruction mechanics: eight compiled words, the complete " +
            "falling loop, real floor-impact handoff, three impact frames, shared shot " +
            "deletion, and four installed visual operands pass with mechanics bytes forbidden.");

        void ObserveFrame(RoomEnemyProjectileSlot projectile)
        {
            if (!projectile.IsActive || projectile.InstructionTimer.IsZero) return;
            ushort operand = unchecked((ushort)(projectile.InstructionPointer - 2));
            AssertEqual(operand, projectile.PresentationOperandAddress,
                "timed projectile frame retains its installed visual operand");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | (unchecked((ushort)(operand - 2)))),
                projectile.InstructionTimer.RawValue, "projectile frame duration matches cartridge data");
            observedOperands.Add(projectile.PresentationOperandAddress);
        }
        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = NativeCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, samus, Position16<RoomPixels>.Zero);
                ObserveFrame(projectile);
            }
        }
    }

    /// <summary>Alternates falling and impact words repeatedly to expose mechanics lookup allocations.</summary>
    private static int ProbeEyeDoorSweatInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EyeDoorSweatInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EyeDoorSweatInstructionProgramDefinitions.Initial
                    : EyeDoorSweatInstructionProgramDefinitions.Impact);
        }
        return checksum;
    }

    /// <summary>Runs Eye Door sweat instruction checks from the verification command dispatcher.</summary>
    [VerificationCommand("--eye-door-sweat-instruction-mechanics")]
    internal static int EyeDoorSweatInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyEyeDoorSweatInstructionProgramDefinitions), () => VerifyEyeDoorSweatInstructionProgramDefinitions());
        return 0;
    }
}
