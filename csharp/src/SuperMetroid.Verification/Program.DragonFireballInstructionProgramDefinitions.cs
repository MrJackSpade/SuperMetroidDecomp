using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and verifies compiled Dragon-fireball instruction data and execution behavior.</summary>
    private static void VerifyDragonFireballInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDragonFireballInstructionProgramDefinitions), () => VerifyDragonFireballInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with bank 86 and exercises both projectile directions through their rising and falling loops.</summary>
    /// <param name="rom">Retail address space containing Dragon-fireball instruction words and presentation operands.</param>
    private static void VerifyDragonFireballInstructionProgramDefinitions(
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
                ReadDragonFireballInstructionWord(rom, definition.Address),
                $"Dragon-fireball mechanics word $86:{definition.Address:X4}");
        }

        var guard = new DragonFireballInstructionReadGuard(rom);
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
            selectedPresentation.Count,
            "all Dragon-fireball installed presentation operands are selected");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
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
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
                AssertEqual((ushort)(program + 2 + (step % 2) * 4), projectile.PresentationOperandAddress,
                    $"Dragon-fireball {name} selects the exact native frame operand");
                selectedPresentation.Add(projectile.PresentationOperandAddress);
            }
            AssertEqual(unchecked((ushort)(program + 4)), projectile.InstructionPointer,
                $"Dragon-fireball {name} loops and schedules its first frame again");
        }
    }

    /// <summary>Repeatedly reads representative rising and falling mechanics words for the warmed-allocation check.</summary>
    /// <returns>A checksum that keeps the repeated lookups observable.</returns>
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

    /// <summary>Reads one little-endian projectile instruction word from the code bank.</summary>
    /// <param name="source">Retail address space containing the projectile code bank.</param>
    /// <param name="address">Offset of the low byte within that bank.</param>
    /// <returns>The word formed by the addressed byte and its successor.</returns>
    private static ushort ReadDragonFireballInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects reads of compiled Dragon and shared projectile mechanics while recording presentation-selector reads.</summary>
    /// <param name="source">Wrapped address space used for reads and writes permitted by the guard.</param>
    private sealed class DragonFireballInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets Dragon-fireball presentation-word offsets whose bytes production requested.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets attempts to read bytes owned by compiled Dragon or shared projectile mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte request through the guard's read policy.</summary>
        /// <param name="address">Cartridge bus address to read.</param>
        /// <returns>The byte returned by the wrapped source when permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics, records presentation-selector reads, and forwards other bytes.</summary>
        /// <param name="address">Bus address of the requested byte.</param>
        /// <returns>The byte returned by the wrapped source when the guard permits the read.</returns>
        public byte ReadByte(int address)
        {
            if (DragonFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
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

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte written at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
