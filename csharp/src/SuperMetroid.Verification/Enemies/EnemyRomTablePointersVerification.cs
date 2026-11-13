using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Tooling;
using System.Reflection;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotInitialSelectionVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks named enemy ROM ranges and compiled instruction selectors against their cartridge sources.</summary>
internal static class EnemyRomTablePointersVerification
{
    /// <summary>
    /// Protects the enemy-data catalog from accidentally acquiring WRAM addresses,
    /// duplicate names, or pointers outside mapped cartridge space. With the private
    /// retail ROM present, this also reads both ends of representative multi-byte ranges.
    /// </summary>
    internal static void VerifyEnemyRomTablePointerCatalog()
    {
        // The retail ROM oracle's tables live in test-support (#1412): production reads none of them, and the
        // import reads the ones it needs through its own source catalogs.
        Type[] families =
        [
            .. MetadataShape.NestedTypes(typeof(EnemyRomTablePointers), BindingFlags.Public | BindingFlags.NonPublic),
            .. typeof(Program).Assembly.GetTypes().Where(type => type != typeof(EnemyRomTablePointers) &&
                type.Name.StartsWith(nameof(EnemyRomTablePointers), StringComparison.Ordinal)),
        ];
        AssertTrue(families.Length >= 14, "enemy ROM data is grouped by owning family");

        FieldInfo[] fields = families
            .SelectMany(family => family.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.IsLiteral && field.FieldType == typeof(int))
            .ToArray();
        AssertTrue(fields.Length >= 50, "enemy ROM data catalog covers fixed data tables");
        foreach (FieldInfo field in fields)
        {
            int address = field.RequiredConstant<int>();
            AssertTrue(
                address is >= 0x808000 and <= 0xffffff,
                $"{(field.DeclaringType ?? throw new InvalidOperationException("Verification requires field.DeclaringType.")).Name}.{field.Name} is a mapped SNES ROM address");
        }

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                $"  Enemy ROM data: {fields.Length} named ranges are structurally valid; " +
                "retail ROM reads skipped (private input absent).");
            return;
        }

        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        foreach ((int start, int byteLength) in RepresentativeEnemyRomRanges())
        {
            _ = bus.ReadCartridgeByte(start);
            _ = bus.ReadCartridgeByte(start + byteLength - 1);
        }
        Suite(nameof(VerifyCompiledEnemyInstructionSelectors), () => VerifyCompiledEnemyInstructionSelectors(bus));

        Console.WriteLine(
            $"  Enemy ROM data: {fields.Length} named ranges and " +
            $"{RepresentativeEnemyRomRanges().Length} representative retail ranges verified.");
    }

    /// <summary>
    /// Uses one nontrivial range from every represented storage shape: byte arrays,
    /// word arrays, palettes, pointer arrays, interleaved records, and transfer tables.
    /// </summary>
    private static RepresentativeEnemyRomRangesResult[] RepresentativeEnemyRomRanges() =>
    [
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointersCommon.SignedSineCosineWords, 512),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointersTorizo.WakeXPositions, 4),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointersChozoStatue.CarryVelocityWords, 64),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointersGunship.DustInstructionPointers, 12),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointersCrocomire.DeathGraphicsSourceWords, 14),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointersDeadSidehopper.HorizontalVelocityWords, 8),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointersGunship.LiftoffVramDestinationWords, 10),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointers.Kraid.RoomBackgroundPaletteWords, 32),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointersPhantoon.FirstRoundHidingTimerWords, 16),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointers.Ridley.HealthPaletteWords, 84),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointers.TourianStatue.StatuePaletteWords, 32),
        new RepresentativeEnemyRomRangesResult(EnemyRomTablePointers.WorkRobot.InitialAnimationScriptWords, 4),
    ];

    /// <summary>Compares Golden Torizo, Work Robot, and Tourian statue selectors with cartridge words and production initializers.</summary>
    private static void VerifyCompiledEnemyInstructionSelectors(
        CartridgeImportAddressSpace rom)
    {

        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.SuperMissileInstructionPointers),
            TorizoNativeListAddresses.ProjectileEntry(TorizoProjectileProgram.SuperMissileLeftInitial),
            "Golden Torizo left reflected-Super list");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointersTorizo.SuperMissileInstructionPointers + 2)),
            TorizoNativeListAddresses.ProjectileEntry(TorizoProjectileProgram.SuperMissileRightInitial),
            "Golden Torizo right reflected-Super list");
        Suite(nameof(VerifyWorkRobotInitialSelection), () => VerifyWorkRobotInitialSelection(rom));
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.TourianStatue.AnimationScriptWords + parameter)),
                NativeEnemyAnimationOracle.EntryOf(TourianEntranceStatueNativeListAddresses.Entries,
                    SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.TourianStatue(parameter)),
                $"Tourian entrance statue initial selector {parameter}");
        AssertThrows<InvalidDataException>(() => SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.TourianStatue(1),
            "Tourian statue selector rejects odd byte offsets");

        var guarded = NewEnemyInstructionSelectionReadGuard(rom);
        Type type = typeof(RoomEnemySystem);
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = InstalledFixtures.BossArtwork };
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
            slot.SetSpawnParameter1(EnemyDefinitionId.TourianStatue, parameter);
            slot.Bind(EnemyDefinitionId.TourianStatue, slot.Definition);
            system.TourianEntranceStatueBehavior.InitializeTourianEntranceStatue(slot);
            // The statues' animations stop at once and so compare equal; the one started is told apart by identity.
            AnimationPlayback<TourianEntranceStatueProgram> started = slot.RequireSpeciesState<TourianEntranceStatueState>().Playback;
            AssertTrue(ReferenceEquals(TourianEntranceStatueAnimations.StartingWith(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.TourianStatue(parameter)), started.Animation)
                && started.Place == AnimationPlace.StartOfAnimation,
                $"Tourian statue production initializer {parameter} avoids selector ROM");
        }

        foreach (bool facingRight in new[] { false, true })
        {
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = InstalledFixtures.BossArtwork };
            var torizo = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            };
            system.BombTorizoBehavior.BindFixtureState(torizo, facingRight ? 0x8000 : 0, isGolden: true);
            system.BombTorizoBehavior.Attacks.SpawnGoldenTorizoSuperMissile(torizo);
            EnemyProjectile projectile = system.Projectiles.EnemyProjectiles.Single(
                candidate => candidate.IsActive);
            AssertEqual(
                facingRight ? TorizoProjectileProgram.SuperMissileRightInitial : TorizoProjectileProgram.SuperMissileLeftInitial,
                (projectile.Animation as TorizoProjectileAnimation)?.PlayingSegment,
                $"Golden Torizo production reflected-Super selector {facingRight} avoids ROM");
        }

        Console.WriteLine(
            "  Enemy instruction selectors: nine native words and all three production initializers avoid their source tables.");
    }

    /// <summary>Rejects runtime reads of the three instruction-selection tables replaced by compiled definitions.</summary>
    private static CartridgeReadGuard NewEnemyInstructionSelectionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= EnemyRomTablePointersTorizo.SuperMissileInstructionPointers and
                    < EnemyRomTablePointersTorizo.SuperMissileInstructionPointers + 4 or
                    >= EnemyRomTablePointers.WorkRobot.InitialAnimationScriptWords and
                    < EnemyRomTablePointers.WorkRobot.InitialAnimationScriptWords + 8 or
                    >= EnemyRomTablePointers.TourianStatue.AnimationScriptWords and
                    < EnemyRomTablePointers.TourianStatue.AnimationScriptWords + 6,
                address => $"Enemy instruction selector still reads compiled ROM byte ${address:X6}.");

    /// <summary>Runs the enemy ROM pointer catalog check from the command dispatcher.</summary>
    [VerificationCommand("--enemy-pointer-fixture")]
    internal static int EnemyPointerFixtureCommand()
    {
     VerifyEnemyRomTablePointerCatalog(); return 0; 
    }

    /// <summary>Runs selector checks as part of the enemy ROM pointer catalog suite.</summary>
    [VerificationCommand("--enemy-instruction-selectors")]
    internal static int EnemyInstructionSelectorsCommand()
    {
        Suite(nameof(VerifyEnemyRomTablePointerCatalog), () => VerifyEnemyRomTablePointerCatalog());
        return 0;
    }
}
