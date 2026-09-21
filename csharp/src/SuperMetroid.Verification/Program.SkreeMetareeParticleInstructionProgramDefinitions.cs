using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySkreeMetareeParticleInstructionProgramDefinitions()
    {
        VerifySkreeMetareeParticleInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifySkreeMetareeParticleInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            SkreeMetareeParticleInstructionMechanicsWord definition =
                SkreeMetareeParticleInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadSkreeMetareeParticleInstructionWord(rom, definition.Address),
                $"Skree/Metaree particle mechanics word $86:{definition.Address:X4}");
        }

        var guard = new SkreeMetareeParticleInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        var spawnSkreeBurst = typeof(RoomEnemySystem).GetMethod(
                "SpawnSkreeParticleBurst",
                instanceFlags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var spawnMetareeBurst = typeof(RoomEnemySystem).GetMethod(
                "SpawnMetareeParticleBurst",
                instanceFlags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var source = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 96,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var samus = new SamusState();

        spawnSkreeBurst(source);
        RoomEnemyProjectileSlot[] skree = enemies.EnemyProjectiles
            .Where(projectile => projectile.IsActive)
            .ToArray();
        AssertEqual(4, skree.Length, "Skree burst allocates all four native particles");
        AssertTrue(skree.All(projectile =>
                projectile.Kind is
                    RoomEnemyProjectileKind.SkreeParticleDownRight or
                    RoomEnemyProjectileKind.SkreeParticleUpRight or
                    RoomEnemyProjectileKind.SkreeParticleDownLeft or
                    RoomEnemyProjectileKind.SkreeParticleUpLeft),
            "Skree burst uses all four Skree projectile owners");
        AssertTrue(skree.All(projectile => projectile.InstructionPointer ==
                SkreeMetareeParticleInstructionProgramDefinitions.Skree),
            "Skree burst selects the named Skree particle program");
        RunCompleteLoops(
            skree,
            SkreeMetareeParticleInstructionProgramDefinitions.Skree);

        spawnMetareeBurst(source);
        RoomEnemyProjectileSlot[] metaree = enemies.EnemyProjectiles
            .Where(projectile => projectile.IsActive)
            .ToArray();
        AssertEqual(4, metaree.Length, "Metaree burst allocates all four native particles");
        AssertTrue(metaree.All(projectile =>
                projectile.Kind is
                    RoomEnemyProjectileKind.MetareeParticleDownRight or
                    RoomEnemyProjectileKind.MetareeParticleUpRight or
                    RoomEnemyProjectileKind.MetareeParticleDownLeft or
                    RoomEnemyProjectileKind.MetareeParticleUpLeft),
            "Metaree burst uses all four Metaree projectile owners");
        AssertTrue(metaree.All(projectile => projectile.InstructionPointer ==
                SkreeMetareeParticleInstructionProgramDefinitions.Metaree),
            "Metaree burst selects the named Metaree particle program");
        RunCompleteLoops(
            metaree,
            SkreeMetareeParticleInstructionProgramDefinitions.Metaree);

        AssertEqual(
            SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "both live Skree/Metaree particle spritemap operands remain cartridge reads");
        for (int index = 0;
             index < SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = SkreeMetareeParticleInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads particle presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled particle and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeParticleInstructionProgramDefinitions.ReadMechanicsWord(0x8abf),
            "Skree particle spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeParticleInstructionProgramDefinitions.ReadMechanicsWord(0x8acd),
            "adjacent particle initializer code is rejected as mechanics");

        _ = ProbeSkreeMetareeParticleInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSkreeMetareeParticleInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "particle allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Skree/Metaree particle mechanics lookups allocate no storage");

        Console.WriteLine(
            "Skree/Metaree particle instruction mechanics: six compiled words, all " +
            "eight real burst owners, two complete loops, shared shot deletion, and " +
            "both live spritemap reads pass with mechanics bytes forbidden.");

        void RunCompleteLoops(
            IEnumerable<RoomEnemyProjectileSlot> projectiles,
            ushort expectedInitial)
        {
            foreach (RoomEnemyProjectileSlot projectile in projectiles)
            {
                RunForcedTick(projectile);
                RunForcedTick(projectile);
                AssertTrue(projectile.IsActive,
                    $"{projectile.Kind} remains active after its complete animation loop");
                AssertEqual(
                    unchecked((ushort)(expectedInitial + 4)),
                    projectile.InstructionPointer,
                    $"{projectile.Kind} returns to its first timed frame");

                projectile.InstructionPointer =
                    CommonEnemyProjectileInstructionProgramDefinitions.Delete;
                RunForcedTick(projectile);
                AssertTrue(!projectile.IsActive,
                    $"{projectile.Kind} shot reaction reaches shared delete");
            }
        }

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, samus, (ushort)0, (ushort)0]);
        }
    }

    private static int ProbeSkreeMetareeParticleInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SkreeMetareeParticleInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SkreeMetareeParticleInstructionProgramDefinitions.Skree
                    : SkreeMetareeParticleInstructionProgramDefinitions.Metaree);
        }
        return checksum;
    }

    private static ushort ReadSkreeMetareeParticleInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class SkreeMetareeParticleInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (SkreeMetareeParticleInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled particle mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SkreeMetareeParticleInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = SkreeMetareeParticleInstructionProgramDefinitions
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
