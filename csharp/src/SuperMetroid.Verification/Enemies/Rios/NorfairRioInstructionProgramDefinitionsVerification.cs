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

/// <summary>Checks compiled Norfair Rio parent and flame instruction programs.</summary>
internal static class NorfairRioInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for Norfair Rio instruction comparison.</summary>
    internal static void VerifyNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, seven guarded programs, callbacks, visual selectors, and allocation-free reads.</summary>
    internal static void VerifyNorfairRioInstructionProgramDefinitions(
        CartridgeImportAddressSpace rom)
    {
        // Each update shows the next pose; the cues before it place the flames, and a one-shot swoop start ends by
        // telling main AI it has finished.
        NorfairRioInstructionProgramDefinitionsProgram[] programs =
        [
            new NorfairRioInstructionProgramDefinitionsProgram(NorfairRioSegment.Idle, 5, 8, false, "idle"),
            new NorfairRioInstructionProgramDefinitionsProgram(NorfairRioSegment.StartDescending, 7, -16, true, "start descending"),
            new NorfairRioInstructionProgramDefinitionsProgram(NorfairRioSegment.Descending, 5, -12, false, "descending"),
            new NorfairRioInstructionProgramDefinitionsProgram(NorfairRioSegment.StartAscending, 9, 12, true, "start ascending"),
            new NorfairRioInstructionProgramDefinitionsProgram(NorfairRioSegment.Ascending, 5, 12, false, "ascending"),
            new NorfairRioInstructionProgramDefinitionsProgram(NorfairRioSegment.FlamesAscending, 5, 0, false, "ascending flames"),
            new NorfairRioInstructionProgramDefinitionsProgram(NorfairRioSegment.FlamesDescending, 5, 0, false, "descending flames"),
        ];
        foreach ((NorfairRioSegment entry, int calls, short offset, bool signal, string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, NorfairRioEnemyState state) =
                NewNorfairRioInstructionSystem(entry);
            object?[] arguments =
                [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
            RunNorfairRioInstructionFrames(arguments,
                place,
                calls);
            AssertEqual(offset, state.FollowerYOffset,
                $"Norfair Rio {name} publishes its final follower offset");
            AssertEqual(signal, state.AnimationSignal,
                $"Norfair Rio {name} animation signal");
        }

        // The game's follower picks its flames by the sign of the offset its parent's instruction placed: falling flames
        // above the Rio, rising ones level with or below it. Each native placement names those flames (#2450).
        foreach (NorfairRioCue.PlaceFlames placement in NorfairRioNativeListAddresses.Instructions.Values.Select(instruction => instruction.Cue).OfType<NorfairRioCue.PlaceFlames>())
        {
            AssertEqual(placement.Below < 0 ? NorfairRioFlames.Descending : NorfairRioFlames.Ascending, placement.Flames,
                $"Norfair Rio flames placed {placement.Below} pixels below it");
        }

        Suite(nameof(VerifyNorfairRioInitializerSelections), () => VerifyNorfairRioInitializerSelections());
        Suite(nameof(NorfairRioAnimationsVerification.VerifyNorfairRioAnimations), () => NorfairRioAnimationsVerification.VerifyNorfairRioAnimations(rom));

        Console.WriteLine(
            "Norfair Rio: all seven parent and flame programs with their flame placements and swoop-start handshakes, " +
            "and the initializer selections pass.");
    }

    /// <summary>Checks that parent and follower initialization choose idle and ascending-flames programs respectively.</summary>
    private static void VerifyNorfairRioInitializerSelections(
)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);

        RoomEnemyPlace parent = enemies.Places.InUpdateOrder[0];
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        parent.Bind(EnemyDefinitionId.Geruta, parent.Definition);
        parent.Bind(parent.DefinitionId, default(RoomEnemyDefinition));
        NorfairRioBehavior.InitializeNorfairRio(parent);
        AssertEqual(new AnimationPlayback<NorfairRioSegment>(NorfairRioAnimations.StartingWith(NorfairRioSegment.Idle), AnimationPlace.StartOfAnimation),
            enemies.Places.InUpdateOrder[0].EntityState<NorfairRioEntity, NorfairRioEnemyState>()?.Playback,
            "Norfair Rio parent initializer selects idle");

        RoomEnemyPlace follower = enemies.Places.InUpdateOrder[1];
        follower.SetSpawnParameter1(EnemyDefinitionId.Geruta, 0x8000);
        follower.Bind(EnemyDefinitionId.Geruta, follower.Definition);
        follower.Bind(follower.DefinitionId, default(RoomEnemyDefinition));
        enemies.NorfairRioBehavior.InitializeNorfairRioFollower(follower);
        AssertEqual(new AnimationPlayback<NorfairRioSegment>(NorfairRioAnimations.StartingWith(NorfairRioSegment.FlamesAscending), AnimationPlace.StartOfAnimation),
            enemies.Places.InUpdateOrder[1].EntityState<NorfairRioFollowerEntity, NorfairRioFollowerState>()?.Playback,
            "Norfair Rio follower initializer selects ascending flames");
    }

    /// <summary>Seeds a Geruta-backed Norfair Rio actor with an instruction entry and optional installed artwork.</summary>
    internal static NewNorfairRioInstructionSystemResult NewNorfairRioInstructionSystem(
            NorfairRioSegment entry,
            ImportEnemyArtwork? art = null)
    {
        var enemies = EnemySystemFixtures.New((art is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(art.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: art?.Bosses);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        place.Bind(EnemyDefinitionId.Geruta, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.AnimationClock.StepOnNextUpdate();
        var state = new NorfairRioEnemyState(NorfairRioAction.WaitForAttackOpportunity) { Playback = new(NorfairRioAnimations.StartingWith(entry), AnimationPlace.StartOfAnimation) };
        place.SetSpeciesState(state);
        return new NewNorfairRioInstructionSystemResult(enemies, place, state);
    }

    /// <summary>Processes the selected instruction entry on each requested actor update.</summary>
    internal static void RunNorfairRioInstructionFrames(object?[] arguments,
        RoomEnemyPlace place,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            place.AnimationClock.StepOnNextUpdate();
            EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Runs the retail-cartridge Norfair Rio instruction verification.</summary>
    [VerificationCommand("--norfair-rio-instruction-program-definitions")]
    internal static int NorfairRioInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions());
        return 0;
    }
}
