using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Fune's waiting count against its cooldown time (#2045: a plain count up to a one-byte cooldown).</summary>
internal static class FuneNamiheCooldownVerification
{
    /// <summary>The cooldown time the cases wait for.</summary>
    private const int CooldownTime = 16;

    /// <summary>
    /// Each waiting update counts up, then ends the wait once the count reaches the cooldown time: a count short of the
    /// time keeps waiting, and the count that reaches it ends the wait, installs the active program and resets the count
    /// to zero. The count and the cooldown are plain counts below 256, so a count beyond that is refused rather than
    /// wrapped.
    /// </summary>
    internal static void VerifyFuneNamiheCooldown()
    {
        FuneNamiheEnemyState waiting = Wait(CooldownTime - 2);
        AssertEqual(CooldownTime - 1, waiting.CooldownTimer, "a count short of the cooldown time advances by one");
        AssertEqual(FuneNamiheAction.FuneWaitForCooldown, waiting.Action, "a count short of the cooldown time keeps waiting");

        FuneNamiheEnemyState reached = Wait(CooldownTime - 1);
        AssertEqual(0, reached.CooldownTimer, "ending the wait resets the count");
        AssertEqual(FuneNamiheAction.Spitting, reached.Action, "a count reaching the cooldown time ends the wait");
        AssertTrue(reached.ShowsActiveAnimation && !reached.FacingRight, "ending the wait selects the active animation for the same facing");
        AssertEqual(new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(FuneNamiheSegment.FuneActiveLeft), AnimationPlace.StartOfAnimation), reached.Playback,
            "ending the wait installs the active program for the same facing");

        AssertThrows<ArgumentOutOfRangeException>(() => _ = new FuneNamiheEnemyState { CooldownTimer = byte.MaxValue + 1 },
            "a waiting count past the largest cooldown is refused");
        Console.WriteLine("  Fune cooldown: short and reached counts pass.");
    }

    /// <summary>Runs one waiting update of a left-facing Fune whose count stands at <paramref name="count"/>.</summary>
    private static FuneNamiheEnemyState Wait(int count)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Fune, place.Definition);
        var state = new FuneNamiheEnemyState
        {
            ShowsActiveAnimation = false,
            FacingRight = false,
            Action = FuneNamiheAction.FuneWaitForCooldown,
            IsNamihe = false,
            CooldownTime = CooldownTime,
            CooldownTimer = count,
        };
        place.SetSpeciesState(state);
        FuneNamiheBehavior.RunFuneNamiheMain(place, state, RoomEnemyFrameFixtures.StandaloneSamus());
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
