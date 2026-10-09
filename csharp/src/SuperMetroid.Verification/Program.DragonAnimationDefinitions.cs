using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks all native Dragon animation selectors and validates their facing and phase mappings.</summary>
    /// <param name="rom">The retail address space containing the six-entry animation pointer table.</param>
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

    /// <summary>Exercises every selector through the production instruction-list installer.</summary>
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

    /// <summary>Checks body and wing idle selections and the body attack transition for one facing direction.</summary>
    /// <param name="rom">The retail address space wrapped to reject reads from the migrated selector table.</param>
    /// <param name="facingLeft">Whether Samus is positioned to make the Dragon face left.</param>
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

    /// <summary>Creates a slot with recognizable instruction and timer values for verifying installer updates.</summary>
    /// <param name="index">The enemy slot index assigned to the new slot.</param>
    /// <returns>A slot initialized with sentinel list and timer values.</returns>
    private static RoomEnemySlot NewDragonAnimationSlot(int index) => new(index)
    {
        CurrentInstruction = 0x7777,
        InstructionTimer = 0x2222,
        Timer = 0x3333,
    };

    /// <summary>Creates Dragon state with independent per-enemy arrays sized to the production slot limit.</summary>
    /// <param name="slot">The physical enemy slot associated with the state.</param>
    /// <returns>A state ready for an animation-list handoff check.</returns>
    private static DragonEnemyState NewDragonAnimationState(RoomEnemySlot slot) =>
        new(slot, new ushort[RoomEnemySystem.MaximumEnemyCount],
            new ushort[RoomEnemySystem.MaximumEnemyCount],
            new ushort[RoomEnemySystem.MaximumEnemyCount]);

    /// <summary>Checks that a requested Dragon selector is installed in both state and slot with reset timers.</summary>
    /// <param name="slot">The physical slot receiving the instruction-list handoff.</param>
    /// <param name="state">The Dragon state tracking requested and installed selector identities.</param>
    /// <param name="expected">The selector expected to be requested and installed.</param>
    /// <param name="context">Description included in assertion messages to identify the handoff under test.</param>
    /// <param name="expectedInstructionTimer">The expected instruction timer after the handoff.</param>
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

    /// <summary>Reads a native animation-table word from two adjacent little-endian bytes.</summary>
    /// <param name="bus">The address space containing the native pointer table.</param>
    /// <param name="address">The address of the word's low byte.</param>
    /// <returns>The two bytes combined into a 16-bit value.</returns>
    private static ushort ReadDragonAnimationWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects runtime reads from the migrated Dragon animation-selector pointer table.</summary>
    /// <param name="source">The underlying address space used for allowed reads and all writes.</param>
    private sealed class DragonAnimationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge reads through the selector-table guard.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The byte supplied by the guarded address space when the read is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads within the migrated selector table and delegates all other byte reads.</summary>
        /// <param name="address">The bus address to inspect and read.</param>
        /// <returns>The byte supplied by the underlying address space when the address is allowed.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to the migrated Dragon animation-selector table.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa2e5ef and < 0xa2e5fb
                ? throw new InvalidOperationException(
                    $"Dragon attempted migrated animation-selector read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards byte writes unchanged to the underlying address space.</summary>
        /// <param name="address">The bus address to write.</param>
        /// <param name="value">The byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
