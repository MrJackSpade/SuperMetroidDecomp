using SuperMetroid.Core.Fx;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Nuclear;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Nuclear Waffle sweep geometry and body-loop mechanics against bank-$A6 ROM and production enemy state.</summary>
internal static class NuclearWaffleDefinitionsVerification
{
    /// <summary>Compares both sweep directions and the full body loop with retail data while guarding migrated geometry and program reads.</summary>
    internal static void VerifyNuclearWaffleDefinitions(SuperMetroidAddressSpace rom)
    {
        const int endpointTable = 0xa695f6;
        const int spacingTable = 0xa695fe;
        const int thresholdTable = 0xa69606;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (byte direction = 0; direction < 2; direction++)
        {
            int offset = direction * 4;
            NuclearWaffleSweepDefinition definition = NuclearWaffleDefinitions.Sweep(direction);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((endpointTable + offset)), definition.StartAngle,
                $"Nuclear Waffle start angle {direction}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((endpointTable + offset + 2)), definition.EndAngle,
                $"Nuclear Waffle end angle {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((spacingTable + offset))),
                definition.SegmentSpacing,
                $"Nuclear Waffle segment spacing {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((spacingTable + offset + 2))),
                definition.InterleavedSegmentOffset,
                $"Nuclear Waffle interleaved offset {direction}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((thresholdTable + offset)),
                definition.SecondTurnThreshold,
                $"Nuclear Waffle second turn threshold {direction}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((thresholdTable + offset + 2)),
                definition.FirstTurnThreshold,
                $"Nuclear Waffle first turn threshold {direction}");

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot> initialize = enemies.NuclearWaffleBehavior.InitializeNuclearWaffle;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Puromi, slot.Definition);
            slot.Parameter1 = 0x2001;
            slot.Parameter2 = (ushort)(0x2000 | direction);
            slot.Position = slot.Position.WithWhole(new(0x0180, 0x0200));
            initialize(slot);

            NuclearWaffleEnemyState state = (enemies.NuclearWaffleStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NuclearWaffleStates[0]."));
            AssertEqual(definition.StartAngle, (state ?? throw new InvalidOperationException("Verification requires state.")).SweepStartAngle,
                $"Nuclear Waffle production start angle {direction}");
            AssertEqual(definition.StartAngle, state.CurrentAngle,
                $"Nuclear Waffle production current angle {direction}");
            AssertEqual(definition.EndAngle, state.SweepEndAngle,
                $"Nuclear Waffle production end angle {direction}");
            AssertEqual(definition.SegmentSpacing, state.SegmentSpacing,
                $"Nuclear Waffle production spacing {direction}");
            AssertEqual(definition.InterleavedSegmentOffset, state.InterleavedSegmentOffset,
                $"Nuclear Waffle production interleaved offset {direction}");
            AssertEqual(definition.SecondTurnThreshold, state.SecondTurnThreshold,
                $"Nuclear Waffle production second threshold {direction}");
            AssertEqual(definition.FirstTurnThreshold, state.FirstTurnThreshold,
                $"Nuclear Waffle production first threshold {direction}");
            AssertEqual(AnimationProgramCursor<NuclearWaffleProgram>.Start(NuclearWaffleProgram.BodyLoop), state.Program,
                $"Nuclear Waffle production initial animation {direction}");
            AssertEqual(4, state.ProjectileSegments.Count(segment => segment is not null),
                $"Nuclear Waffle production projectile links {direction}");
            AssertEqual(3, state.SpriteSegments.Count(segment => segment is not null),
                $"Nuclear Waffle production sprite links {direction}");
        }

        AssertThrows<InvalidDataException>(() => NuclearWaffleDefinitions.Sweep(2),
            "Nuclear Waffle direction beyond authored table");
        AssertThrows<InvalidDataException>(() => NuclearWaffleDefinitions.Sweep(byte.MaxValue),
            "Nuclear Waffle restored direction does not read adjacent enemy code");

        Console.WriteLine(
            "Nuclear Waffle definitions: twelve geometry words and both complete production " +
            "initializers pass with runtime ROM reads forbidden.");
    }


    /// <summary>Runs the Nuclear Waffle geometry and body-animation verification command.</summary>
    [VerificationCommand("--nuclear-waffle-instruction-mechanics")]
    internal static int NuclearWaffleInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(rom));
        Suite(nameof(NuclearWaffleAnimationsVerification.VerifyNuclearWaffleAnimations),
            () => NuclearWaffleAnimationsVerification.VerifyNuclearWaffleAnimations(rom));
        return 0;
    }
}
