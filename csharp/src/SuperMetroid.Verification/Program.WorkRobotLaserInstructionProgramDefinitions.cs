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

internal static class WorkRobotLaserInstructionProgramDefinitionsVerification
{
    internal static void VerifyWorkRobotLaserInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWorkRobotLaserInstructionProgramDefinitions), () => VerifyWorkRobotLaserInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyWorkRobotLaserInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < WorkRobotLaserInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                WorkRobotLaserInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Work Robot laser mechanics word $86:{definition.Address:X4}");
        }

        var observedInstalledOperands = new HashSet<ushort>();
        var guard = NewWorkRobotLaserInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);

        var robot = new RoomEnemySlot(0)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            Radius = new(12, 16),
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var state = new WorkRobotEnemyState(robot);
        var definitions = new (WorkRobotLaser Definition, ushort XVelocity)[]
        {
            (WorkRobotLaser.UpLeft, unchecked((ushort)-0x0200)),
            (WorkRobotLaser.Horizontal, unchecked((ushort)-0x0200)),
            (WorkRobotLaser.DownLeft, unchecked((ushort)-0x0200)),
            (WorkRobotLaser.UpRight, 0x0200),
            (WorkRobotLaser.DownRight, 0x0200),
        };

        foreach ((WorkRobotLaser definition, ushort xVelocity) in definitions)
        {
            state.LaserXVelocity = xVelocity;
            enemies.WorkRobotBehavior.SpawnWorkRobotLaser(robot, state, definition, Position16<RoomPixels>.Zero);
        }

        RoomEnemyProjectileSlot[] lasers = enemies.Projectiles.EnemyProjectiles
            .Where(projectile => WorkRobotLaserInstructionProgramDefinitions.Owns(projectile.Kind))
            .ToArray();
        AssertEqual(5, lasers.Length,
            "all five real Work Robot laser definitions spawn through production code");
        foreach (RoomEnemyProjectileSlot laser in lasers)
        {
            AssertEqual(WorkRobotLaserInstructionProgramDefinitions.Initial,
                laser.InstructionPointer,
                "real Work Robot laser producer selects the named animation prefix");
            RunForcedTicks(laser, 8);
            AssertEqual(
                unchecked((ushort)(WorkRobotLaserInstructionProgramDefinitions.Loop + 4)),
                laser.InstructionPointer,
                "Work Robot laser completes its prefix and full loop before returning");
            AssertTrue(laser.IsActive,
                "Work Robot laser loop remains active until collision or a shot deletes it");
        }

        lasers[0].InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(lasers[0], 1);
        AssertTrue(!lasers[0].IsActive,
            "Work Robot laser shot reaction reaches the compiled shared delete program");

        AssertEqual(WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount,
            observedInstalledOperands.Count,
            "all Work Robot laser presentation operands select installed artwork");
        AssertEqual(0, guard.ObservedWords.Count, "Work Robot laser presentation requires no runtime cartridge reads");
        for (int index = 0;
             index < WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = WorkRobotLaserInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(observedInstalledOperands.Contains(address),
                $"production execution selects installed Work Robot laser presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Work Robot laser and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => WorkRobotLaserInstructionProgramDefinitions.ReadMechanicsWord(0xd2ee),
            "Work Robot laser spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WorkRobotLaserInstructionProgramDefinitions.ReadMechanicsWord(0xd30c),
            "adjacent Work Robot laser initializer is rejected as mechanics");

        _ = ProbeWorkRobotLaserInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWorkRobotLaserInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Work Robot laser allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Work Robot laser mechanics lookups allocate no storage");

        Console.WriteLine(
            "Work Robot laser instruction mechanics: nine compiled words, all five real " +
            "definitions, complete prefix/loop execution, shared shot deletion, and seven " +
            "installed presentation operands pass without runtime cartridge reads.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                ushort nativeCursor = projectile.InstructionPointer;
                // The final native Goto operand returns to the fourth frame, after the prefix.
                if (CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | nativeCursor) == (ushort)EnemyProjectileInstruction.GotoY)
                    nativeCursor = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | ((ushort)(nativeCursor + 2)));
                projectile.InstructionTimer = 1;
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
                if (projectile.IsActive)
                {
                    ushort expectedOperand = (ushort)(nativeCursor + 2);
                    AssertEqual(expectedOperand, projectile.PresentationOperandAddress,
                        "Work Robot laser selects native presentation operand after instruction control flow");
                    AssertTrue(EnemyProjectilePresentationFrameDefinitions.Contains(expectedOperand),
                        "Work Robot laser selected operand belongs to installed artwork");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | nativeCursor), projectile.InstructionTimer,
                        "Work Robot laser native duration survives calculated mechanics");
                    observedInstalledOperands.Add(projectile.PresentationOperandAddress);
                }
            }
        }
    }

    private static int ProbeWorkRobotLaserInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WorkRobotLaserInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? WorkRobotLaserInstructionProgramDefinitions.Initial
                    : WorkRobotLaserInstructionProgramDefinitions.LoopCommand);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewWorkRobotLaserInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => WorkRobotLaserInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Work Robot laser mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, WorkRobotLaserInstructionProgramDefinitions.PresentationWordCount, WorkRobotLaserInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--work-robot-laser-instruction-mechanics")]
    internal static int WorkRobotLaserInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyWorkRobotLaserInstructionProgramDefinitions), () => VerifyWorkRobotLaserInstructionProgramDefinitions());
        return 0;
    }
}
