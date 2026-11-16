using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

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
            var initializedEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace initializedPlace = initializedEnemies.Places.InUpdateOrder[0];
            initializedPlace.Bind(strong
                ? EnemyDefinitionId.Zebbo
                : EnemyDefinitionId.Zeb, initializedPlace.Definition);
            initializedPlace.SetSpawnParameter1(EnemyDefinitionId.Zeb, strong ? (ushort)1 : (ushort)0);
            initializedPlace.Position = initializedPlace.Position.WithWhole(new(0x0100, 0x0080));
            PipeBugsBehavior.InitializeBrinstarPipeBug(initializedPlace);
            AssertEqual(new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(
                    PipeBugAnimationChoices.Rising(strong, HorizontalDirection.Left)), AnimationPlace.StartOfAnimation),
                (initializedEnemies.BrinstarPipeBugStates[0] ?? throw new InvalidOperationException("Verification requires a Pipe Bug state.")).Playback,
                $"production {(strong ? "strong" : "normal")} Pipe Bug initialization");

            for (int index = 0; index < 4; index++)
            {
                PipeBugSegment selector = PipeBugsNativeWords.BrinstarAnimations(strong).Decode(index, "Pipe Bug animation index");
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(((strong ? strongTable : normalTable) + index * 2));
                AssertEqual(expected,
                    NativeEnemyAnimationOracle.EntryOf(PipeBugNativeListAddresses.Entries, selector),
                    $"{(strong ? "strong" : "normal")} Pipe Bug {selector}");

                var place = new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>()).WithAnimationClock(hold: 0x2222);
                place.SetSpawnParameter1(EnemyDefinitionId.Zeb, strong ? 1 : 0);
                var state = new BrinstarPipeBugState(BrinstarPipeBugAction.WaitUntilOnScreen)
                {
                    InstalledAnimation = selector == PipeBugAnimationChoices.Rising(strong, HorizontalDirection.Left)
                        ? PipeBugAnimationChoices.ShootingWithSameFacing(selector)
                        : PipeBugAnimationChoices.Rising(strong, HorizontalDirection.Left),
                };
                PipeBugsBehavior.SelectBrinstarPipeBugAnimation(place, state, selector);
                AssertEqual(selector, state.InstalledAnimation,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug selector {selector}");
                AssertEqual(new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(selector), AnimationPlace.StartOfAnimation), state.Playback,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug animation {selector}");
                AssertEqual(1, place.AnimationClock.Hold.RawValue,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug timer {selector}");
            }
        }

        // The requests main AI makes keep the cartridge's bit arithmetic on its animation index: the emergence sets the
        // facing bit from Samus's side, and the switch to horizontal flight sets the shooting bit.
        foreach (bool strong in new[] { false, true })
        {
            NativeWordIdentities<PipeBugSegment> animations = PipeBugsNativeWords.BrinstarAnimations(strong);
            AssertEqual(false, animations.TryDecode(4, out _), "Pipe Bug animation index past table");
            AssertEqual(animations.Decode(2, "Pipe Bug animation index"),
                PipeBugAnimationChoices.Rising(strong, HorizontalDirection.Right), "Pipe Bug rising facing right");
            AssertEqual(animations.Decode(0, "Pipe Bug animation index"),
                PipeBugAnimationChoices.Rising(strong, HorizontalDirection.Left), "Pipe Bug rising facing left");
            for (int index = 0; index < 4; index++)
            {
                PipeBugSegment selector = animations.Decode(index, "Pipe Bug animation index");
                AssertEqual(animations.Decode(index | 1, "Pipe Bug animation index"),
                    PipeBugAnimationChoices.ShootingWithSameFacing(selector), $"Pipe Bug {selector} shooting");
            }
        }
        Console.WriteLine(
            "Pipe Bug animation definitions: eight native selectors, all eight production handoffs and the named animation requests pass.");
    }
}
