using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMagdolliteLavaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMagdolliteLavaInstructionProgramDefinitions), () => VerifyMagdolliteLavaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

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
             index < CommonEnemyProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CommonEnemyProjectileInstructionProgramDefinitions.MechanicsWord(index);
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
        AssertEqual(source.XPosition, enemies.LastMagdolliteLavaDropRequest!.Value.X,
            "Magdollite drop preserves projectile X");
        AssertEqual(unchecked((ushort)(source.YPosition + 2)),
            enemies.LastMagdolliteLavaDropRequest!.Value.Y,
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

    private static ushort ReadMagdolliteLavaInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class MagdolliteLavaInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (MagdolliteLavaInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
