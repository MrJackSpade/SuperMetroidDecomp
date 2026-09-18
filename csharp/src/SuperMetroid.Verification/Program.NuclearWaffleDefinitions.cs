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

        Console.WriteLine(
            "Nuclear Waffle definitions: twelve native words and both complete production initializers pass with geometry reads forbidden.");
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
}
