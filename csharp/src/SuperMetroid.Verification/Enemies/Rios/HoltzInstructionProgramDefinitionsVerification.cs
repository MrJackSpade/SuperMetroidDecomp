using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Holtz's (Holtz's) typed parent and flame animations.</summary>
internal static class HoltzInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Holtz animation checks.</summary>
    internal static void VerifyHoltzInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyHoltzInstructionProgramDefinitions), () => VerifyHoltzInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Each of the seven programs, advanced one pose per update, publishes its flame visibility and finished handshake;
    /// the initializers pick idle for the parent and flames for the follower; the program oracle matches bank $A2.
    /// </summary>
    internal static void VerifyHoltzInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        HoltzInstructionProgramDefinitionsProgram[] programs =
        [
            new HoltzInstructionProgramDefinitionsProgram(HoltzSegment.Idle, 5, false, false, "idle"),
            new HoltzInstructionProgramDefinitionsProgram(HoltzSegment.PrepareToSwoop, 10, false, true, "prepare to swoop"),
            new HoltzInstructionProgramDefinitionsProgram(HoltzSegment.Descending, 2, false, false, "descending"),
            new HoltzInstructionProgramDefinitionsProgram(HoltzSegment.AscentOpening, 4, false, true, "ascending part one"),
            new HoltzInstructionProgramDefinitionsProgram(HoltzSegment.Ascending, 4, true, false, "ascending part two"),
            new HoltzInstructionProgramDefinitionsProgram(HoltzSegment.Cooldown, 10, true, true, "cooldown"),
            new HoltzInstructionProgramDefinitionsProgram(HoltzSegment.Flames, 4, false, false, "flames"),
        ];
        foreach ((HoltzSegment entry, int calls, bool visible, bool signal, string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, HoltzEnemyState state) = NewHoltzInstructionSystem(entry);
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
            for (int call = 0; call < calls; call++)
            {
                place.AnimationClock.StepOnNextUpdate();
                EnemyInstructionRunner.Process(arguments);
            }
            AssertEqual(visible, state.FollowerVisible, $"Lower Norfair Rio {name} publishes follower visibility");
            AssertEqual(signal, state.Finish.IsRaised, $"Lower Norfair Rio {name} animation signal");
        }

        Suite(nameof(VerifyHoltzInitializerSelections), () => VerifyHoltzInitializerSelections());
        Suite(nameof(HoltzAnimationsVerification.VerifyHoltzAnimations), () => HoltzAnimationsVerification.VerifyHoltzAnimations(rom));
        Console.WriteLine(
            "Lower Norfair Rio: all seven parent and flame programs with their flame visibility and finished handshakes, " +
            "and the initializer selections pass.");
    }

    /// <summary>Confirms that the parent starts idle while its follower starts the flame program.</summary>
    private static void VerifyHoltzInitializerSelections()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The room's population load sets up the parent and then its flame follower (#2554).
        _ = enemies.LoadPopulation(
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Holtz, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader)),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Holtz, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Follower)));
        AssertEqual(new AnimationPlayback<HoltzSegment>(HoltzAnimations.StartingWith(HoltzSegment.Idle), AnimationPlace.StartOfAnimation), enemies.Places.InUpdateOrder[0].EntityState<HoltzEntity, HoltzEnemyState>()?.Playback,
            "Lower Norfair Rio parent initializer selects idle");

        RoomEnemyPlace follower = enemies.Places.InUpdateOrder[1];
        // The follower shows the first picture of the flames its setup started on its next animation step; the room spawn
        // primes the instruction timer before the species' setup runs.
        follower.AnimationClock.StepOnNextUpdate();
        follower.AdvanceAnimation(RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual<EnemyFrameId?>(HoltzAnimations.Segments.FirstFrameShown(HoltzAnimations.StartingWith(HoltzSegment.Flames)), follower.Frame,
            "Lower Norfair Rio follower initializer selects flames");
    }

    /// <summary>Creates a Holtz running <paramref name="entry"/> in the first enemy slot.</summary>
    internal static NewHoltzInstructionSystemResult NewHoltzInstructionSystem(
        HoltzSegment entry)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The Rio is set up as its room's population sets it up, then plays the animation the check starts it at.
        RoomEnemyPlace place = enemies.LoadPopulation(PopulationPlacementAccess.Entry(EnemyDefinitionId.Holtz, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader)))[0];
        HoltzEnemyState state = place.RequireSpeciesState<HoltzEnemyState>();
        state.Playback = new(HoltzAnimations.StartingWith(entry), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
        return new NewHoltzInstructionSystemResult(enemies, place, state);
    }

    /// <summary>
    /// Initializes a Holtz parent and its flame follower and confirms parameter one's sign bit selects the
    /// follower.
    /// </summary>
    internal static void VerifyHoltzOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        IReadOnlyList<RoomEnemyPlace> places = enemies.LoadPopulation(
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Holtz, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader)),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Holtz, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Follower)));
        RoomEnemyPlace parent = places[0];
        RoomEnemyPlace follower = places[1];
        // The placement builds the part's own entity (#2164): the parent a Rio, the follower its flames.
        AssertTrue(parent.Entity is HoltzEntity && follower.Entity is HoltzFollowerEntity,
            "parameter one's sign bit selects the follower, which is built as a follower");
        Console.WriteLine("Lower Norfair Rio owned state: the follower selection matches.");
    }

    /// <summary>Runs the focused Holtz owned-state check.</summary>
    [VerificationCommand("--lower-norfair-rio-owned-state")]
    internal static int HoltzOwnedStateCommand()
    {
        VerifyHoltzOwnedState();
        return 0;
    }

    /// <summary>Runs the standalone Holtz animation verification command.</summary>
    [VerificationCommand("--lower-norfair-rio-instruction-program-definitions")]
    internal static int HoltzInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyHoltzInstructionProgramDefinitions), () => VerifyHoltzInstructionProgramDefinitions());
        return 0;
    }
}
