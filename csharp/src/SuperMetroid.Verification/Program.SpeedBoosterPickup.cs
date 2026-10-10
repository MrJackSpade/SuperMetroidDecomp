using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // GR-2: the Chozo pickup must arm native lava motion only on message return.
    private static void VerifySpeedBoosterPickupContinuation()
    {
        var cartridge = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool chozo in new[] { true, false })
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            var vram = new SnesVram();
            RoomLayer3FxState fx = CreateRetailFxState(cartridge);
            fx.Load(cartridge, vram, new SnesCgram(), 0x8600, 0, 0);
            CollectibleFixture fixture = LoadCollectible(bus,
                (ushort)(chozo ? PlmHeaderId.ChozoSpeedBooster : PlmHeaderId.ExposedSpeedBooster),
                roomArgument: 8, precollected: false, roomFx: fx);
            fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, 0, 0, 0);
            if (chozo)
            {
                // The orb must be shot open before its item can be touched.
                AssertTrue(fixture.Plms.TryNotifyCollectibleProjectileHit(fixture.BlockIndex, 0x0100),
                    "Speed Booster orb accepts a projectile");
                StepUntil(
                    () => fixture.Plms.CollectiblePhases[0] == CollectiblePhase.Visible,
                    _ => fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, 0, 0, 0),
                    maximumFrames: 20,
                    context: "Speed Booster orb burst");
            }
            AssertTrue(fixture.Plms.TryNotifyCollectibleTouch(fixture.BlockIndex),
                "Speed Booster pickup accepts contact");
            fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertTrue(fixture.Samus.CollectedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
                "Speed Booster acquired before message continuation");
            AssertEqual((ushort)0, fx.PackedYVelocity, "message has not yet armed lava");
            AssertEqual((ushort)0x20, fx.Timer, "pickup preserves native liquid delay");
            AssertEqual((ushort)0xda, fx.BaseYPosition, "pickup preserves native liquid surface");

            // Runtime suspends gameplay while the message is active and invokes this
            // continuation when the routine returns, before the next FX pass.
            fixture.Plms.CompleteCollectibleMessage(bus, fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertEqual(chozo ? (ushort)0xffe0 : (ushort)0, fx.PackedYVelocity,
                "only the Chozo instruction list writes the native upward velocity");
            fx.Step(cartridge, vram, 0, 0, timeIsFrozen: false, mainGameLoopCarry: true);
            AssertEqual((ushort)0x20, fx.Timer, "first FX pass selects waiting phase");
            for (int frame = 0; frame < 32; frame++)
            {
                fx.Step(cartridge, vram, 0, 0, timeIsFrozen: false, mainGameLoopCarry: true);
                AssertEqual((ushort)0xda, fx.BaseYPosition, "surface waits throughout native delay");
                AssertEqual(chozo, fx.EarthquakeRequest.HasValue,
                    "native liquid owner publishes pickup earthquake during delay");
            }
            // Native liquid initialization starts at subposition $8000. At -$20
            // in 8.8, the first four moving frames consume that half pixel.
            for (int frame = 1; frame <= 5; frame++)
            {
                fx.Step(cartridge, vram, 0, 0, timeIsFrozen: false, mainGameLoopCarry: true);
                AssertEqual(chozo && frame == 5 ? (ushort)0xd9 : (ushort)0xda,
                    fx.BaseYPosition, "lava rises at native subpixel speed after delay");
            }
            fx.ApplyCartridgeMotionWrites(packedYVelocity: 0);
            fixture.Plms.CompleteCollectibleMessage(bus, fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertEqual((ushort)0, fx.PackedYVelocity, "message continuation is consumed once");
        }
        Console.WriteLine("Speed Booster pickup continuation, delay, rise and earthquake verified.");
    }
}
