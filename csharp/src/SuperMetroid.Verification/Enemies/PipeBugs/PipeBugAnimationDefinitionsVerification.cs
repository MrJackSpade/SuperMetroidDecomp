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

        foreach (BrinstarPipeBugKind kind in Enum.GetValues<BrinstarPipeBugKind>())
        {
            bool strong = kind == BrinstarPipeBugKind.Strong;
            var initializedEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            // The pipe bug of its kind, set up as its room's population sets it up (#2554).
            _ = initializedEnemies.LoadPopulation(PopulationPlacementAccess.Entry(strong ? EnemyDefinitionId.Zebbo : EnemyDefinitionId.Zeb,
                new BrinstarPipeBugSpawnSettings(kind), new(0x0100, 0x0080)));
            AssertEqual(new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(
                    PipeBugAnimationChoices.Rising(kind, HorizontalDirection.Left)), AnimationPlace.StartOfAnimation),
                (initializedEnemies.BrinstarPipeBugStates[0] ?? throw new InvalidOperationException("Verification requires a Pipe Bug state.")).Playback,
                $"production {(strong ? "strong" : "normal")} Pipe Bug initialization");

            for (int index = 0; index < 4; index++)
            {
                PipeBugSegment selector = PipeBugsNativeWords.BrinstarAnimations(kind).Decode(index, "Pipe Bug animation index");
                ushort expected = rom.ReadCartridgeWord(((strong ? strongTable : normalTable) + index * 2));
                AssertEqual(expected,
                    NativeEnemyAnimationOracle.EntryOf(PipeBugNativeListAddresses.Entries, selector),
                    $"{(strong ? "strong" : "normal")} Pipe Bug {selector}");

                var place = new RoomEnemyPlace(0, new RemovedEnemyLeftovers<IEnemyEntity>()).WithAnimationClock(hold: 0x2222);
                PipeBugSegment playing = selector == PipeBugAnimationChoices.Rising(kind, HorizontalDirection.Left)
                    ? PipeBugAnimationChoices.Shooting(kind, HorizontalDirection.Left)
                    : PipeBugAnimationChoices.Rising(kind, HorizontalDirection.Left);
                var state = new BrinstarPipeBugState(BrinstarPipeBugAction.WaitUntilOnScreen)
                {
                    Playback = new(PipeBugAnimations.StartingWith(playing), AnimationPlace.StartOfAnimation),
                };
                PipeBugsBehavior.SelectBrinstarPipeBugAnimation(place, state, selector);
                AssertEqual(new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(selector), AnimationPlace.StartOfAnimation), state.Playback,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug animation {selector}");
                AssertEqual(1, place.AnimationClock.Hold,
                    $"production {(strong ? "strong" : "normal")} Pipe Bug timer {selector}");
            }
        }

        // The requests main AI makes keep the cartridge's bit arithmetic on its animation index: the emergence sets the
        // facing bit from Samus's side, and the switch to horizontal flight sets the shooting bit.
        foreach (BrinstarPipeBugKind kind in Enum.GetValues<BrinstarPipeBugKind>())
        {
            NativeWordIdentities<PipeBugSegment> animations = PipeBugsNativeWords.BrinstarAnimations(kind);
            AssertEqual(false, animations.TryDecode(4, out _), "Pipe Bug animation index past table");
            AssertEqual(animations.Decode(2, "Pipe Bug animation index"),
                PipeBugAnimationChoices.Rising(kind, HorizontalDirection.Right), "Pipe Bug rising facing right");
            AssertEqual(animations.Decode(0, "Pipe Bug animation index"),
                PipeBugAnimationChoices.Rising(kind, HorizontalDirection.Left), "Pipe Bug rising facing left");
            AssertEqual(animations.Decode(2 | 1, "Pipe Bug animation index"),
                PipeBugAnimationChoices.Shooting(kind, HorizontalDirection.Right), "Pipe Bug shooting facing right");
            AssertEqual(animations.Decode(0 | 1, "Pipe Bug animation index"),
                PipeBugAnimationChoices.Shooting(kind, HorizontalDirection.Left), "Pipe Bug shooting facing left");
        }
        Console.WriteLine(
            "Pipe Bug animation definitions: eight native selectors, all eight production handoffs and the named animation requests pass.");
    }
}
