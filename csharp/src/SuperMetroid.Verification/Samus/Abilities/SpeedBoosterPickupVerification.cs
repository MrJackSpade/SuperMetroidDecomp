using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectiblesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Samus.Abilities;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks the Chozo Speed Booster message continuation that starts the room's lava rise.</summary>
internal static class SpeedBoosterPickupVerification
{
    /// <summary>Contrasts Chozo and exposed pickups through message return, FX delay, earthquake, and subpixel rise.</summary>
    internal static void VerifySpeedBoosterPickupContinuation()
    {
        foreach (bool chozo in new[] { true, false })
        {
            var bus = new TestAddressSpace();
            SeedCollectibleRom(bus);
            var vram = new SnesVram();
            var fxSession = new RoomFxSession();
            var fx = RoomFxOverlayState.ForRoom(fxSession, RoomFxDefinitions.Suffix(0x8600), null, 0);
            CollectibleFixture fixture = LoadCollectible(bus, RoomObjectNativeWords.Word(chozo ? RoomObjectKind.ChozoSpeedBooster : RoomObjectKind.ExposedSpeedBooster), roomArgument: 8, precollected: false, roomFx: fx);
            fixture.RoomObjects.Step(fixture.Level);
            if (chozo)
            {
                // The orb must be shot open before its item can be touched.
                AssertTrue(fixture.RoomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(fixture.Block),
                    "Speed Booster orb accepts a projectile");
                StepUntil(
                    () => fixture.RoomObjects.CollectiblePhases[0] == CollectiblePhase.Visible,
                    _ => fixture.RoomObjects.Step(fixture.Level),
                    maximumFrames: 20,
                    context: "Speed Booster orb burst");
            }
            AssertTrue(fixture.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(fixture.Block),
                "Speed Booster pickup accepts contact");
            fixture.RoomObjects.Step(fixture.Level);
            AssertTrue(fixture.Samus.CollectedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
                "Speed Booster acquired before message continuation");
            AssertEqual((ushort)0, fx.YVelocity.RawValue, "message has not yet armed lava");
            AssertEqual((ushort)0x20, fx.Timer.RawValue, "pickup preserves native liquid delay");
            AssertEqual((ushort)0xda, fx.NamedBaseY.Whole, "pickup preserves native liquid surface");

            // Runtime suspends gameplay while the message is active and invokes this
            // continuation when the routine returns, before the next FX pass.
            fixture.RoomObjects.CollectibleObjects.CompleteCollectibleMessage(fixture.Level);
            AssertEqual(chozo ? (ushort)0xffe0 : (ushort)0, fx.YVelocity.RawValue,
                "only the Chozo instruction list writes the native upward velocity");
            fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
            AssertEqual((ushort)0x20, fx.Timer.RawValue, "first FX pass selects waiting phase");
            for (int frame = 0; frame < 32; frame++)
            {
                fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
                AssertEqual((ushort)0xda, fx.NamedBaseY.Whole, "surface waits throughout native delay");
                AssertEqual(chozo, fx.SustainedEarthquake.HasValue,
                    "native liquid owner publishes pickup earthquake during delay");
            }
            // Native liquid initialization starts at subposition $8000. At -$20
            // in 8.8, the first four moving frames consume that half pixel.
            for (int frame = 1; frame <= 5; frame++)
            {
                fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
                AssertEqual(chozo && frame == 5 ? (ushort)0xd9 : (ushort)0xda,
                    fx.NamedBaseY.Whole, "lava rises at native subpixel speed after delay");
            }
            fx.SetLiquidMotion(yVelocity: SignedEightEight.Zero);
            fixture.RoomObjects.CollectibleObjects.CompleteCollectibleMessage(fixture.Level);
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
