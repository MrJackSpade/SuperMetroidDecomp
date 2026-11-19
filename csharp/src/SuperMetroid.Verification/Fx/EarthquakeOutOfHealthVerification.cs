using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;

namespace SuperMetroid.Verification.Fx;

/// <summary>
/// Checks the room's earthquake on the update Samus runs out of health (#2519). The cartridge's out-of-health check runs
/// after room main and before room shaking, and freezes time itself as it sends Samus to the AUTO reserves' refill or to
/// her death; room shaking then sees time frozen and neither shakes the room nor counts the earthquake down on that
/// update.
/// </summary>
internal static class EarthquakeOutOfHealthVerification
{
    /// <summary>A shake that throws the foreground, the background and the enemies.</summary>
    private static readonly EarthquakeShake Shake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Horizontal, EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <summary>The frames the fixture's earthquake starts with.</summary>
    private const int QuakeFrames = 20;

    /// <summary>
    /// An earthquake running when Samus runs out of health without reserves: the update that begins her death neither
    /// shakes the room nor counts the earthquake down.
    /// </summary>
    internal static void VerifyEarthquakeFrozenOnDeathUpdate()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = CreateMainGameplayFixture();
        samus.ReserveEnergy = 0;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        RoomEarthquake earthquake = ShakingRoom(game, runtime, samus);

        samus.Health = 0;
        game.Step(0);
        AssertEqual(SuperMetroidGameState.DeathSequence, game.GameState, "zero health without reserves begins the death");
        AssertTrue(runtime.Freeze.GameplayTimeFrozen, "the death freezes gameplay");
        AssertEqual(QuakeFrames - 1, earthquake.RemainingFrames, "the update the death begins on does not count the earthquake down");
        AssertEqual(default(RoomShakeFrameResult), earthquake.LastRoomShake, "the update the death begins on does not shake the room");

        Console.WriteLine("Earthquake on the death's update: the out-of-health check freezes time before the room shakes.");
    }

    /// <summary>
    /// An earthquake running when Samus runs out of health with AUTO reserves: the update that begins the refill neither
    /// shakes the room nor counts the earthquake down.
    /// </summary>
    internal static void VerifyEarthquakeFrozenOnReserveRefillUpdate()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = CreateMainGameplayFixture();
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 3;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        RoomEarthquake earthquake = ShakingRoom(game, runtime, samus);

        samus.Health = 0;
        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "zero health with AUTO reserves begins the refill");
        AssertTrue(runtime.Freeze.GameplayTimeFrozen, "the refill freezes gameplay");
        AssertEqual(QuakeFrames - 1, earthquake.RemainingFrames, "the update the refill begins on does not count the earthquake down");
        AssertEqual(default(RoomShakeFrameResult), earthquake.LastRoomShake, "the update the refill begins on does not shake the room");

        Console.WriteLine("Earthquake on the reserve refill's update: the out-of-health check freezes time before the room shakes.");
    }

    /// <summary>
    /// Starts the fixture room's earthquake and runs one update with Samus's health left, which shakes the room and counts
    /// the earthquake down once.
    /// </summary>
    private static RoomEarthquake ShakingRoom(SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus)
    {
        AssertTrue(samus.Health > 0, "the fixture's Samus starts with health");
        RoomEarthquake earthquake = runtime.Enemies.Earthquake;
        earthquake.Start(Shake, QuakeFrames);
        game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the update with health left stays in main gameplay");
        AssertEqual(QuakeFrames - 1, earthquake.RemainingFrames, "the update with health left counts the earthquake down");
        AssertTrue(earthquake.LastRoomShake != default, "the update with health left shakes the room");
        return earthquake;
    }
}
