using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDragonAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            var selector = (DragonAnimationSelector)index;
            AssertEqual(ReadDragonAnimationWord(rom, 0xa2e5ef + index * 2),
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

        VerifyAllDragonAnimationInstalls();
        VerifyLiveDragonAnimationHandoffs(rom, facingLeft: false);
        VerifyLiveDragonAnimationHandoffs(rom, facingLeft: true);

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

    private static void VerifyAllDragonAnimationInstalls()
    {
        MethodInfo install = typeof(RoomEnemySystem).GetMethod(
            "InstallDragonInstructionList",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        for (int index = 0; index < 6; index++)
        {
            var requested = (DragonAnimationSelector)index;
            var slot = NewDragonAnimationSlot(0);
            DragonEnemyState state = NewDragonAnimationState(slot);
            state.RequestedInstructionListIndex = requested;
            state.InstalledInstructionListIndex = DragonAnimationSelector.ForceReinstall;
            install.Invoke(null, [slot, state]);
            AssertDragonAnimationHandoff(slot, state, requested, $"Dragon {requested}");
        }
    }

    private static void VerifyLiveDragonAnimationHandoffs(
        SuperMetroidAddressSpace rom,
        bool facingLeft)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(enemies, new DragonAnimationReadGuard(rom));
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot wing = enemies.Slots[1];
        body.EnemyDefinitionPointer = RoomEnemySystem.DragonDefinition;
        wing.EnemyDefinitionPointer = RoomEnemySystem.DragonDefinition;
        body.Parameter1 = 0;
        wing.Parameter1 = 1;
        body.XPosition = wing.XPosition = 0x0100;
        body.YPosition = wing.YPosition = 0x0100;

        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeDragon", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo wait = typeof(RoomEnemySystem).GetMethod(
            "RunDragonWaitToRise", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo rise = typeof(RoomEnemySystem).GetMethod(
            "RunDragonRising", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo attack = typeof(RoomEnemySystem).GetMethod(
            "RunDragonAttacking", BindingFlags.Instance | BindingFlags.NonPublic)!;
        initialize.Invoke(enemies, [body]);
        initialize.Invoke(enemies, [wing]);
        DragonEnemyState bodyState = enemies.DragonStates[0]!;
        DragonEnemyState wingState = enemies.DragonStates[1]!;
        var samus = new SamusState
        {
            XPosition = unchecked((ushort)(body.XPosition + (facingLeft ? -0x10 : 0x10))),
        };

        wait.Invoke(enemies, [body, bodyState, samus]);
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
        rise.Invoke(enemies, [body, bodyState]);
        attack.Invoke(enemies, [body, bodyState]);
        DragonAnimationSelector attacking = facingLeft
            ? DragonAnimationSelector.AttackingFacingLeft
            : DragonAnimationSelector.AttackingFacingRight;
        AssertDragonAnimationHandoff(
            body,
            bodyState,
            attacking,
            facingLeft ? "live left-facing Dragon attack" : "live right-facing Dragon attack");
    }

    private static RoomEnemySlot NewDragonAnimationSlot(int index) => new(index)
    {
        CurrentInstruction = 0x7777,
        InstructionTimer = 0x2222,
        Timer = 0x3333,
    };

    private static DragonEnemyState NewDragonAnimationState(RoomEnemySlot slot) =>
        new(slot, new ushort[RoomEnemySystem.MaximumEnemyCount],
            new ushort[RoomEnemySystem.MaximumEnemyCount],
            new ushort[RoomEnemySystem.MaximumEnemyCount]);

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

    private static ushort ReadDragonAnimationWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class DragonAnimationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa2e5ef and < 0xa2e5fb
                ? throw new InvalidOperationException(
                    $"Dragon attempted migrated animation-selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
