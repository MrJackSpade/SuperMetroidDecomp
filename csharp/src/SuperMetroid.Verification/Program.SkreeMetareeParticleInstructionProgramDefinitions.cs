using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks compiled Skree and Metaree particle mechanics, burst ownership, complete animation loops, and runtime read isolation.</summary>
    private static void VerifySkreeMetareeParticleInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySkreeMetareeParticleInstructionProgramDefinitions), () => VerifySkreeMetareeParticleInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with retail data and exercises both particle bursts through their timed loops and delete path.</summary>
    /// <param name="rom">Retail address space used for expected instruction words and selected spritemap compositions.</param>
    private static void VerifySkreeMetareeParticleInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < SkreeMetareeParticleInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SkreeMetareeParticleInstructionProgramDefinitionsTooling.MechanicsWord(index);
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
        var selectedCompositions = new HashSet<ushort>();

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
        // SpawnEprojInner copies the definition's $0004 properties: four damage, contact enabled.
        AssertTrue(skree.All(projectile => projectile.CanDamageSamus && projectile.Damage == 4 &&
                projectile.InvincibilityFrames == 96 && projectile.XRadius == 2 && projectile.YRadius == 2),
            "Skree particles damage Samus with the definition's four-point contact");
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
        // SpawnEprojInner copies the definition's $0004 properties: four damage, contact enabled.
        AssertTrue(metaree.All(projectile => projectile.CanDamageSamus && projectile.Damage == 4 &&
                projectile.InvincibilityFrames == 96 && projectile.XRadius == 2 && projectile.YRadius == 2),
            "Metaree particles damage Samus with the definition's four-point contact");
        RunCompleteLoops(
            metaree,
            SkreeMetareeParticleInstructionProgramDefinitions.Metaree);

        AssertEqual(
            SkreeMetareeParticleInstructionProgramDefinitionsTooling.PresentationWordCount,
            selectedCompositions.Count,
            "production selects both installed Skree/Metaree particle compositions");
        for (int index = 0;
             index < SkreeMetareeParticleInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = SkreeMetareeParticleInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            AssertTrue(selectedCompositions.Contains(ReadSkreeMetareeParticleInstructionWord(rom, address)),
                $"production execution selects native particle composition from $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "production particle frames do not read presentation operands from ROM");
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
            "both installed compositions pass without instruction ROM reads.");

        void RunCompleteLoops(
            IEnumerable<RoomEnemyProjectileSlot> projectiles,
            ushort expectedInitial)
        {
            foreach (RoomEnemyProjectileSlot projectile in projectiles)
            {
                for (int loop = 0; loop < 2; loop++)
                {
                    RunForcedTick(projectile);
                    ushort operand = (ushort)(expectedInitial + 2);
                    AssertEqual(ReadSkreeMetareeParticleInstructionWord(rom, operand), projectile.SpritemapPointer,
                        "particle loop selects the exact native installed composition");
                    AssertEqual(ReadSkreeMetareeParticleInstructionWord(rom, expectedInitial),
                        projectile.InstructionTimer, "particle frame retains native duration");
                    selectedCompositions.Add(projectile.SpritemapPointer);
                }
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

    /// <summary>Repeats alternating Skree and Metaree mechanics lookups for the warmed allocation check.</summary>
    /// <returns>A checksum that keeps both lookup paths observable.</returns>
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

    /// <summary>Reads a little-endian word from the enemy-projectile instruction bank.</summary>
    /// <param name="source">Address space supplying the instruction bytes.</param>
    /// <param name="address">Bank-relative address of the low byte; the high byte follows with 16-bit address wrapping.</param>
    /// <returns>The combined 16-bit instruction word.</returns>
    private static ushort ReadSkreeMetareeParticleInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects runtime reads of compiled particle mechanics while recording accesses to installed composition operands.</summary>
    /// <param name="source">Underlying address space for allowed reads and forwarded writes.</param>
    private sealed class SkreeMetareeParticleInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation operand addresses observed during production particle execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of reads attempted against compiled particle or shared-delete mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge import read through the checked byte-read path.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The underlying byte when the read does not target compiled mechanics.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, tracks bank-$86 presentation operands, and delegates other reads.</summary>
        /// <param name="address">Address requested from the wrapped SNES memory.</param>
        /// <returns>The underlying byte for an allowed read.</returns>
        public byte ReadByte(int address)
        {
            if (SkreeMetareeParticleInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled particle mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SkreeMetareeParticleInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation = SkreeMetareeParticleInstructionProgramDefinitionsTooling
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
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
