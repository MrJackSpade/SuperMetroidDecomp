using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Evir;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Evir body's, arms' and projectile's typed animations.</summary>
internal static class EvirInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Evir animation checks.</summary>
    internal static void VerifyEvirInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// The four body and arms loops run on time; the normal projectile holds its flight pose; regeneration follows the
    /// body's installed facing, moves the mouth offset back eight times, plays the spit sound and returns the projectile
    /// to idle; the program oracle matches bank $A8.
    /// </summary>
    internal static void VerifyEvirInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        foreach (EvirSegment loop in new[] { EvirSegment.BodyFacingLeft, EvirSegment.ArmsFacingLeft, EvirSegment.BodyFacingRight, EvirSegment.ArmsFacingRight })
        {
            var enemies = NewEvirInstructionSystem();
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            PrepareEvirBodyLoop(place, loop);
            // The arms wave four times and rest folded before they loop (#2369): the check reads the animation's whole pass.
            Animation<EvirSegment> animation = EvirAnimations.StartingWith(loop);
            AssertEqual(EnemyInstructionRunner.ExpectedAnimationLoop(EvirAnimations.Segments, animation, EvirFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.AnimationLoopUpdates(EvirAnimations.Segments, animation)),
                $"Evir {loop} loop");
        }

        {
            var enemies = NewEvirInstructionSystem();
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            // The held pose runs no cue, so the spit's body is never read.
            EvirSpitState state = PrepareEvirSpit(place, EvirSegment.ProjectileNormal, body: place);
            _ = EnemyInstructionRunner.ShownFrames(place, 4);
            AssertEqual(EvirAnimationsConstants.Spritemaps(EvirSegment.ProjectileNormal).Single(), place.SpritemapPointer,
                "normal Evir projectile shows its flight pose");
            AnimationPlayback<EvirSegment> held = state.Playback;
            _ = EnemyInstructionRunner.ShownFrames(place, 4);
            AssertEqual(held, state.Playback, "normal Evir projectile holds its flight pose");
        }

        {
            var enemies = NewEvirInstructionSystem();
            // The regeneration cues read Evir.instList-$80,X: the body record two slots before the projectile, as the
            // retail population lays them out.
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[2];
            RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
            body.SetSpawnSettings(PlacedBody);
            body.Bind(EnemyDefinitionId.Evir, body.Definition);
            var bodyState = new EvirBodyState(spit: place) { Facing = HorizontalDirection.Left };
            body.SetSpeciesState(bodyState);
            // The spit finds its body as production places it, two places after the body.
            place.Bind(EnemyDefinitionId.EvirProjectile, place.Definition);
            enemies.EvirBehavior.InitializeEvirProjectile(place);
            EvirSpitState state = PrepareEvirSpit(place, EvirSegment.ProjectileRegenerating, body);
            // #1269: the projectile's own facing word disagrees with its body on purpose; $A8:879B/$87B6 follow the
            // body's installed list, so the offset starts at +8.
            state.Facing = HorizontalDirection.Right;
            state.Action = EvirSpitAction.Regenerating;
            enemies.Places.InUpdateOrder[0].SetSpeciesState(new EvirBodyState(spit: place) { Facing = HorizontalDirection.Left });
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
            for (int call = 0; call < 10; call++)
            {
                place.AnimationClock.StepOnNextUpdate();
                EnemyInstructionRunner.Process(arguments);
                if (call == 0)
                    AssertEqual(new Offset16<RoomPixels>(8, 0), state.RegenerationOffset,
                        "a left-facing body sets the +8 offset whatever the projectile's facing word");
            }
            AssertEqual(EvirAnimationsConstants.Spritemaps(EvirSegment.ProjectileNormal).Single(), place.SpritemapPointer,
                "regenerating Evir projectile ends on its flight pose");
            AssertEqual(Offset16<RoomPixels>.Zero, state.RegenerationOffset, "regeneration loop advances the projectile mouth offset eight times");
            AssertEqual(EvirSpitAction.Idle, state.Action, "regeneration finish cue restores idle AI, the spit held at the mouth");
            const ushort nativeSpitSound = 0x005e;
            AssertEqual((SoundEffectId?)SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, nativeSpitSound),
                enemies.Audio.LatestSound, "regeneration publishes the native library-two spit sound");
        }

        Suite(nameof(EvirAnimationsVerification.VerifyEvirAnimations), () => EvirAnimationsVerification.VerifyEvirAnimations(rom));
        Console.WriteLine(
            "Evir: the four body and arms loops, the held flight pose and the body-relative regeneration cues pass.");
    }

    /// <summary>Creates an enemy system bound to the supplied address space.</summary>
    internal static RoomEnemySystem NewEvirInstructionSystem()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        return enemies;
    }

    /// <summary>Seeds an Evir body place with its bank-$A8 definition and starting program, a body or arms loop.</summary>
    internal static EvirBodyState PrepareEvirBodyLoop(RoomEnemyPlace place, EvirSegment program)
    {
        Bind(place, EnemyDefinitionId.Evir);
        var state = new EvirBodyState(spit: place)
        {
            Playback = new(EvirAnimations.StartingWith(program), AnimationPlace.StartOfAnimation),
        };
        place.SetSpeciesState(state);
        return state;
    }

    /// <summary>
    /// Seeds an Evir spit place with its bank-$A8 definition and starting program, its body at <paramref name="body"/>.
    /// </summary>
    internal static EvirSpitState PrepareEvirSpit(RoomEnemyPlace place, EvirSegment program, RoomEnemyPlace body)
    {
        Bind(place, EnemyDefinitionId.EvirProjectile);
        var state = new EvirSpitState(EvirSpitAction.Idle, body)
        {
            Playback = new(EvirAnimations.StartingWith(program), AnimationPlace.StartOfAnimation),
        };
        place.SetSpeciesState(state);
        return state;
    }

    /// <summary>The settings an Evir body is placed with, which make the Evir definition its body rather than its arms (#2146).</summary>
    private static readonly EvirSpawnSettings PlacedBody =
        new(Part: EvirBodyPart.Body, Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(0), DirectionChangePeriod: 0);

    /// <summary>
    /// Binds <paramref name="place"/> to <paramref name="definition"/> with its bank-$A8 definition, its animation stepping
    /// on the next update; an Evir is placed as a body.
    /// </summary>
    private static void Bind(RoomEnemyPlace place, EnemyDefinitionId definition)
    {
        if (definition == EnemyDefinitionId.Evir)
            place.SetSpawnSettings(PlacedBody);
        place.Bind(definition, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs retail-cartridge Evir animation verification.</summary>
    [VerificationCommand("--evir-instruction-program-definitions")]
    internal static int EvirInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions());
        return 0;
    }
}
