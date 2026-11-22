using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Choot;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Choot state the species owns: the ascent index's apex underflow.</summary>
internal static class ChootOwnedStateVerification
{
    /// <summary>
    /// Initializes a two-loop Choot and confirms its ascent starts at an even record index, the jump reaches its apex
    /// on the update that takes the index below zero (the cartridge's signed underflow of its 8.8 cursor).
    /// </summary>
    internal static void VerifyChootOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Choot, place.KeptDefinition);
        place.Position = place.Position.WithWhole(new(0x0100, 0x0200));
        place.SetSpawnParameter1(EnemyDefinitionId.Choot, 2);
        ChootEnemyState state = ChootBehavior.InitializeChoot(place.HeldOccupant().SpawnSettings<ChootSpawnSettings>(), place);

        int initial = state.InitialYSpeedCurvePlace.WholeMarks;
        AssertTrue(initial > 0 && initial % 2 == 0, "the ascent starts at a positive even record index");
        AssertEqual(QuadraticSpeedCurvePlace.FromMarks(initial), state.YSpeedCurvePlace, "the ascent starts at the calculated index");
        // The jump runs as the Choot's main AI runs it once the jump has started.
        ChootSpawnSettings settings = place.HeldOccupant().SpawnSettings<ChootSpawnSettings>();
        IEnemyAiFrame frame = PlacedEnemyAccess.AiFrame(RoomEnemyFrameFixtures.StandaloneSamus());
        state.Action = ChootAction.Jumping;
        int jumpingUpdates = 0;
        while (state.Action != ChootAction.Falling)
        {
            ChootBehavior.RunChootMainAi(settings, place, state, frame);
            jumpingUpdates++;
            AssertTrue(jumpingUpdates <= initial, "the jump reaches its apex");
        }
        AssertEqual(initial / 2 + 1, jumpingUpdates, "the apex is the update that takes the index below zero");
        AssertEqual(QuadraticSpeedCurvePlace.FromMarks(-2), state.YSpeedCurvePlace, "the index stays below zero through the fall");
        AssertEqual(0, ((ChootFallLoop.AtSample)state.FallingLoop).Age, "the fall starts at its first frame");
        Console.WriteLine("Choot owned state: the apex underflow matches.");
    }

    /// <summary>Runs the focused Choot owned-state check.</summary>
    [VerificationCommand("--choot-owned-state")]
    internal static int ChootOwnedStateCommand()
    {
        VerifyChootOwnedState();
        return 0;
    }
}
