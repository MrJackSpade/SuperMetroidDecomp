using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCeresRidleyWallImpact()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom);
        var ridley = runtime.Enemies.CeresRidley!;
        var body = runtime.Enemies.Slots[0];
        // Continue an in-flight lunge just before its left-boundary collision. The real
        // attack dispatcher still calculates acceleration and the shared mover clips X.
        ridley.Function = RidleyAiFunction.CeresLungeMain;
        ridley.FunctionTimer = 100;
        ridley.MovementAnimationEnabled = 1;
        ridley.HorizontalVelocity = unchecked((ushort)-768);
        body.XPosition = 41;
        body.YPosition = 100;
        runtime.Samus!.XPosition = 20;
        runtime.Samus.YPosition = 168;
        runtime.StepFrame(0);
        runtime.RunNmi(0, true);
        AssertEqual(40, body.XPosition, "Ceres lunge clips at native left bound");
        AssertEqual(33, runtime.Enemies.EarthquakeType, "Ceres left impact requests native quake type");
        AssertEqual(11, runtime.Enemies.EarthquakeTimer, "first draw consumes one of twelve quake frames");
        AssertTrue(runtime.Enemies.LastRoomShake.Applied, "impact reaches room shaking consumer");
        AssertEqual(0, runtime.Enemies.LastRoomShake.Bg1X, "native impact does not shift BG1 X");
        AssertEqual(0, runtime.Enemies.LastRoomShake.Bg1Y, "native impact does not shift BG1 Y");
        AssertEqual(3, runtime.Enemies.LastRoomShake.Bg2X, "native impact shifts BG2 three pixels on first frame");
        AssertTrue(runtime.Enemies.LastRoomShake.ShakesEnemies, "native impact also shakes selected enemies");
        AssertEqual(runtime.Enemies.LastRoomShake, runtime.DisplayedGameplayPpu.RoomShake,
            "accepted display snapshot carries the wall-impact displacement");
        // Stop further attack acceleration so this checks one impact's lifetime, not
        // repeated wall contacts refreshing its timer.
        ridley.Function = RidleyAiFunction.WaitBeforeRoar;
        ridley.HorizontalVelocity = ridley.VerticalVelocity = 0;
        for (int remaining = 10; remaining >= 0; remaining--)
        {
            runtime.StepFrame(0);
            runtime.RunNmi(0, true);
            AssertEqual(remaining, runtime.Enemies.EarthquakeTimer, "impact timer consumes exactly twelve frames");
            AssertTrue(runtime.DisplayedGameplayPpu.RoomShake.Applied, "each impact frame reaches the display");
        }
        runtime.StepFrame(0);
        runtime.RunNmi(0, true);
        AssertTrue(!runtime.DisplayedGameplayPpu.RoomShake.Applied, "impact shake ends after twelve displayed frames");
        Console.WriteLine("Ceres Ridley: real lunge collision requests and publishes native room shake.");
    }
}
