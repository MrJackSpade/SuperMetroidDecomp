using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyStokeProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyStokeProjectileInstructionProgramDefinitions), () => VerifyStokeProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyStokeProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < StokeProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                StokeProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Stoke-projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewStokeProjectileInstructionReadGuard(rom);
        var selectedPresentation = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnStokeProjectile", flags)!;

        var source = new RoomEnemySlot(0)
        {
            Position = new(new(128, 0x1234), new(112, 0x5678)),
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        spawn.Invoke(enemies, [source, (ushort)0]);
        spawn.Invoke(enemies, [source, (ushort)1]);

        RoomEnemyProjectileSlot[] projectiles = enemies.EnemyProjectiles
            .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.StokeProjectile)
            .ToArray();
        AssertEqual(2, projectiles.Length,
            "both real Stoke attack directions spawn through production code");
        AssertTrue(projectiles.Select(projectile => projectile.DirectionParameter)
                .Order().SequenceEqual(new ushort[] { 0, 1 }),
            "real Stoke projectile producers retain both direction parameters");
        foreach (RoomEnemyProjectileSlot projectile in projectiles)
        {
            AssertEqual(StokeProjectileInstructionProgramDefinitions.Initial,
                projectile.InstructionPointer,
                "real Stoke projectile producer selects the named animation loop");
            RunForcedTicks(projectile, 3);
            AssertEqual(
                unchecked((ushort)(StokeProjectileInstructionProgramDefinitions.Initial + 4)),
                projectile.InstructionPointer,
                "Stoke projectile completes both frames and loops to its first frame");
            AssertTrue(projectile.IsActive,
                "Stoke projectile loop remains active until viewport culling or a shot");
        }

        projectiles[0].InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(projectiles[0], 1);
        AssertTrue(!projectiles[0].IsActive,
            "Stoke projectile shot reaction reaches the compiled shared delete program");

        AssertEqual(0, guard.ObservedWords.Count,
            "StokeProjectile execution performs no live spritemap operand reads");
        AssertEqual(StokeProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "StokeProjectile executes every native visual operand");
        for (int index = 0; index < StokeProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = StokeProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"StokeProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "StokeProjectile has a compiled visual selector");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address), selector,
                "StokeProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Stoke-projectile and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => StokeProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xdb0e),
            "Stoke-projectile spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => StokeProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xdb18),
            "adjacent Stoke-projectile initializer is rejected as mechanics");

        _ = ProbeStokeProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeStokeProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Stoke-projectile allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Stoke-projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Stoke-projectile instruction mechanics: four compiled words, both real " +
            "directions, complete loops, shared shot deletion, and both installed sprite frames " +
            "pass with mechanics and visual operand ROM reads forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), Position16<RoomPixels>.Zero]);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
                if (projectile.IsActive)
                {
                    ushort expected = (ushort)(StokeProjectileInstructionProgramDefinitions.Initial + 2 + 4 * (tick % 2));
                    AssertEqual(expected, projectile.PresentationOperandAddress,
                        "Stoke projectile actual installed operand matches native loop step");
                    selectedPresentation.Add(projectile.PresentationOperandAddress);
                }
            }
        }
    }

    private static int ProbeStokeProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += StokeProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? StokeProjectileInstructionProgramDefinitions.Initial
                    : StokeProjectileInstructionProgramDefinitions.LoopCommand);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewStokeProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => StokeProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Stoke-projectile mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, StokeProjectileInstructionProgramDefinitions.PresentationWordCount, StokeProjectileInstructionProgramDefinitions.PresentationWordAddress);
}
