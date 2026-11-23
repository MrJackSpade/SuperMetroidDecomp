using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Production-execution checks for the Fune's and Namihe's eight typed programs.</summary>
internal static class FuneNamiheInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Fune/Namihe animation checks.</summary>
    internal static void VerifyFuneNamiheInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Fune/Namihe instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions(rom));
    }

    /// <summary>
    /// The selector table names each of the eight programs; every active program spits one fireball in its facing, plays
    /// the spit sound and hands main AI back to waiting, while an idle program spits nothing; the program oracle matches
    /// bank $A8.
    /// </summary>
    internal static void VerifyFuneNamiheInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        FuneNamiheProgramCase[] programs =
        [
            new(FuneNamiheSegment.FuneActiveLeft, 0x96d3, false, false, true),
            new(FuneNamiheSegment.FuneActiveRight, 0x96d5, false, true, true),
            new(FuneNamiheSegment.FuneIdleLeft, 0x96d7, false, false, false),
            new(FuneNamiheSegment.FuneIdleRight, 0x96d9, false, true, false),
            new(FuneNamiheSegment.NamiheActiveLeft, 0x96db, true, false, true),
            new(FuneNamiheSegment.NamiheActiveRight, 0x96dd, true, true, true),
            new(FuneNamiheSegment.NamiheIdleLeft, 0x96df, true, false, false),
            new(FuneNamiheSegment.NamiheIdleRight, 0x96e1, true, true, false),
        ];

        foreach (FuneNamiheProgramCase program in programs)
        {
            AssertEqual(program.Entry, FuneNamiheSelectorOracle.ProgramAt(program.SelectorCursor),
                $"Fune/Namihe selector $A8:{program.SelectorCursor:X4} names {program.Entry}");
            (RoomEnemySystem enemies, RoomEnemyPlace place) = RunFuneNamiheProgram(program);
            int activeProjectiles = enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive);
            AssertEqual(program.Active ? 1 : 0, activeProjectiles, $"Fune/Namihe {program.Entry} projectile count");

            if (!program.Active)
                continue;

            EnemyProjectile projectile = enemies.Projectiles.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(program.Right, projectile.Velocity.X.InTwoHundredFiftySixthsOfPixel > 0, $"Fune/Namihe {program.Entry} projectile flies in its facing");
            AssertEqual(FuneNamiheDefinitions.SpitSoundEffect, enemies.Audio.LatestSound,
                $"Fune/Namihe {program.Entry} sound cue");
            AssertTrue(program.Namihe
                    ? place.RequireSpeciesState<NamiheEnemyState>().Action == NamiheAction.WaitForSamus
                    : place.RequireSpeciesState<FuneEnemyState>().Action == FuneAction.WaitForCooldown,
                $"Fune/Namihe {program.Entry} returns main-AI ownership");
        }
        AssertThrows<InvalidDataException>(() => FuneNamiheSelectorOracle.ProgramAt(0x96e3), "Fune/Namihe selector beyond the table fails loudly");

        Suite(nameof(FuneNamiheAnimationsVerification.VerifyFuneNamiheAnimations), () => FuneNamiheAnimationsVerification.VerifyFuneNamiheAnimations(rom));
        Console.WriteLine(
            "  Fune/Namihe: the selector table, all eight programs' fireballs, spit sound and hand-back to waiting pass.");
    }

    /// <summary>How near Samus must stand above or below a placed Namihe for it to wake and spit.</summary>
    private const int NamiheWakingReach = 64;

    /// <summary>
    /// Sets one variant and facing up as its room's population sets it up (#2554), starts the case's program through the
    /// enemy's own main AI (an active program, from a Fune whose one-update wait ends or a Namihe Samus stands level with)
    /// or keeps the idle program its setup starts, then advances enough updates to observe projectile and AI handoff behavior.
    /// </summary>
    private static RunFuneNamiheProgramResult RunFuneNamiheProgram(
        FuneNamiheProgramCase program)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        EnemyDefinitionId species = program.Namihe ? EnemyDefinitionId.Namihe : EnemyDefinitionId.Fune;
        EnemySpawnSettings placed = SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(species,
            SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Blank(species) with
            {
                Parameter1 = unchecked((ushort)((program.Namihe ? 1 : 0) | (program.Right ? 0x10 : 0))),
                Parameter2 = 0,
            });
        if (placed is NamiheSpawnSettings namiheSettings)
            placed = namiheSettings with { YProximity = NamiheWakingReach };
        Position16<RoomPixels> at = new(128, 128);
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        samus.Position = FixedPosition<RoomPixels>.FromWhole(at);
        RoomEnemyPlace place = enemies.LoadPopulation(samus, PopulationPlacementAccess.Entry(species, placed, at))[0];
        if (program.Active)
            place.RunMainAi(samus);

        // The animation runs as the shared enemy animation runs a Fune's or Namihe's, over the state its setup built.
        RoomEnemyOccupant spitter = place.HeldOccupant();
        FuneNamiheSpawnSettings settings = spitter.SpawnSettings<FuneNamiheSpawnSettings>();
        Action animate;
        FuneNamiheEnemyState started;
        if (program.Namihe)
        {
            NamiheEnemyState namihe = place.RequireSpeciesState<NamiheEnemyState>();
            started = namihe;
            animate = () => enemies.FuneNamiheBehavior.AdvanceFuneNamiheTypedAnimation(spitter, settings, place, namihe);
        }
        else
        {
            FuneEnemyState fune = place.RequireSpeciesState<FuneEnemyState>();
            started = fune;
            animate = () => enemies.FuneNamiheBehavior.AdvanceFuneNamiheTypedAnimation(spitter, settings, place, fune);
        }
        AssertEqual(new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(program.Entry), AnimationPlace.StartOfAnimation), started.Playback,
            $"Fune/Namihe {program.Entry} is the program the enemy starts");
        place.AnimationClock.StepOnNextUpdate();

        int frames = program.Active ? 140 : 4;
        for (int frame = 0; frame < frames; frame++)
            animate();

        return new RunFuneNamiheProgramResult(enemies, place);
    }

    /// <summary>Program, selector cursor, variant, facing, and active state for one of the eight production programs.</summary>
    internal readonly record struct FuneNamiheProgramCase(
        FuneNamiheSegment Entry,
        ushort SelectorCursor,
        bool Namihe,
        bool Right,
        bool Active);

    /// <summary>Runs the Fune/Namihe animation checks through their dedicated command-line switch.</summary>
    [VerificationCommand("--fune-namihe-instruction-mechanics")]
    internal static int FuneNamiheInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions());
        return 0;
    }
}
