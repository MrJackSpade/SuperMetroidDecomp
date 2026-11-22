using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Dachora's phase countdowns put its speed at rest.</summary>
internal static class DachoraPhaseCountdownsVerification
{
    /// <summary>
    /// Confirms the warning countdown starts while the waiting Dachora stays at rest, and the charge countdown stops a
    /// running Dachora so the launch starts at rest.
    /// </summary>
    internal static void VerifyDachoraPhaseCountdowns()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        // A Dachora runner set up as its room's population sets it up, waiting for Samus (#2554).
        RoomEnemyPlace place = enemies.LoadPopulation(
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Dachora, new DachoraSpawnSettings(HorizontalDirection.Right, DachoraRole.Runner)))[0];
        DachoraEnemyState state = place.RequireSpeciesState<DachoraEnemyState>();
        AssertEqual(DachoraAction.WaitingForSamus, state.Action, "the set-up Dachora waits for Samus");

        state.StartBlinking(5);
        AssertEqual(5, state.PhaseTimer.UpdatesLeft, "the warning countdown starts");
        AssertEqual(SignedSixteenSixteen.Zero, state.Speed, "the run after the warning countdown starts at rest");
        state.Speed = SignedSixteenSixteen.FromWholePixels(7) + SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(128);
        state.StartCharging(5);
        AssertEqual(5, state.PhaseTimer.UpdatesLeft, "the charge countdown starts");
        AssertEqual(SignedSixteenSixteen.Zero, state.Speed, "the launch after the charge countdown starts at rest");
        Console.WriteLine("Dachora phase countdowns: each countdown leaves the next phase at rest.");
    }

    /// <summary>Runs the focused Dachora phase-countdown check.</summary>
    [VerificationCommand("--dachora-phase-countdowns")]
    internal static int DachoraPhaseCountdownsCommand()
    {
        VerifyDachoraPhaseCountdowns();
        return 0;
    }
}
