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

internal static class SkreeMetareeParticleInstructionProgramDefinitionsVerification
{
    internal static void VerifySkreeMetareeParticleInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySkreeMetareeParticleInstructionProgramDefinitions), () => VerifySkreeMetareeParticleInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

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
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
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
                projectile.InvincibilityFrames == 96 && projectile.Radius.X == 2 && projectile.Radius.Y == 2),
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
            process.Invoke(enemies, [projectile, samus, Position16<RoomPixels>.Zero]);
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

    internal static CartridgeReadGuard NewSkreeMetareeParticleInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    SkreeMetareeParticleInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled particle mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                SkreeMetareeParticleInstructionProgramDefinitionsTooling.PresentationWordCount,
                SkreeMetareeParticleInstructionProgramDefinitionsTooling.PresentationWordAddress);

    [VerificationCommand("--skree-metaree-particle-instruction-mechanics")]
    internal static int SkreeMetareeParticleInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySkreeMetareeParticleInstructionProgramDefinitions), () => VerifySkreeMetareeParticleInstructionProgramDefinitions());
        return 0;
    }
}
