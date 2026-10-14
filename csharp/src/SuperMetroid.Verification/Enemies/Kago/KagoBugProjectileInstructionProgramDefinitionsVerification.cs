using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Kago;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Kago bug projectile landing, jump, and shot programs against compiled native words.</summary>
internal static class KagoBugProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Kago bug program comparison.</summary>
    internal static void VerifyKagoBugProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKagoBugProjectileInstructionProgramDefinitions), () => VerifyKagoBugProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs a real spawn through movement loops and its drop-producing shot sequence under read guards.</summary>
    internal static void VerifyKagoBugProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KagoBugProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                KagoBugProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Kago-bug projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewKagoBugProjectileInstructionGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0x0002);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x0080);
        enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState { Health = 99, MaxHealth = 99 };

        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = source.Position.WithWhole(new(128, 112));
        source.VramTilesIndex = 0x0200;
        source.PaletteIndex = 0x0c00;
        AssertTrue((bool)enemies.KagoBehavior.SpawnKagoBug(source),
            "real Kago producer allocates its bug");

        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(
            candidate => candidate.Kind == RoomEnemyProjectileKind.KagoBug);
        AssertTrue(projectile.Animation is KraidRockAnimation { Program: KraidRockProgram.SharedPose },
            "real Kago producer retains the cartridge's shared Kraid-rock initial pose");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Landed;
        RunForcedTicks(projectile, 3);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.Landed + 4)),
            projectile.InstructionPointer,
            "Kago landed program runs its callback and loops");
        AssertEqual(
            (ushort)EnemyProjectilePreInstruction.KagoBug_Idle,
            projectile.PreInstruction,
            "Kago landed callback restores idle movement");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Falling;
        RunForcedTicks(projectile, 2);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.Falling + 4)),
            projectile.InstructionPointer,
            "Kago falling pose completes its loop");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.JumpStart;
        RunForcedTicks(projectile, 3);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.JumpLoop + 4)),
            projectile.InstructionPointer,
            "Kago jump introduction reaches the looping airborne pose");
        AssertEqual(
            (ushort)EnemyProjectilePreInstruction.KagoBug_Jumping,
            projectile.PreInstruction,
            "Kago jump callback installs airborne movement");
        RunForcedTicks(projectile, 1);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.JumpLoop + 4)),
            projectile.InstructionPointer,
            "Kago airborne pose loops");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Shot;
        projectile.GraphicsIndex = 0xffff;
        RunForcedTicks(projectile, 1);
        AssertEqual((ushort)0, projectile.GraphicsIndex,
            "Kago shot program applies its palette-zero callback");
        RunForcedTicks(projectile, 4);
        AssertEqual((ushort)0xd07a, projectile.InstructionPointer,
            "Kago shot program displays all five explosion frames");
        RunForcedTicks(projectile, 1);
        AssertTrue(!projectile.IsActive,
            "Kago shot program requests its drop and reaches shared deletion");
        AssertTrue(enemies.KagoBehavior.LastKagoBugDropRequest is not null,
            "Kago shot program emits its production drop request before deletion");

        AssertEqual(0, guard.ObservedWords.Count, "compiled Kago bug visuals require no cartridge reads");
        for (int index = 0;
             index < KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KagoBugProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "compiled Kago bug selector exists");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "compiled Kago bug selector matches original operand");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Kago, shared initial-pose, and shared-delete mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => KagoBugProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd03e),
            "Kago-bug spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KagoBugProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd080),
            "unreferenced duplicate delete list is rejected as Kago-bug mechanics");

        _ = ProbeKagoBugProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKagoBugProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kago-bug allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kago-bug projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kago-bug projectile instruction mechanics: twenty-three private words, " +
            "the real producer, landed/falling/jump loops, complete shot/drop deletion, " +
            "and eleven native compiled spritemap operands pass with mechanics bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot target, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                target.InstructionTimer = NativeCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(target, new SamusState(), Position16<RoomPixels>.Zero);
            }
        }
    }

    /// <summary>Alternates landed and shot words to detect allocation during warmed instruction reads.</summary>
    private static int ProbeKagoBugProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KagoBugProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KagoBugProjectileInstructionProgramDefinitions.Landed
                    : KagoBugProjectileInstructionProgramDefinitions.Shot);
        }
        return checksum;
    }

    /// <summary>Rejects Kago and shared-program mechanics reads while observing sprite operand access.</summary>
    private static CartridgeReadGuard NewKagoBugProjectileInstructionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                KagoBugProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Kago-bug projectile mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount, KagoBugProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Kago bug instruction check from the command dispatcher.</summary>
    [VerificationCommand("--kago-bug-projectile-instruction-mechanics")]
    internal static int KagoBugProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKagoBugProjectileInstructionProgramDefinitions), () => VerifyKagoBugProjectileInstructionProgramDefinitions());
        return 0;
    }
}
