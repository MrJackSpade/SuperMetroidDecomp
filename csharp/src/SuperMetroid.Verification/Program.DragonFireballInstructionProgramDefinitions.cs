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

internal static class DragonFireballInstructionProgramDefinitionsVerification
{
    internal static void VerifyDragonFireballInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDragonFireballInstructionProgramDefinitions), () => VerifyDragonFireballInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyDragonFireballInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < DragonFireballInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DragonFireballInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Dragon-fireball mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewDragonFireballInstructionReadGuard(rom);
        var selectedPresentation = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeDragon", instanceFlags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnDragonFireball", instanceFlags)!
            .CreateDelegate<Action<RoomEnemySlot, DragonEnemyState>>(enemies);
        var runPreInstruction = typeof(RoomEnemySystem).GetMethod(
                "RunDragonFireballPreInstruction",
                BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot, ushort>>();
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;

        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.Dragon;
        body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        body.Position = body.Position.WithWhole(new(0x0180, 0x00a0));
        body.VramTilesIndex = 0x0200;
        body.PaletteIndex = 0x0a00;
        initialize(body);
        DragonEnemyState state = enemies.DragonStates[0]!;

        state.DirectionWord = 0x8000;
        spawn(body, state);
        state.DirectionWord = 0;
        spawn(body, state);
        RoomEnemyProjectileSlot left = enemies.EnemyProjectiles.Single(
            projectile =>
                projectile.Kind == RoomEnemyProjectileKind.DragonFireball &&
                unchecked((short)projectile.Velocity.X.RawValue) < 0);
        RoomEnemyProjectileSlot right = enemies.EnemyProjectiles.Single(
            projectile =>
                projectile.Kind == RoomEnemyProjectileKind.DragonFireball &&
                unchecked((short)projectile.Velocity.X.RawValue) >= 0);
        AssertEqual(
            DragonFireballInstructionProgramDefinitions.RisingLeft,
            left.InstructionPointer,
            "left Dragon fireball selects its named rising loop");
        AssertEqual(
            DragonFireballInstructionProgramDefinitions.RisingRight,
            right.InstructionPointer,
            "right Dragon fireball selects its named rising loop");

        RunLoop(left, DragonFireballInstructionProgramDefinitions.RisingLeft, "rising left");
        RunLoop(right, DragonFireballInstructionProgramDefinitions.RisingRight, "rising right");

        left.Velocity = left.Velocity.WithY(new SnesSignedEightEight(0xfff0));
        right.Velocity = right.Velocity.WithY(new SnesSignedEightEight(0xfff0));
        runPreInstruction(left, 0);
        runPreInstruction(right, 0);
        AssertEqual(
            DragonFireballInstructionProgramDefinitions.FallingLeft,
            left.InstructionPointer,
            "left Dragon fireball zero crossing selects its falling loop");
        AssertEqual(
            DragonFireballInstructionProgramDefinitions.FallingRight,
            right.InstructionPointer,
            "right Dragon fireball zero crossing selects its falling loop");
        AssertEqual((ushort)1, left.InstructionTimer,
            "left Dragon fireball zero crossing requests a same-pass animation tick");
        AssertEqual((ushort)1, right.InstructionTimer,
            "right Dragon fireball zero crossing requests a same-pass animation tick");

        RunLoop(left, DragonFireballInstructionProgramDefinitions.FallingLeft, "falling left");
        RunLoop(right, DragonFireballInstructionProgramDefinitions.FallingRight, "falling right");

        left.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        left.InstructionTimer = 1;
        process.Invoke(enemies, [left, null, Position16<RoomPixels>.Zero]);
        AssertTrue(!left.IsActive,
            "Dragon's shared shot list deletes through the common projectile owner");

        AssertEqual(
            DragonFireballInstructionProgramDefinitions.PresentationWordCount,
            selectedPresentation.Count,
            "all Dragon-fireball installed presentation operands are selected");
        AssertEqual(0, guard.ObservedWords.Count,
            "Dragon-fireball installed presentation requires zero cartridge reads");
        for (int index = 0;
             index < DragonFireballInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                DragonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(selectedPresentation.Contains(address),
                $"production execution selects installed Dragon-fireball presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Dragon-fireball and shared-delete mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => DragonFireballInstructionProgramDefinitions.ReadMechanicsWord(0xb4c1),
            "Dragon-fireball spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DragonFireballInstructionProgramDefinitions.ReadMechanicsWord(0xb4ef),
            "adjacent Dragon-fireball initializer code is rejected as mechanics");

        _ = ProbeDragonFireballInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDragonFireballInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dragon-fireball allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dragon-fireball mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dragon-fireball instruction mechanics: sixteen compiled words, four complete " +
            "rising/falling loops, both zero-crossing handoffs, eight installed spritemap selections, " +
            "and shared shot deletion pass with mechanics bytes forbidden.");

        void RunLoop(
            RoomEnemyProjectileSlot projectile,
            ushort program,
            string name)
        {
            projectile.InstructionPointer = program;
            for (int step = 0; step < 3; step++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, null, Position16<RoomPixels>.Zero]);
                AssertEqual((ushort)(program + 2 + (step % 2) * 4), projectile.PresentationOperandAddress,
                    $"Dragon-fireball {name} selects the exact native frame operand");
                selectedPresentation.Add(projectile.PresentationOperandAddress);
            }
            AssertEqual(unchecked((ushort)(program + 4)), projectile.InstructionPointer,
                $"Dragon-fireball {name} loops and schedules its first frame again");
        }
    }

    private static int ProbeDragonFireballInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DragonFireballInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DragonFireballInstructionProgramDefinitions.RisingLeft
                    : DragonFireballInstructionProgramDefinitions.FallingRight);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewDragonFireballInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    DragonFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Dragon/shared mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                DragonFireballInstructionProgramDefinitions.PresentationWordCount,
                DragonFireballInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--dragon-fireball-instruction-mechanics")]
    internal static int DragonFireballInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDragonFireballInstructionProgramDefinitions), () => VerifyDragonFireballInstructionProgramDefinitions());
        return 0;
    }
}
