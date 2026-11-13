using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
    internal static void VerifyLowerNorfairRioInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        LowerNorfairRioInstructionProgramDefinitionsProgram[] programs =
        [
            new LowerNorfairRioInstructionProgramDefinitionsProgram(LowerNorfairRioSegment.Idle, 5, false, false, "idle"),
            new LowerNorfairRioInstructionProgramDefinitionsProgram(LowerNorfairRioSegment.PrepareToSwoop, 10, false, true, "prepare to swoop"),
            new LowerNorfairRioInstructionProgramDefinitionsProgram(LowerNorfairRioSegment.Descending, 2, false, false, "descending"),
            new LowerNorfairRioInstructionProgramDefinitionsProgram(LowerNorfairRioSegment.AscendingPart1, 4, false, true, "ascending part one"),
            new LowerNorfairRioInstructionProgramDefinitionsProgram(LowerNorfairRioSegment.AscendingPart2, 4, true, false, "ascending part two"),
            new LowerNorfairRioInstructionProgramDefinitionsProgram(LowerNorfairRioSegment.Cooldown, 10, true, true, "cooldown"),
            new LowerNorfairRioInstructionProgramDefinitionsProgram(LowerNorfairRioSegment.Flames, 4, false, false, "flames"),
        ];
        foreach ((LowerNorfairRioSegment entry, int calls, bool visible, bool signal, string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, LowerNorfairRioEnemyState state) = NewLowerNorfairRioInstructionSystem(entry);
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int call = 0; call < calls; call++)
            {
                slot.AnimationClock.StepOnNextUpdate();
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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);

        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Holtz, parent.Definition);
        parent.Bind(parent.DefinitionId, default(RoomEnemyDefinition));
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(parent);
        AssertEqual(new AnimationPlayback<LowerNorfairRioSegment>(LowerNorfairRioAnimations.StartingWith(LowerNorfairRioSegment.Idle), AnimationPlace.StartOfAnimation), enemies.SlotTable.Slots[0].EntityState<LowerNorfairRioEntity, LowerNorfairRioEnemyState>()?.Playback,
            "Lower Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Holtz, follower.Definition);
        follower.Bind(follower.DefinitionId, default(RoomEnemyDefinition));
        follower.SetSpawnParameter1(EnemyDefinitionId.Holtz, 0x8000);
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(follower);
        AssertEqual(new AnimationPlayback<LowerNorfairRioSegment>(LowerNorfairRioAnimations.StartingWith(LowerNorfairRioSegment.Flames), AnimationPlace.StartOfAnimation), enemies.SlotTable.Slots[1].EntityState<LowerNorfairRioEntity, LowerNorfairRioEnemyState>()?.Playback,
            "Lower Norfair Rio follower initializer selects flames");
    }

    /// <summary>Creates a Lower Norfair Rio running <paramref name="entry"/> in the first enemy slot.</summary>
    internal static NewLowerNorfairRioInstructionSystemResult NewLowerNorfairRioInstructionSystem(
        LowerNorfairRioSegment entry)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Holtz, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.AnimationClock.StepOnNextUpdate();
        var state = new LowerNorfairRioEnemyState { Playback = new(LowerNorfairRioAnimations.StartingWith(entry), AnimationPlace.StartOfAnimation) };
        slot.SetSpeciesState(state);
        return new NewLowerNorfairRioInstructionSystemResult(enemies, slot, state);
    }

    /// <summary>
    /// Initializes a Lower Norfair Rio parent and its flame follower and confirms parameter one's sign bit selects the
    /// follower.
    /// </summary>
    internal static void VerifyLowerNorfairRioOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Holtz, parent.Definition);
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(parent);
        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Holtz, follower.Definition);
        follower.SetSpawnParameter1(EnemyDefinitionId.Holtz, EnemyParameterMasks.FollowerBit);
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(follower);
        LowerNorfairRioEnemyState parentState = parent.EntityState<LowerNorfairRioEntity, LowerNorfairRioEnemyState>()
            ?? throw new InvalidOperationException("The Lower Norfair Rio parent has no state.");
        LowerNorfairRioEnemyState followerState = follower.EntityState<LowerNorfairRioEntity, LowerNorfairRioEnemyState>()
            ?? throw new InvalidOperationException("The Lower Norfair Rio follower has no state.");
        AssertTrue(!parentState.IsFollower && followerState.IsFollower, "parameter one's sign bit selects the follower");
        Console.WriteLine("Lower Norfair Rio owned state: the follower selection matches.");
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
