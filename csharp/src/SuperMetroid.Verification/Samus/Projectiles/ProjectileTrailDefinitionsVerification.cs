using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailCoordinatesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks compiled projectile trail selector words and actual left/right spawn selections.</summary>
internal static class ProjectileTrailDefinitionsVerification
{
    /// <summary>Compares all reachable selector words and 64 spawn choices with retail data while forbidding runtime reads of the migrated selector window.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled projectile trail selector words")]
    internal static void VerifyProjectileTrailDefinitions(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(bus));
        int start = SamusProjectileRomData.Trails.LeftInstructionPointers;
        int end = SamusProjectileRomData.Trails.RightInstructionPointers + 64 * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailDefinitions.ReadSelector(address, InstalledFixtures.SamusOverreads), "Trail selectors retain every reachable aligned native word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start - 2, InstalledFixtures.SamusOverreads), "Trail selector rejects the preceding word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start + 1, InstalledFixtures.SamusOverreads), "Trail selector rejects unaligned reads");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(end, InstalledFixtures.SamusOverreads), "Trail selector rejects the following word");
        for (int selection = 0; selection < 64; selection++)
        {
            var projectiles = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Overreads = InstalledFixtures.SamusOverreads };
            var projectile = new SamusProjectileSlot(0)
            {
                Type = (ushort)selection,
                Cursor = SamusProjectileProgramAddresses.CursorAt(0x86e3),
                Position = new(new(100, 0), new(200, 0)),
            };
            projectiles.Trails.SpawnTrail(projectile);
            var trail = projectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            // An empty trail family (programs, direction list or coordinates outside the compiled data) draws no trail (#1318).
            if (trail.Left.InstructionTimer.IsZero)
                continue;
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects left trail including adjacent right-table entries");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Trails.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn retains right-table overrun behavior");
            AssertEqual(1, trail.Left.InstructionTimer.RawValue, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.Position.X, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine("Trail selectors: 103 reachable native words and 64 real spawn selections pass with the complete selector window forbidden.");
    }
}
