using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that Crocomire holds apart the values the cartridge keeps in one record word.</summary>
internal static class CrocomireSeparateValuesVerification
{
    /// <summary>
    /// Confirms that the values the cartridge keeps in one record word are held apart (#1476): the queued steps leave the
    /// rumble step alone.
    /// </summary>
    internal static void VerifyCrocomireSeparateValues()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        var state = new CrocomireEnemyState(body);
        body.SetSpeciesState(state);

        state.StepCounter = 5;
        AssertEqual(0, state.RumbleStep, "the queued steps leave the rumble step alone");
        Console.WriteLine("Crocomire separate values: the steps and rumble step are held apart.");
    }

    /// <summary>Runs the focused Crocomire separate-values check.</summary>
    [VerificationCommand("--crocomire-separate-values")]
    internal static int CrocomireSeparateValuesCommand()
    {
        VerifyCrocomireSeparateValues();
        return 0;
    }
}
