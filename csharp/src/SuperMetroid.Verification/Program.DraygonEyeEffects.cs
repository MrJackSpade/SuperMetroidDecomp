using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class DraygonEyeEffectsVerification
{
    internal static void VerifyDraygonEyeEffects()
    {
        foreach (bool facingRight in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Draygon);
            var eye = runtime.Enemies.Draygon!.Eye!;
            eye.Position = eye.Position.WithWhole(new(128, 128));
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 256)) };
            var track = typeof(RoomEnemySystem).GetMethod("TrackSamusWithDraygonEye",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
            foreach (ushort frame in new ushort[] { 127, 128, 129 })
            {
                typeof(RoomEnemySlot).GetProperty(nameof(eye.FrameCounter))!.SetValue(eye, frame);
                track.Invoke(runtime.Enemies, [eye, samus, facingRight]);
                var particles = runtime.Enemies.EnemyProjectiles.Where(p =>
                    p.Kind == RoomEnemyProjectileKind.MiscDustExplosion).ToArray();
                AssertEqual(frame == 127 ? 0 : 1, particles.Length, "Eye particle cadence with unchanged target angle");
                if (particles.Length == 1)
                {
                    AssertEqual((ushort)(facingRight ? 152 : 104), particles[0].Position.X.Whole, "Eye particle facing-relative X");
                    AssertEqual((ushort)96, particles[0].Position.Y.Whole, "Eye particle vertical origin");
                }
            }
        }
    }

    [VerificationCommand("--draygon-eye-effects")]
    internal static int DraygonEyeEffectsCommand()
    {
        Suite(nameof(VerifyDraygonEyeEffects), () => VerifyDraygonEyeEffects());
        return 0;
    }
}
