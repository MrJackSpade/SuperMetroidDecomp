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
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Bull, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        enemies.BullBehavior.InitializeBull(slot);
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException("Bull initializer did not publish typed state.");
        AssertEqual(new AnimationPlayback<BullSegment>(BullAnimations.StartingWith(BullSegment.Normal), AnimationPlace.StartOfAnimation), state.Playback, "Bull initializer animation");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BullAnimations.Segments, BullSegment.Normal, BullFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(BullAnimations.Segments, BullSegment.Normal)),
            "Bull normal loop");

        BullBehavior.ResolveBullImmuneShot(slot, state, SamusProjectileDirection.Right);
        AssertEqual(new AnimationPlayback<BullSegment>(BullAnimations.StartingWith(BullSegment.Shot), AnimationPlace.StartOfAnimation), state.Playback, "Bull immune-shot animation");
        ushort[] shot = BullAnimationsConstants.Spritemaps(BullSegment.ShotLoop).ToArray();
        ushort[] expected = [.. Enumerable.Repeat(shot, 5).SelectMany(frames => frames), BullAnimationsConstants.Spritemaps(BullSegment.Normal).First()];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(expected, Enumerable.Repeat(3, expected.Length)),
            EnemyInstructionRunner.ShownFrames(slot, 61), "Bull shot program repeats five times and returns to normal");
        Console.WriteLine("  Bull behavior: the normal loop and the five-repeat immune-shot reaction run as the cartridge does.");
    }

    /// <summary>
    /// #2065: a Bull's speed stays within 0 through 2880 256ths of a pixel an update, so its moves read it as an ordinary
    /// speed. A flight from rest under the fastest cap, 2815 256ths, accelerates while below it and overshoots once, to
    /// 2880; braking runs the acceleration out with 360 256ths left. An immune shot's kick (six pixels an update, braked
    /// from a pixel) runs the acceleration below zero with 304 256ths left; the same kick while the Bull waits, with no
    /// braking change, brakes onto exactly zero. The speed is never below zero before a decelerating update stops the
    /// flight.
    /// </summary>
    internal static void VerifyBullSpeedStaysInFlightRange()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Bull, slot.Definition);
        slot.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        enemies.BullBehavior.InitializeBull(slot);
        BullEnemyState state = slot.RequireSpeciesState<BullEnemyState>();
        SignedEightEight brakingStep = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

        state.MaxSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(2815);
        state.AccelerationDelta = brakingStep;
        int accelerations = 0;
        while (state.IsBelowMaximumSpeed)
        {
            state.Accelerate();
            accelerations++;
        }
        AssertEqual(15, accelerations, "a flight under the fastest cap accelerates fifteen times");
        AssertEqual(2880, state.Speed.InTwoHundredFiftySixthsOfPixel, "the last acceleration overshoots the fastest cap to 2880 256ths");
        int Brake()
        {
            while (!state.HasStopped)
            {
                AssertTrue(state.Speed > SignedEightEight.Zero, "a braking Bull flies at a speed above zero");
                state.Decelerate();
            }
            AssertTrue(state.Speed >= SignedEightEight.Zero, "a Bull stops at a speed of zero or more");
            return state.Speed.InTwoHundredFiftySixthsOfPixel;
        }
        AssertEqual(360, Brake(), "braking a flight runs its acceleration out with 360 256ths left");
        AssertTrue(state.Acceleration.IsZero, "braking a flight runs its acceleration out exactly");

        BullBehavior.ResolveBullImmuneShot(slot, state, SamusProjectileDirection.Right);
        AssertEqual(1536, state.Speed.InTwoHundredFiftySixthsOfPixel, "a kick throws the Bull at six pixels an update");
        state.AccelerationDelta = brakingStep;
        AssertEqual(304, Brake(), "a braked kick runs its acceleration below zero with 304 256ths left");
        AssertEqual(-8, state.Acceleration.InTwoHundredFiftySixthsOfPixel, "a braked kick's acceleration ends below zero");

        state.ShotReactionDisabled = false;
        BullBehavior.ResolveBullImmuneShot(slot, state, SamusProjectileDirection.Right);
        state.AccelerationDelta = SignedEightEight.Zero;
        AssertEqual(0, Brake(), "a kick while the Bull waits brakes a pixel an interval onto exactly zero");
        Console.WriteLine("  Bull speed: flights and kicks keep the speed within 0 through 2880 256ths of a pixel an update.");
    }

    /// <summary>Runs the Bull behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--bull-instruction-mechanics")]
    internal static int BullInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBullBehavior), () => VerifyBullBehavior());
        Suite(nameof(VerifyBullSpeedStaysInFlightRange), () => VerifyBullSpeedStaysInFlightRange());
        Suite(nameof(BullAnimationsVerification.VerifyBullAnimations), () => BullAnimationsVerification.VerifyBullAnimations(rom));
        return 0;
    }
}
