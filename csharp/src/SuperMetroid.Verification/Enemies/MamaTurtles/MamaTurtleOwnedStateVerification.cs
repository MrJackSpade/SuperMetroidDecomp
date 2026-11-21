using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.MamaTurtles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Mama Turtle's owned 16.16 hover acceleration: the carried add, the speed cap and the wall reversal's carry quirk.</summary>
internal static class MamaTurtleOwnedStateVerification
{
    /// <summary>
    /// Confirms the hover add carries the fraction into the whole pixels and caps at three, a wall reversal negates
    /// the velocity exactly and the acceleration without its final increment (so negating -1 + 15/16 gives
    /// -1 + 1/16), and a Mama Turtle listed anywhere links the four babies listed after her.
    /// </summary>
    internal static void VerifyMamaTurtleOwnedState()
    {
        var state = new MamaTurtleEnemyState(MamaTurtleAction.Placed)
        {
            Velocity = new(SignedSixteenSixteen.FromParts(2, 0xf800), SignedSixteenSixteen.FromParts(1, 0x2000)),
            XAcceleration = SignedSixteenSixteen.FromParts(0, 0x1000),
        };
        MamaTurtleBehavior.AddAndClampMamaTurtleHorizontalAcceleration(state);
        AssertEqual(SignedSixteenSixteen.FromParts(3, 0x0800), state.Velocity.X, "the fraction carries and the whole speed caps at three");

        state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.FromParts(2, 0x8000));
        MamaTurtleBehavior.ReverseMamaTurtleAtWall(state);
        AssertEqual(SignedSixteenSixteen.FromParts(-3, 0x8000), state.Velocity.X, "a wall reversal negates 2.5 to -2.5");
        AssertEqual(SignedSixteenSixteen.FromParts(-1, 0xf000), state.XAcceleration, "a wall reversal negates a sixteenth exactly without a carry");

        state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.FromParts(-1, 0));
        MamaTurtleBehavior.ReverseMamaTurtleAtWall(state);
        AssertEqual(SignedSixteenSixteen.FromParts(0, 0), state.Velocity.X, "the carry-in wraps a -1 whole-pixel reversal to zero");
        AssertEqual(SignedSixteenSixteen.FromParts(-1, 0x1000), state.XAcceleration, "the carried reversal skips the acceleration's final increment");

        // Mama links the four babies her room lists right after her when she is placed, wherever she is listed (#1816), and
        // the last of them, placed after her as a room places them, completes the family (#2375): one listed second still
        // finds them and falls asleep on her first step.
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace mama = enemies.Places.InUpdateOrder[1];
        mama.Bind(EnemyDefinitionId.MamaTurtle, mama.KeptDefinition);
        mama.SetUpAsPlaced();
        for (int place = 2; place < 6; place++)
        {
            RoomEnemyPlace baby = enemies.Places.InUpdateOrder[place];
            baby.Bind(EnemyDefinitionId.BabyTurtle, baby.KeptDefinition);
            baby.SetSpawnSettings(new BabyTurtleSpawnSettings(Heading: HorizontalDirection.Right));
            baby.SetUpAsPlaced();
        }
        var samus = new SuperMetroid.Core.Samus.SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        enemies.MamaTurtleBehavior.RunMamaTurtleMainAi(mama.HeldOccupant(), mama.RequireSpeciesState<MamaTurtleEnemyState>(), new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(16, 16)), 0) with { Samus = samus },
            new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero)));
        AssertEqual(MamaTurtleAction.Asleep, mama.RequireSpeciesState<MamaTurtleEnemyState>().Action,
            "a Mama listed second links the four babies listed after her and falls asleep");
        Console.WriteLine("Mama Turtle owned state: the hover add, cap, wall reversal quirk and her babies' links match.");
    }

    /// <summary>Runs the focused Mama Turtle owned-state check.</summary>
    [VerificationCommand("--mama-turtle-owned-state")]
    internal static int MamaTurtleOwnedStateCommand()
    {
        VerifyMamaTurtleOwnedState();
        return 0;
    }
}
