using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Lower Norfair Rio's (Holtz's) typed parent and flame animations.</summary>
internal static class LowerNorfairRioInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Lower Norfair Rio animation checks.</summary>
    internal static void VerifyLowerNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Each of the seven programs, advanced one pose per update, publishes its flame visibility and finished handshake;
    /// the initializers pick idle for the parent and flames for the follower; the program oracle matches bank $A2.
    /// </summary>
    internal static void VerifyLowerNorfairRioInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        (LowerNorfairRioProgram Entry, int Calls, bool Visible, bool Signal, string Name)[] programs =
        [
            (LowerNorfairRioProgram.Idle, 5, false, false, "idle"),
            (LowerNorfairRioProgram.PrepareToSwoop, 10, false, true, "prepare to swoop"),
            (LowerNorfairRioProgram.Descending, 2, false, false, "descending"),
            (LowerNorfairRioProgram.AscendingPart1, 4, false, true, "ascending part one"),
            (LowerNorfairRioProgram.AscendingPart2, 4, true, false, "ascending part two"),
            (LowerNorfairRioProgram.Cooldown, 10, true, true, "cooldown"),
            (LowerNorfairRioProgram.Flames, 4, false, false, "flames"),
        ];
        foreach ((LowerNorfairRioProgram entry, int calls, bool visible, bool signal, string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, LowerNorfairRioEnemyState state) = NewLowerNorfairRioInstructionSystem(entry);
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
            for (int call = 0; call < calls; call++)
            {
                slot.InstructionTimer = NativeCountdown.Start(1);
                EnemyInstructionRunner.Process(arguments);
            }
            AssertEqual(visible, state.FollowerVisible, $"Lower Norfair Rio {name} publishes follower visibility");
            AssertEqual(signal, state.AnimationSignal, $"Lower Norfair Rio {name} animation signal");
        }

        Suite(nameof(VerifyLowerNorfairRioInitializerSelections), () => VerifyLowerNorfairRioInitializerSelections());
        Suite(nameof(LowerNorfairRioAnimationsVerification.VerifyLowerNorfairRioAnimations), () => LowerNorfairRioAnimationsVerification.VerifyLowerNorfairRioAnimations(rom));
        Console.WriteLine(
            "Lower Norfair Rio: all seven parent and flame programs with their flame visibility and finished handshakes, " +
            "and the initializer selections pass.");
    }

    /// <summary>Confirms that the parent starts idle while its follower starts the flame program.</summary>
    private static void VerifyLowerNorfairRioInitializerSelections()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);

        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Holtz, parent.Definition);
        parent.Bind(parent.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa2 });
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(parent);
        AssertEqual(AnimationProgramCursor<LowerNorfairRioProgram>.Start(LowerNorfairRioProgram.Idle), enemies.SlotTable.Slots[0].EntityState<LowerNorfairRioEntity, LowerNorfairRioEnemyState>()?.Program,
            "Lower Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Holtz, follower.Definition);
        follower.Bind(follower.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa2 });
        follower.SetSpawnParameter1(0x8000);
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(follower);
        AssertEqual(AnimationProgramCursor<LowerNorfairRioProgram>.Start(LowerNorfairRioProgram.Flames), enemies.SlotTable.Slots[1].EntityState<LowerNorfairRioEntity, LowerNorfairRioEnemyState>()?.Program,
            "Lower Norfair Rio follower initializer selects flames");
    }

    /// <summary>Creates a Lower Norfair Rio running <paramref name="entry"/> in the first enemy slot.</summary>
    internal static (RoomEnemySystem Enemies, RoomEnemySlot Slot, LowerNorfairRioEnemyState State) NewLowerNorfairRioInstructionSystem(
        LowerNorfairRioProgram entry)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Holtz, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa2 });
        slot.InstructionTimer = NativeCountdown.Start(1);
        var state = new LowerNorfairRioEnemyState { Program = AnimationProgramCursor<LowerNorfairRioProgram>.Start(entry) };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        return (enemies, slot, state);
    }

    /// <summary>
    /// Initializes a Lower Norfair Rio parent and its flame follower and confirms parameter one's sign bit selects the
    /// follower, then confirms a kill empties the velocity, behaviour and follower selection kept in the common record
    /// while the follower visibility outlives it.
    /// </summary>
    internal static void VerifyLowerNorfairRioOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Holtz, parent.Definition);
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(parent);
        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Holtz, follower.Definition);
        follower.SetSpawnParameter1(EnemyParameterMasks.FollowerBit);
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(follower);
        LowerNorfairRioEnemyState parentState = parent.EntityState<LowerNorfairRioEntity, LowerNorfairRioEnemyState>()
            ?? throw new InvalidOperationException("The Lower Norfair Rio parent has no state.");
        LowerNorfairRioEnemyState followerState = follower.EntityState<LowerNorfairRioEntity, LowerNorfairRioEnemyState>()
            ?? throw new InvalidOperationException("The Lower Norfair Rio follower has no state.");
        AssertTrue(!parentState.IsFollower && followerState.IsFollower, "parameter one's sign bit selects the follower");

        followerState.FollowerVisible = true;
        followerState.Velocity = new(new SnesSignedEightEight(0x0180), new SnesSignedEightEight(0xff00));
        follower.Clear();
        AssertEqual<LowerNorfairRioEnemyFunction?>(null, followerState.Function, "a kill empties the behaviour");
        AssertEqual(default(EightEightVector<RoomPixels>), followerState.Velocity, "a kill empties the velocity");
        AssertTrue(!followerState.IsFollower, "a kill empties the follower selection with its parameter");
        AssertTrue(followerState.FollowerVisible, "the follower visibility outlives the kill");
        Console.WriteLine("Lower Norfair Rio owned state: the follower selection and the cleared record match.");
    }

    /// <summary>Runs the focused Lower Norfair Rio owned-state check.</summary>
    [VerificationCommand("--lower-norfair-rio-owned-state")]
    internal static int LowerNorfairRioOwnedStateCommand()
    {
        VerifyLowerNorfairRioOwnedState();
        return 0;
    }

    /// <summary>Runs the standalone Lower Norfair Rio animation verification command.</summary>
    [VerificationCommand("--lower-norfair-rio-instruction-program-definitions")]
    internal static int LowerNorfairRioInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions());
        return 0;
    }
}
