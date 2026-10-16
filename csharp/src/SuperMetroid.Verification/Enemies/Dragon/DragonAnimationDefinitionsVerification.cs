using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>Checks Dragon's six facing and phase selectors against retail pointers and live handoffs.</summary>
internal static class DragonAnimationDefinitionsVerification
{
    /// <summary>Compares the selector table with the cartridge, then exercises installs and both facings.</summary>
    internal static void VerifyDragonAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            var selector = (DragonAnimationSelector)index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2e5ef + index * 2)),
                NativeEnemyProgramOracle.EntryOf(DragonNativeListAddresses.Entries, DragonAnimationDefinitions.Program(selector)),
                $"Dragon animation selector {selector}");
            AssertEqual((DragonAnimationSelector)(index & ~1),
                DragonAnimationDefinitions.WithFacing(selector, facingLeft: true),
                $"Dragon left-facing mapping {selector}");
            AssertEqual((DragonAnimationSelector)(index | 1),
                DragonAnimationDefinitions.WithFacing(selector, facingLeft: false),
                $"Dragon right-facing mapping {selector}");
            AssertEqual((DragonAnimationSelector)(4 | (index & 1)),
                DragonAnimationDefinitions.AttackingWithSameFacing(selector),
                $"Dragon attack mapping {selector}");
            AssertEqual((DragonAnimationSelector)(index & 1),
                DragonAnimationDefinitions.IdleWithSameFacing(selector),
                $"Dragon idle mapping {selector}");
        }

        Suite(nameof(VerifyAllDragonAnimationInstalls), () => VerifyAllDragonAnimationInstalls());
        Suite(nameof(VerifyLiveDragonAnimationHandoffs), () => VerifyLiveDragonAnimationHandoffs(rom, facingLeft: false));
        Suite(nameof(VerifyLiveDragonAnimationHandoffs), () => VerifyLiveDragonAnimationHandoffs(rom, facingLeft: true));

        AssertThrows<InvalidDataException>(
            () => DragonAnimationDefinitions.Program(
                DragonAnimationSelector.ForceReinstall),
            "Dragon reinstall sentinel cannot select a list");
        AssertThrows<InvalidDataException>(
            () => DragonAnimationDefinitions.WithFacing(
                DragonAnimationSelector.ForceReinstall,
                facingLeft: true),
            "Dragon reinstall sentinel has no facing mapping");
        Console.WriteLine(
            "Dragon animation definitions: six native selectors, phase/facing mappings, all six installs, and both live body/wing facing and attack handoffs pass with the pointer table forbidden.");
    }

    /// <summary>Forces each selector into a fresh slot to confirm pointer and timer installation.</summary>
    private static void VerifyAllDragonAnimationInstalls()
    {
        for (int index = 0; index < 6; index++)
        {
            var requested = (DragonAnimationSelector)index;
            var slot = NewDragonAnimationSlot(0);
            DragonEnemyState state = NewDragonAnimationState(slot);
            state.RequestedInstructionListIndex = requested;
            state.InstalledInstructionListIndex = DragonAnimationSelector.ForceReinstall;
            DragonBehavior.InstallDragonInstructionList(slot, state);
            AssertDragonAnimationHandoff(slot, state, requested, $"Dragon {requested}");
        }
    }

    /// <summary>Runs body and wing through idle, rise, and attack transitions for one Samus-facing direction.</summary>
    private static void VerifyLiveDragonAnimationHandoffs(
        SuperMetroidAddressSpace rom,
        bool facingLeft)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewDragonAnimationReadGuard(rom));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot wing = enemies.SlotTable.Slots[1];
        body.Bind(EnemyDefinitionId.Dragon, body.Definition);
        wing.Bind(EnemyDefinitionId.Dragon, wing.Definition);
        body.Parameter1 = 0;
        wing.Parameter1 = 1;
        wing.Position = wing.Position.WithWholeX(0x0100);
        body.Position = body.Position.WithWholeX(0x0100);
        wing.Position = wing.Position.WithWholeY(0x0100);
        body.Position = body.Position.WithWholeY(0x0100);

        enemies.DragonBehavior.InitializeDragon(body);
        enemies.DragonBehavior.InitializeDragon(wing);
        DragonEnemyState bodyState = (enemies.DragonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.DragonStates[0]."));
        DragonEnemyState wingState = (enemies.DragonStates[1] ?? throw new InvalidOperationException("Verification requires enemies.DragonStates[1]."));
        var samus = new SamusState
        {
            Position = new(new(unchecked((ushort)(body.Position.X.Whole + (facingLeft ? -0x10 : 0x10))), 0), new(0, 0)),
        };

        enemies.DragonBehavior.RunDragonWaitToRise(body, (bodyState ?? throw new InvalidOperationException("Verification requires bodyState.")), samus);
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
            expectedInstructionTimer: facingLeft ? (ushort)0 : (ushort)1);
        AssertDragonAnimationHandoff(
            wing,
            (wingState ?? throw new InvalidOperationException("Verification requires wingState.")),
            wings,
            facingLeft ? "live left-facing Dragon wing" : "live right-facing Dragon wing",
            expectedInstructionTimer: facingLeft ? (ushort)0 : (ushort)1);

        bodyState.FunctionTimer = NativeCountdown.Start(0);
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
    private static RoomEnemySlot NewDragonAnimationSlot(int index) => new(index, EnemySlotFixtures.Registry)
    {
        InstructionTimer = NativeCountdown.Start(0x2222),
        Timer = NativeCountdown.Start(0x3333),
    };

    /// <summary>Provides Dragon's per-enemy work arrays for a standalone animation slot.</summary>
    private static DragonEnemyState NewDragonAnimationState(RoomEnemySlot slot) =>
        new(slot, new ushort[RoomEnemySystem.MaximumEnemyCount],
            new ushort[RoomEnemySystem.MaximumEnemyCount],
            new ushort[RoomEnemySystem.MaximumEnemyCount]);

    /// <summary>Confirms the requested selector reached state and slot, clearing the previous loop timer.</summary>
    private static void AssertDragonAnimationHandoff(
        RoomEnemySlot slot,
        DragonEnemyState state,
        DragonAnimationSelector expected,
        string context,
        ushort expectedInstructionTimer = 1)
    {
        AssertEqual(expected, state.RequestedInstructionListIndex,
            $"{context} requested selector");
        AssertEqual(expected, state.InstalledInstructionListIndex,
            $"{context} installed selector");
        AssertEqual(AnimationProgramCursor<DragonProgram>.Start(DragonAnimationDefinitions.Program(expected)),
            state.Program,
            $"{context} animation");
        AssertEqual(expectedInstructionTimer, slot.InstructionTimer.RawValue,
            $"{context} instruction timer");
        AssertEqual(0, slot.Timer.RawValue, $"{context} loop timer");
    }

    /// <summary>Rejects live access to the migrated Dragon selector pointer table.</summary>
    private static CartridgeReadGuard NewDragonAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa2e5ef and < 0xa2e5fb,
                address => $"Dragon attempted migrated animation-selector read ${address:X6}.");
}
