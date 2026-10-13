using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class WaverAnimationDefinitionsVerification
{
    internal static void VerifyWaverAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            var selector = (WaverAnimationSelector)index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa386db + index * 2)),
                WaverAnimationDefinitions.InstructionList(selector),
                $"Waver animation selector {selector}");
        }

        for (int index = 0; index < 4; index++)
        {
            var selector = (WaverAnimationSelector)index;
            var slot = new RoomEnemySlot(0)
            {
                CurrentInstruction = 0x7777,
                InstructionTimer = NativeCountdown.Start(0x2222),
                Timer = NativeCountdown.Start(0x3333),
            };
            var state = new WaverEnemyState(slot)
            {
                CurrentInstructionListIndex = selector == WaverAnimationSelector.None
                    ? WaverAnimationSelector.FacingRight
                    : WaverAnimationSelector.None,
                RequestedInstructionListIndex = selector,
            };
            WaverBehavior.SetWaverInstructionList(slot, state);
            AssertEqual(selector, state.CurrentInstructionListIndex,
                $"production Waver current selector {selector}");
            AssertEqual(WaverAnimationDefinitions.InstructionList(selector),
                slot.CurrentInstruction,
                $"production Waver instruction {selector}");
            AssertEqual(1, slot.InstructionTimer.RawValue,
                $"production Waver instruction timer {selector}");
            AssertEqual(0, slot.Timer.RawValue,
                $"production Waver loop timer {selector}");
        }

        var guarded = NewWaverAnimationReadGuard(rom);
        for (ushort parameter = 0; parameter < 2; parameter++)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter1 = parameter;
            enemies.WaverBehavior.InitializeWaver(slot);
            var expected = (WaverAnimationSelector)parameter;
            AssertEqual(expected, enemies.WaverStates[0]!.RequestedInstructionListIndex,
                $"production Waver initial requested selector {parameter}");
            AssertEqual(expected, enemies.WaverStates[0]!.CurrentInstructionListIndex,
                $"production Waver initial current selector {parameter}");
            AssertEqual(WaverAnimationDefinitions.InstructionList(expected), slot.CurrentInstruction,
                $"production Waver initial instruction {parameter}");
        }

        AssertThrows<InvalidDataException>(
            () => WaverAnimationDefinitions.InstructionList((WaverAnimationSelector)4),
            "Waver animation selector past table");
        Console.WriteLine(
            "Waver animation definitions: four native selectors, all four real handoffs, and both production initial facing paths pass with the pointer table forbidden.");
    }

    private static CartridgeReadGuard NewWaverAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa386db and < 0xa386e3,
            address => $"Waver attempted migrated animation-selector read ${address:X6}.");
}
