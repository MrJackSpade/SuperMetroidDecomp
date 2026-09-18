using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySkreeMetareeAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            var phase = (SkreeMetareeAnimationPhase)index;
            AssertEqual(ReadSkreeMetareeAnimationWord(rom, 0xa3894e + index * 2),
                SkreeMetareeAnimationDefinitions.MetareeInstructionList(phase),
                $"Metaree animation phase {phase}");
            AssertEqual(ReadSkreeMetareeAnimationWord(rom, 0xa3c69c + index * 2),
                SkreeMetareeAnimationDefinitions.SkreeInstructionList(phase),
                $"Skree animation phase {phase}");
        }

        VerifyAllSkreeMetareeInstallHandoffs();
        VerifyLiveMetareeAnimationHandoffs(rom);
        VerifyLiveSkreeAnimationHandoffs(rom);

        var invalid = (SkreeMetareeAnimationPhase)4;
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeAnimationDefinitions.MetareeInstructionList(invalid),
            "Metaree animation phase past table");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeAnimationDefinitions.SkreeInstructionList(invalid),
            "Skree animation phase past table");
        Console.WriteLine(
            "Skree/Metaree animation definitions: eight native selectors, all eight install handoffs, and both live attack transitions per family pass with pointer tables forbidden.");
    }

    private static void VerifyAllSkreeMetareeInstallHandoffs()
    {
        MethodInfo installMetaree = typeof(RoomEnemySystem).GetMethod(
            "InstallRequestedMetareeInstruction",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo installSkree = typeof(RoomEnemySystem).GetMethod(
            "InstallRequestedSkreeInstruction",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        for (int index = 0; index < 4; index++)
        {
            var phase = (SkreeMetareeAnimationPhase)index;
            var metareeSlot = NewSkreeMetareeAnimationSlot();
            var metareeState = new MetareeEnemyState(metareeSlot)
            {
                RequestedInstructionListIndex = phase,
                InstalledInstructionListIndex = DifferentSkreeMetareePhase(phase),
            };
            installMetaree.Invoke(null, [metareeSlot, metareeState]);
            AssertSkreeMetareeHandoff(
                metareeSlot,
                metareeState.InstalledInstructionListIndex,
                phase,
                SkreeMetareeAnimationDefinitions.MetareeInstructionList(phase),
                $"Metaree {phase}");

            var skreeSlot = NewSkreeMetareeAnimationSlot();
            var skreeState = new SkreeEnemyState(skreeSlot)
            {
                RequestedInstructionIndex = phase,
                InstalledInstructionIndex = DifferentSkreeMetareePhase(phase),
            };
            installSkree.Invoke(null, [skreeSlot, skreeState]);
            AssertSkreeMetareeHandoff(
                skreeSlot,
                skreeState.InstalledInstructionIndex,
                phase,
                SkreeMetareeAnimationDefinitions.SkreeInstructionList(phase),
                $"Skree {phase}");
        }
    }

    private static void VerifyLiveMetareeAnimationHandoffs(SuperMetroidAddressSpace rom)
    {
        var enemies = NewSkreeMetareeGuardedSystem(rom);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.XPosition = 0x0100;
        slot.YPosition = 0x0080;

        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeMetaree", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo idle = typeof(RoomEnemySystem).GetMethod(
            "RunMetareeIdle", BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo prepare = typeof(RoomEnemySystem).GetMethod(
            "RunMetareePreparation", BindingFlags.Instance | BindingFlags.NonPublic)!;
        initialize.Invoke(enemies, [slot]);
        MetareeEnemyState state = enemies.MetareeStates[0]!;
        var samus = new SamusState
        {
            XPosition = slot.XPosition,
            YPosition = unchecked((ushort)(slot.YPosition + 0x60)),
        };

        idle.Invoke(null, [slot, state, samus]);
        AssertSkreeMetareeHandoff(
            slot,
            state.InstalledInstructionListIndex,
            SkreeMetareeAnimationPhase.PreparingAttack,
            SkreeMetareeAnimationDefinitions.MetareeInstructionList(
                SkreeMetareeAnimationPhase.PreparingAttack),
            "live Metaree preparation");

        state.AttackReady = true;
        prepare.Invoke(enemies, [slot, state]);
        AssertSkreeMetareeHandoff(
            slot,
            state.InstalledInstructionListIndex,
            SkreeMetareeAnimationPhase.Diving,
            SkreeMetareeAnimationDefinitions.MetareeInstructionList(
                SkreeMetareeAnimationPhase.Diving),
            "live Metaree dive");
    }

    private static void VerifyLiveSkreeAnimationHandoffs(SuperMetroidAddressSpace rom)
    {
        var enemies = NewSkreeMetareeGuardedSystem(rom);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.XPosition = 0x0100;
        slot.YPosition = 0x0080;

        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeSkree", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo runMain = typeof(RoomEnemySystem).GetMethod(
            "RunSkreeMain", BindingFlags.Instance | BindingFlags.NonPublic)!;
        initialize.Invoke(enemies, [slot]);
        SkreeEnemyState state = enemies.SkreeStates[0]!;
        var samus = new SamusState
        {
            XPosition = slot.XPosition,
            YPosition = unchecked((ushort)(slot.YPosition + 0x60)),
        };

        runMain.Invoke(enemies, [slot, samus, null]);
        AssertSkreeMetareeHandoff(
            slot,
            state.InstalledInstructionIndex,
            SkreeMetareeAnimationPhase.PreparingAttack,
            SkreeMetareeAnimationDefinitions.SkreeInstructionList(
                SkreeMetareeAnimationPhase.PreparingAttack),
            "live Skree preparation");

        state.AttackReady = true;
        runMain.Invoke(enemies, [slot, samus, null]);
        AssertSkreeMetareeHandoff(
            slot,
            state.InstalledInstructionIndex,
            SkreeMetareeAnimationPhase.Diving,
            SkreeMetareeAnimationDefinitions.SkreeInstructionList(
                SkreeMetareeAnimationPhase.Diving),
            "live Skree dive");
    }

    private static RoomEnemySystem NewSkreeMetareeGuardedSystem(
        SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(enemies, new SkreeMetareeAnimationReadGuard(rom));
        return enemies;
    }

    private static RoomEnemySlot NewSkreeMetareeAnimationSlot() => new(0)
    {
        CurrentInstruction = 0x7777,
        InstructionTimer = 0x2222,
        Timer = 0x3333,
    };

    private static SkreeMetareeAnimationPhase DifferentSkreeMetareePhase(
        SkreeMetareeAnimationPhase phase) =>
        phase == SkreeMetareeAnimationPhase.Idling
            ? SkreeMetareeAnimationPhase.PreparingAttack
            : SkreeMetareeAnimationPhase.Idling;

    private static void AssertSkreeMetareeHandoff(
        RoomEnemySlot slot,
        SkreeMetareeAnimationPhase installed,
        SkreeMetareeAnimationPhase expectedPhase,
        ushort expectedInstruction,
        string context)
    {
        AssertEqual(expectedPhase, installed, $"{context} installed phase");
        AssertEqual(expectedInstruction, slot.CurrentInstruction, $"{context} instruction");
        AssertEqual(1, slot.InstructionTimer, $"{context} instruction timer");
        AssertEqual(0, slot.Timer, $"{context} loop timer");
    }

    private static ushort ReadSkreeMetareeAnimationWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class SkreeMetareeAnimationReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa3894e and < 0xa38956 or >= 0xa3c69c and < 0xa3c6a4
                ? throw new InvalidOperationException(
                    $"Skree/Metaree attempted migrated selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
