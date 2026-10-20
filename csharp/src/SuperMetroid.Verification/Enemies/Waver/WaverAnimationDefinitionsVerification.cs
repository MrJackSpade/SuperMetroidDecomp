using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies.Waver;

/// <summary>Checks compiled Waver animation selectors in both handoff and initialization paths.</summary>
internal static class WaverAnimationDefinitionsVerification
{
    /// <summary>Compares four native pointers and confirms timer reset without rereading their table.</summary>
    internal static void VerifyWaverAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            var selector = (WaverAnimationSelector)index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa386db + index * 2)),
                NativeEnemyProgramOracle.EntryOf(WaverNativeListAddresses.Entries, WaverAnimationDefinitions.Program(selector)),
                $"Waver animation selector {selector}");
        }

        for (int index = 0; index < 4; index++)
        {
            var selector = (WaverAnimationSelector)index;
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                CurrentInstruction = 0x7777,
                InstructionTimer = NativeCountdown.Start(0x2222),
                Timer = NativeCountdown.Start(0x3333),
            };
            var state = new WaverEnemyState
            {
                CurrentInstructionListIndex = selector == WaverAnimationSelector.None
                    ? WaverAnimationSelector.FacingRight
                    : WaverAnimationSelector.None,
                RequestedInstructionListIndex = selector,
            };
            WaverBehavior.SetWaverProgram(slot, state);
            AssertEqual(selector, state.CurrentInstructionListIndex,
                $"production Waver current selector {selector}");
            AssertEqual(AnimationProgramCursor<WaverProgram>.Start(WaverAnimationDefinitions.Program(selector)),
                state.Program,
                $"production Waver animation {selector}");
            AssertEqual(1, slot.InstructionTimer.RawValue,
                $"production Waver instruction timer {selector}");
            AssertEqual(0, slot.Timer.RawValue,
                $"production Waver loop timer {selector}");
        }

        var guarded = NewWaverAnimationReadGuard(rom);
        for (ushort parameter = 0; parameter < 2; parameter++)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Waver, slot.Definition);
            slot.SetSpawnParameter1(parameter);
            WaverBehavior.InitializeWaver(slot);
            var expected = (WaverAnimationSelector)parameter;
            WaverEnemyState waver = enemies.WaverStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WaverStates[0].");
            AssertEqual(expected, waver.RequestedInstructionListIndex,
                $"production Waver initial requested selector {parameter}");
            AssertEqual(expected, waver.CurrentInstructionListIndex,
                $"production Waver initial current selector {parameter}");
            AssertEqual(AnimationProgramCursor<WaverProgram>.Start(WaverAnimationDefinitions.Program(expected)),
                waver.Program,
                $"production Waver initial animation {parameter}");
        }

        AssertThrows<InvalidDataException>(
            () => WaverAnimationDefinitions.Program((WaverAnimationSelector)4),
            "Waver animation selector past table");
        Console.WriteLine(
            "Waver animation definitions: four native selectors, all four real handoffs, and both production initial facing paths pass with the pointer table forbidden.");
    }

    /// <summary>Rejects runtime reads of the compiled bank A3 animation pointer table.</summary>
    private static CartridgeReadGuard NewWaverAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa386db and < 0xa386e3,
            address => $"Waver attempted migrated animation-selector read ${address:X6}.");
}
