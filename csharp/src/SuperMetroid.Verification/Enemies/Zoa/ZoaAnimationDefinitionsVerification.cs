using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Zoa;

/// <summary>Checks Zoa animation selectors and live rise-to-shoot list handoffs.</summary>
internal static class ZoaAnimationDefinitionsVerification
{
    /// <summary>Compares four native animation pointers and optionally exercises live state transitions.</summary>
    internal static void VerifyZoaAnimationDefinitions(CartridgeImportAddressSpace rom, bool definitionsOnly = false)
    {
        for (int index = 0; index < 4; index++)
        {
            ZoaSegment selector = ZoaNativeWords.AnimationSelectorOf(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((ZoaAnimationDefinitionsConstants.ReferenceAddress + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(ZoaNativeListAddresses.Entries, selector),
                $"Zoa animation selector {selector}");
        }

        foreach (ushort invalid in new ushort[] { 4, 5, 0x8000, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() =>
                ZoaNativeWords.AnimationSelectorOf(invalid),
                "Zoa animation selector word rejects words naming no animation");
        if (definitionsOnly)
        {
            Console.WriteLine("Zoa animation cases: four original pointers and the selector word's bounds pass.");
            return;
        }
        Suite(nameof(VerifyAllZoaAnimationHandoffs), () => VerifyAllZoaAnimationHandoffs());
        Suite(nameof(VerifyLiveZoaAnimationHandoffs), () => VerifyLiveZoaAnimationHandoffs(facingRight: false));
        Suite(nameof(VerifyLiveZoaAnimationHandoffs), () => VerifyLiveZoaAnimationHandoffs(facingRight: true));

        Console.WriteLine(
            "Zoa animation definitions: four native selectors, all four installs, and both complete live facing/rise/shoot handoffs pass with the pointer table forbidden.");
    }

    /// <summary>Checks all four selector installs reset the instruction and loop timers.</summary>
    private static void VerifyAllZoaAnimationHandoffs()
    {
        foreach (ZoaSegment selector in Enum.GetValues<ZoaSegment>())
        {
            var place = NewZoaAnimationPlace();
            var state = new ZoaEnemyState(ZoaAction.WaitForSamus)
            {
                RequestedAnimation = selector,
                InstalledAnimation = selector == ZoaSegment.FacingLeftShooting
                    ? ZoaSegment.FacingLeftRising
                    : ZoaSegment.FacingLeftShooting,
            };
            ZoaBehavior.SetZoaAnimation(place, state);
            AssertZoaAnimationHandoff(place, state, selector, $"Zoa {selector}");
        }
    }

    /// <summary>Drives Zoa from waiting through rising to shooting for one facing direction.</summary>
    private static void VerifyLiveZoaAnimationHandoffs(
        bool facingRight)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Position = place.Position.WithWhole(new(0x0100, 0x0080));

        place.Bind(EnemyDefinitionId.Zoa, place.Definition);
        ZoaBehavior.InitializeZoa(place);
        ZoaEnemyState state = (enemies.ZoaStates[0] ?? throw new InvalidOperationException("Verification requires enemies.ZoaStates[0]."));

        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = new(new(unchecked((ushort)(place.Position.X.Whole + (facingRight ? 0x10 : -0x10))), 0), new(place.Position.Y.Whole, 0)),
        };
        ZoaBehavior.RunZoaWait(place, (state), samus);
        ZoaSegment direction = facingRight
            ? ZoaSegment.FacingRightShooting
            : ZoaSegment.FacingLeftShooting;
        ZoaSegment rising = facingRight
            ? ZoaSegment.FacingRightRising
            : ZoaSegment.FacingLeftRising;
        AssertZoaAnimationHandoff(
            place,
            state,
            rising,
            facingRight ? "live right-facing Zoa rise" : "live left-facing Zoa rise");

        ZoaBehavior.RunZoaRising(place, state, samus);
        AssertZoaAnimationHandoff(
            place,
            state,
            direction,
            facingRight ? "live right-facing Zoa shoot" : "live left-facing Zoa shoot");
    }

    /// <summary>Seeds nondefault list and timer values so animation handoff writes are visible.</summary>
    private static RoomEnemyPlace NewZoaAnimationPlace() =>
        new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry).WithAnimationClock(hold: 0x2222);

    /// <summary>Checks requested and installed selectors, the started animation, and reset timers.</summary>
    private static void AssertZoaAnimationHandoff(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        ZoaSegment expected,
        string context)
    {
        AssertEqual(expected, state.RequestedAnimation,
            $"{context} requested selector");
        AssertEqual(expected, state.InstalledAnimation,
            $"{context} installed selector");
        AssertEqual(new AnimationPlayback<ZoaSegment>(ZoaAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation),
            state.Playback,
            $"{context} animation");
        AssertEqual(1, place.AnimationClock.Hold.RawValue, $"{context} instruction timer");
    }

}
