using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
        ShaktoolChain chain = enemies.ShaktoolBehavior.ChainOf(center);

        foreach ((bool reversed, CompassDirection expectedOrientation) in new[] { new VerifyShaktoolOwnedStateCase(false, CompassDirection.Down), new VerifyShaktoolOwnedStateCase(true, CompassDirection.Up) })
        {
            chain.Motion = ShaktoolChainMotion.CurlingClockwise;
            centerState.OrbitAngle = RotationAngleConstants.FromRaw(0xff80);
            centerState.AngularVelocity = RotationAngle.FromSteps(1);
            centerState.ResetOrientation();
            centerState.OrientationReversed = reversed;
            states[4].OrbitAngle = RotationAngleConstants.FromRaw(0x8080);
            enemies.ShaktoolBehavior.AdvanceAndOrientShaktoolCenter(center, centerState);
            AssertEqual((ushort)0x0080, centerState.OrbitAngle.RawValue, "the centre's orbit wraps through zero");
            AssertEqual(expectedOrientation, centerState.Orientation, $"reversed={reversed} selects the expected head orientation");
            AssertEqual(reversed, centerState.OrientationReversed, "orienting keeps the chain's reversal");
        }

        chain.Motion = ShaktoolChainMotion.CurlingClockwise with { Phase = ShaktoolChainPhase.Straightening };
        centerState.ResetOrientation();
        RoomEnemyPlace tail = enemies.Places.InUpdateOrder[6];
        states[6].CurlProgress = RotationAngle.FromSteps(0x12, 0x34);
        enemies.ShaktoolBehavior.ReverseShaktoolAfterCollision(tail, states[6], tail.Position.Whole);
        AssertEqual(RotationAngle.Zero, states[6].CurlProgress, "a wall reversal empties the saw's curl progress");
        AssertTrue(centerState.OrientationReversed, "a first wall reversal swaps the chain's ends");
        AssertEqual(ShaktoolChainMotion.CurlingClockwise with { NextWall = ShaktoolWallResponse.ReverseOrbit }, chain.Motion, "the chain curls again and its next wall reverses its orbit");
        for (int index = 0; index < states.Length; index++)
            AssertEqual(ShaktoolSegmentMotion.Idle, states[index].Motion, $"segment {index} idles through the collision response");

        // The chain's death deletes the first seven slots, so a chain starting anywhere else is refused (#1500).
        var misplacedEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace misplaced = misplacedEnemies.Places.InUpdateOrder[1];
        misplaced.Bind(EnemyDefinitionId.Shaktool, misplaced.Definition);
        misplaced.Bind(misplaced.DefinitionId, default(RoomEnemyDefinition));
        misplaced.SetSpawnParameter2(EnemyDefinitionId.Shaktool, 0);
        AssertThrows<InvalidDataException>(() => misplacedEnemies.ShaktoolBehavior.InitializeShaktool(misplaced, misplaced.SpawnSettings<ShaktoolSpawnSettings>()),
            "a Shaktool chain starting after the first slot is refused");

        // The chain moves as one, so a segment placed moving otherwise than its leading saw is refused (#2452).
        var mismatchedEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 0; index < 2; index++)
        {
            RoomEnemyPlace place = mismatchedEnemies.Places.InUpdateOrder[index];
            place.Bind(EnemyDefinitionId.Shaktool, place.Definition);
            place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
            place.SetSpawnSettings(new ShaktoolSpawnSettings(
                index == 0 ? ShaktoolChainMotion.CurlingClockwise : ShaktoolChainMotion.CurlingClockwise with { OrbitDirection = RotationDirection.Counterclockwise },
                ShaktoolSegmentDefinitions.Segments[index]));
        }
        mismatchedEnemies.ShaktoolBehavior.InitializeShaktool(mismatchedEnemies.Places.InUpdateOrder[0], mismatchedEnemies.Places.InUpdateOrder[0].SpawnSettings<ShaktoolSpawnSettings>());
        AssertThrows<InvalidDataException>(() => mismatchedEnemies.ShaktoolBehavior.InitializeShaktool(mismatchedEnemies.Places.InUpdateOrder[1], mismatchedEnemies.Places.InUpdateOrder[1].SpawnSettings<ShaktoolSpawnSettings>()),
            "a Shaktool segment placed moving otherwise than its leading saw is refused");
        Console.WriteLine("Shaktool owned state: orbit wrap, reversed orientation, wall reversal, the first-slot chain and the one chain motion match.");
    }

    /// <summary>Initializes the seven Shaktool segments in slots zero through six.</summary>
    private static RoomEnemySystem CreateShaktoolGroup()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 0; index < 7; index++)
        {
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[index];
            place.Bind(EnemyDefinitionId.Shaktool, place.Definition);
            place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
            place.SetSpawnParameter2(EnemyDefinitionId.Shaktool, unchecked((ushort)(index * 2)));
            place.Position = place.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
            enemies.ShaktoolBehavior.InitializeShaktool(place, place.SpawnSettings<ShaktoolSpawnSettings>());
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
