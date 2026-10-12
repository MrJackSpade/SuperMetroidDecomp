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
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Represents magdollite lava instruction program definitions verification.</summary>
internal static class MagdolliteLavaInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies magdollite lava instruction program definitions.</summary>
    internal static void VerifyMagdolliteLavaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMagdolliteLavaInstructionProgramDefinitions), () => VerifyMagdolliteLavaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies magdollite lava instruction program definitions.</summary>
    internal static void VerifyMagdolliteLavaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < MagdolliteLavaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MagdolliteLavaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Magdollite-lava mechanics word $86:{definition.Address:X4}");
        }

        for (int index = 0;
             index < CommonEnemyProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"shared projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewMagdolliteLavaInstructionGuard(rom);
        var observedOperands = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 1);
        enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState
            {
                Health = 99,
                MaxHealth = 99,
            };

        Action<RoomEnemySlot, ushort> spawn = enemies.MagdolliteBehavior.SpawnMagdolliteLava;
        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = source.Position.WithWhole(new(0x0120, 0x0080));
        source.VramTilesIndex = 0x0200;
        source.PaletteIndex = 0x0c00;

        spawn(source, 0);
        spawn(source, 1);
        RoomEnemyProjectileSlot left = enemies.Projectiles.EnemyProjectiles.Single(
            projectile =>
                projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite &&
                projectile.DirectionParameter == 0);
        RoomEnemyProjectileSlot right = enemies.Projectiles.EnemyProjectiles.Single(
            projectile =>
                projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite &&
                projectile.DirectionParameter == 1);
        AssertEqual(
            MagdolliteLavaInstructionProgramDefinitions.Left,
            left.InstructionPointer,
            "left-facing Magdollite lava selects the named program");
        AssertEqual(
            MagdolliteLavaInstructionProgramDefinitions.Right,
            right.InstructionPointer,
            "right-facing Magdollite lava selects the named program");

        RunToSleep(left, MagdolliteLavaInstructionProgramDefinitions.Left, "left");
        RunToSleep(right, MagdolliteLavaInstructionProgramDefinitions.Right, "right");

        left.InstructionPointer = MagdolliteLavaInstructionProgramDefinitions.Shot;
        left.InstructionTimer = 1;
        enemies.Projectiles.ProcessEnemyProjectileInstructions(left, null, Position16<RoomPixels>.Zero);
        AssertTrue(!left.IsActive,
            "Magdollite shot program follows its native goto into shared delete");
        AssertTrue(enemies.MagdolliteBehavior.LastMagdolliteLavaDropRequest is not null,
            "Magdollite shot program executes its native drop callback");
        // `$86:DFEA` allocates its pickup actor at the shot projectile's position.
        RoomEnemyProjectileSlot drop = enemies.Projectiles.EnemyProjectiles.Single(
            projectile =>
                projectile.IsActive &&
                projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup);
        AssertEqual(source.Position.X.Whole, drop.Position.X.Whole,
            "Magdollite drop preserves projectile X");
        AssertEqual(unchecked((ushort)(source.Position.Y.Whole + 2)),
            drop.Position.Y.Whole,
            "Magdollite drop preserves projectile Y");

        AssertEqual(
            MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount,
            observedOperands.Count,
            "both live Magdollite-lava spritemap operands are selected from installed artwork");
        AssertEqual(0, guard.ObservedWords.Count, "compiled visual operands require no cartridge reads");
        for (int index = 0;
             index < MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                MagdolliteLavaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(observedOperands.Contains(address),
                $"production execution selects Magdollite-lava presentation $86:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                $"compiled visual selector exists at $86:{address:X4}");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                $"compiled visual selector matches native operand $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "projectile visuals do not read cartridge bytes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Magdollite-lava and shared-delete mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MagdolliteLavaInstructionProgramDefinitions.ReadMechanicsWord(0xdfda),
            "Magdollite-lava spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MagdolliteLavaInstructionProgramDefinitions.ReadMechanicsWord(0xdfea),
            "adjacent Magdollite-lava callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CommonEnemyProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x84fe),
            "adjacent shared projectile data is rejected as mechanics");

        _ = ProbeMagdolliteLavaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMagdolliteLavaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Magdollite-lava allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Magdollite-lava and shared mechanics lookups allocate no storage");

        Console.WriteLine(
            "Magdollite-lava instruction mechanics: seven private words, one shared " +
            "delete word, both directional poses, shot/drop/delete execution, and two " +
            "installed visual operands pass with mechanics bytes forbidden.");

        void ObserveFrame(RoomEnemyProjectileSlot projectile)
        {
            if (!projectile.IsActive || projectile.InstructionTimer == 0) return;
            ushort operand = unchecked((ushort)(projectile.InstructionPointer - 2));
            AssertEqual(operand, projectile.PresentationOperandAddress,
                "timed projectile frame retains its installed visual operand");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | (unchecked((ushort)(operand - 2)))),
                projectile.InstructionTimer, "projectile frame duration matches cartridge data");
            observedOperands.Add(projectile.PresentationOperandAddress);
        }
        void RunToSleep(
            RoomEnemyProjectileSlot projectile,
            ushort program,
            string direction)
        {
            projectile.InstructionTimer = 1;
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
            ObserveFrame(projectile);
            AssertEqual(unchecked((ushort)(program + 4)), projectile.InstructionPointer,
                $"{direction} Magdollite-lava pose reaches terminal sleep");
            projectile.InstructionTimer = 1;
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
            ObserveFrame(projectile);
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{direction} Magdollite-lava terminal sleep parks the program");
        }
    }

    /// <summary>Probes magdollite lava instruction mechanics allocation.</summary>
    private static int ProbeMagdolliteLavaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MagdolliteLavaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MagdolliteLavaInstructionProgramDefinitions.Left
                    : MagdolliteLavaInstructionProgramDefinitions.Shot);
            _ = CommonEnemyProjectileInstructionProgramDefinitions.TryReadMechanicsWord(
                CommonEnemyProjectileInstructionProgramDefinitions.Delete,
                out ushort sharedWord);
            checksum += sharedWord;
        }
        return checksum;
    }

    /// <summary>New magdollite lava instruction guard.</summary>
    private static CartridgeReadGuard NewMagdolliteLavaInstructionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                MagdolliteLavaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Magdollite/shared mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount, MagdolliteLavaInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Magdollite lava instruction mechanics command.</summary>
    [VerificationCommand("--magdollite-lava-instruction-mechanics")]
    internal static int MagdolliteLavaInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMagdolliteLavaInstructionProgramDefinitions), () => VerifyMagdolliteLavaInstructionProgramDefinitions());
        return 0;
    }
}
