using static GunshipLandingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Samus;

/// <summary>Checks gunship landing brake and idle bob data against native records and production consumers.</summary>
internal static class GunshipMotionDefinitionsVerification
{
    /// <summary>Compares seventeen brake offsets and four hover phases while source reads are forbidden.</summary>
    internal static void VerifyGunshipMotionDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGunshipBrakeAlgorithm), () => VerifyGunshipBrakeAlgorithm(rom));
        Suite(nameof(VerifyGunshipHoverTimerAlgorithm), () => VerifyGunshipHoverTimerAlgorithm(rom));
        Suite(nameof(VerifyGunshipHoverDeltaSelection), () => VerifyGunshipHoverDeltaSelection(rom));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (ushort frame = 0; frame < 17; frame++)
        {
            short expected = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2a622 + frame * 2)));

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(new GunshipMotionFixtureGuard(rom));
            var applyBrake = new Action<RoomEnemySlot, SamusState>(enemies.GunshipBehavior.BouncePostCeresGunship);
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            top.Position = top.Position.WithWhole(new(0x1234, 0x2000));
            top.VariableE = frame;
            enemies.SlotTable.Slots[1].Position = enemies.SlotTable.Slots[1].Position.WithWholeY(0x2100);
            enemies.SlotTable.Slots[2].Position = enemies.SlotTable.Slots[2].Position.WithWholeY(0x2200);
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0, 0x3000)) };

            applyBrake(top, samus);

            AssertEqual(unchecked((ushort)(0x3000 + expected)), samus.Position.Y.Whole,
                $"gunship production brake Samus Y {frame}");
            AssertEqual(unchecked((ushort)(0x2000 + expected)), top.Position.Y.Whole,
                $"gunship production brake top Y {frame}");
            AssertEqual(unchecked((ushort)(0x2100 + expected)), enemies.SlotTable.Slots[1].Position.Y.Whole,
                $"gunship production brake bottom Y {frame}");
            AssertEqual(unchecked((ushort)(0x2200 + expected)), enemies.SlotTable.Slots[2].Position.Y.Whole,
                $"gunship production brake pad Y {frame}");
        }

        for (ushort phase = 0; phase < 4; phase++)
        {
            int address = 0xa2a7cf + phase * 2;
            byte expectedTimer = GunshipMotionDefinitionsRomByte(rom, address);
            sbyte expectedDelta = unchecked((sbyte)GunshipMotionDefinitionsRomByte(rom, address + 1));

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(new GunshipMotionFixtureGuard(rom));
            var stepBob = new Action<RoomEnemySlot>(enemies.GunshipBehavior.StepGunshipBob);
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            top.VariableC = phase;
            top.VariableD = 1;
            top.Position = top.Position.WithWholeY(0x2000);
            enemies.SlotTable.Slots[1].Position = enemies.SlotTable.Slots[1].Position.WithWholeY(0x2100);
            enemies.SlotTable.Slots[2].Position = enemies.SlotTable.Slots[2].Position.WithWholeY(0x2200);

            stepBob(top);

            AssertEqual((ushort)expectedTimer, top.VariableD,
                $"gunship production bob timer {phase}");
            AssertEqual(unchecked((ushort)((phase + 1) & 3)), top.VariableC,
                $"gunship production bob phase {phase}");
            AssertEqual(unchecked((ushort)(0x2000 + expectedDelta)), top.Position.Y.Whole,
                $"gunship production bob top Y {phase}");
            AssertEqual(unchecked((ushort)(0x2100 + expectedDelta)), enemies.SlotTable.Slots[1].Position.Y.Whole,
                $"gunship production bob bottom Y {phase}");
            AssertEqual(unchecked((ushort)(0x2200 + expectedDelta)), enemies.SlotTable.Slots[2].Position.Y.Whole,
                $"gunship production bob pad Y {phase}");
        }

        Console.WriteLine(
            "Gunship motion definitions: all seventeen brake deltas, four idle-bob records, and both production consumers pass with source reads forbidden.");
    }

    /// <summary>Reads one native motion-table byte for the independent gunship oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Gunship motion definitions")]
    private static byte GunshipMotionDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the complete brake displacement sequence and rejects indices past its final frame.</summary>
    private static void VerifyGunshipBrakeAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort frame = 0; frame < 17; frame++)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((GunshipMotionDefinitions.BrakeReferenceAddress + 2 * frame))),
                GunshipMotionDefinitions.LandingBrakeYDelta(frame), $"Original gunship brake {frame}");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.LandingBrakeYDelta(17), "Brake upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.LandingBrakeYDelta(ushort.MaxValue), "Brake invalid maximum");
    }

    /// <summary>Checks all four idle-bob timer reloads and invalid phase rejection.</summary>
    private static void VerifyGunshipHoverTimerAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort phase = 0; phase < 4; phase++)
            AssertEqual(GunshipHoverTimerAlgorithmRomByte(rom, GunshipMotionDefinitions.HoverTimerReferenceAddress + 2 * phase),
                GunshipMotionDefinitions.IdleBob(phase).Timer, $"Original gunship hover timer {phase}");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.IdleBob(4), "Hover timer upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.IdleBob(ushort.MaxValue), "Hover timer invalid maximum");
    }

    /// <summary>Reads a native hover timer byte for comparison with compiled phase data.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Gunship hover timer algorithm")]
    private static byte GunshipHoverTimerAlgorithmRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the signed vertical movement selected in each idle-bob phase.</summary>
    private static void VerifyGunshipHoverDeltaSelection(SuperMetroidAddressSpace rom)
    {
        for (ushort phase = 0; phase < 4; phase++)
            AssertEqual(unchecked((sbyte)GunshipHoverDeltaSelectionRomByte(rom, GunshipMotionDefinitions.HoverDeltaReferenceAddress + 2 * phase)),
                GunshipMotionDefinitions.IdleBob(phase).YDelta, $"Original gunship hover delta {phase}");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.IdleBob(4), "Hover delta upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.IdleBob(ushort.MaxValue), "Hover delta invalid maximum");
    }

    /// <summary>Reads a native signed bob displacement byte for comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Gunship hover delta selection")]
    private static byte GunshipHoverDeltaSelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// The shared cartridge-read guard that also forwards enemy fixture lookups, so a sparse
    /// fixture's constructed population still reaches the production loader.
    /// </summary>
    internal sealed class GunshipMotionFixtureGuard : CartridgeReadGuard, IRoomEnemyFixtureSource
    {
        /// <summary>Underlying source for fixture enemy definitions outside the forbidden motion tables.</summary>
        private readonly ISnesAddressSpace source;

        /// <summary>Forbids both migrated gunship motion ranges while retaining enemy fixture access.</summary>
        public GunshipMotionFixtureGuard(ISnesAddressSpace source)
            : base(source)
        {
            this.source = source;
            Forbid(address => address is >= 0xa2a622 and < 0xa2a644 or >= 0xa2a7cf and < 0xa2a7d7,
                address => $"Gunship attempted migrated motion-definition read ${address:X6}.");
        }

        /// <summary>Forwards enemy definitions to the fixture source or compiled catalog.</summary>
        public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer) =>
            source is IRoomEnemyFixtureSource fixture
                ? fixture.ReadEnemyDefinition(pointer)
                : RoomEnemyDefinitionCatalog.Get(pointer);

        /// <summary>Forwards population data to the fixture source or compiled catalog.</summary>
        public RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer) =>
            source is IRoomEnemyFixtureSource fixture
                ? fixture.ReadEnemyPopulation(pointer)
                : RoomEnemyPopulationDefinitions.Get(pointer);

        /// <summary>Forwards graphics-set data to the fixture source or compiled catalog.</summary>
        public RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer) =>
            source is IRoomEnemyFixtureSource fixture
                ? fixture.ReadEnemyGraphicsSet(pointer)
                : RoomEnemyGraphicsSetDefinitions.Get(pointer);
    }

    /// <summary>Runs gunship motion and post-Ceres landing checks against the retail ROM.</summary>
    [VerificationCommand("--gunship-motion-definitions")]
    internal static int GunshipMotionDefinitionsCommand()
    {
        Suite(nameof(VerifyGunshipMotionDefinitions), () => VerifyGunshipMotionDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyPostCeresGunshipLanding), () => VerifyPostCeresGunshipLanding());
        return 0;
    }
}
