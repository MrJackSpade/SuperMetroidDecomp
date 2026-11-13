using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>Checks Dragon's six facing and phase selectors against retail pointers and live handoffs.</summary>
internal static class DragonAnimationDefinitionsVerification
{
    /// <summary>Compares the selector table with the cartridge, then exercises installs and both facings.</summary>
    internal static void VerifyDragonAnimationDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            // The cartridge pairs its six ordinals as facing left (even) and right (odd) within each phase.
            DragonAnimationSelector selector = Selector(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2e5ef + index * 2)),
                NativeEnemyAnimationOracle.EntryOf(DragonNativeListAddresses.Entries, DragonPhases.Program(selector)),
                $"Dragon animation selector {selector}");
            AssertEqual(Selector(index & ~1),
                DragonPhases.WithFacing(selector, facingLeft: true),
                $"Dragon left-facing mapping {selector}");
            AssertEqual(Selector(index | 1),
                DragonPhases.WithFacing(selector, facingLeft: false),
                $"Dragon right-facing mapping {selector}");
            AssertEqual(Selector(4 | (index & 1)),
                DragonPhases.AttackingWithSameFacing(selector),
                $"Dragon attack mapping {selector}");
            AssertEqual(Selector(index & 1),
                DragonPhases.IdleWithSameFacing(selector),
                $"Dragon idle mapping {selector}");
        }

        Suite(nameof(VerifyAllDragonAnimationInstalls), () => VerifyAllDragonAnimationInstalls());
        Suite(nameof(VerifyLiveDragonAnimationHandoffs), () => VerifyLiveDragonAnimationHandoffs(facingLeft: false));
        Suite(nameof(VerifyLiveDragonAnimationHandoffs), () => VerifyLiveDragonAnimationHandoffs(facingLeft: true));

        Suite(nameof(VerifyDragonReinstallRequest), () => VerifyDragonReinstallRequest());
        Console.WriteLine(
            "Dragon animation definitions: six native selectors, phase/facing mappings, all six installs, the reinstall request, and both live body/wing facing and attack handoffs pass with the pointer table forbidden.");
    }

    /// <summary>The animation the cartridge's table ordinal <paramref name="index"/> names.</summary>
    private static DragonAnimationSelector Selector(int index) =>
        DragonNativeWords.AnimationSelectors.Decode(index, "Dragon animation ordinal");

    /// <summary>
    /// A request for the installed animation installs nothing, unless a reinstall was requested: then the animation
    /// restarts and the request is spent.
    /// </summary>
    private static void VerifyDragonReinstallRequest()
    {
        RoomEnemySlot slot = NewDragonAnimationSlot(0);
        DragonEnemyState state = NewDragonAnimationState();
        state.RequestedAnimation = DragonAnimationSelector.AttackingFacingLeft;
        state.InstalledAnimation = DragonAnimationSelector.AttackingFacingLeft;
        DragonBehavior.InstallDragonAnimationScript(slot, state);
        AssertEqual(0x2222, slot.AnimationClock.Hold.RawValue, "Dragon installs nothing for the installed animation");

        state.ReinstallRequested = true;
        DragonBehavior.InstallDragonAnimationScript(slot, state);
        AssertDragonAnimationHandoff(slot, state, DragonAnimationSelector.AttackingFacingLeft, "Dragon reinstall request");
        AssertEqual(false, state.ReinstallRequested, "Dragon reinstall request is spent by the install");
    }

    /// <summary>Forces each selector into a fresh slot to confirm pointer and timer installation.</summary>
    private static void VerifyAllDragonAnimationInstalls()
    {
        for (int index = 0; index < 6; index++)
        {
            DragonAnimationSelector requested = Selector(index);
            var slot = NewDragonAnimationSlot(0);
            DragonEnemyState state = NewDragonAnimationState();
            state.RequestedAnimation = requested;
            state.ReinstallRequested = true;
            DragonBehavior.InstallDragonAnimationScript(slot, state);
            AssertDragonAnimationHandoff(slot, state, requested, $"Dragon {requested}");
        }
    }

    /// <summary>Runs body and wing through idle, rise, and attack transitions for one Samus-facing direction.</summary>
    private static void VerifyLiveDragonAnimationHandoffs(
        bool facingLeft)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot wing = enemies.SlotTable.Slots[1];
        body.Bind(EnemyDefinitionId.Dragon, body.Definition);
        wing.Bind(EnemyDefinitionId.Dragon, wing.Definition);
        body.SetSpawnParameter1(EnemyDefinitionId.Dragon, 0);
        wing.SetSpawnParameter1(EnemyDefinitionId.Dragon, 1);
        wing.Position = wing.Position.WithWholeX(0x0100);
        body.Position = body.Position.WithWholeX(0x0100);
        wing.Position = wing.Position.WithWholeY(0x0100);
        body.Position = body.Position.WithWholeY(0x0100);

        enemies.DragonBehavior.InitializeDragon(body);
        enemies.DragonBehavior.InitializeDragon(wing);
        DragonEnemyState bodyState = (enemies.DragonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.DragonStates[0]."));
        DragonEnemyState wingState = (enemies.DragonStates[1] ?? throw new InvalidOperationException("Verification requires enemies.DragonStates[1]."));
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = new(new(unchecked((ushort)(body.Position.X.Whole + (facingLeft ? -0x10 : 0x10))), 0), new(0, 0)),
        };

        enemies.DragonBehavior.RunDragonWaitToRise(body, (bodyState), samus);
        DragonAnimationSelector idle = facingLeft
            ? DragonAnimationSelector.IdleFacingLeft
            : DragonAnimationSelector.IdleFacingRight;
        DragonAnimationSelector wings = facingLeft
            ? DragonAnimationSelector.WingsFacingLeft
            : DragonAnimationSelector.WingsFacingRight;
        AssertDragonAnimationHandoff(
            body,
            bodyState,
            idle,
            facingLeft ? "live left-facing Dragon body" : "live right-facing Dragon body",
            expectedFrameHold: facingLeft ? (ushort)0 : (ushort)1);
        AssertDragonAnimationHandoff(
            wing,
            (wingState),
            wings,
            facingLeft ? "live left-facing Dragon wing" : "live right-facing Dragon wing",
            expectedFrameHold: facingLeft ? (ushort)0 : (ushort)1);

        bodyState.FunctionTimer = 0;
        enemies.DragonBehavior.RunDragonRising(body, bodyState);
        enemies.DragonBehavior.RunDragonAttacking(body, bodyState);
        DragonAnimationSelector attacking = facingLeft
            ? DragonAnimationSelector.AttackingFacingLeft
            : DragonAnimationSelector.AttackingFacingRight;
        AssertDragonAnimationHandoff(
            body,
            bodyState,
            attacking,
            facingLeft ? "live left-facing Dragon attack" : "live right-facing Dragon attack");
    }

    /// <summary>Starts with nondefault instruction values so an omitted install remains observable.</summary>
    private static RoomEnemySlot NewDragonAnimationSlot(int index) =>
        new RoomEnemySlot(index, EnemySlotFixtures.Registry).WithAnimationClock(hold: 0x2222, repeats: 0x3333);

    /// <summary>Provides a fresh Dragon state for a standalone animation slot.</summary>
    private static DragonEnemyState NewDragonAnimationState() => new();

    /// <summary>Confirms the requested selector reached state and slot, clearing the previous loop timer.</summary>
    private static void AssertDragonAnimationHandoff(
        RoomEnemySlot slot,
        DragonEnemyState state,
        DragonAnimationSelector expected,
        string context,
        ushort expectedFrameHold = 1)
    {
        AssertEqual(expected, state.RequestedAnimation,
            $"{context} requested selector");
        AssertEqual(expected, state.InstalledAnimation,
            $"{context} installed selector");
        AssertEqual(new AnimationPlayback<DragonProgram>(DragonAnimations.StartingWith(DragonPhases.Program(expected)), AnimationPlace.StartOfAnimation),
            state.Playback,
            $"{context} animation");
        AssertEqual(expectedFrameHold, slot.AnimationClock.Hold.RawValue,
            $"{context} instruction timer");
        AssertEqual(0, slot.AnimationClock.Repeats.RawValue, $"{context} loop timer");
    }

}
