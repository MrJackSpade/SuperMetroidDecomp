using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Waver;

/// <summary>Checks compiled Waver animation selectors in both handoff and initialization paths.</summary>
internal static class WaverAnimationDefinitionsVerification
{
    /// <summary>Compares four native pointers and confirms timer reset without rereading their table.</summary>
    internal static void VerifyWaverAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            WaverSegment selector = WaverNativeWords.Animations.Decode(index, "Waver animation index");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa386db + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(WaverNativeListAddresses.Entries, selector),
                $"Waver animation selector {selector}");
        }

        for (int index = 0; index < 4; index++)
        {
            WaverSegment selector = WaverNativeWords.Animations.Decode(index, "Waver animation index");
            var place = new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry).WithAnimationClock(hold: 0x2222);
            var state = new WaverEnemyState
            {
                InstalledAnimation = selector == WaverSegment.SteadyFacingLeft
                    ? WaverSegment.SteadyFacingRight
                    : WaverSegment.SteadyFacingLeft,
                RequestedAnimation = selector,
            };
            WaverBehavior.SetWaverAnimation(place, state);
            AssertEqual(selector, state.InstalledAnimation,
                $"production Waver current selector {selector}");
            AssertEqual(new AnimationPlayback<WaverSegment>(WaverAnimations.StartingWith(selector), AnimationPlace.StartOfAnimation),
                state.Playback,
                $"production Waver animation {selector}");
            AssertEqual(1, place.AnimationClock.Hold.RawValue,
                $"production Waver instruction timer {selector}");
        }

        for (ushort parameter = 0; parameter < 2; parameter++)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Waver, place.Definition);
            place.SetSpawnParameter1(EnemyDefinitionId.Waver, parameter);
            WaverBehavior.InitializeWaver(place);
            WaverSegment expected = WaverNativeWords.Animations.Decode(parameter, "Waver spawn facing");
            WaverEnemyState waver = enemies.WaverStates[0] ?? throw new InvalidOperationException("Verification requires enemies.WaverStates[0].");
            AssertEqual(expected, waver.RequestedAnimation,
                $"production Waver initial requested selector {parameter}");
            AssertEqual(expected, waver.InstalledAnimation,
                $"production Waver initial current selector {parameter}");
            AssertEqual(new AnimationPlayback<WaverSegment>(WaverAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation),
                waver.Playback,
                $"production Waver initial animation {parameter}");
        }

        AssertEqual(false, WaverNativeWords.Animations.TryDecode(4, out _), "Waver animation index past table");
        VerifyWaverAnimationRequests();
        Console.WriteLine(
            "Waver animation definitions: four native selectors, all four real handoffs, and both production initial facing paths and the named animation requests pass.");
    }

    /// <summary>
    /// The requests main AI makes keep the cartridge's bit arithmetic on its animation index: a wall toggles the facing
    /// bit and clears the spin bit, a spin sets the spin bit, and a finished spin clears it.
    /// </summary>
    private static void VerifyWaverAnimationRequests()
    {
        for (int index = 0; index < 4; index++)
        {
            WaverSegment animation = WaverNativeWords.Animations.Decode(index, "Waver animation index");
            AssertEqual(WaverNativeWords.Animations.Decode((index ^ 1) & 1, "Waver animation index"),
                WaverAnimationChoices.TurnedAround(animation), $"Waver {animation} turned around by a wall");
            AssertEqual(WaverNativeWords.Animations.Decode(index | 2, "Waver animation index"),
                WaverAnimationChoices.Spinning(animation), $"Waver {animation} spinning");
            AssertEqual(WaverNativeWords.Animations.Decode(index & 1, "Waver animation index"),
                WaverAnimationChoices.SteadyWithSameFacing(animation), $"Waver {animation} after a spin");
        }
    }
}
