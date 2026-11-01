using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailCoordinatesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks compiled projectile trail selector words and actual left/right spawn selections.</summary>
internal static class ProjectileTrailDefinitionsVerification
{
    /// <summary>
    /// Compares both selector tables' words and every trailing shot's spawn choices with retail data, and requires a
    /// selection past the tables, which no trailing shot makes, to be rejected (#1500).
    /// </summary>
    internal static void VerifyProjectileTrailDefinitions(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(bus));
        int start = SamusProjectileRomDataTrailsConstants.LeftInstructionPointers;
        int end = SamusProjectileRomDataTrailsConstants.RightInstructionPointers +
            SamusProjectileRomDataTrailsConstants.InstructionPointerCount * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailDefinitions.ReadSelector(address),
                "Trail selectors retain every aligned native word of both tables");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start - 2), "Trail selector rejects the preceding word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start + 1), "Trail selector rejects unaligned reads");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(end), "Trail selector rejects the word past the right table");
        // The game indexes both tables with six bits, but no trailing shot's selection passes the tables' thirty-nine.
        for (int selection = SamusProjectileRomDataTrailsConstants.InstructionPointerCount; selection < 64; selection++)
        {
            foreach (HorizontalDirection side in (HorizontalDirection[])[HorizontalDirection.Left, HorizontalDirection.Right])
                AssertThrows<ArgumentOutOfRangeException>(() => ProjectileTrailSelection.Select(selection, side),
                    $"Trail selection {selection} past the {side} table is rejected");
        }
        int spawned = 0;
        for (int selection = 0; selection < 64; selection++)
        {
            // Of the echo selections only the Spazer special attack's trailing pair ($24) spawns trails; the Shinespark
            // crash's echo ($29) runs its own update, which spawns none, and the typed identity names no other echo (#1470).
            if ((selection & 0x20) != 0 && selection != 0x24)
                continue;
            spawned++;
            var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
            var projectile = new SamusProjectileSlot(0)
            {
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(selection),
                Cursor = SamusProjectileProgramAddresses.CursorAt(0x86e3),
                Position = new(new(100, 0), new(200, 0)),
            };
            projectiles.Trails.SpawnTrail(projectile);
            var trail = projectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            // An empty trail family (programs, direction list or coordinates outside the compiled data) draws no trail (#1318).
            if (trail.Left.FrameHold.IsZero)
                continue;
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects the left table's trail");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomDataTrailsConstants.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn selects the right table's trail");
            AssertEqual(1, trail.Left.FrameHold.RawValue, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.Position.X, "Selector extraction leaves origin offset unchanged");
        }
        var echoProjectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
        var echo = new SamusProjectileSlot(0)
        {
            Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho),
            Cursor = SamusProjectileProgramAddresses.CursorAt(0x86e3),
            Position = new(new(100, 0), new(200, 0)),
        };
        AssertThrows<ArgumentOutOfRangeException>(() => echoProjectiles.Trails.SpawnTrail(echo), "A Shinespark echo's selection past the trail tables is rejected");
        Console.WriteLine($"Trail selectors: both tables' native words, the rejected selections past them and {spawned} real spawn selections pass.");
    }
}
