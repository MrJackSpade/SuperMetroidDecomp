using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Fx.RoomFxRetailInventoryVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Samus.Abilities;

using SuperMetroid.Verification.Harness;

/// <summary>Checks the Chozo Speed Booster message continuation that starts the room's lava rise.</summary>
internal static class SpeedBoosterPickupVerification
{
    /// <summary>Contrasts Chozo and exposed pickups through message return, FX delay, earthquake, and subpixel rise.</summary>
    internal static void VerifySpeedBoosterPickupContinuation()
    {
        var cartridge = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool chozo in new[] { true, false })
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            var vram = new SnesVram();
            var fxSession = CreateRetailFxSession(cartridge);
            var fx = RoomLayer3FxState.ForRoom(fxSession, new SnesCgram(), RoomFxDefinitions.Suffix(0x8600), null, 0);
            CollectibleFixture fixture = LoadCollectible(bus, (ushort)(chozo ? PlmHeaderId.ChozoSpeedBooster : PlmHeaderId.ExposedSpeedBooster), roomArgument: 8, precollected: false, roomFx: fx);
            fixture.Plms.Step(fixture.Level);
            if (chozo)
            {
                // The orb must be shot open before its item can be touched.
                AssertTrue(fixture.Plms.CollectiblePlms.TryNotifyCollectibleProjectileHit(fixture.BlockIndex),
                    "Speed Booster orb accepts a projectile");
                StepUntil(
                    () => fixture.Plms.CollectiblePhases[0] == CollectiblePhase.Visible,
                    _ => fixture.Plms.Step(fixture.Level),
                    maximumFrames: 20,
                    context: "Speed Booster orb burst");
            }
            AssertTrue(fixture.Plms.CollectiblePlms.TryNotifyCollectibleTouch(fixture.BlockIndex),
                "Speed Booster pickup accepts contact");
            fixture.Plms.Step(fixture.Level);
            AssertTrue(fixture.Samus.CollectedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
                "Speed Booster acquired before message continuation");
            AssertEqual((ushort)0, fx.YVelocity.RawValue, "message has not yet armed lava");
            AssertEqual((ushort)0x20, fx.Timer.RawValue, "pickup preserves native liquid delay");
            AssertEqual((ushort)0xda, fx.BaseY.Whole, "pickup preserves native liquid surface");

            // Runtime suspends gameplay while the message is active and invokes this
            // continuation when the routine returns, before the next FX pass.
            fixture.Plms.CollectiblePlms.CompleteCollectibleMessage(fixture.Level);
            AssertEqual(chozo ? (ushort)0xffe0 : (ushort)0, fx.YVelocity.RawValue,
                "only the Chozo instruction list writes the native upward velocity");
            fx.Step(BgArtworkFixtures.For(vram), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true });
            AssertEqual((ushort)0x20, fx.Timer.RawValue, "first FX pass selects waiting phase");
            for (int frame = 0; frame < 32; frame++)
            {
                fx.Step(BgArtworkFixtures.For(vram), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true });
                AssertEqual((ushort)0xda, fx.BaseY.Whole, "surface waits throughout native delay");
                AssertEqual(chozo, fx.EarthquakeRequest.HasValue,
                    "native liquid owner publishes pickup earthquake during delay");
            }
            // Native liquid initialization starts at subposition $8000. At -$20
            // in 8.8, the first four moving frames consume that half pixel.
            for (int frame = 1; frame <= 5; frame++)
            {
                fx.Step(BgArtworkFixtures.For(vram), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true });
                AssertEqual(chozo && frame == 5 ? (ushort)0xd9 : (ushort)0xda,
                    fx.BaseY.Whole, "lava rises at native subpixel speed after delay");
            }
            fx.ApplyCartridgeMotionWrites(packedYVelocity: 0);
            fixture.Plms.CollectiblePlms.CompleteCollectibleMessage(fixture.Level);
            AssertEqual((ushort)0, fx.YVelocity.RawValue, "message continuation is consumed once");
        }
        Console.WriteLine("Speed Booster pickup continuation, delay, rise and earthquake verified.");
    }

    /// <summary>Runs the focused Speed Booster pickup and lava-motion check.</summary>
    [VerificationCommand("--speed-booster-pickup")]
    internal static int SpeedBoosterPickupCommand()
    {
        Suite(nameof(VerifySpeedBoosterPickupContinuation), () => VerifySpeedBoosterPickupContinuation());
        return 0;
    }
}
