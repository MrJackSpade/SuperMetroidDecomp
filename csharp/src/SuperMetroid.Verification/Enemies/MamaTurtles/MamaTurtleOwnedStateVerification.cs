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
    /// -1 + 1/16), and a Mama Turtle is refused anywhere but the first place.
    /// </summary>
    internal static void VerifyMamaTurtleOwnedState()
    {
        var state = new MamaTurtleEnemyState
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

        // Her fall and contact box read the first slot's values, so a Mama anywhere else is refused (#1500).
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot misplaced = enemies.SlotTable.Slots[1];
        misplaced.Bind(EnemyDefinitionId.MamaTurtle, misplaced.Definition);
        AssertThrows<InvalidDataException>(() => enemies.MamaTurtleBehavior.InitializeMamaTurtle(misplaced),
            "a Mama Turtle after the first slot is refused");
        Console.WriteLine("Mama Turtle owned state: the hover add, cap, wall reversal quirk and first-slot placement match.");
    }

    /// <summary>Runs the focused Mama Turtle owned-state check.</summary>
    [VerificationCommand("--mama-turtle-owned-state")]
    internal static int MamaTurtleOwnedStateCommand()
    {
        VerifyMamaTurtleOwnedState();
        return 0;
    }
}
