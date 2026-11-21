using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay;

/// <summary>Checks the Shaktool segment state the chain owns: wrapping orbit angles, and the centre's orientation split from the saw's curl.</summary>
internal static class ShaktoolOwnedStateVerification
{
    /// <summary>
    /// On the real seven-segment group: the centre's orbit wraps through zero, a reversed chain flips the centre's
    /// midpoint a half turn, a first wall reversal empties the saw's curl progress, stops the chain straightening, marks it
    /// reversed once and toggles the centre's reversal, and a chain is refused anywhere but the first place or with
    /// segments placed moving differently.
    /// </summary>
    internal static void VerifyShaktoolOwnedState()
    {
        RoomEnemySystem enemies = CreateShaktoolGroup();
        ShaktoolSegmentState[] states = [.. Enumerable.Range(0, 7).Select(index =>
            enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException($"Shaktool segment {index} has no state."))];
        RoomEnemyPlace center = enemies.Places.InUpdateOrder[3];
        ShaktoolSegmentState centerState = states[3];
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();

        // The chain curls clockwise as it was placed; the centre's orbit never changes how the chain moves.
        foreach ((bool reversed, CompassDirection expectedOrientation) in new[] { new VerifyShaktoolOwnedStateCase(false, CompassDirection.Down), new VerifyShaktoolOwnedStateCase(true, CompassDirection.Up) })
        {
            centerState.OrbitAngle = RotationAngleConstants.FromRaw(0xff80);
            centerState.AngularVelocity = RotationAngle.FromSteps(1);
            centerState.ResetOrientation();
            centerState.OrientationReversed = reversed;
            states[4].OrbitAngle = RotationAngleConstants.FromRaw(0x8080);
            center.RunMainAi(samus);
            AssertEqual((ushort)0x0080, centerState.OrbitAngle.RawValue, "the centre's orbit wraps through zero");
            AssertEqual(expectedOrientation, centerState.Orientation, $"reversed={reversed} selects the expected head orientation");
            AssertEqual(reversed, centerState.OrientationReversed, "orienting keeps the chain's reversal");
        }

        centerState.ResetOrientation();
        RoomEnemyPlace tail = enemies.Places.InUpdateOrder[6];
        states[6].CurlProgress = RotationAngle.FromSteps(0x12, 0x34);
        // The trailing saw's move meets a wall, which reverses the chain.
        ShaktoolInstructionDefinitionsVerification.DriveTailIntoWall(tail);
        AssertEqual(RotationAngle.Zero, states[6].CurlProgress, "a wall reversal empties the saw's curl progress");
        AssertTrue(centerState.OrientationReversed, "a first wall reversal swaps the chain's ends");
        for (int index = 0; index < states.Length; index++)
            AssertEqual(ShaktoolSegmentMotion.Idle, states[index].Motion, $"segment {index} idles through the collision response");

        // Once the head bob ends each segment takes its course again; the chain then curls on, and its next wall reverses
        // its orbit instead of turning it end for end again, so the centre keeps its swapped ends and orbits the other way.
        for (int index = 0; index < states.Length; index++)
            states[index].Motion = ShaktoolSegmentDefinitions.Segments[index].Motion;
        ShaktoolInstructionDefinitionsVerification.DriveTailIntoWall(tail);
        AssertTrue(centerState.OrientationReversed, "the chain's second wall reverses its orbit and leaves its ends as they are");
        for (int index = 0; index < states.Length; index++)
            states[index].Motion = ShaktoolSegmentDefinitions.Segments[index].Motion;
        RotationAngle beforeOrbit = centerState.OrbitAngle;
        centerState.AngularVelocity = RotationAngle.FromSteps(1);
        center.RunMainAi(samus);
        AssertEqual(beforeOrbit - RotationAngle.FromSteps(1), centerState.OrbitAngle, "the reversed chain's centre orbits counterclockwise");

        // The chain's death deletes the first seven slots, so a chain starting anywhere else is refused (#1500).
        var misplacedEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace misplaced = misplacedEnemies.Places.InUpdateOrder[1];
        misplaced.Bind(EnemyDefinitionId.Shaktool, misplaced.KeptDefinition);
        misplaced.Bind(misplaced.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        misplaced.SetSpawnParameter2(EnemyDefinitionId.Shaktool, 0);
        AssertThrows<InvalidDataException>(() => misplaced.SetUpAsPlaced(),
            "a Shaktool chain starting after the first slot is refused");

        // The chain moves as one, so a segment placed moving otherwise than its leading saw is refused (#2452).
        var mismatchedEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 0; index < 2; index++)
        {
            RoomEnemyPlace place = mismatchedEnemies.Places.InUpdateOrder[index];
            place.Bind(EnemyDefinitionId.Shaktool, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            place.SetSpawnSettings(new ShaktoolSpawnSettings(
                index == 0 ? ShaktoolChainMotion.CurlingClockwise : ShaktoolChainMotion.CurlingClockwise with { OrbitDirection = RotationDirection.Counterclockwise },
                ShaktoolSegmentDefinitions.Segments[index]));
        }
        mismatchedEnemies.Places.InUpdateOrder[0].SetUpAsPlaced();
        AssertThrows<InvalidDataException>(() => mismatchedEnemies.Places.InUpdateOrder[1].SetUpAsPlaced(),
            "a Shaktool segment placed moving otherwise than its leading saw is refused");
        Console.WriteLine("Shaktool owned state: orbit wrap, reversed orientation, wall reversal, the first-slot chain and the one chain motion match.");
    }

    /// <summary>
    /// #2554: a recorded chain target reaches the chain through its wall, as the game sets one. The import of a nonzero
    /// target reverses the chain (every segment stops for its head bob), and the target it leaves is the one the trailing
    /// saw's curl then reaches: an orbit turning onto it puts every segment at it. A zero target leaves the placed chain as
    /// it is, and a target between steps, which no wall leaves, is refused.
    /// </summary>
    internal static void VerifyShaktoolImportedTarget()
    {
        RoomEnemySystem untouched = CreateShaktoolGroup();
        NativeEnemyFamilyStateImport.ImportShaktoolTarget(untouched, 0);
        AssertEqual(ShaktoolSegmentDefinitions.TrailingSaw.Motion, untouched.ShaktoolSegments[6]?.Motion, "a zero target leaves the placed chain moving");
        AssertThrows<InvalidDataException>(() => NativeEnemyFamilyStateImport.ImportShaktoolTarget(CreateShaktoolGroup(), 0x4080),
            "a target between steps is refused");

        RotationAngle target = RotationAngle.FromSteps(0x60);
        RoomEnemySystem enemies = CreateShaktoolGroup();
        NativeEnemyFamilyStateImport.ImportShaktoolTarget(enemies, target.RawValue);
        ShaktoolSegmentState[] states = [.. Enumerable.Range(0, 7).Select(index =>
            enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException($"Shaktool segment {index} has no state."))];
        for (int index = 0; index < states.Length; index++)
            AssertEqual(ShaktoolSegmentMotion.Idle, states[index].Motion, $"segment {index} stops for the wall that sets the target");

        // The head bob over, the chain curls on clockwise; the trailing saw's turn onto the target puts the chain at it.
        for (int index = 0; index < states.Length; index++)
            states[index].Motion = ShaktoolSegmentDefinitions.Segments[index].Motion;
        states[6].AngularVelocity = RotationAngle.FromSteps(1);
        states[6].OrbitAngle = target - RotationAngle.FromSteps(2);
        RoomEnemyPlace tail = enemies.Places.InUpdateOrder[6];
        // The fixture's blank definition gives the saw no body; its move through the open room tests the rows a body spans.
        tail.Radius = new(8, 8);
        tail.RunMainAi(RoomEnemyFrameFixtures.StandaloneSamus());
        AssertTrue(states.Take(6).All(state => state.OrbitAngle != target), "a turn short of the target leaves the chain where it was");
        tail.RunMainAi(RoomEnemyFrameFixtures.StandaloneSamus());
        for (int index = 0; index < states.Length; index++)
            AssertEqual(target, states[index].OrbitAngle, $"segment {index} stands at the imported target once the trailing saw reaches it");
        Console.WriteLine("Shaktool imported target: reached through the chain's wall, and the curl reaches it.");
    }

    /// <summary>Initializes the seven Shaktool segments in slots zero through six.</summary>
    private static RoomEnemySystem CreateShaktoolGroup()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 0; index < 7; index++)
        {
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[index];
            place.Bind(EnemyDefinitionId.Shaktool, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // The chain is placed curling clockwise, as its population entry names how it starts moving.
            place.SetSpawnSettings(new ShaktoolSpawnSettings(ShaktoolChainMotion.CurlingClockwise, ShaktoolSegmentDefinitions.Segments[index]));
            place.Position = place.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
            place.SetUpAsPlaced();
        }
        return enemies;
    }

    /// <summary>Runs the focused Shaktool owned-state check.</summary>
    [VerificationCommand("--shaktool-owned-state")]
    internal static int ShaktoolOwnedStateCommand()
    {
        VerifyShaktoolOwnedState();
        return 0;
    }
}
