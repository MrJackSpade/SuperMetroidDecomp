using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Checks Bull's normal loop and its immune-shot reaction animation.</summary>
internal static class BullBehaviorVerification
{
    /// <summary>
    /// The initializer starts the normal loop (four frames, ten updates apiece). An immune shot starts the shot program,
    /// which plays its four three-update frames five times on the native repeat timer and then returns to the normal loop.
    /// </summary>
    internal static void VerifyBullBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Bull, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        enemies.BullBehavior.InitializeBull(place);
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException("Bull initializer did not publish typed state.");
        AssertEqual(new AnimationPlayback<BullSegment>(BullAnimations.StartingWith(BullSegment.Normal), AnimationPlace.StartOfAnimation), state.Playback, "Bull initializer animation");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BullAnimations.Segments, BullSegment.Normal, BullFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(BullAnimations.Segments, BullSegment.Normal)),
            "Bull normal loop");

        BullBehavior.ResolveBullImmuneShot(place, state, SamusProjectileDirection.Right);
        AssertEqual(new AnimationPlayback<BullSegment>(BullAnimations.StartingWith(BullSegment.Shot), AnimationPlace.StartOfAnimation), state.Playback, "Bull immune-shot animation");
        ushort[] shot = BullAnimationsConstants.Spritemaps(BullSegment.ShotLoop).ToArray();
        ushort[] expected = [.. Enumerable.Repeat(shot, 5).SelectMany(frames => frames), BullAnimationsConstants.Spritemaps(BullSegment.Normal).First()];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(expected, Enumerable.Repeat(3, expected.Length)),
            EnemyInstructionRunner.ShownFrames(place, 61), "Bull shot program repeats five times and returns to normal");
        Console.WriteLine("  Bull behavior: the normal loop and the five-repeat immune-shot reaction run as the cartridge does.");
    }

    /// <summary>
    /// A deceleration that overshoots: speed 10 256ths shrinks by the acceleration 24 to 14 256ths below zero, which the
    /// state keeps; the next decelerating update reads that speed as below zero and stops the flight, clearing speed,
    /// acceleration and its delta and returning to the movement delay. Both updates run the production main AI.
    /// </summary>
    internal static void VerifyBullDecelerationOvershoot()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Bull, place.Definition);
        place.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        enemies.BullBehavior.InitializeBull(place);
        BullEnemyState state = place.RequireSpeciesState<BullEnemyState>();
        state.Action = BullAction.Decelerating;
        state.Speed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(10);
        state.Acceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(48);
        state.AccelerationDelta = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);
        state.AccelerationIntervalTimer = 1;

        BullBehavior.RunBullMain(place, state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(24, state.Acceleration.InTwoHundredFiftySixthsOfPixel, "the expiring interval shrinks the acceleration by its delta");
        AssertEqual(-14, state.Speed.InTwoHundredFiftySixthsOfPixel, "the overshooting deceleration leaves the speed below zero");
        AssertEqual(BullAction.Decelerating, state.Action, "the overshoot is read on the following update");

        BullBehavior.RunBullMain(place, state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(BullAction.MovementDelay, state.Action, "a speed below zero stops the flight");
        AssertTrue(state.Speed.IsZero, "stopping clears the speed");
        AssertTrue(state.Acceleration.IsZero, "stopping clears the acceleration");
        AssertTrue(state.AccelerationDelta.IsZero, "stopping clears the acceleration delta");
        Console.WriteLine("  Bull deceleration: an overshoot keeps the speed below zero and stops the flight on the next update.");
    }

    /// <summary>Runs the Bull behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--bull-instruction-mechanics")]
    internal static int BullInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBullBehavior), () => VerifyBullBehavior());
        Suite(nameof(VerifyBullDecelerationOvershoot), () => VerifyBullDecelerationOvershoot());
        Suite(nameof(BullAnimationsVerification.VerifyBullAnimations), () => BullAnimationsVerification.VerifyBullAnimations(rom));
        return 0;
    }
}
