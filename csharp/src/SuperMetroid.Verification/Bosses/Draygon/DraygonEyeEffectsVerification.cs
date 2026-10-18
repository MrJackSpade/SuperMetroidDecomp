using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Draygon eye dust emission timing and facing-relative spawn position.</summary>
internal static class DraygonEyeEffectsVerification
{
    /// <summary>Tracks a stationary target across the eye's frame-128 particle boundary in both facings.</summary>
    internal static void VerifyDraygonEyeEffects()
    {
        foreach (bool facingRight in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
            var eye = (runtime.Enemies.DraygonBehavior.Draygon ?? throw new InvalidOperationException("Verification requires runtime.Enemies.DraygonBehavior.Draygon.")).Eye;
            eye.Position = eye.Position.WithWhole(new(128, 128));
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 256)) };
            foreach (ushort frame in new ushort[] { 127, 128, 129 })
            {
                eye.FrameCounter = frame;
                runtime.Enemies.DraygonBehavior.TrackSamusWithDraygonEye(eye, samus, facingRight);
                var particles = runtime.Enemies.Projectiles.EnemyProjectiles.Where(p =>
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

    /// <summary>Runs the focused Draygon eye effect verification command.</summary>
    [VerificationCommand("--draygon-eye-effects")]
    internal static int DraygonEyeEffectsCommand()
    {
        Suite(nameof(VerifyDraygonEyeEffects), () => VerifyDraygonEyeEffects());
        return 0;
    }
}
