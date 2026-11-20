using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled Geruta parent and flame instruction programs.</summary>
internal static class GerutaInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for Geruta instruction comparison.</summary>
    internal static void VerifyGerutaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyGerutaInstructionProgramDefinitions), () => VerifyGerutaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, seven guarded programs, callbacks, visual selectors, and allocation-free reads.</summary>
    internal static void VerifyGerutaInstructionProgramDefinitions(
        CartridgeImportAddressSpace rom)
    {
        // Each update shows the next pose; the cues before it place the flames, and a one-shot swoop start ends by
        // telling main AI it has finished.
        GerutaInstructionProgramDefinitionsProgram[] programs =
        [
            new GerutaInstructionProgramDefinitionsProgram(GerutaSegment.Idle, 5, 8, false, "idle"),
            new GerutaInstructionProgramDefinitionsProgram(GerutaSegment.StartDescending, 7, -16, true, "start descending"),
            new GerutaInstructionProgramDefinitionsProgram(GerutaSegment.Descending, 5, -12, false, "descending"),
            new GerutaInstructionProgramDefinitionsProgram(GerutaSegment.StartAscending, 9, 12, true, "start ascending"),
            new GerutaInstructionProgramDefinitionsProgram(GerutaSegment.Ascending, 5, 12, false, "ascending"),
            new GerutaInstructionProgramDefinitionsProgram(GerutaSegment.FlamesAscending, 5, 0, false, "ascending flames"),
            new GerutaInstructionProgramDefinitionsProgram(GerutaSegment.FlamesDescending, 5, 0, false, "descending flames"),
        ];
        foreach ((GerutaSegment entry, int calls, short offset, bool signal, string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, GerutaEnemyState state) =
                NewGerutaInstructionSystem(entry);
            object?[] arguments =
                [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
            RunGerutaInstructionFrames(arguments,
                place,
                calls);
            AssertEqual(offset, state.FollowerYOffset,
                $"Norfair Rio {name} publishes its final follower offset");
            AssertEqual(signal, state.Finish.IsRaised,
                $"Norfair Rio {name} animation signal");
        }

        // The game's follower picks its flames by the sign of the offset its parent's instruction placed: falling flames
        // above the Rio, rising ones level with or below it. Each native placement names those flames (#2450).
        foreach (GerutaCue.PlaceFlames placement in GerutaNativeListAddresses.Instructions.Values.Select(instruction => instruction.Cue).OfType<GerutaCue.PlaceFlames>())
        {
            AssertEqual(placement.Below < 0 ? GerutaFlames.Descending : GerutaFlames.Ascending, placement.Flames,
                $"Norfair Rio flames placed {placement.Below} pixels below it");
        }

        Suite(nameof(VerifyGerutaInitializerSelections), () => VerifyGerutaInitializerSelections());
        Suite(nameof(GerutaAnimationsVerification.VerifyGerutaAnimations), () => GerutaAnimationsVerification.VerifyGerutaAnimations(rom));

        Console.WriteLine(
            "Norfair Rio: all seven parent and flame programs with their flame placements and swoop-start handshakes, " +
            "and the initializer selections pass.");
    }

    /// <summary>Checks that parent and follower initialization choose idle and ascending-flames programs respectively.</summary>
    private static void VerifyGerutaInitializerSelections(
)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);

        RoomEnemyPlace parent = enemies.Places.InUpdateOrder[0];
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        parent.Bind(EnemyDefinitionId.Geruta, parent.Definition);
        parent.Bind(parent.HeldOccupant().DefinitionId, default(RoomEnemyDefinition));
        GerutaBehavior.InitializeGeruta(parent);
        AssertEqual(new AnimationPlayback<GerutaSegment>(GerutaAnimations.StartingWith(GerutaSegment.Idle), AnimationPlace.StartOfAnimation),
            enemies.Places.InUpdateOrder[0].EntityState<GerutaEntity, GerutaEnemyState>()?.Playback,
            "Norfair Rio parent initializer selects idle");

        RoomEnemyPlace follower = enemies.Places.InUpdateOrder[1];
        follower.SetSpawnParameter1(EnemyDefinitionId.Geruta, 0x8000);
        follower.Bind(EnemyDefinitionId.Geruta, follower.Definition);
        follower.Bind(follower.HeldOccupant().DefinitionId, default(RoomEnemyDefinition));
        enemies.GerutaBehavior.InitializeGerutaFollower(follower);
        AssertEqual(new AnimationPlayback<GerutaSegment>(GerutaAnimations.StartingWith(GerutaSegment.FlamesAscending), AnimationPlace.StartOfAnimation),
            enemies.Places.InUpdateOrder[1].EntityState<GerutaFollowerEntity, GerutaFollowerState>()?.Playback,
            "Norfair Rio follower initializer selects ascending flames");
    }

    /// <summary>Seeds a Geruta-backed Geruta actor with an instruction entry and optional installed artwork.</summary>
    internal static NewGerutaInstructionSystemResult NewGerutaInstructionSystem(
            GerutaSegment entry,
            ImportEnemyArtwork? art = null)
    {
        var enemies = EnemySystemFixtures.New((art is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(art.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: art?.Bosses);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        place.Bind(EnemyDefinitionId.Geruta, place.Definition);
        place.Bind(place.HeldOccupant().DefinitionId, default(RoomEnemyDefinition));
        place.AnimationClock.StepOnNextUpdate();
        var state = new GerutaEnemyState(GerutaAction.WaitForAttackOpportunity) { Playback = new(GerutaAnimations.StartingWith(entry), AnimationPlace.StartOfAnimation) };
        place.SetSpeciesState(state);
        return new NewGerutaInstructionSystemResult(enemies, place, state);
    }

    /// <summary>Processes the selected instruction entry on each requested actor update.</summary>
    internal static void RunGerutaInstructionFrames(object?[] arguments,
        RoomEnemyPlace place,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            place.AnimationClock.StepOnNextUpdate();
            EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Runs the retail-cartridge Geruta instruction verification.</summary>
    [VerificationCommand("--norfair-rio-instruction-program-definitions")]
    internal static int GerutaInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyGerutaInstructionProgramDefinitions), () => VerifyGerutaInstructionProgramDefinitions());
        return 0;
    }
}
