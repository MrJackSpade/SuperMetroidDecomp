using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>Checks normal and strong Brinstar pipe-bug animation selectors in production AI.</summary>
internal static class PipeBugAnimationDefinitionsVerification
{
    /// <summary>Compares eight native list pointers and verifies selection resets both instruction timers.</summary>
    internal static void VerifyPipeBugAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        const int normalTable = 0xb3882b;
        const int strongTable = 0xb38833;
        var guarded = NewPipeBugAnimationReadGuard(rom);

        foreach (bool strong in new[] { false, true })
        {
            var initializedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot initializedSlot = initializedEnemies.SlotTable.Slots[0];
            initializedSlot.Bind(strong
                ? EnemyDefinitionId.Zebbo
                : EnemyDefinitionId.Zeb, initializedSlot.Definition);
            initializedSlot.SetSpawnParameter1(strong ? (ushort)1 : (ushort)0);
            initializedSlot.Position = initializedSlot.Position.WithWhole(new(0x0100, 0x0080));
            PipeBugsBehavior.InitializeBrinstarPipeBug(initializedSlot);
            AssertEqual(AnimationProgramCursor<PipeBugProgram>.Start(PipeBugDefinitions.BrinstarProgram(
                    strong,
                    PipeBugAnimationSelector.None)),
                (initializedEnemies.PipeBugStates[0] ?? throw new InvalidOperationException("Verification requires a Pipe Bug state.")).Program,
                $"production {(strong ? "strong" : "normal")} Pipe Bug initialization");

            for (int index = 0; index < 4; index++)
            {
                var selector = (PipeBugAnimationSelector)index;
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(((strong ? strongTable : normalTable) + index * 2));
                AssertEqual(expected,
                    NativeEnemyProgramOracle.EntryOf(PipeBugNativeListAddresses.Entries, PipeBugDefinitions.BrinstarProgram(strong, selector)),
                    $"{(strong ? "strong" : "normal")} Pipe Bug {selector}");

                var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
                var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
                {
                    InstructionTimer = NativeCountdown.Start(0x2222),
                    Timer = NativeCountdown.Start(0x3333),
                };
                slot.SetSpawnParameter1(strong ? 1 : 0);
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
                AssertEqual(AnimationProgramCursor<PipeBugProgram>.Start(PipeBugDefinitions.BrinstarProgram(strong, selector)), state.Program,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug animation {selector}");
                AssertEqual(1, slot.InstructionTimer.RawValue,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug timer {selector}");
                AssertEqual(0, slot.Timer.RawValue,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug loop {selector}");
            }
        }

        AssertThrows<InvalidDataException>(
            () => PipeBugDefinitions.BrinstarProgram(
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
