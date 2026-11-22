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
            AssertEqual(rom.ReadCartridgeWord((0xa386db + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(WaverNativeListAddresses.Entries, selector),
                $"Waver animation selector {selector}");
        }

        for (int index = 0; index < 4; index++)
        {
            WaverSegment selector = WaverNativeWords.Animations.Decode(index, "Waver animation index");
            var place = new RoomEnemyPlace(0, new RemovedEnemyLeftovers<IEnemyEntity>()).WithAnimationClock(hold: 0x2222);
            var state = new WaverEnemyState
            {
                Playback = new(WaverAnimations.StartingWith(selector == WaverSegment.SteadyFacingLeft
                    ? WaverSegment.SteadyFacingRight
                    : WaverSegment.SteadyFacingLeft), AnimationPlace.StartOfAnimation),
            };
            WaverBehavior.SetWaverAnimation(place, state, selector);
            AssertEqual(new AnimationPlayback<WaverSegment>(WaverAnimations.StartingWith(selector), AnimationPlace.StartOfAnimation),
                state.Playback,
                $"production Waver animation {selector}");
            AssertEqual(1, place.AnimationClock.Hold,
                $"production Waver instruction timer {selector}");
        }

        for (ushort parameter = 0; parameter < 2; parameter++)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Waver, place.KeptDefinition);
            place.SetSpawnParameter1(EnemyDefinitionId.Waver, parameter);
            WaverEnemyState waver = WaverBehavior.InitializeWaver(place.HeldOccupant().SpawnSettings<WaverSpawnSettings>(), place);
            WaverSegment expected = WaverNativeWords.Animations.Decode(parameter, "Waver spawn facing");
            AssertEqual(WaverAnimationChoicesConstants.Facing(expected), waver.Facing,
                $"production Waver initial facing {parameter}");
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
    /// The requests main AI makes keep the cartridge's bit arithmetic on its animation index, whose low bit is the
    /// facing: a wall toggles the facing and clears the spin bit, a spin sets the spin bit, and a finished spin clears it.
    /// </summary>
    private static void VerifyWaverAnimationRequests()
    {
        foreach (HorizontalDirection facing in new[] { HorizontalDirection.Left, HorizontalDirection.Right })
        {
            int index = facing == HorizontalDirection.Right ? 1 : 0;
            HorizontalDirection turned = facing == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right;
            AssertEqual(WaverNativeWords.Animations.Decode(index, "Waver animation index"),
                WaverAnimationChoices.Steady(facing), $"Waver steady facing {facing}, as a finished spin returns to");
            AssertEqual(WaverNativeWords.Animations.Decode(index ^ 1, "Waver animation index"),
                WaverAnimationChoices.Steady(turned), $"Waver facing {facing} turned around by a wall");
            AssertEqual(WaverNativeWords.Animations.Decode(index | 2, "Waver animation index"),
                WaverAnimationChoices.Spinning(facing), $"Waver spinning facing {facing}");
            AssertEqual(facing, WaverAnimationChoicesConstants.Facing(WaverAnimationChoices.Spinning(facing)), $"Waver spin facing {facing} keeps its facing");
        }
    }
}
