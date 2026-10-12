using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class ZoaAnimationDefinitionsVerification
{
    internal static void VerifyZoaAnimationDefinitions(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        for (int index = 0; index < 4; index++)
        {
            var selector = (ZoaAnimationSelector)index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((ZoaAnimationDefinitions.ReferenceAddress + index * 2)),
                ZoaAnimationDefinitions.InstructionList(selector),
                $"Zoa animation selector {selector}");
        }

        foreach (ushort invalid in new ushort[] { 4, 5, 0x8000, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() =>
                ZoaAnimationDefinitions.InstructionList((ZoaAnimationSelector)invalid),
                "Zoa animation rejects unsupported flag combinations");
        if (definitionsOnly)
        {
            Console.WriteLine("Zoa animation cases: four original pointers and unsupported flag bounds pass.");
            return;
        }
        Suite(nameof(VerifyAllZoaAnimationHandoffs), () => VerifyAllZoaAnimationHandoffs());
        Suite(nameof(VerifyLiveZoaAnimationHandoffs), () => VerifyLiveZoaAnimationHandoffs(rom, facingRight: false));
        Suite(nameof(VerifyLiveZoaAnimationHandoffs), () => VerifyLiveZoaAnimationHandoffs(rom, facingRight: true));

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
            .SetValue(enemies, NewZoaAnimationReadGuard(rom));
        RoomEnemySlot slot = enemies.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));

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
            Position = new(new(unchecked((ushort)(slot.Position.X.Whole + (facingRight ? 0x10 : -0x10))), 0), new(slot.Position.Y.Whole, 0)),
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

    private static CartridgeReadGuard NewZoaAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa3b40d and < 0xa3b415,
            address => $"Zoa attempted migrated animation-selector read ${address:X6}.");
}
