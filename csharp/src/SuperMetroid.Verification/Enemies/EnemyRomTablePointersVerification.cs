using SuperMetroid.Tooling;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotInitialSelectionVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks named enemy ROM ranges and compiled instruction selectors against their cartridge sources.</summary>
internal static class EnemyRomTablePointersVerification
{
    /// <summary>
    /// Protects the enemy-data catalog from accidentally acquiring WRAM addresses,
    /// duplicate names, or pointers outside mapped cartridge space. With the private
    /// retail ROM present, this also reads both ends of representative multi-byte ranges.
    /// </summary>
    [CpuBusBoundary("exact bus test: proves representative enemy ROM table ranges map to readable retail cartridge ROM")]
    internal static void VerifyEnemyRomTablePointerCatalog()
    {
        // Families production reads stay nested in the catalog; oracle-only families live in
        // test-support as flattened EnemyRomTablePointers* classes.
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

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
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
    private static (int Start, int ByteLength)[] RepresentativeEnemyRomRanges() =>
    [
        (EnemyRomTablePointersCommon.SignedSineCosineWords, 512),
        (EnemyRomTablePointersTorizo.WakeXPositions, 4),
        (EnemyRomTablePointersChozoStatue.CarryVelocityWords, 64),
        (EnemyRomTablePointersGunship.DustInstructionPointers, 12),
        (EnemyRomTablePointersCrocomire.DeathGraphicsSourceWords, 14),
        (EnemyRomTablePointersDeadSidehopper.HorizontalVelocityWords, 8),
        (EnemyRomTablePointersGunship.LiftoffVramDestinationWords, 10),
        (EnemyRomTablePointers.Kraid.RoomBackgroundPaletteWords, 32),
        (EnemyRomTablePointersPhantoon.FirstRoundHidingTimerWords, 16),
        (EnemyRomTablePointers.Ridley.HealthPaletteWords, 84),
        (EnemyRomTablePointers.TourianStatue.StatuePaletteWords, 32),
        (EnemyRomTablePointers.WorkRobot.InitialInstructionListWords, 4),
    ];

    /// <summary>Compares Golden Torizo, Work Robot, and Tourian statue selectors with cartridge words and production initializers.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled enemy instruction selectors")]
    private static void VerifyCompiledEnemyInstructionSelectors(
        SuperMetroidAddressSpace rom)
    {

        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.SuperMissileInstructionPointers),
            GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(false),
            "Golden Torizo left reflected-Super list");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointersTorizo.SuperMissileInstructionPointers + 2)),
            GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(true),
            "Golden Torizo right reflected-Super list");
        Suite(nameof(VerifyWorkRobotInitialSelection), () => VerifyWorkRobotInitialSelection(rom));
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.TourianStatue.InstructionListWords + parameter)),
                TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(parameter),
                $"Tourian entrance statue initial selector {parameter}");
        AssertThrows<ArgumentOutOfRangeException>(
            () => TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(1),
            "Tourian statue selector rejects odd byte offsets");

        var guarded = NewEnemyInstructionSelectionReadGuard(rom);
        Type type = typeof(RoomEnemySystem);
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            var system = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
            system.BindFixtureBus(guarded);
            system.Presentation._cgram = new SnesCgram();
            var slot = new RoomEnemySlot(0) { Parameter1 = parameter };
            system.TourianEntranceStatueBehavior.InitializeTourianEntranceStatue(slot);
            AssertEqual(
                TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(
                    parameter),
                slot.CurrentInstruction,
                $"Tourian statue production initializer {parameter} avoids selector ROM");
        }

        foreach (bool facingRight in new[] { false, true })
        {
            var system = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
            system.BindFixtureBus(guarded);
            var torizo = new RoomEnemySlot(0)
            {
                Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0,
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            };
            system.BombTorizoBehavior.SpawnGoldenTorizoSuperMissile(torizo);
            RoomEnemyProjectileSlot projectile = system.Projectiles.EnemyProjectiles.Single(
                candidate => candidate.IsActive);
            AssertEqual(
                GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(facingRight),
                projectile.InstructionPointer,
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
                    >= EnemyRomTablePointersWorkRobotConstants.InitialInstructionListWords and
                    < EnemyRomTablePointersWorkRobotConstants.InitialInstructionListWords + 8 or
                    >= EnemyRomTablePointersTourianStatueConstants.InstructionListWords and
                    < EnemyRomTablePointersTourianStatueConstants.InstructionListWords + 6,
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
