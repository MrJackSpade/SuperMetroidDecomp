using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.FuneNamihe;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Fune's waiting count against its cooldown time, including the count's wrap and signed comparison.</summary>
internal static class FuneNamiheCooldownVerification
{
    /// <summary>The cooldown time the cases wait for.</summary>
    private const int CooldownTime = 16;

    /// <summary>
    /// Each waiting update counts up, then ends the wait only when the count minus the cooldown time is not negative as
    /// a signed word: a count short of the time keeps waiting, a count that wraps from the top of the word to zero keeps
    /// waiting, and a count that crosses the signed boundary ends the wait, installs the active program and resets the
    /// count to zero.
    /// </summary>
    internal static void VerifyFuneNamiheCooldown()
    {
        FuneNamiheEnemyState waiting = Wait(CooldownTime - 2);
        AssertEqual(CooldownTime - 1, waiting.CooldownTimer, "a count short of the cooldown time advances by one");
        AssertEqual(FuneNamiheAction.FuneWaitForCooldown, waiting.Action, "a count short of the cooldown time keeps waiting");

        FuneNamiheEnemyState wrapped = Wait(ushort.MaxValue);
        AssertEqual(0, wrapped.CooldownTimer, "the count wraps from the top of the word to zero");
        AssertEqual(FuneNamiheAction.FuneWaitForCooldown, wrapped.Action, "a count wrapped to zero keeps waiting");

        FuneNamiheEnemyState crossed = Wait(short.MaxValue);
        AssertEqual(0, crossed.CooldownTimer, "ending the wait resets the count");
        AssertEqual(FuneNamiheAction.Spitting, crossed.Action, "a count crossing the signed boundary ends the wait");
        AssertTrue(crossed.ShowsActiveAnimation && !crossed.FacingRight, "ending the wait selects the active animation for the same facing");
        AssertEqual(new AnimationPlayback<FuneNamiheSegment>(FuneNamiheAnimations.StartingWith(FuneNamiheSegment.FuneActiveLeft), AnimationPlace.StartOfAnimation), crossed.Playback,
            "ending the wait installs the active program for the same facing");
        Console.WriteLine("  Fune cooldown: short, wrapped and signed-boundary counts pass.");
    }

    /// <summary>Runs one waiting update of a left-facing Fune whose count stands at <paramref name="count"/>.</summary>
    private static FuneNamiheEnemyState Wait(int count)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Fune, slot.Definition);
        var state = new FuneNamiheEnemyState
        {
            ShowsActiveAnimation = false,
            FacingRight = false,
            Action = FuneNamiheAction.FuneWaitForCooldown,
            Variant = FuneNamiheVariant.Fune,
            CooldownTime = CooldownTime,
            CooldownTimer = count,
        };
        slot.SetSpeciesState(state);
        FuneNamiheBehavior.RunFuneNamiheMain(slot, state, null);
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
