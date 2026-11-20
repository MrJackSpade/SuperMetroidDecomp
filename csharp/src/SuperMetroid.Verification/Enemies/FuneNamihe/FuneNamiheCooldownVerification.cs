using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Fune's waiting count against its cooldown time (#2045: a plain count up to the cooldown).</summary>
internal static class FuneNamiheCooldownVerification
{
    /// <summary>The cooldown time the cases wait for.</summary>
    private const int CooldownTime = 16;

    /// <summary>
    /// Each waiting update counts the cooldown, which ends on its cooldown time's update: a wait short of its end keeps
    /// waiting, and the update that ends it installs the active program and starts the wait again.
    /// </summary>
    internal static void VerifyFuneNamiheCooldown()
    {
        FuneEnemyState waiting = Wait(CooldownTime - 2);
        AssertEqual(CooldownTime - 1, waiting.Cooldown.Age, "a wait short of the cooldown time advances by one");
        AssertEqual(FuneAction.WaitForCooldown, waiting.Action, "a count short of the cooldown time keeps waiting");

        FuneEnemyState reached = Wait(CooldownTime - 1);
        AssertEqual(0, reached.Cooldown.Age, "ending the wait starts it again");
        AssertEqual(FuneAction.Spitting, reached.Action, "a count reaching the cooldown time ends the wait");
        AssertTrue(reached.IsSpitting, "ending the wait selects the active animation");
        AssertEqual(new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(FuneNamiheSegment.FuneActiveLeft), AnimationPlace.StartOfAnimation), reached.Playback,
            "ending the wait installs the active program for the same facing");

        Console.WriteLine("  Fune cooldown: short and reached counts pass.");
    }

    /// <summary>Runs one waiting update of a left-facing Fune whose count stands at <paramref name="count"/>.</summary>
    private static FuneEnemyState Wait(int count)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Fune, place.KeptDefinition);
        place.SetSpawnSettings(new FuneSpawnSettings(Facing: HorizontalDirection.Left, CooldownTime: CooldownTime, LaunchSpeed: SignedEightEight.Zero));
        var state = new FuneEnemyState(FuneAction.WaitForCooldown, place.SpawnSettings<FuneSpawnSettings>()) { Cooldown = UpdateCountdownAccess.AtAge(CooldownTime, count) };
        FuneNamiheBehavior.RunFuneMain(place.HeldOccupant(), place, state);
        return state;
    }

    /// <summary>Runs the Fune cooldown check through its dedicated command-line switch.</summary>
    [VerificationCommand("--fune-namihe-cooldown")]
    internal static int FuneNamiheCooldownCommand()
    {
        Suite(nameof(VerifyFuneNamiheCooldown), () => VerifyFuneNamiheCooldown());
        return 0;
    }
}
