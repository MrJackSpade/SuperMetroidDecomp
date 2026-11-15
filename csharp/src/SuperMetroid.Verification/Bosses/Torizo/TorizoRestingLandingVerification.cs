using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks the Torizo's resting fall (#1945): the cartridge marked a resting Torizo by a fall speed of exactly one pixel,
/// which kept its landing quake from firing again while it stood. The port keeps the rest as typed state; a falling Torizo
/// meeting the floor quakes once and rests, a resting one meeting it does nothing, and a leap's landing and a walk off a
/// ledge leave it resting until its vertical speed changes.
/// </summary>
internal static class TorizoRestingLandingVerification
{
    /// <summary>Torizo's collision radii, as Bomb Torizo's setup gives them.</summary>
    private static readonly Extent16<RoomPixels> Radius = new(18, 48);

    /// <summary>Where each case's Torizo starts: in the air above the floor.</summary>
    private static readonly Position16<RoomPixels> AboveFloor = new(128, 60);

    /// <summary>Drives the gravity, leap and walk motions over a floor and checks when the landing quake starts.</summary>
    internal static void VerifyTorizoRestingLanding()
    {
        // Open air with a floor at block row 12.
        RoomLevelData floor = CreateSolidRowsRoom(new(16, 16), firstSolidRow: 12, solidRowCount: 1);

        // A falling Torizo lands once: the quake starts on the update it meets the floor, and it then rests.
        (RoomEnemySystem falling, RoomEnemySlot slot, TorizoEnemyState state) = NewTorizo(AboveFloor);
        state.Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 1024);
        int updates = 0;
        while (falling.Earthquake.RemainingFrames == 0)
        {
            AssertTrue(updates < 64, "the falling Torizo reaches the floor");
            AssertTrue(!state.FallsAtRestingSpeed, $"update {updates}: the falling Torizo does not rest before it lands");
            falling.TorizoBehavior.Movement.ApplyTorizoGravity(slot, state, floor);
            updates++;
        }
        AssertEqual(TorizoMotionDefinitions.LandingQuakeUpdates, falling.Earthquake.RemainingFrames, "the landing starts the quake");
        AssertTrue(state.FallsAtRestingSpeed, "the landed Torizo rests");
        AssertEqual(TorizoMotionDefinitions.RestingFallSpeed, state.Velocity.Y, "the landed Torizo falls at the resting speed");

        // A resting Torizo on the floor meets it without a quake, update after update.
        (RoomEnemySystem resting, RoomEnemySlot restingSlot, TorizoEnemyState restingState) = NewTorizo(slot.Position.Whole);
        restingState.FallAtRestingSpeed();
        for (int update = 0; update < 3; update++)
            resting.TorizoBehavior.Movement.ApplyTorizoGravity(restingSlot, restingState, floor);
        AssertEqual(0, resting.Earthquake.RemainingFrames, "a resting Torizo on the floor starts no quake");
        AssertTrue(restingState.FallsAtRestingSpeed, "a resting Torizo on the floor keeps resting");

        // Gravity in the air ends the rest, so the next meeting with the floor is a landing.
        (RoomEnemySystem airborne, RoomEnemySlot airborneSlot, TorizoEnemyState airborneState) = NewTorizo(AboveFloor);
        airborneState.FallAtRestingSpeed();
        airborne.TorizoBehavior.Movement.ApplyTorizoGravity(airborneSlot, airborneState, floor);
        AssertTrue(!airborneState.FallsAtRestingSpeed, "gravity ends a resting Torizo's rest in the air");
        AssertEqual(TorizoMotionDefinitions.RestingFallSpeed + TorizoMotionDefinitions.StatueGravity, airborneState.Velocity.Y,
            "the resting Torizo in the air gains gravity");

        // A sideways change keeps the rest; a vertical one ends it.
        restingState.Velocity = restingState.Velocity.WithX(SignedEightEight.FromPixels(2));
        AssertTrue(restingState.FallsAtRestingSpeed, "a sideways speed change keeps the rest");
        restingState.Velocity = restingState.Velocity.WithY(TorizoMotionDefinitions.BackwardJumpRise);
        AssertTrue(!restingState.FallsAtRestingSpeed, "a jump's rise ends the rest");

        // A leap's landing quakes and rests the Torizo.
        (RoomEnemySystem leaping, RoomEnemySlot leapingSlot, TorizoEnemyState leapingState) = NewTorizo(AboveFloor);
        leapingState.Motion = TorizoBodyMotion.Leaping;
        leapingState.ReturnPlayback = TorizoAnimations.Start(TorizoAnimation.FacingLeftLanding);
        leapingState.Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 1024);
        leapingState.JumpAcceleration = new(SignedEightEight.Zero, TorizoMotionDefinitions.LeapGravity);
        updates = 0;
        while (leaping.Earthquake.RemainingFrames == 0)
        {
            AssertTrue(updates < 64, "the leaping Torizo reaches the floor");
            leaping.TorizoBehavior.Movement.MoveBody(leapingSlot, leapingState, floor);
            updates++;
        }
        AssertTrue(leapingState.FallsAtRestingSpeed, "a leap's landing rests the Torizo");

        // A walk off a ledge starts the fall at the resting speed with no sideways speed.
        (RoomEnemySystem walking, RoomEnemySlot walkingSlot, TorizoEnemyState walkingState) = NewTorizo(AboveFloor);
        walkingState.Motion = TorizoBodyMotion.Walking;
        walkingState.Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-256, 1024);
        walking.TorizoBehavior.Movement.MoveBody(walkingSlot, walkingState, CreateEmptyRoom(new(16, 16)));
        AssertTrue(walkingState.FallsAtRestingSpeed, "a walk off a ledge starts the fall resting");
        AssertEqual(EightEightVector<RoomPixels>.Zero.WithY(TorizoMotionDefinitions.RestingFallSpeed), walkingState.Velocity,
            "a walk off a ledge starts the fall at the resting speed");

        Console.WriteLine($"Torizo resting landing: a fall lands once with a {TorizoMotionDefinitions.LandingQuakeUpdates}-update quake and rests on the floor without another; leaps and walks rest it until its vertical speed changes.");
    }

    /// <summary>A Torizo of a fresh room's enemies at <paramref name="at"/>, facing left, with Bomb Torizo's radii.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, TorizoEnemyState State) NewTorizo(Position16<RoomPixels> at)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = FixedPosition<RoomPixels>.FromWhole(at);
        slot.Radius = Radius;
        var state = new TorizoEnemyState(isGolden: false);
        slot.SetSpeciesState(state);
        return (enemies, slot, state);
    }

    /// <summary>Runs the Torizo resting landing check.</summary>
    [VerificationCommand("--torizo-resting-landing")]
    internal static int TorizoRestingLandingCommand()
    {
        Suite(nameof(VerifyTorizoRestingLanding), VerifyTorizoRestingLanding);
        return 0;
    }
}
