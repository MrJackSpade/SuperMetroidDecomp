using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyNuclearWaffleDefinitions(SuperMetroidAddressSpace rom)
    {
        const int endpointTable = 0xa695f6;
        const int spacingTable = 0xa695fe;
        const int thresholdTable = 0xa69606;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (byte direction = 0; direction < 2; direction++)
        {
            int offset = direction * 4;
            NuclearWaffleSweepDefinition definition = NuclearWaffleDefinitions.Sweep(direction);
            AssertEqual(ReadNuclearWaffleWord(rom, endpointTable + offset), definition.StartAngle,
                $"Nuclear Waffle start angle {direction}");
            AssertEqual(ReadNuclearWaffleWord(rom, endpointTable + offset + 2), definition.EndAngle,
                $"Nuclear Waffle end angle {direction}");
            AssertEqual(unchecked((short)ReadNuclearWaffleWord(rom, spacingTable + offset)),
                definition.SegmentSpacing,
                $"Nuclear Waffle segment spacing {direction}");
            AssertEqual(unchecked((short)ReadNuclearWaffleWord(rom, spacingTable + offset + 2)),
                definition.InterleavedSegmentOffset,
                $"Nuclear Waffle interleaved offset {direction}");
            AssertEqual(ReadNuclearWaffleWord(rom, thresholdTable + offset),
                definition.SecondTurnThreshold,
                $"Nuclear Waffle second turn threshold {direction}");
            AssertEqual(ReadNuclearWaffleWord(rom, thresholdTable + offset + 2),
                definition.FirstTurnThreshold,
                $"Nuclear Waffle first turn threshold {direction}");

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new NuclearWaffleDefinitionReadGuard(rom));
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeNuclearWaffle", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = 0x2001;
            slot.Parameter2 = (ushort)(0x2000 | direction);
            slot.XPosition = 0x0180;
            slot.YPosition = 0x0200;
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
             index < NuclearWaffleInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            NuclearWaffleInstructionMechanicsWord definition =
                NuclearWaffleInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadNuclearWaffleWord(rom, 0xa60000 | definition.Address),
                $"Nuclear Waffle mechanics word $A6:{definition.Address:X4}");
        }

        var programGuard = new NuclearWaffleProgramReadGuard(rom);
        var programSystem = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            programSystem,
            programGuard);
        RoomEnemySlot programSlot = programSystem.Slots[0];
        programSlot.EnemyDefinitionPointer = NuclearWaffleDefinitions.EnemyDefinition;
        programSlot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        programSlot.CurrentInstruction = NuclearWaffleInstructionProgramDefinitions.BodyLoop;
        programSlot.InstructionTimer = 1;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments =
            [programSlot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        // Twelve three-frame entries total 36 frames; the margin proves the terminal goto
        // restarts the production stream rather than merely reaching its target.
        for (int frame = 0; frame < 40; frame++)
            process.Invoke(programSystem, arguments);

        AssertEqual(
            NuclearWaffleInstructionProgramDefinitions.PresentationWordCount,
            programGuard.ObservedPresentationWords.Count,
            "all live Nuclear Waffle spritemap words remain cartridge reads");
        for (int index = 0;
             index < NuclearWaffleInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(programGuard.ObservedPresentationWords.Contains(address),
                $"production execution reads Nuclear Waffle presentation word $A6:{address:X4}");
        }
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
            "mechanics reads forbidden; 12 spritemap words remain live.");
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

    private static ushort ReadNuclearWaffleWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class NuclearWaffleDefinitionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa695f6 and < 0xa6960e
            ? throw new InvalidOperationException(
                $"Nuclear Waffle attempted migrated geometry read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class NuclearWaffleProgramReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (NuclearWaffleInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Nuclear Waffle mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NuclearWaffleInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
