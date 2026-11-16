using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
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
                    ? (enemies.NamiheStates[place.Ordinal] ?? throw new InvalidOperationException("Verification requires the Namihe's state.")).Action == NamiheAction.WaitForSamus
                    : (enemies.FuneStates[place.Ordinal] ?? throw new InvalidOperationException("Verification requires the Fune's state.")).Action == FuneAction.WaitForCooldown,
                $"Fune/Namihe {program.Entry} returns main-AI ownership");
        }
        AssertThrows<InvalidDataException>(() => FuneNamiheSelectorOracle.ProgramAt(0x96e3), "Fune/Namihe selector beyond the table fails loudly");

        Suite(nameof(FuneNamiheAnimationsVerification.VerifyFuneNamiheAnimations), () => FuneNamiheAnimationsVerification.VerifyFuneNamiheAnimations(rom));
        Console.WriteLine(
            "  Fune/Namihe: the selector table, all eight programs' fireballs, spit sound and hand-back to waiting pass.");
    }

    /// <summary>Seeds one variant and direction into an enemy slot, then advances enough updates to observe projectile and AI handoff behavior.</summary>
    private static RunFuneNamiheProgramResult RunFuneNamiheProgram(
        FuneNamiheProgramCase program)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(program.Namihe ? EnemyDefinitionId.Namihe : EnemyDefinitionId.Fune, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.SetSpawnParameter1(place.DefinitionId, unchecked((ushort)((program.Namihe ? 1 : 0) | (program.Right ? 0x10 : 0))));
        place.SetSpawnParameter2(place.DefinitionId, 0);
        place.Position = place.Position.WithWhole(new(128, 128));
        place.AnimationClock.StepOnNextUpdate();

        var playback = new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(program.Entry), AnimationPlace.StartOfAnimation);
        if (program.Namihe)
        {
            enemies.Places.InUpdateOrder[0].SetSpeciesState(new NamiheEnemyState(program.Active ? NamiheAction.Spitting : NamiheAction.WaitForSamus)
            {
                ShowsActiveAnimation = program.Active,
                FacingRight = program.Right,
                LaunchSpeed = place.SpawnSettings<NamiheSpawnSettings>().LaunchSpeed,
                Playback = playback,
            });
        }
        else
        {
            enemies.Places.InUpdateOrder[0].SetSpeciesState(new FuneEnemyState(program.Active ? FuneAction.Spitting : FuneAction.WaitForCooldown)
            {
                ShowsActiveAnimation = program.Active,
                FacingRight = program.Right,
                LaunchSpeed = place.SpawnSettings<FuneSpawnSettings>().LaunchSpeed,
                Playback = playback,
            });
        }

        object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        int frames = program.Active ? 140 : 4;
        for (int frame = 0; frame < frames; frame++)
            EnemyInstructionRunner.Process(arguments);

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
