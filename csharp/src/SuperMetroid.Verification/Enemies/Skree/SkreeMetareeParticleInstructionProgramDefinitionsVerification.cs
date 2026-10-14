using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Skree;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Skree and Metaree particle programs, burst producers, and shared deletion against ROM.</summary>
internal static class SkreeMetareeParticleInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the Skree and Metaree particle suite.</summary>
    internal static void VerifySkreeMetareeParticleInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySkreeMetareeParticleInstructionProgramDefinitions), () => VerifySkreeMetareeParticleInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled words and executes all eight directional burst particles with runtime mechanics reads forbidden.</summary>
    internal static void VerifySkreeMetareeParticleInstructionProgramDefinitions(
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
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Skree/Metaree particle mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewSkreeMetareeParticleInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        var spawnSkreeBurst = new Action<RoomEnemySlot>(enemies.Projectiles.SpawnSkreeParticleBurst);
        var spawnMetareeBurst = new Action<RoomEnemySlot>(enemies.Projectiles.SpawnMetareeParticleBurst);
        var source = new RoomEnemySlot(0)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var samus = new SamusState();
        var selectedCompositions = new HashSet<ushort>();

        spawnSkreeBurst(source);
        RoomEnemyProjectileSlot[] skree = enemies.Projectiles.EnemyProjectiles
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
                projectile.InvincibilityFrames == 96 && projectile.Radius.X == 2 && projectile.Radius.Y == 2),
            "Skree particles damage Samus with the definition's four-point contact");
        RunCompleteLoops(
            skree,
            SkreeMetareeParticleInstructionProgramDefinitions.Skree);

        spawnMetareeBurst(source);
        RoomEnemyProjectileSlot[] metaree = enemies.Projectiles.EnemyProjectiles
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
                projectile.InvincibilityFrames == 96 && projectile.Radius.X == 2 && projectile.Radius.Y == 2),
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
            AssertTrue(selectedCompositions.Contains(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address)),
                $"production execution selects native particle composition from $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedWords.Count,
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
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | operand), projectile.SpritemapPointer,
                        "particle loop selects the exact native installed composition");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | expectedInitial),
                        projectile.InstructionTimer.RawValue, "particle frame retains native duration");
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
            projectile.InstructionTimer = NativeCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, samus, Position16<RoomPixels>.Zero);
        }
    }

    /// <summary>Consumes repeated Skree and Metaree control lookups for the warmed allocation check.</summary>
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

    /// <summary>Forbids particle and shared-delete mechanics reads while observing visual selector addresses.</summary>
    internal static CartridgeReadGuard NewSkreeMetareeParticleInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    SkreeMetareeParticleInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled particle mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                SkreeMetareeParticleInstructionProgramDefinitionsTooling.PresentationWordCount,
                SkreeMetareeParticleInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Runs the particle instruction suite through the verification command dispatcher.</summary>
    [VerificationCommand("--skree-metaree-particle-instruction-mechanics")]
    internal static int SkreeMetareeParticleInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySkreeMetareeParticleInstructionProgramDefinitions), () => VerifySkreeMetareeParticleInstructionProgramDefinitions());
        return 0;
    }
}
