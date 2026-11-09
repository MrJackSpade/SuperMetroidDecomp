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
    /// Confirms the warning countdown leaves the next run starting at rest with the speed's fraction kept, and the charge
    /// countdown leaves the launch starting fully at rest, as the countdown that ends at zero in the speed's place does
    /// on the cartridge.
    /// </summary>
    internal static void VerifyDachoraPhaseCountdowns()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new DachoraEnemyState();
        slot.SetSpeciesState(state);

        state.Speed = SignedSixteenSixteen.FromParts(3, 0x8000);
        state.StartBlinking(5);
        AssertEqual(5, state.PhaseTimer, "the warning countdown starts");
        AssertEqual(SignedSixteenSixteen.FromParts(0, 0x8000), state.Speed,
            "the run after the warning countdown starts at rest, keeping the speed's fraction");
        state.Speed = SignedSixteenSixteen.FromParts(7, 0x8000);
        state.StartCharging(5);
        AssertEqual(5, state.PhaseTimer, "the charge countdown starts");
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
