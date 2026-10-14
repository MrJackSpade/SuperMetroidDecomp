using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks the room shake produced by Ceres Ridley's real left-wall lunge collision.</summary>
internal static class CeresRidleyWallVerification
{
    /// <summary>Confirms the lunge clips at X=40, publishes a BG2-only first-frame displacement, and lasts twelve displayed frames.</summary>
    internal static void VerifyCeresRidleyWallImpact()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom);
        var ridley = runtime.Enemies.RidleyEncounter.CeresRidley;
        var body = runtime.Enemies.SlotTable.Slots[0];
        // Continue an in-flight lunge just before its left-boundary collision. The real
        // attack dispatcher still calculates acceleration and the shared mover clips X.
        (ridley ?? throw new InvalidOperationException("Verification requires ridley.")).Function = RidleyAiFunction.CeresLungeMain;
        ridley.FunctionTimer = NativeCountdown.Start(100);
        ridley.MovementAnimationEnabled = 1;
        ridley.Velocity = ridley.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-768)));
        body.Position = body.Position.WithWhole(new(41, 100));
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWholeX(20);
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWholeY(168);
        runtime.StepFrame(0);
        runtime.NmiHandler.RunNmi(0, true);
        AssertEqual(40, body.Position.X.Whole, "Ceres lunge clips at native left bound");
        AssertEqual(33, runtime.Enemies.Earthquake.EarthquakeType, "Ceres left impact requests native quake type");
        AssertEqual(11, runtime.Enemies.Earthquake.EarthquakeTimer.RawValue, "first draw consumes one of twelve quake frames");
        AssertTrue(runtime.Enemies.Earthquake.LastRoomShake != default, "impact reaches room shaking consumer");
        AssertEqual(0, runtime.Enemies.Earthquake.LastRoomShake.Bg1.X, "native impact does not shift BG1 X");
        AssertEqual(0, runtime.Enemies.Earthquake.LastRoomShake.Bg1.Y, "native impact does not shift BG1 Y");
        AssertEqual(3, runtime.Enemies.Earthquake.LastRoomShake.Bg2.X, "native impact shifts BG2 three pixels on first frame");
        AssertTrue(!body.ShakeTimer.IsZero, "native impact also shakes selected enemies");
        AssertEqual(runtime.Enemies.Earthquake.LastRoomShake, runtime.Display.DisplayedGameplayPpu.RoomShake,
            "accepted display snapshot carries the wall-impact displacement");
        // Stop further attack acceleration so this checks one impact's lifetime, not
        // repeated wall contacts refreshing its timer.
        ridley.Function = RidleyAiFunction.WaitBeforeRoar;
        ridley.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        for (int remaining = 10; remaining >= 0; remaining--)
        {
            runtime.StepFrame(0);
            runtime.NmiHandler.RunNmi(0, true);
            AssertEqual(remaining, runtime.Enemies.Earthquake.EarthquakeTimer.RawValue, "impact timer consumes exactly twelve frames");
            AssertTrue(runtime.Display.DisplayedGameplayPpu.RoomShake != default, "each impact frame reaches the display");
        }
        runtime.StepFrame(0);
        runtime.NmiHandler.RunNmi(0, true);
        AssertTrue(runtime.Display.DisplayedGameplayPpu.RoomShake == default, "impact shake ends after twelve displayed frames");
        Console.WriteLine("Ceres Ridley: real lunge collision requests and publishes native room shake.");
    }
}
