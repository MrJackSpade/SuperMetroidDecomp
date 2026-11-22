using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.World;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>A Powamp placed as its room places it, and its death as the game runs it (#2553).</summary>
internal static class PowampPairFixtures
{
    /// <summary>
    /// Places a Powamp's balloon in the room's first place and its body in the second, each as its room's settings name
    /// the part it is (#2163), and initializes each as its room's load does: the balloon deflated, the body resting on its
    /// slow pulse, joined to the balloon listed before it.
    /// </summary>
    /// <returns>The balloon's place, the body's and the state the body's setup built.</returns>
    internal static PowampPair PlacePair(RoomEnemySystem enemies)
    {
        RoomEnemyPlace balloon = enemies.Places.InUpdateOrder[0];
        balloon.SetSpawnParameter1(EnemyDefinitionId.Powamp, 1);
        balloon.Bind(EnemyDefinitionId.Powamp, balloon.KeptDefinition);
        balloon.Bind(balloon.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        // The balloon is set up as its room sets it up, so the body that follows it finds it.
        _ = enemies.LoadAuthoredPopulation();
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[1];
        body.BindPlaced(EnemyDefinitionId.Powamp, new PowampBodySpawnSettings(PowampContact.Accepts));
        body.Bind(EnemyDefinitionId.Powamp, body.KeptDefinition);
        body.Bind(body.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        // The body's setup builds the state its main AI and its animation run on.
        return new PowampPair(balloon, body, PowampBehavior.InitializePowampBody(body));
    }

    /// <summary>
    /// Kills the Powamp whose body stands in <paramref name="pair"/>'s body place as a lethal shot does, emptying its health and
    /// starting its fatal action, and runs its AI in an open room until its death delay runs out and it leaves its place:
    /// its balloon goes, its spikes burst out and it dies in the explosion its burst selects.
    /// </summary>
    internal static void ShootDeadAndRunDeathDelay(RoomEnemySystem enemies, PowampPair pair)
    {
        RoomEnemyPlace body = Ensure.NotNull(pair).Body;
        PowampBodyState state = pair.BodyState;
        PowampBalloonEntity balloon = enemies.PowampBehavior.PlacedBalloon(body);
        RoomEnemyOccupant occupant = body.HeldOccupant();
        body.Health = 0;
        state.Action = PowampAction.FatalDamage;
        var level = RoomEnemyFrameFixtures.OpenRoom(new(16, 16));
        var samus = RoomEnemyFrameFixtures.StandaloneSamus();
        // The fatal action's update starts the delay, which then counts its updates down to the death.
        for (int update = 0; body.Occupant is not null; update++)
        {
            AssertTrue(update <= PowampMotionDefinitions.DeathDelay + 1, "the Powamp dies once its death delay runs out");
            enemies.PowampBehavior.RunPowampMain(occupant, body, state, balloon, level, samus);
        }
    }
}

/// <summary>A Powamp placed by <see cref="PowampPairFixtures.PlacePair"/>.</summary>
/// <param name="Balloon">The balloon's place.</param>
/// <param name="Body">The body's place.</param>
/// <param name="BodyState">The state the body's setup built, which its main AI and its animation run on.</param>
internal sealed record PowampPair(RoomEnemyPlace Balloon, RoomEnemyPlace Body, PowampBodyState BodyState);
