using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;

internal static class NuclearWaffleDefinitionsVerification
{
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
            enemies.BindFixtureBus(NewNuclearWaffleDefinitionReadGuard(rom));
            Action<RoomEnemySlot> initialize = enemies.NuclearWaffleBehavior.InitializeNuclearWaffle;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter1 = 0x2001;
            slot.Parameter2 = (ushort)(0x2000 | direction);
            slot.Position = slot.Position.WithWhole(new(0x0180, 0x0200));
            initialize(slot);

            NuclearWaffleEnemyState state = enemies.NuclearWaffleStates[0]!;
            AssertEqual(definition.StartAngle, state.SweepStartAngle,
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
            AssertEqual(NuclearWaffleDefinitions.InitialInstructionList, slot.CurrentInstruction,
                $"Nuclear Waffle production initial instruction {direction}");
            AssertEqual(4, state.ProjectileSegments.Count(segment => segment is not null),
                $"Nuclear Waffle production projectile links {direction}");
            AssertEqual(3, state.SpriteSegments.Count(segment => segment is not null),
                $"Nuclear Waffle production sprite links {direction}");
        }

        AssertThrows<InvalidDataException>(() => NuclearWaffleDefinitions.Sweep(2),
            "Nuclear Waffle direction beyond authored table");
        AssertThrows<InvalidDataException>(() => NuclearWaffleDefinitions.Sweep(byte.MaxValue),
            "Nuclear Waffle restored direction does not read adjacent enemy code");

        for (int index = 0;
             index < NuclearWaffleInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NuclearWaffleInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | definition.Address)),
                $"Nuclear Waffle mechanics word $A6:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var programGuard = NewNuclearWaffleProgramReadGuard(rom);
        var programSystem = new RoomEnemySystem();
        programSystem.BindFixtureBus(programGuard);
        RoomEnemySlot programSlot = programSystem.SlotTable.Slots[0];
        programSlot.EnemyDefinitionPointer = EnemyDefinitionId.Puromi;
        programSlot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        programSlot.CurrentInstruction = NuclearWaffleInstructionProgramDefinitions.BodyLoop;
        programSlot.InstructionTimer = 1;
        object?[] arguments =
            [programSlot, null, null, Position16<RoomPixels>.Zero, (ushort)0];

        // Twelve three-frame entries total 36 frames; the margin proves the terminal goto
        // restarts the production stream rather than merely reaching its target.
        var installed = new HashSet<ushort>();
        for (int frame = 0; frame < 40; frame++)
        {
            EnemyInstructionRunner.Process(programSystem, arguments);
            VerifyExecutedEnemySelector(rom, programSlot, executedOperands);
            ushort operand = unchecked((ushort)(programSlot.CurrentInstruction - 2));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | operand)), programSlot.SpritemapPointer,
                "actual installed Nuclear Waffle native visual selector");
            installed.Add(operand);
        }

        AssertEqual(
            NuclearWaffleInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "all Nuclear Waffle selectors execute and match cartridge operands");
        for (int index = 0;
             index < NuclearWaffleInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution covers Nuclear Waffle presentation word $A6:{address:X4}");
        }
        AssertEqual(0, programGuard.ObservedWords.Count,
            "compiled presentation selectors require no runtime ROM reads");
        AssertEqual(0, programGuard.ForbiddenReadAttempts,
            "production execution avoids every compiled Nuclear Waffle mechanics byte");

        AssertThrows<InvalidDataException>(
            () => NuclearWaffleInstructionProgramDefinitions.ReadMechanicsWord(0x9492),
            "interleaved Nuclear Waffle spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NuclearWaffleInstructionProgramDefinitions.ReadMechanicsWord(0x94c4),
            "adjacent Nuclear Waffle initialization code is rejected as mechanics");

        _ = ProbeNuclearWaffleInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNuclearWaffleInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Nuclear Waffle allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Nuclear Waffle mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Nuclear Waffle definitions: twelve geometry words, both complete production " +
            "initializers, 14 compiled instruction words, and the full body loop pass with " +
            "runtime ROM reads forbidden; 12 executed selectors match the cartridge.");
    }

    private static int ProbeNuclearWaffleInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NuclearWaffleInstructionProgramDefinitions.ReadMechanicsWord(
                NuclearWaffleInstructionProgramDefinitions.BodyLoop);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewNuclearWaffleDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa695f6 and < 0xa6960e,
                address => $"Nuclear Waffle attempted migrated geometry read ${address:X6}.");

    private static CartridgeReadGuard NewNuclearWaffleProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => NuclearWaffleInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Nuclear Waffle mechanics byte ${address:X6}.")
            .ObserveWords(0xa60000, NuclearWaffleInstructionProgramDefinitions.PresentationWordCount, NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--nuclear-waffle-instruction-mechanics")]
    internal static int NuclearWaffleInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
