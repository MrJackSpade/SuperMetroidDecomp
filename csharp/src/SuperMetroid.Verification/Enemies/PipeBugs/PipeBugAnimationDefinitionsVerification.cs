using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>Checks normal and strong Brinstar pipe-bug animation selectors in production AI.</summary>
internal static class PipeBugAnimationDefinitionsVerification
{
    /// <summary>Compares eight native list pointers and verifies selection resets both instruction timers.</summary>
    internal static void VerifyPipeBugAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        const int normalTable = 0xb3882b;
        const int strongTable = 0xb38833;

        foreach (bool strong in new[] { false, true })
        {
            var initializedEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot initializedSlot = initializedEnemies.SlotTable.Slots[0];
            initializedSlot.Bind(strong
                ? EnemyDefinitionId.Zebbo
                : EnemyDefinitionId.Zeb, initializedSlot.Definition);
            initializedSlot.SetSpawnParameter1(EnemyDefinitionId.Zeb, strong ? (ushort)1 : (ushort)0);
            initializedSlot.Position = initializedSlot.Position.WithWhole(new(0x0100, 0x0080));
            PipeBugsBehavior.InitializeBrinstarPipeBug(initializedSlot);
            AssertEqual(new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugPrograms.BrinstarProgram(
                    strong,
                    PipeBugAnimationSelector.RisingFacingLeft)), AnimationPlace.StartOfAnimation),
                (initializedEnemies.PipeBugStates[0] ?? throw new InvalidOperationException("Verification requires a Pipe Bug state.")).Playback,
                $"production {(strong ? "strong" : "normal")} Pipe Bug initialization");

            for (int index = 0; index < 4; index++)
            {
                PipeBugAnimationSelector selector = PipeBugsNativeWords.BrinstarAnimations.Decode(index, "Pipe Bug animation index");
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(((strong ? strongTable : normalTable) + index * 2));
                AssertEqual(expected,
                    NativeEnemyAnimationOracle.EntryOf(PipeBugNativeListAddresses.Entries, PipeBugPrograms.BrinstarProgram(strong, selector)),
                    $"{(strong ? "strong" : "normal")} Pipe Bug {selector}");

                var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
                {
                    FrameHold = WrappingCountdown.Start(0x2222),
                    Timer = WrappingCountdown.Start(0x3333),
                };
                slot.SetSpawnParameter1(EnemyDefinitionId.Zeb, strong ? 1 : 0);
                var state = new PipeBugEnemyState(slot)
                {
                    RequestedAnimation = selector,
                    InstalledAnimation = selector == PipeBugAnimationSelector.RisingFacingLeft
                        ? PipeBugAnimationSelector.ShootingFacingLeft
                        : PipeBugAnimationSelector.RisingFacingLeft,
                };
                PipeBugsBehavior.SelectBrinstarPipeBugAnimation(slot, state);
                AssertEqual(selector, state.InstalledAnimation,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug selector {selector}");
                AssertEqual(new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugPrograms.BrinstarProgram(strong, selector)), AnimationPlace.StartOfAnimation), state.Playback,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug animation {selector}");
                AssertEqual(1, slot.FrameHold.RawValue,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug timer {selector}");
                AssertEqual(0, slot.Timer.RawValue,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug loop {selector}");
            }
        }

        AssertEqual(false, PipeBugsNativeWords.BrinstarAnimations.TryDecode(4, out _), "Pipe Bug animation index past table");
        // The requests main AI makes keep the cartridge's bit arithmetic on its animation index: the emergence sets the
        // facing bit from Samus's side, and the switch to horizontal flight sets the shooting bit.
        AssertEqual(PipeBugsNativeWords.BrinstarAnimations.Decode(2, "Pipe Bug animation index"),
            PipeBugPrograms.Rising(HorizontalDirection.Right), "Pipe Bug rising facing right");
        AssertEqual(PipeBugsNativeWords.BrinstarAnimations.Decode(0, "Pipe Bug animation index"),
            PipeBugPrograms.Rising(HorizontalDirection.Left), "Pipe Bug rising facing left");
        for (int index = 0; index < 4; index++)
        {
            PipeBugAnimationSelector selector = PipeBugsNativeWords.BrinstarAnimations.Decode(index, "Pipe Bug animation index");
            AssertEqual(PipeBugsNativeWords.BrinstarAnimations.Decode(index | 1, "Pipe Bug animation index"),
                PipeBugPrograms.ShootingWithSameFacing(selector), $"Pipe Bug {selector} shooting");
        }
        Console.WriteLine(
            "Pipe Bug animation definitions: eight native selectors, all eight production handoffs and the named animation requests pass.");
    }
}
