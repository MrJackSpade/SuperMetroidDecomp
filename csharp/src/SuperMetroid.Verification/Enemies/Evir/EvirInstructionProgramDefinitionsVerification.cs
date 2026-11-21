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
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[BodyPlace];
            PrepareEvirBodyLoop(enemies, loop);
            // The arms wave four times and rest folded before they loop (#2369): the check reads the animation's whole pass.
            Animation<EvirSegment> animation = EvirAnimations.StartingWith(loop);
            AssertEqual(EnemyInstructionRunner.ExpectedAnimationLoop(EvirAnimations.Segments, animation, EvirFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.AnimationLoopUpdates(EvirAnimations.Segments, animation)),
                $"Evir {loop} loop");
        }

        {
            var enemies = NewEvirInstructionSystem();
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[SpitPlace];
            EvirSpitState state = PrepareEvirSpit(enemies, EvirSegment.ProjectileIdle);
            _ = EnemyInstructionRunner.ShownFrames(place, 4);
            AssertEqual(EvirAnimationsConstants.Spritemaps(EvirSegment.ProjectileIdle).Single(), place.SpritemapPointer,
                "normal Evir projectile shows its flight pose");
            AnimationPlayback<EvirSegment> held = state.Playback;
            _ = EnemyInstructionRunner.ShownFrames(place, 4);
            AssertEqual(held, state.Playback, "normal Evir projectile holds its flight pose");
        }

        {
            var enemies = NewEvirInstructionSystem();
            // The regeneration cues read Evir.instList-$80,X: the body record two slots before the projectile, as the
            // retail population lays them out.
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[SpitPlace];
            // The spit finds its body as production places it, two places before the spit.
            EvirSpitState state = PrepareEvirSpit(enemies, EvirSegment.ProjectileRegenerating);
            // #1269: the projectile's own facing word disagrees with its body on purpose; $A8:879B/$87B6 follow the
            // body's installed list, so the offset starts at +8.
            state.Facing = HorizontalDirection.Right;
            state.Action = EvirSpitAction.Regenerating;
            enemies.Places.InUpdateOrder[BodyPlace].RequireSpeciesState<EvirBodyState>().Facing = HorizontalDirection.Left;
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
            for (int call = 0; call < 10; call++)
            {
                place.AnimationClock.StepOnNextUpdate();
                EnemyInstructionRunner.Process(arguments);
                if (call == 0)
                    AssertEqual(new Offset16<RoomPixels>(8, 0), state.RegenerationOffset,
                        "a left-facing body sets the +8 offset whatever the projectile's facing word");
            }
            AssertEqual(EvirAnimationsConstants.Spritemaps(EvirSegment.ProjectileIdle).Single(), place.SpritemapPointer,
                "regenerating Evir projectile ends on its flight pose");
            AssertEqual(Offset16<RoomPixels>.Zero, state.RegenerationOffset, "regeneration loop advances the projectile mouth offset eight times");
            AssertEqual(EvirSpitAction.Idle, state.Action, "regeneration finish cue restores idle AI, the spit held at the mouth");
            const ushort nativeSpitSound = 0x005e;
            AssertEqual((SoundEffectId?)SoundEffectCommands.DecodeCartridge(SoundEffectLibrary.Enemies, nativeSpitSound),
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

    /// <summary>The place an Evir's body is placed in: the first, as the checks place it.</summary>
    internal const int BodyPlace = 0;

    /// <summary>The place an Evir's spit is placed in: two after its body, its arms between them.</summary>
    private const int SpitPlace = 2;

    /// <summary>
    /// Places an Evir in the first places as its room's population places it, and starts its body on
    /// <paramref name="program"/>, a body or arms loop, stepping on the next update.
    /// </summary>
    internal static EvirBodyState PrepareEvirBodyLoop(RoomEnemySystem enemies, EvirSegment program)
    {
        PlaceEvir(enemies);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[BodyPlace];
        EvirBodyState state = place.RequireSpeciesState<EvirBodyState>();
        state.Playback = new(EvirAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
        return state;
    }

    /// <summary>
    /// Places an Evir in the first places as its room's population places it, and starts its spit on
    /// <paramref name="program"/>, stepping on the next update.
    /// </summary>
    internal static EvirSpitState PrepareEvirSpit(RoomEnemySystem enemies, EvirSegment program)
    {
        PlaceEvir(enemies);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[SpitPlace];
        EvirSpitState state = place.RequireSpeciesState<EvirSpitState>();
        state.Playback = new(EvirAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
        return state;
    }

    /// <summary>
    /// Places an Evir as its room's population places it: its body, then its arms, then its spit, each placed and set up
    /// in turn, the arms and spit finding the body listed before them.
    /// </summary>
    private static void PlaceEvir(RoomEnemySystem enemies)
    {
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[BodyPlace];
        body.BindPlaced(EnemyDefinitionId.Evir, PlacedBody);
        body.SetUpAsPlaced();
        RoomEnemyPlace arms = enemies.Places.InUpdateOrder[BodyPlace + 1];
        arms.BindPlaced(EnemyDefinitionId.Evir, PlacedBody with { Part = EvirBodyPart.Arms });
        arms.SetUpAsPlaced();
        RoomEnemyPlace spit = enemies.Places.InUpdateOrder[SpitPlace];
        spit.Bind(EnemyDefinitionId.EvirProjectile, spit.KeptDefinition);
        spit.SetUpAsPlaced();
    }

    /// <summary>
    /// The settings an Evir body is placed with, which make the Evir definition its body rather than its arms (#2146), bobbing
    /// at the slower of the speeds the game places, whose first half-cycle its arms lend it.
    /// </summary>
    private static readonly EvirSpawnSettings PlacedBody =
        new(Part: EvirBodyPart.Body, Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), DirectionChangePeriod: 0);

    /// <summary>Runs retail-cartridge Evir animation verification.</summary>
    [VerificationCommand("--evir-instruction-program-definitions")]
    internal static int EvirInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions());
        return 0;
    }
}
