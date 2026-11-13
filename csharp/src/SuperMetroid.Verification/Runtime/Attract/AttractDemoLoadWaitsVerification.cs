using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>
/// Checks the attract demo schedule's loading waits (#1627): before a scene's runtime exists the schedule holds no wait
/// count and awaits the scene; a begun scene counts its enemy-transfer waits down to the last; a reset, a title request
/// or the next scene await a scene again.
/// </summary>
internal static class AttractDemoLoadWaitsVerification
{
    /// <summary>Runs one scene's loading waits and each way back to awaiting a scene.</summary>
    internal static void VerifyAttractDemoLoadWaits()
    {
        var schedule = new AttractDemoSchedule();
        AssertTrue(schedule.AwaitsScene, "a new schedule awaits its first scene");
        AssertThrows<InvalidOperationException>(() => schedule.CountLoadFrame(), "no wait is counted before a scene begins");
        AttractDemoScene scene = schedule.NextScene ?? throw new InvalidOperationException("The first set has a first scene.");
        schedule.Begin(scene);
        AssertTrue(!schedule.AwaitsScene, "a begun scene no longer awaits its runtime");
        for (int wait = 1; wait < AttractDemoDefinitions.EnemyGraphicsLoadFrames; wait++)
            AssertTrue(!schedule.CountLoadFrame(), $"enemy-transfer wait {wait} is not the last");
        AssertTrue(schedule.CountLoadFrame(), "the last enemy-transfer wait ends the loading");
        AssertTrue(!schedule.AwaitsScene, "a loaded scene does not await another");
        schedule.LoadNextScene();
        AssertTrue(schedule.AwaitsScene, "the next scene awaits its runtime");
        schedule.Begin(scene);
        schedule.RequestFromTitle();
        AssertTrue(schedule.AwaitsScene, "a title request awaits a scene");
        schedule.Begin(scene);
        schedule.Reset();
        AssertTrue(schedule.AwaitsScene, "a soft reset awaits a scene");
    }
}
