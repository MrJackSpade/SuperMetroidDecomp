using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyZoaAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            var selector = (ZoaAnimationSelector)index;
            AssertEqual(ReadZoaAnimationWord(rom, 0xa3b40d + index * 2),
                ZoaAnimationDefinitions.InstructionList(selector),
                $"Zoa animation selector {selector}");
        }

        VerifyAllZoaAnimationHandoffs();
        VerifyLiveZoaAnimationHandoffs(rom, facingRight: false);
        VerifyLiveZoaAnimationHandoffs(rom, facingRight: true);

        AssertThrows<InvalidDataException>(
            () => ZoaAnimationDefinitions.InstructionList((ZoaAnimationSelector)4),
            "Zoa animation selector past table");
        Console.WriteLine(
            "Zoa animation definitions: four native selectors, all four installs, and both complete live facing/rise/shoot handoffs pass with the pointer table forbidden.");
    }

    private static void VerifyAllZoaAnimationHandoffs()
    {
        MethodInfo setInstruction = typeof(RoomEnemySystem).GetMethod(
            "SetZoaInstructionList", BindingFlags.Static | BindingFlags.NonPublic)!;
        for (int index = 0; index < 4; index++)
        {
            var selector = (ZoaAnimationSelector)index;
            var slot = NewZoaAnimationSlot();
            var state = new ZoaEnemyState(slot)
            {
                InstructionListTableIndex = selector,
                PreviousInstructionListTableIndex = selector == ZoaAnimationSelector.None
                    ? ZoaAnimationSelector.Rising
                    : ZoaAnimationSelector.None,
            };
            setInstruction.Invoke(null, [slot, state]);
            AssertZoaAnimationHandoff(slot, state, selector, $"Zoa {selector}");
        }
    }

    private static void VerifyLiveZoaAnimationHandoffs(
        SuperMetroidAddressSpace rom,
        bool facingRight)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(enemies, new ZoaAnimationReadGuard(rom));
        RoomEnemySlot slot = enemies.Slots[0];
        slot.XPosition = 0x0100;
        slot.YPosition = 0x0080;

        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeZoa", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo wait = typeof(RoomEnemySystem).GetMethod(
            "RunZoaWait", BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo rise = typeof(RoomEnemySystem).GetMethod(
            "RunZoaRising", BindingFlags.Static | BindingFlags.NonPublic)!;
        initialize.Invoke(enemies, [slot]);
        ZoaEnemyState state = enemies.ZoaStates[0]!;

        var samus = new SamusState
        {
            XPosition = unchecked((ushort)(slot.XPosition + (facingRight ? 0x10 : -0x10))),
            YPosition = slot.YPosition,
        };
        wait.Invoke(null, [slot, state, samus]);
        ZoaAnimationSelector direction = facingRight
            ? ZoaAnimationSelector.FacingRight
            : ZoaAnimationSelector.None;
        ZoaAnimationSelector rising = direction | ZoaAnimationSelector.Rising;
        AssertZoaAnimationHandoff(
            slot,
            state,
            rising,
            facingRight ? "live right-facing Zoa rise" : "live left-facing Zoa rise");

        rise.Invoke(null, [slot, state, samus]);
        AssertZoaAnimationHandoff(
            slot,
            state,
            direction,
            facingRight ? "live right-facing Zoa shoot" : "live left-facing Zoa shoot");
    }

    private static RoomEnemySlot NewZoaAnimationSlot() => new(0)
    {
        CurrentInstruction = 0x7777,
        InstructionTimer = 0x2222,
        Timer = 0x3333,
    };

    private static void AssertZoaAnimationHandoff(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        ZoaAnimationSelector expected,
        string context)
    {
        AssertEqual(expected, state.InstructionListTableIndex,
            $"{context} requested selector");
        AssertEqual(expected, state.PreviousInstructionListTableIndex,
            $"{context} installed selector");
        AssertEqual(ZoaAnimationDefinitions.InstructionList(expected),
            slot.CurrentInstruction,
            $"{context} instruction");
        AssertEqual(1, slot.InstructionTimer, $"{context} instruction timer");
        AssertEqual(0, slot.Timer, $"{context} loop timer");
    }

    private static ushort ReadZoaAnimationWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class ZoaAnimationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa3b40d and < 0xa3b415
                ? throw new InvalidOperationException(
                    $"Zoa attempted migrated animation-selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
