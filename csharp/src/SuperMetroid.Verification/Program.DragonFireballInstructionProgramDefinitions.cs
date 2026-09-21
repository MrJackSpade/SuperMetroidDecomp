using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDragonFireballInstructionProgramDefinitions()
    {
        VerifyDragonFireballInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyDragonFireballInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < DragonFireballInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DragonFireballInstructionMechanicsWord definition =
                DragonFireballInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadDragonFireballInstructionWord(rom, definition.Address),
                $"Dragon-fireball mechanics word $86:{definition.Address:X4}");
        }

        var guard = new DragonFireballInstructionReadGuard(rom);
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
        body.EnemyDefinitionPointer = RoomEnemySystem.DragonDefinition;
        body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        body.XPosition = 0x0180;
        body.YPosition = 0x00a0;
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
                unchecked((short)projectile.XVelocity) < 0);
        RoomEnemyProjectileSlot right = enemies.EnemyProjectiles.Single(
            projectile =>
                projectile.Kind == RoomEnemyProjectileKind.DragonFireball &&
                unchecked((short)projectile.XVelocity) >= 0);
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

        left.YVelocity = 0xfff0;
        right.YVelocity = 0xfff0;
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
        process.Invoke(enemies, [left, null, (ushort)0, (ushort)0]);
        AssertTrue(!left.IsActive,
            "Dragon's shared shot list deletes through the common projectile owner");

        AssertEqual(
            DragonFireballInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Dragon-fireball spritemap operands remain cartridge reads");
        for (int index = 0;
             index < DragonFireballInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                DragonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Dragon-fireball presentation $86:{address:X4}");
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
            "rising/falling loops, both zero-crossing handoffs, eight live spritemap reads, " +
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
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
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

    private static ushort ReadDragonFireballInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class DragonFireballInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (DragonFireballInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dragon/shared mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DragonFireballInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = DragonFireballInstructionProgramDefinitions
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
