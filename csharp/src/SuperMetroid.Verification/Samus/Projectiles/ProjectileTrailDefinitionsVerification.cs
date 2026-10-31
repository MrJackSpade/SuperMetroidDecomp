using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailCoordinatesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks compiled projectile trail selector words and actual left/right spawn selections.</summary>
internal static class ProjectileTrailDefinitionsVerification
{
    /// <summary>Compares all reachable selector words and 64 spawn choices with retail data while forbidding runtime reads of the migrated selector window.</summary>
    internal static void VerifyProjectileTrailDefinitions(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(bus));
        int start = SamusProjectileRomDataTrailsConstants.LeftInstructionPointers;
        int end = SamusProjectileRomDataTrailsConstants.RightInstructionPointers + 64 * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
        {
            ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            if (ProjectileTrailDefinitions.ReadSelector(address) is { } selected)
                AssertEqual(native, selected, "Trail selectors retain every reachable aligned native word");
            else
                AssertTrue(!ProjectileTrailProgramDefinitions.TryRead(SamusSourceDefinitions.MovementBank.Value | native, out _),
                    "A selector word past the right table names no trail step");
        }
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start - 2), "Trail selector rejects the preceding word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start + 1), "Trail selector rejects unaligned reads");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(end), "Trail selector rejects the following word");
        int spawned = 0;
        for (int selection = 0; selection < 64; selection++)
        {
            // Echo selections name a projectile only for the Spazer combo trail ($24) and the shinespark echo ($29); the
            // typed identity names no other echo, so no projectile spawns the rest (#1470).
            if ((selection & 0x20) != 0 && selection is not (0x24 or 0x29))
                continue;
            spawned++;
            var projectiles = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None, InstalledFixtures.SamusOverreads);
            var projectile = new SamusProjectileSlot(0)
            {
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(selection),
                Cursor = SamusProjectileProgramAddresses.CursorAt(0x86e3),
                Position = new(new(100, 0), new(200, 0)),
            };
            projectiles.Trails.SpawnTrail(projectile);
            var trail = projectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            // An empty trail family (programs, direction list or coordinates outside the compiled data) draws no trail (#1318).
            if (trail.Left.InstructionTimer.IsZero)
                continue;
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects left trail including adjacent right-table entries");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomDataTrailsConstants.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn retains right-table overrun behavior");
            AssertEqual(1, trail.Left.InstructionTimer.RawValue, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.Position.X, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine($"Trail selectors: 103 reachable native words and {spawned} real spawn selections pass with the complete selector window forbidden.");
    }
}
