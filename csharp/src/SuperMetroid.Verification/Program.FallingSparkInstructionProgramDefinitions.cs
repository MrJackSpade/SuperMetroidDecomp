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
using static SuiteTimingVerification;

internal static class FallingSparkInstructionProgramDefinitionsVerification
{
    internal static void VerifyFallingSparkInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFallingSparkInstructionProgramDefinitions), () => VerifyFallingSparkInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyFallingSparkInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < FallingSparkInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                FallingSparkInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Falling Spark mechanics word $86:{definition.Address:X4}");
        }

        var guard = new CartridgeReadGuard(rom)
            .Forbid(FallingSparkInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Falling Spark mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                FallingSparkInstructionProgramDefinitions.PresentationWordCount,
                FallingSparkInstructionProgramDefinitions.PresentationWordAddress);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
        Action<RoomEnemySlot> spawn = enemies.FallingSparksBehavior.SpawnFallingSpark;
        Action<RoomEnemyProjectileSlot> beginFloorImpact = FallingSparksBehavior.BeginFallingSparkFloorImpact;

        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = source.Position.WithWhole(new(0x0120, 0x0060));
        spawn(source);
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(
            candidate => candidate.Kind == RoomEnemyProjectileKind.FallingSpark);
        AssertEqual(
            FallingSparkInstructionProgramDefinitions.Falling,
            projectile.InstructionPointer,
            "Falling Spark definition selects the named falling program");

        var observedOperands = new HashSet<ushort>();
        for (int frame = 0; frame < 4; frame++)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, (SamusState?)null, Position16<RoomPixels>.Zero);
            AssertEqual((ushort)(FallingSparkInstructionProgramDefinitions.Falling + 2 + (frame % 3) * 4),
                projectile.PresentationOperandAddress, "falling spark frame order and loop restart");
            AssertEqual((ushort)3, projectile.InstructionTimer.RawValue, "falling spark frame duration");
            observedOperands.Add(projectile.PresentationOperandAddress);
        }
        AssertEqual(
            unchecked((ushort)(FallingSparkInstructionProgramDefinitions.Falling + 4)),
            projectile.InstructionPointer,
            "falling program loops to its first timed frame");

        beginFloorImpact(projectile);
        AssertEqual(
            FallingSparkInstructionProgramDefinitions.HitFloor,
            projectile.InstructionPointer,
            "floor collision selects the named impact program");
        for (int frame = 0; frame < 11; frame++)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, (SamusState?)null, Position16<RoomPixels>.Zero);
            AssertEqual((ushort)(FallingSparkInstructionProgramDefinitions.HitFloor + 2 + frame * 4),
                projectile.PresentationOperandAddress, "falling spark impact blink frame order");
            AssertEqual((ushort)1, projectile.InstructionTimer.RawValue, "falling spark impact frame duration");
            observedOperands.Add(projectile.PresentationOperandAddress);
        }
        AssertEqual(
            FallingSparkInstructionProgramDefinitions.HitFloorTerminalDelete,
            projectile.InstructionPointer,
            "floor impact reaches the native terminal delete");
        projectile.InstructionTimer = NativeCountdown.Start(1);
        enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, (SamusState?)null, Position16<RoomPixels>.Zero);
        AssertTrue(!projectile.IsActive,
            "floor impact deletes Falling Spark after all eleven blinking frames");

        AssertEqual(
            FallingSparkInstructionProgramDefinitions.PresentationWordCount,
            observedOperands.Count,
            "all Falling Spark installed visual operands are selected");
        for (int index = 0;
             index < FallingSparkInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                FallingSparkInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(
                observedOperands.Contains(address),
                $"production execution selects Falling Spark presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ObservedWords.Count,
            "Falling Spark presentation does not read cartridge bytes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Falling Spark mechanics byte");
        AssertThrows<InvalidDataException>(
            () => FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(0xf355),
            "Falling Spark spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(0xf391),
            "adjacent Falling Spark initializer code is rejected as mechanics");

        _ = ProbeFallingSparkInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFallingSparkInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Falling Spark allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Falling Spark mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Falling Spark instruction mechanics: seventeen compiled words, complete " +
            "falling/floor-impact execution, and fourteen installed visual operands pass " +
            "with mechanics bytes forbidden.");
    }

    private static int ProbeFallingSparkInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FallingSparkInstructionProgramDefinitions.Falling
                    : FallingSparkInstructionProgramDefinitions.HitFloor);
        }
        return checksum;
    }

    [VerificationCommand("--falling-spark-instruction-mechanics")]
    internal static int FallingSparkInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFallingSparkInstructionProgramDefinitions), () => VerifyFallingSparkInstructionProgramDefinitions());
        return 0;
    }
}
