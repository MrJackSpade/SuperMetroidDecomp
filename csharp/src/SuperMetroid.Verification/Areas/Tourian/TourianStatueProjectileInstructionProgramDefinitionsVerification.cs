using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Tourian statue effect and decoration programs against ROM and their live projectile producers.</summary>
internal static class TourianStatueProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the Tourian statue projectile suite.</summary>
    internal static void VerifyTourianStatueProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTourianStatueProjectileInstructionProgramDefinitions), () => VerifyTourianStatueProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks compiled words and eye, particle, tail, splash, soul, and entrance-statue instruction paths.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Tourian statue projectile instruction program definitions")]
    internal static void VerifyTourianStatueProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < TourianStatueProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TourianStatueProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Tourian statue projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewTourianStatueProjectileInstructionReadGuard(rom);
        Func<EnemyProjectileAnimation, ushort?> nativePointers =
            TourianStatueProjectileAnimationsVerification.Family.NativePointers(CartridgeImportSource.Require(rom));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
        enemies.Presentation.BindCgram(new GameplayColours());
        enemies.VerificationServices().NextRandom = () => 4;

        enemies.TourianUnlockBehavior.SpawnTourianEyeGlow(0);
        RoomEnemyProjectileSlot eye = Single(RoomEnemyProjectileKind.TourianStatueEyeGlow);
        Run(eye, 10);
        AssertEqual((ushort)0xb7db, NativePointer(eye),
            "eye glow reaches its sound and release tail after ten display frames");
        Run(eye, 1);
        AssertTrue(!eye.IsActive, "eye glow deletes after releasing four particles");
        AssertEqual(4, enemies.Projectiles.EnemyProjectiles.Count(
            projectile => projectile.Kind == RoomEnemyProjectileKind.TourianStatueParticle),
            "eye glow releases four real unlocking particles");
        AssertEqual(1, enemies.Earthquake.EarthquakeType, "eye glow starts the native earthquake");
        AssertEqual((ushort)0x20, enemies.Earthquake.EarthquakeTimer.RawValue,
            "eye glow publishes the native earthquake duration bit");

        RoomEnemyProjectileSlot particle = Single(RoomEnemyProjectileKind.TourianStatueParticle);
        Run(particle, 3);
        RoomEnemyProjectileSlot tail = Single(RoomEnemyProjectileKind.TourianStatueTail);
        Run(particle, 2);
        AssertEqual(ushort.MaxValue, particle.GeneralTimer,
            "unset particle loop timer wraps from zero to FFFF like the cartridge");
        AssertEqual((ushort)0xb806, NativePointer(particle),
            "particle counted branch immediately begins its next display cycle");

        Run(tail, 1);
        ushort tailY = tail.Position.Y.Whole;
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 8)), tail.Position.Y.Whole,
            "particle tail applies its first compiled Y displacement");
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 12)), tail.Position.Y.Whole,
            "particle tail applies its second compiled Y displacement");
        Run(tail, 1);
        AssertEqual(unchecked((ushort)(tailY + 14)), tail.Position.Y.Whole,
            "particle tail applies its third compiled Y displacement");
        Run(tail, 1);
        AssertTrue(!tail.IsActive, "particle tail deletes after four authored poses");

        enemies.TourianUnlockBehavior.SpawnTourianParticleChild(particle, RoomEnemyProjectileKind.TourianStatueSplash);
        RoomEnemyProjectileSlot splash = Single(RoomEnemyProjectileKind.TourianStatueSplash);
        Run(splash, 4);
        AssertEqual((ushort)0xb7b1, NativePointer(splash),
            "water splash reaches deletion after four authored poses");
        Run(splash, 1);
        AssertTrue(!splash.IsActive, "water splash deletes on its fifth instruction tick");

        enemies.TourianUnlockBehavior.SpawnTourianSoul(2);
        RoomEnemyProjectileSlot soul = Single(RoomEnemyProjectileKind.TourianStatueSoul);
        Run(soul, 3);
        AssertEqual((ushort)0xb852, NativePointer(soul),
            "statue soul loops through both authored poses");
        soul.StartAnimation(new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Delete));
        Run(soul, 1);
        AssertTrue(!soul.IsActive, "private Tourian delete list clears a soul actor");

        RoomEnemyProjectileSlot baseDecoration = SpawnStatue(
            RoomEnemyProjectileKind.TourianStatueBaseDecoration);
        Run(baseDecoration, 2);
        AssertEqual(
            EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess,
            baseDecoration.PreInstruction,
            "base decoration installs its delayed position callback");
        AssertEqual((ushort)0xb866, NativePointer(baseDecoration),
            "base decoration reaches its stable loop");
        Run(baseDecoration, 1);
        AssertEqual((ushort)0xb866, NativePointer(baseDecoration),
            "base decoration loop remains stable");

        RoomEnemyProjectileSlot ridley = SpawnStatue(RoomEnemyProjectileKind.TourianStatueRidley);
        Run(ridley, 2);
        AssertEqual((ushort)0xb86e, NativePointer(ridley),
            "Ridley statue loop remains stable");
        RoomEnemyProjectileSlot phantoon = SpawnStatue(
            RoomEnemyProjectileKind.TourianStatuePhantoon);
        Run(phantoon, 2);
        AssertEqual((ushort)0xb876, NativePointer(phantoon),
            "Phantoon statue loop remains stable");

        AssertEqual(0, guard.ObservedWords.Count,
            "TourianStatueProjectile execution performs no live spritemap operand reads");
        AssertEqual(TourianStatueProjectileInstructionProgramDefinitionsTooling.PresentationWordCount,
            executedOperands.Count, "TourianStatueProjectile executes every native visual operand");
        for (int index = 0; index < TourianStatueProjectileInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            ushort address = TourianStatueProjectileInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"TourianStatueProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "TourianStatueProjectile has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "TourianStatueProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Tourian statue mechanics byte");

        Console.WriteLine(
            "Tourian statue projectile instruction mechanics: fifty-eight compiled words, " +
            "all eight real actor families, and twenty-eight installed sprite frames match native OAM " +
            "with mechanics bytes forbidden.");

        ushort NativePointer(RoomEnemyProjectileSlot projectile) =>
            nativePointers(projectile.Animation ?? throw new InvalidOperationException("A statue projectile runs a typed animation."))
                ?? throw new InvalidOperationException("The statue projectile runs another family's animation.");

        RoomEnemyProjectileSlot Single(RoomEnemyProjectileKind kind) =>
            enemies.Projectiles.EnemyProjectiles.First(projectile => projectile.Kind == kind);

        RoomEnemyProjectileSlot SpawnStatue(RoomEnemyProjectileKind kind)
        {
            enemies.TourianEntranceStatueBehavior.SpawnTourianEntranceStatueProjectile(kind, new Position16<RoomPixels>(128, 128));
            return enemies.Projectiles.EnemyProjectiles.First(projectile => projectile.Kind == kind);
        }

        void Run(RoomEnemyProjectileSlot projectile, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                projectile.InstructionTimer = WrappingCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
                // The shown frame's operand sits just before the instruction the native pointer stands on.
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands,
                    projectile.IsActive ? unchecked((ushort)(NativePointer(projectile) - 2)) : null);
            }
        }
    }

    /// <summary>Forbids compiled Tourian statue mechanics reads and observes visual operands.</summary>
    private static CartridgeReadGuard NewTourianStatueProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(TourianStatueProjectileInstructionProgramDefinitionsToolingConstants.IsCompiledMechanicsByte,
                address => $"Production read compiled Tourian statue mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, TourianStatueProjectileInstructionProgramDefinitionsTooling.PresentationWordCount, TourianStatueProjectileInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Runs the Tourian statue projectile suite through the verification command dispatcher.</summary>
    [VerificationCommand("--tourian-statue-projectile-instruction-mechanics")]
    internal static int TourianStatueProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyTourianStatueProjectileInstructionProgramDefinitions), () => VerifyTourianStatueProjectileInstructionProgramDefinitions());
        return 0;
    }
}
