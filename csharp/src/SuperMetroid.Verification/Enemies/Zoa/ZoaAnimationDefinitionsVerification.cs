using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Zoa;

/// <summary>Checks Zoa animation selectors and live rise-to-shoot list handoffs.</summary>
internal static class ZoaAnimationDefinitionsVerification
{
    /// <summary>A selector holding every value no animation flag names, which no animation table entry selects.</summary>
    private const ZoaAnimationSelector UnnamedSelector = ~(ZoaAnimationSelector.Rising | ZoaAnimationSelector.FacingRight);

    /// <summary>Compares four native animation pointers and optionally exercises live state transitions.</summary>
    internal static void VerifyZoaAnimationDefinitions(CartridgeImportAddressSpace rom, bool definitionsOnly = false)
    {
        for (int index = 0; index < 4; index++)
        {
            ZoaAnimationSelector selector = ZoaNativeWords.AnimationSelectorOf(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((ZoaAnimationDefinitions.ReferenceAddress + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(ZoaNativeListAddresses.Entries, ZoaPrograms.Program(selector)),
                $"Zoa animation selector {selector}");
        }

        foreach (ushort invalid in new ushort[] { 4, 5, 0x8000, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() =>
                ZoaNativeWords.AnimationSelectorOf(invalid),
                "Zoa animation selector word rejects unsupported flag combinations");
        AssertThrows<InvalidDataException>(() =>
            ZoaPrograms.Program(UnnamedSelector),
            "Zoa animation rejects unsupported flag combinations");
        if (definitionsOnly)
        {
            Console.WriteLine("Zoa animation cases: four original pointers and unsupported flag bounds pass.");
            return;
        }
        Suite(nameof(VerifyAllZoaAnimationHandoffs), () => VerifyAllZoaAnimationHandoffs());
        Suite(nameof(VerifyLiveZoaAnimationHandoffs), () => VerifyLiveZoaAnimationHandoffs(facingRight: false));
        Suite(nameof(VerifyLiveZoaAnimationHandoffs), () => VerifyLiveZoaAnimationHandoffs(facingRight: true));

        AssertThrows<InvalidDataException>(
            () => ZoaPrograms.Program(UnnamedSelector),
            "Zoa animation selector past table");
        Console.WriteLine(
            "Zoa animation definitions: four native selectors, all four installs, and both complete live facing/rise/shoot handoffs pass with the pointer table forbidden.");
    }

    /// <summary>Checks all four selector installs reset the instruction and loop timers.</summary>
    private static void VerifyAllZoaAnimationHandoffs()
    {
        foreach (ZoaAnimationSelector selector in new[]
            {
                ZoaAnimationSelector.None, ZoaAnimationSelector.Rising, ZoaAnimationSelector.FacingRight,
                ZoaAnimationSelector.FacingRight | ZoaAnimationSelector.Rising,
            })
        {
            var slot = NewZoaAnimationSlot();
            var state = new ZoaEnemyState()
            {
                AnimationScriptTableIndex = selector,
                PreviousAnimationScriptTableIndex = selector == ZoaAnimationSelector.None
                    ? ZoaAnimationSelector.Rising
                    : ZoaAnimationSelector.None,
            };
            ZoaBehavior.SetZoaProgram(slot, state);
            AssertZoaAnimationHandoff(slot, state, selector, $"Zoa {selector}");
        }
    }

    /// <summary>Drives Zoa from waiting through rising to shooting for one facing direction.</summary>
    private static void VerifyLiveZoaAnimationHandoffs(
        bool facingRight)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));

        slot.Bind(EnemyDefinitionId.Zoa, slot.Definition);
        ZoaBehavior.InitializeZoa(slot);
        ZoaEnemyState state = (enemies.ZoaStates[0] ?? throw new InvalidOperationException("Verification requires enemies.ZoaStates[0]."));

        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = new(new(unchecked((ushort)(slot.Position.X.Whole + (facingRight ? 0x10 : -0x10))), 0), new(slot.Position.Y.Whole, 0)),
        };
        ZoaBehavior.RunZoaWait(slot, (state), samus);
        ZoaAnimationSelector direction = facingRight
            ? ZoaAnimationSelector.FacingRight
            : ZoaAnimationSelector.None;
        ZoaAnimationSelector rising = direction | ZoaAnimationSelector.Rising;
        AssertZoaAnimationHandoff(
            slot,
            state,
            rising,
            facingRight ? "live right-facing Zoa rise" : "live left-facing Zoa rise");

        ZoaBehavior.RunZoaRising(slot, state, samus);
        AssertZoaAnimationHandoff(
            slot,
            state,
            direction,
            facingRight ? "live right-facing Zoa shoot" : "live left-facing Zoa shoot");
    }

    /// <summary>Seeds nondefault list and timer values so animation handoff writes are visible.</summary>
    private static RoomEnemySlot NewZoaAnimationSlot() => new(0, EnemySlotFixtures.Registry)
    {
        FrameHold = WrappingCountdown.Start(0x2222),
        Timer = WrappingCountdown.Start(0x3333),
    };

    /// <summary>Checks requested and installed selectors, the started animation, and reset timers.</summary>
    private static void AssertZoaAnimationHandoff(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        ZoaAnimationSelector expected,
        string context)
    {
        AssertEqual(expected, state.AnimationScriptTableIndex,
            $"{context} requested selector");
        AssertEqual(expected, state.PreviousAnimationScriptTableIndex,
            $"{context} installed selector");
        AssertEqual(new AnimationPlayback<ZoaProgram>(ZoaAnimations.StartingWith(ZoaPrograms.Program(expected)), AnimationPlace.StartOfAnimation),
            state.Playback,
            $"{context} animation");
        AssertEqual(1, slot.FrameHold.RawValue, $"{context} instruction timer");
        AssertEqual(0, slot.Timer.RawValue, $"{context} loop timer");
    }

}
