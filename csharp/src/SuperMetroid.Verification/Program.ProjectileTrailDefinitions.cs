using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static AssertionsVerification;
using static ProjectileTrailCoordinatesVerification;
using static SuiteTimingVerification;

internal static class ProjectileTrailDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled projectile trail selector words")]
    internal static void VerifyProjectileTrailDefinitions(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(bus));
        int start = SamusProjectileRomData.Trails.LeftInstructionPointers;
        int end = SamusProjectileRomData.Trails.RightInstructionPointers + 64 * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailDefinitions.ReadSelector(address), "Trail selectors retain every reachable aligned native word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start - 2), "Trail selector rejects the preceding word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start + 1), "Trail selector rejects unaligned reads");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(end), "Trail selector rejects the following word");
        for (int selection = 0; selection < 64; selection++)
        {
            var projectiles = new SamusProjectileSystem();
            var projectile = new SamusProjectileSlot(0)
            {
                Type = (ushort)selection,
                InstructionPointer = 0x86e3,
                Position = new(new(100, 0), new(200, 0)),
            };
            projectiles.Trails.SpawnTrail(NewTrailSelectorGuard(start, end), projectile);
            var trail = projectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects left trail including adjacent right-table entries");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Trails.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn retains right-table overrun behavior");
            AssertEqual(1, trail.Left.InstructionTimer.RawValue, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.Position.X, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine("Trail selectors: 103 reachable native words and 64 real spawn selections pass with the complete selector window forbidden.");
    }

    internal static MutableCartridgeReadGuard NewTrailSelectorGuard(int start, int end)
    {
        // Unrelated visual offsets and a wrapped trail-coordinate pointer into the low WRAM
        // mirror read the zero-filled sparse fixture, so all low-six-bit combinations have
        // deterministic origins without weakening the selector guard.
        var guard = new MutableCartridgeReadGuard(new TestAddressSpace());
        guard.Forbid(address => address >= start && address < end,
                _ => new InvalidDataException("Trail spawn still reads authored selector ROM."))
            .Forbid(address => SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) == SnesDmaSourceKind.SaveRam,
                _ => "Trail selector fixture cannot read SRAM.")
            .RejectWrites(_ => "Trail spawn wrote ROM.");
        return guard;
    }
}
