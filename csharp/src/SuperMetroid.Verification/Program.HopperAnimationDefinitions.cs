using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHopperAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        int[] tables = [0xa3aac2, 0xa3aaca, 0xa3aad2, 0xa3aada];
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;

        for (ushort variant = 0; variant < 4; variant++)
        for (int selector = 0; selector < tables.Length; selector++)
        {
            bool upsideDown = (selector & 1) != 0;
            bool jumping = (selector & 2) != 0;
            ushort native = ReadHopperAnimationWord(rom, tables[selector] + variant * 2);
            AssertEqual(native,
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping),
                $"hopper animation variant {variant}, selector {selector}");
        }

        for (ushort variant = 0; variant < 4; variant++)
        for (int orientation = 0; orientation < 2; orientation++)
        {
            bool upsideDown = orientation != 0;
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies, new HopperAnimationReadGuard(rom));
            typeof(RoomEnemySystem).GetField("_setRandomNumber", flags)!.SetValue(
                enemies, (Action<ushort>)(_ => { }));
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies, (Func<ushort>)(() => 0));

            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeHopper", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var startJump = typeof(RoomEnemySystem).GetMethod("StartHopperJump", flags)!
                .CreateDelegate<Action<RoomEnemySlot, HopperEnemyState, bool, bool>>();
            var land = typeof(RoomEnemySystem).GetMethod("LandHopper", flags)!
                .CreateDelegate<Action<RoomEnemySlot, HopperEnemyState>>();

            RoomEnemySlot slot = enemies.Slots[0];
            slot.Definition = default(RoomEnemyDefinition) with { VariantIndex = variant };
            slot.Parameter1 = upsideDown ? (ushort)1 : (ushort)0;
            initialize(slot);
            HopperEnemyState state = enemies.HopperStates[0]!;
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: false),
                slot.CurrentInstruction,
                $"hopper production initial landed list {variant}/{orientation}");

            state.XVelocity = 3;
            startJump(slot, state, upsideDown, false);
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: true),
                slot.CurrentInstruction,
                $"hopper production jumping list {variant}/{orientation}");

            land(slot, state);
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: false),
                slot.CurrentInstruction,
                $"hopper production landed handoff {variant}/{orientation}");
        }

        AssertThrows<InvalidDataException>(
            () => HopperAnimationDefinitions.InstructionList(4, false, false),
            "hopper animation variant beyond authored table");
        AssertThrows<InvalidDataException>(
            () => HopperAnimationDefinitions.InstructionList(ushort.MaxValue, true, true),
            "restored hopper animation variant does not wrap into authored table");
        Console.WriteLine(
            "Hopper animation definitions: sixteen native selectors and all floor/ceiling initializer, jump, and landing consumers pass with source tables forbidden.");
    }

    private static ushort ReadHopperAnimationWord(SuperMetroidAddressSpace source, int address) =>
        (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

    private sealed class HopperAnimationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa3aac2 and < 0xa3aae2
            ? throw new InvalidOperationException(
                $"Hopper attempted migrated animation-selector read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
