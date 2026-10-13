using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks the four native animation selectors and live instruction handoffs for both Skree and Metaree.</summary>
internal static class SkreeMetareeAnimationDefinitionsVerification
{
    /// <summary>Compares both compiled phase tables with ROM and exercises all install and attack transitions with table reads forbidden.</summary>
    internal static void VerifySkreeMetareeAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            var phase = (SkreeMetareeAnimationPhase)index;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3894e + index * 2)),
                SkreeMetareeAnimationDefinitions.MetareeInstructionList(phase),
                $"Metaree animation phase {phase}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3c69c + index * 2)),
                SkreeMetareeAnimationDefinitions.SkreeInstructionList(phase),
                $"Skree animation phase {phase}");
        }

        Suite(nameof(VerifyAllSkreeMetareeInstallHandoffs), () => VerifyAllSkreeMetareeInstallHandoffs());
        Suite(nameof(VerifyLiveMetareeAnimationHandoffs), () => VerifyLiveMetareeAnimationHandoffs(rom));
        Suite(nameof(VerifyLiveSkreeAnimationHandoffs), () => VerifyLiveSkreeAnimationHandoffs(rom));

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

    /// <summary>Installs every requested phase from a different prior phase and checks each slot's pointer and timer reset.</summary>
    private static void VerifyAllSkreeMetareeInstallHandoffs()
    {

        for (int index = 0; index < 4; index++)
        {
            var phase = (SkreeMetareeAnimationPhase)index;
            var metareeSlot = NewSkreeMetareeAnimationSlot();
            var metareeState = new MetareeEnemyState(metareeSlot)
            {
                RequestedInstructionListIndex = phase,
                InstalledInstructionListIndex = DifferentSkreeMetareePhase(phase),
            };
            MetareeBehavior.InstallRequestedMetareeInstruction(metareeSlot, metareeState);
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
            SkreeBehavior.InstallRequestedSkreeInstruction(skreeSlot, skreeState);
            AssertSkreeMetareeHandoff(
                skreeSlot,
                skreeState.InstalledInstructionIndex,
                phase,
                SkreeMetareeAnimationDefinitions.SkreeInstructionList(phase),
                $"Skree {phase}");
        }
    }

    /// <summary>Drives a live Metaree from idle through attack preparation into its diving animation.</summary>
    private static void VerifyLiveMetareeAnimationHandoffs(SuperMetroidAddressSpace rom)
    {
        var enemies = NewSkreeMetareeGuardedSystem(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));

        enemies.MetareeBehavior.InitializeMetaree(slot);
        MetareeEnemyState state = enemies.MetareeStates[0]!;
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + 0x60)))),
        };

        MetareeBehavior.RunMetareeIdle(slot, state, samus);
        AssertSkreeMetareeHandoff(
            slot,
            state.InstalledInstructionListIndex,
            SkreeMetareeAnimationPhase.PreparingAttack,
            SkreeMetareeAnimationDefinitions.MetareeInstructionList(
                SkreeMetareeAnimationPhase.PreparingAttack),
            "live Metaree preparation");

        state.AttackReady = true;
        enemies.MetareeBehavior.RunMetareePreparation(slot, state);
        AssertSkreeMetareeHandoff(
            slot,
            state.InstalledInstructionListIndex,
            SkreeMetareeAnimationPhase.Diving,
            SkreeMetareeAnimationDefinitions.MetareeInstructionList(
                SkreeMetareeAnimationPhase.Diving),
            "live Metaree dive");
    }

    /// <summary>Drives a live Skree from idle through attack preparation into its diving animation.</summary>
    private static void VerifyLiveSkreeAnimationHandoffs(SuperMetroidAddressSpace rom)
    {
        var enemies = NewSkreeMetareeGuardedSystem(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0080));

        enemies.SkreeBehavior.InitializeSkree(slot);
        SkreeEnemyState state = enemies.SkreeStates[0]!;
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + 0x60)))),
        };

        enemies.SkreeBehavior.RunSkreeMain(slot, samus, null);
        AssertSkreeMetareeHandoff(
            slot,
            state.InstalledInstructionIndex,
            SkreeMetareeAnimationPhase.PreparingAttack,
            SkreeMetareeAnimationDefinitions.SkreeInstructionList(
                SkreeMetareeAnimationPhase.PreparingAttack),
            "live Skree preparation");

        state.AttackReady = true;
        enemies.SkreeBehavior.RunSkreeMain(slot, samus, null);
        AssertSkreeMetareeHandoff(
            slot,
            state.InstalledInstructionIndex,
            SkreeMetareeAnimationPhase.Diving,
            SkreeMetareeAnimationDefinitions.SkreeInstructionList(
                SkreeMetareeAnimationPhase.Diving),
            "live Skree dive");
    }

    /// <summary>Creates an enemy system that rejects reads from the two migrated animation selector tables.</summary>
    private static RoomEnemySystem NewSkreeMetareeGuardedSystem(
        SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewSkreeMetareeAnimationReadGuard(rom));
        return enemies;
    }

    /// <summary>Seeds nondefault instruction and timer values so installation must visibly replace them.</summary>
    private static RoomEnemySlot NewSkreeMetareeAnimationSlot() => new(0)
    {
        CurrentInstruction = 0x7777,
        InstructionTimer = 0x2222,
        Timer = 0x3333,
    };

    /// <summary>Chooses an installed phase distinct from the requested one for each handoff fixture.</summary>
    private static SkreeMetareeAnimationPhase DifferentSkreeMetareePhase(
        SkreeMetareeAnimationPhase phase) =>
        phase == SkreeMetareeAnimationPhase.Idling
            ? SkreeMetareeAnimationPhase.PreparingAttack
            : SkreeMetareeAnimationPhase.Idling;

    /// <summary>Checks the installed phase and native instruction pointer plus both timer resets.</summary>
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

    /// <summary>Forbids runtime reads of bank-$A3 Metaree and Skree animation selector words.</summary>
    private static CartridgeReadGuard NewSkreeMetareeAnimationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa3894e and < 0xa38956 or >= 0xa3c69c and < 0xa3c6a4,
            address => $"Skree/Metaree attempted migrated selector read ${address:X6}.");
}
