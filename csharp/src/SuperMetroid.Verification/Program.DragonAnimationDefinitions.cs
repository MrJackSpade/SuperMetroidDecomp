using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Represents dragon animation definitions verification.</summary>
internal static class DragonAnimationDefinitionsVerification
{
    /// <summary>Verifies dragon animation definitions.</summary>
    internal static void VerifyDragonAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            var selector = (DragonAnimationSelector)index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2e5ef + index * 2)),
                DragonAnimationDefinitions.InstructionList(selector),
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
            () => DragonAnimationDefinitions.InstructionList(
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

    /// <summary>Verifies all dragon animation installs.</summary>
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

    /// <summary>Verifies live dragon animation handoffs.</summary>
    private static void VerifyLiveDragonAnimationHandoffs(
        SuperMetroidAddressSpace rom,
        bool facingLeft)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewDragonAnimationReadGuard(rom));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot wing = enemies.SlotTable.Slots[1];
        body.EnemyDefinitionPointer = EnemyDefinitionId.Dragon;
        wing.EnemyDefinitionPointer = EnemyDefinitionId.Dragon;
        body.Parameter1 = 0;
        wing.Parameter1 = 1;
        wing.Position = wing.Position.WithWholeX(0x0100);
        body.Position = body.Position.WithWholeX(0x0100);
        wing.Position = wing.Position.WithWholeY(0x0100);
        body.Position = body.Position.WithWholeY(0x0100);

        enemies.DragonBehavior.InitializeDragon(body);
        enemies.DragonBehavior.InitializeDragon(wing);
        DragonEnemyState bodyState = enemies.DragonStates[0]!;
        DragonEnemyState wingState = enemies.DragonStates[1]!;
        var samus = new SamusState
        {
            Position = new(new(unchecked((ushort)(body.Position.X.Whole + (facingLeft ? -0x10 : 0x10))), 0), new(0, 0)),
        };

        enemies.DragonBehavior.RunDragonWaitToRise(body, bodyState, samus);
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
            wingState,
            wings,
            facingLeft ? "live left-facing Dragon wing" : "live right-facing Dragon wing",
            expectedInstructionTimer: facingLeft ? (ushort)0 : (ushort)1);

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

    /// <summary>New dragon animation slot.</summary>
    private static RoomEnemySlot NewDragonAnimationSlot(int index) => new(index)
    {
        CurrentInstruction = 0x7777,
        InstructionTimer = 0x2222,
        Timer = 0x3333,
    };

    /// <summary>New dragon animation state.</summary>
    private static DragonEnemyState NewDragonAnimationState(RoomEnemySlot slot) =>
        new(slot, new ushort[RoomEnemySystem.MaximumEnemyCount],
            new ushort[RoomEnemySystem.MaximumEnemyCount],
            new ushort[RoomEnemySystem.MaximumEnemyCount]);

    /// <summary>Asserts dragon animation handoff.</summary>
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
        AssertEqual(DragonAnimationDefinitions.InstructionList(expected),
            slot.CurrentInstruction,
            $"{context} instruction");
        AssertEqual(expectedInstructionTimer, slot.InstructionTimer,
            $"{context} instruction timer");
        AssertEqual(0, slot.Timer, $"{context} loop timer");
    }

    /// <summary>New dragon animation read guard.</summary>
    private static CartridgeReadGuard NewDragonAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa2e5ef and < 0xa2e5fb,
                address => $"Dragon attempted migrated animation-selector read ${address:X6}.");
}
