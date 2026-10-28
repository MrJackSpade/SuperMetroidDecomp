using static SuperMetroid.Verification.Enemies.Gunship.GunshipLandingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Gunship;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks gunship landing brake and idle bob data against native records and production consumers.</summary>
internal static class GunshipMotionDefinitionsVerification
{
    /// <summary>Compares seventeen brake offsets and four hover phases while source reads are forbidden.</summary>
    internal static void VerifyGunshipMotionDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGunshipBrakeAlgorithm), () => VerifyGunshipBrakeAlgorithm(rom));
        Suite(nameof(VerifyGunshipHoverTimerAlgorithm), () => VerifyGunshipHoverTimerAlgorithm(rom));
        Suite(nameof(VerifyGunshipHoverDeltaSelection), () => VerifyGunshipHoverDeltaSelection(rom));

        for (ushort frame = 0; frame < 17; frame++)
        {
            short expected = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2a622 + frame * 2)));

            var enemies = new RoomEnemySystem(new EnemySession(), definitions: new GunshipMotionFixtureGuard(rom));
            var applyBrake = new Action<RoomEnemySlot, SamusState>(enemies.GunshipBehavior.BouncePostCeresGunship);
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            top.Position = top.Position.WithWhole(new(0x1234, 0x2000));
            var part = new GunshipPartState();
            top.SetSpeciesState(part, part.ClearWithRecord);
            part.LandingBrakeStep = frame;
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

            var enemies = new RoomEnemySystem(new EnemySession(), definitions: new GunshipMotionFixtureGuard(rom));
            var stepBob = new Action<RoomEnemySlot>(enemies.GunshipBehavior.StepGunshipBob);
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            var part = new GunshipPartState();
            top.SetSpeciesState(part, part.ClearWithRecord);
            part.BobPhase = phase;
            part.BobTimer = 1;
            top.Position = top.Position.WithWholeY(0x2000);
            enemies.SlotTable.Slots[1].Position = enemies.SlotTable.Slots[1].Position.WithWholeY(0x2100);
            enemies.SlotTable.Slots[2].Position = enemies.SlotTable.Slots[2].Position.WithWholeY(0x2200);

            stepBob(top);

            AssertEqual(expectedTimer, part.BobTimer,
                $"gunship production bob timer {phase}");
            AssertEqual((phase + 1) & 3, part.BobPhase,
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
    private static byte GunshipMotionDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the complete brake displacement sequence and rejects indices past its final frame.</summary>
    private static void VerifyGunshipBrakeAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort frame = 0; frame < 17; frame++)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((GunshipMotionDefinitions.BrakeReferenceAddress + 2 * frame))),
                GunshipMotion.LandingBrakeYDelta(frame), $"Original gunship brake {frame}");
        AssertThrows<InvalidDataException>(() => GunshipMotion.LandingBrakeYDelta(17), "Brake upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotion.LandingBrakeYDelta(ushort.MaxValue), "Brake invalid maximum");
    }

    /// <summary>Checks all four idle-bob timer reloads and invalid phase rejection.</summary>
    private static void VerifyGunshipHoverTimerAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort phase = 0; phase < 4; phase++)
            AssertEqual(GunshipHoverTimerAlgorithmRomByte(rom, GunshipMotionDefinitions.HoverTimerReferenceAddress + 2 * phase),
                GunshipMotion.IdleBob(phase).Timer, $"Original gunship hover timer {phase}");
        AssertThrows<InvalidDataException>(() => GunshipMotion.IdleBob(4), "Hover timer upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotion.IdleBob(ushort.MaxValue), "Hover timer invalid maximum");
    }

    /// <summary>Reads a native hover timer byte for comparison with compiled phase data.</summary>
    private static byte GunshipHoverTimerAlgorithmRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the signed vertical movement selected in each idle-bob phase.</summary>
    private static void VerifyGunshipHoverDeltaSelection(SuperMetroidAddressSpace rom)
    {
        for (ushort phase = 0; phase < 4; phase++)
            AssertEqual(unchecked((sbyte)GunshipHoverDeltaSelectionRomByte(rom, GunshipMotionDefinitions.HoverDeltaReferenceAddress + 2 * phase)),
                GunshipMotion.IdleBob(phase).YDelta, $"Original gunship hover delta {phase}");
        AssertThrows<InvalidDataException>(() => GunshipMotion.IdleBob(4), "Hover delta upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotion.IdleBob(ushort.MaxValue), "Hover delta invalid maximum");
    }

    /// <summary>Reads a native signed bob displacement byte for comparison.</summary>
    private static byte GunshipHoverDeltaSelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// The shared cartridge-read guard that also forwards enemy fixture lookups, so a sparse
    /// fixture's constructed population still reaches the production loader.
    /// </summary>
    internal sealed class GunshipMotionFixtureGuard : CartridgeReadGuard, IRoomEnemyDefinitionSource
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
            source is IRoomEnemyDefinitionSource fixture
                ? fixture.ReadEnemyDefinition(pointer)
                : RoomEnemyDefinitionCatalog.Get(pointer);

    /// <summary>Uses the compiled falling-tube placement; this fixture constructs none.</summary>
    public RoomEnemyPopulationRecord ReadFallingTubePopulation(SuperMetroid.Core.Enemies.MotherBrainTubePiece piece) =>
        CompiledRoomEnemyDefinitions.Instance.ReadFallingTubePopulation(piece);
    }

    /// <summary>Runs gunship motion and post-Ceres landing checks against the retail ROM.</summary>
    [VerificationCommand("--gunship-motion-definitions")]
    internal static int GunshipMotionDefinitionsCommand()
    {
        Suite(nameof(VerifyGunshipMotionDefinitions), () => VerifyGunshipMotionDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyPostCeresGunshipLanding), () => VerifyPostCeresGunshipLanding());
        return 0;
    }
}
