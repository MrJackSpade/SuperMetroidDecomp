using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Mother Brain ceiling-tube projectile programs, sprites, and shared deletion.</summary>
internal static class MotherBrainTopTubeInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for ceiling-tube instruction checks.</summary>
    internal static void VerifyMotherBrainTopTubeInstructionProgramDefinitions() =>
        Suite(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Compares native words and executes four producers through display and terminal sleep.</summary>
    internal static void VerifyMotherBrainTopTubeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MotherBrainTopTubeInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MotherBrainTopTubeInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Mother Brain ceiling-tube mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewMotherBrainTopTubeInstructionReadGuard(rom);
        Func<EnemyProjectileAnimation, ushort?> nativePointers =
            MotherBrainTopTubeAnimationsVerification.Family.NativePointers(CartridgeImportSource.Require(rom));

        foreach (TopTubeLaunchCase item in
                 new TopTubeLaunchCase[]
                 {
                     new(RoomEnemyProjectileKind.MotherBrainTopRightTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopRight,
                         152, 47),
                     new(RoomEnemyProjectileKind.MotherBrainTopLeftTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopLeft,
                         104, 47),
                     new(RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft,
                         120, 59),
                     new(RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight,
                         136, 59),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            enemies.MotherBrainBehavior.FakeDeath.SpawnMotherBrainTopTube(item.Kind, new Position16<RoomPixels>(item.X, item.Y));
            RoomEnemyProjectileSlot tube = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == item.Kind);
            AssertEqual(item.Program, NativePointer(tube),
                $"real {item.Kind} producer selects its named pose");
            AssertEqual(item.X, tube.Position.X.Whole, $"real {item.Kind} producer X position");
            AssertEqual(item.Y, tube.Position.Y.Whole, $"real {item.Kind} producer Y position");
            RunForcedTick(enemies, tube);
            AssertEqual(unchecked((ushort)(item.Program + 4)), NativePointer(tube),
                $"{item.Kind} displays its pose before terminal sleep");
            RunForcedTick(enemies, tube);
            AssertEqual(unchecked((ushort)(item.Program + 4)), NativePointer(tube),
                $"{item.Kind} terminal sleep retains its own opcode");
        }

        RoomEnemySystem shotSystem = NewSystem();
        shotSystem.MotherBrainBehavior.FakeDeath.SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopRightTube, new Position16<RoomPixels>(152, 47));
        RoomEnemyProjectileSlot shot = shotSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainTopRightTube);
        shot.StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        RunForcedTick(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "Mother Brain ceiling-tube shot reaction reaches shared compiled deletion");

        AssertEqual(0, guard.ObservedWords.Count,
            "Projectile presentation performs zero live cartridge reads");
        AssertEqual(MotherBrainTopTubeInstructionProgramDefinitionsTooling.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared ceiling-tube mechanics bytes");


        Console.WriteLine(
            "Mother Brain ceiling-tube instruction mechanics: eight compiled words, all " +
            "four real producers, terminal sleeps, shared deletion, and four native " +
            "sprite compositions pass with zero live operand reads.");

        ushort NativePointer(RoomEnemyProjectileSlot projectile) =>
            nativePointers(projectile.Animation ?? throw new InvalidOperationException("A ceiling tube runs a typed animation."))
                ?? throw new InvalidOperationException("The ceiling tube runs another family's animation.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            return enemies;
        }

        void RunForcedTick(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = WrappingCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(InstalledFixtures.AnimationContinuation), Position16<RoomPixels>.Zero);
            // The shown frame's operand sits just before the instruction the native pointer stands on.
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands,
                projectile.IsActive ? unchecked((ushort)(NativePointer(projectile) - 2)) : null);
        }
    }

    /// <summary>Blocks private and shared control bytes and observes visual operand reads.</summary>
    private static CartridgeReadGuard NewMotherBrainTopTubeInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(MotherBrainTopTubeInstructionProgramDefinitionsToolingConstants.IsCompiledMechanicsByte,
                address => $"Production read compiled Mother Brain ceiling-tube mechanics byte " +
                    $"${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, MotherBrainTopTubeInstructionProgramDefinitionsTooling.PresentationWordCount, MotherBrainTopTubeInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Runs the Mother Brain ceiling-tube instruction mechanics command.</summary>
    [VerificationCommand("--mother-brain-top-tube-instruction-mechanics")]
    internal static int MotherBrainTopTubeInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions());
        return 0;
    }
}
