using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyKiHunterAcidSpitInstructionProgramDefinitions()
    {
        VerifyKiHunterAcidSpitInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKiHunterAcidSpitInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KiHunterAcidSpitInstructionMechanicsWord definition =
                KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKiHunterAcidSpitInstructionWord(rom, definition.Address),
                $"KiHunter acid-spit mechanics word $86:{definition.Address:X4}");
        }

        var guard = new KiHunterAcidSpitInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnKiHunterAcidSpit", flags)!;
        MethodInfo move = typeof(RoomEnemySystem).GetMethod(
            "RunKiHunterAcidMovement", flags)!;

        var body = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 112,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        spawn.Invoke(enemies, [body, false]);
        spawn.Invoke(enemies, [body, true]);

        RoomEnemyProjectileSlot left = Find(RoomEnemyProjectileKind.KiHunterAcidSpitLeft);
        RoomEnemyProjectileSlot right = Find(RoomEnemyProjectileKind.KiHunterAcidSpitRight);
        VerifyIntroduction(
            left,
            KiHunterAcidSpitInstructionProgramDefinitions.Left,
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Left);
        VerifyIntroduction(
            right,
            KiHunterAcidSpitInstructionProgramDefinitions.Right,
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Right);

        var blocks = new ushort[64];
        Array.Fill(blocks, (ushort)0x8000, 32, 32);
        var level = new RoomLevelData(
            8, 8, blocks, new byte[64], new ushort[64], new byte[8]);
        left.XPosition = 64;
        left.YPosition = 63;
        left.XRadius = 2;
        left.YRadius = 2;
        left.YVelocity = 0x0100;
        move.Invoke(enemies, [left, level]);
        AssertEqual(KiHunterAcidSpitInstructionProgramDefinitions.HitFloor,
            left.InstructionPointer,
            "real solid-floor collision installs the named acid-splash program");

        RunForcedTicks(left, 5);
        AssertEqual((ushort)0xcf6c, left.InstructionPointer,
            "KiHunter acid splash reaches deletion after all five authored frames");
        RunForcedTicks(left, 1);
        AssertTrue(!left.IsActive,
            "KiHunter acid splash deletes after its complete floor impact");

        right.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(right, 1);
        AssertTrue(!right.IsActive,
            "KiHunter acid shot reaction reaches the compiled shared delete program");

        AssertEqual(KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live KiHunter acid-spit spritemap operands remain cartridge reads");
        for (int index = 0;
             index < KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KiHunterAcidSpitInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads KiHunter acid presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled KiHunter acid and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => KiHunterAcidSpitInstructionProgramDefinitions.ReadMechanicsWord(0xcf36),
            "KiHunter acid spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KiHunterAcidSpitInstructionProgramDefinitions.ReadMechanicsWord(0xcf90),
            "adjacent KiHunter acid initializer is rejected as mechanics");

        _ = ProbeKiHunterAcidSpitInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKiHunterAcidSpitInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "KiHunter acid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed KiHunter acid mechanics lookups allocate no storage");

        Console.WriteLine(
            "KiHunter acid-spit instruction mechanics: twenty-seven compiled words, both " +
            "real directional producers, terminal sleeps, real floor impact, complete " +
            "splash, shared shot deletion, and nineteen live spritemap reads pass.");

        RoomEnemyProjectileSlot Find(RoomEnemyProjectileKind kind) =>
            enemies.EnemyProjectiles.Single(projectile => projectile.Kind == kind);

        void VerifyIntroduction(
            RoomEnemyProjectileSlot projectile,
            ushort initial,
            ushort installedPreInstruction)
        {
            AssertEqual(initial, projectile.InstructionPointer,
                $"real {projectile.Kind} producer selects its named introduction");
            RunForcedTicks(projectile, 7);
            AssertEqual(unchecked((ushort)(initial + 0x20)), projectile.InstructionPointer,
                $"{projectile.Kind} reaches its terminal sleep after all seven frames");
            AssertEqual(installedPreInstruction, projectile.PreInstruction,
                $"{projectile.Kind} installs its one-shot movement callback");
            RunForcedTicks(projectile, 1);
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{projectile.Kind} terminal sleep stops its instruction timer");
        }

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeKiHunterAcidSpitInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KiHunterAcidSpitInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KiHunterAcidSpitInstructionProgramDefinitions.Left
                    : KiHunterAcidSpitInstructionProgramDefinitions.HitFloor);
        }
        return checksum;
    }

    private static ushort ReadKiHunterAcidSpitInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class KiHunterAcidSpitInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KiHunterAcidSpitInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled KiHunter acid mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = KiHunterAcidSpitInstructionProgramDefinitions
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
