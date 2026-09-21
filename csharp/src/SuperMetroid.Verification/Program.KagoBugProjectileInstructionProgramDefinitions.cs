using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKagoBugProjectileInstructionProgramDefinitions()
    {
        VerifyKagoBugProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKagoBugProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KagoBugProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KagoBugProjectileInstructionMechanicsWord definition =
                KagoBugProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKagoBugProjectileInstructionWord(rom, definition.Address),
                $"Kago-bug projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new KagoBugProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0x0002));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0x0080));
        typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
            enemies,
            new SamusState { Health = 99, MaxHealth = 99 });

        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod("SpawnKagoBug", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        RoomEnemySlot source = enemies.Slots[0];
        source.XPosition = 128;
        source.YPosition = 112;
        source.VramTilesIndex = 0x0200;
        source.PaletteIndex = 0x0c00;
        AssertTrue((bool)spawn.Invoke(enemies, [source])!,
            "real Kago producer allocates its bug");

        RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
            candidate => candidate.Kind == RoomEnemyProjectileKind.KagoBug);
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            projectile.InstructionPointer,
            "real Kago producer retains the cartridge's shared Kraid-rock initial pose");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Landed;
        RunForcedTicks(projectile, 3);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.Landed + 4)),
            projectile.InstructionPointer,
            "Kago landed program runs its callback and loops");
        AssertEqual(
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Idle,
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
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Jumping,
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
        AssertTrue(enemies.LastKagoBugDropRequest is not null,
            "Kago shot program emits its production drop request before deletion");

        AssertEqual(KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Kago-bug spritemap operands remain cartridge reads");
        for (int index = 0;
             index < KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KagoBugProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Kago-bug presentation $86:{address:X4}");
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
            "and eleven live spritemap reads pass with mechanics bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot target, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                target.InstructionTimer = 1;
                process.Invoke(enemies, [target, new SamusState(), (ushort)0, (ushort)0]);
            }
        }
    }

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

    private static ushort ReadKagoBugProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class KagoBugProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KagoBugProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                KraidRockProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kago-bug projectile mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = KagoBugProjectileInstructionProgramDefinitions
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
