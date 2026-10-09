using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Loads the retail ROM and runs the Magdollite lava projectile program checks against it.
    /// </summary>
    private static void VerifyMagdolliteLavaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMagdolliteLavaInstructionProgramDefinitions), () => VerifyMagdolliteLavaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Verifies compiled mechanics against cartridge words and exercises both lava poses,
    /// the shot callback and deletion flow, installed visual operands, and guarded runtime reads.
    /// </summary>
    /// <param name="rom">The retail address space used to verify compiled instruction data and visuals.</param>
    private static void VerifyMagdolliteLavaInstructionProgramDefinitions(
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
                ReadMagdolliteLavaInstructionWord(rom, definition.Address),
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
                ReadMagdolliteLavaInstructionWord(rom, definition.Address),
                $"shared projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new MagdolliteLavaInstructionReadGuard(rom);
        var observedOperands = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_nextRandom", instanceFlags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 1));
        typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", instanceFlags)!.SetValue(
            enemies,
            new SamusState
            {
                Health = 99,
                MaxHealth = 99,
            });

        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnMagdolliteLava", instanceFlags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        RoomEnemySlot source = enemies.Slots[0];
        source.XPosition = 0x0120;
        source.YPosition = 0x0080;
        source.VramTilesIndex = 0x0200;
        source.PaletteIndex = 0x0c00;

        spawn(source, 0);
        spawn(source, 1);
        RoomEnemyProjectileSlot left = enemies.EnemyProjectiles.Single(
            projectile =>
                projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite &&
                projectile.DirectionParameter == 0);
        RoomEnemyProjectileSlot right = enemies.EnemyProjectiles.Single(
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
        process.Invoke(enemies, [left, null, (ushort)0, (ushort)0]);
        AssertTrue(!left.IsActive,
            "Magdollite shot program follows its native goto into shared delete");
        AssertTrue(enemies.LastMagdolliteLavaDropRequest is not null,
            "Magdollite shot program executes its native drop callback");
        // `$86:DFEA` allocates its pickup actor at the shot projectile's position.
        RoomEnemyProjectileSlot drop = enemies.EnemyProjectiles.Single(
            projectile =>
                projectile.IsActive &&
                projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup);
        AssertEqual(source.XPosition, drop.XPosition,
            "Magdollite drop preserves projectile X");
        AssertEqual(unchecked((ushort)(source.YPosition + 2)),
            drop.YPosition,
            "Magdollite drop preserves projectile Y");

        AssertEqual(
            MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount,
            observedOperands.Count,
            "both live Magdollite-lava spritemap operands are selected from installed artwork");
        AssertEqual(0, guard.ObservedPresentationWords.Count, "compiled visual operands require no cartridge reads");
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

        AssertEqual(0, guard.ObservedPresentationWords.Count,
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
            AssertEqual(ReadMagdolliteLavaInstructionWord(rom, unchecked((ushort)(operand - 2))),
                projectile.InstructionTimer, "projectile frame duration matches cartridge data");
            observedOperands.Add(projectile.PresentationOperandAddress);
        }
        void RunToSleep(
            RoomEnemyProjectileSlot projectile,
            ushort program,
            string direction)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
            ObserveFrame(projectile);
            AssertEqual(unchecked((ushort)(program + 4)), projectile.InstructionPointer,
                $"{direction} Magdollite-lava pose reaches terminal sleep");
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
            ObserveFrame(projectile);
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{direction} Magdollite-lava terminal sleep parks the program");
        }
    }

    /// <summary>
    /// Repeatedly reads the left/shot and shared-delete mechanics words to measure warmed lookup allocations.
    /// </summary>
    /// <returns>A checksum that ensures the probed mechanics values are consumed.</returns>
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

    /// <summary>
    /// Reads a little-endian word from adjacent bytes in the enemy-projectile code bank.
    /// </summary>
    /// <param name="source">The cartridge address space containing bank 86 data.</param>
    /// <param name="address">The bank-relative address of the word's low byte.</param>
    /// <returns>The value formed from the addressed byte and the following byte.</returns>
    private static ushort ReadMagdolliteLavaInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Wraps the cartridge bus to reject runtime reads of compiled Magdollite or shared
    /// projectile mechanics and record accesses to Magdollite presentation words.
    /// </summary>
    /// <param name="source">The underlying address space receiving allowed reads and all writes.</param>
    private sealed class MagdolliteLavaInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Bank 86 presentation-word addresses observed through this guard.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>The number of attempted reads from compiled Magdollite or shared mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte request through the guard's runtime-read checks.</summary>
        /// <param name="address">The absolute cartridge address to read.</param>
        /// <returns>The underlying byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads of compiled mechanics, records presentation-word accesses, and forwards
        /// all other reads to the wrapped address space.
        /// </summary>
        /// <param name="address">The absolute address to read.</param>
        /// <returns>The underlying byte for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The requested byte is part of compiled mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (MagdolliteLavaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Magdollite/shared mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = MagdolliteLavaInstructionProgramDefinitions
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

        /// <summary>Forwards the write without changing its address or value.</summary>
        /// <param name="address">The absolute address receiving the byte.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
