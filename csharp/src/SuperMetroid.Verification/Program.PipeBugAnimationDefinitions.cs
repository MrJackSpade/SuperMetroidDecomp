using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks normal and strong Brinstar pipe-bug animation selectors in production AI.</summary>
internal static class PipeBugAnimationDefinitionsVerification
{
    /// <summary>Compares eight native list pointers and verifies selection resets both instruction timers.</summary>
    internal static void VerifyPipeBugAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        const int normalTable = 0xb3882b;
        const int strongTable = 0xb38833;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guarded = NewPipeBugAnimationReadGuard(rom);

        foreach (bool strong in new[] { false, true })
        {
            var initializedEnemies = new RoomEnemySystem();
            initializedEnemies.BindFixtureBus(guarded);
            RoomEnemySlot initializedSlot = initializedEnemies.SlotTable.Slots[0];
            initializedSlot.EnemyDefinitionPointer = strong
                ? EnemyDefinitionId.Zebbo
                : EnemyDefinitionId.Zeb;
            initializedSlot.Parameter1 = strong ? (ushort)1 : (ushort)0;
            initializedSlot.Position = initializedSlot.Position.WithWhole(new(0x0100, 0x0080));
            initializedEnemies.PipeBugsBehavior.InitializeBrinstarPipeBug(initializedSlot);
            AssertEqual(PipeBugDefinitions.BrinstarInstructionList(
                    strong,
                    PipeBugAnimationSelector.None),
                initializedSlot.CurrentInstruction,
                $"production {(strong ? "strong" : "normal")} Pipe Bug initialization");

            for (int index = 0; index < 4; index++)
            {
                var selector = (PipeBugAnimationSelector)index;
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(((strong ? strongTable : normalTable) + index * 2));
                AssertEqual(expected,
                    PipeBugDefinitions.BrinstarInstructionList(strong, selector),
                    $"{(strong ? "strong" : "normal")} Pipe Bug {selector}");

                var enemies = new RoomEnemySystem();
                enemies.BindFixtureBus(guarded);
                var slot = new RoomEnemySlot(0)
                {
                    Parameter1 = strong ? (ushort)1 : (ushort)0,
                    CurrentInstruction = 0x7777,
                    InstructionTimer = 0x2222,
                    Timer = 0x3333,
                };
                var state = new PipeBugEnemyState(slot)
                {
                    AnimationState = selector,
                    InstalledAnimationState = selector == PipeBugAnimationSelector.None
                        ? PipeBugAnimationSelector.Shooting
                        : PipeBugAnimationSelector.None,
                };
                PipeBugsBehavior.SelectBrinstarPipeBugAnimation(slot, state);
                AssertEqual(selector, state.InstalledAnimationState,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug selector {selector}");
                AssertEqual(expected, slot.CurrentInstruction,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug instruction {selector}");
                AssertEqual(1, slot.InstructionTimer,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug timer {selector}");
                AssertEqual(0, slot.Timer,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug loop {selector}");
            }
        }

        AssertThrows<InvalidDataException>(
            () => PipeBugDefinitions.BrinstarInstructionList(
                strong: false,
                (PipeBugAnimationSelector)4),
            "Pipe Bug animation selector past table");
        Console.WriteLine(
            "Pipe Bug animation definitions: eight native selectors and all eight production handoffs pass with both pointer tables forbidden.");
    }

    /// <summary>Forbids runtime reads from both compiled bank B3 animation pointer tables.</summary>
    private static CartridgeReadGuard NewPipeBugAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xb3882b and < 0xb3883b,
            address => $"Pipe Bug attempted migrated animation read ${address:X6}.");
}
