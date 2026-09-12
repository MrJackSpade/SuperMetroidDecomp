using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyDraygonEyeEffects()
    {
        foreach (bool facingRight in new[] { false, true })
        {
            var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Draygon);
            var eye = runtime.Enemies.Draygon!.Eye!;
            typeof(RoomEnemySlot).GetProperty(nameof(eye.XPosition))!.SetValue(eye, (ushort)128);
            typeof(RoomEnemySlot).GetProperty(nameof(eye.YPosition))!.SetValue(eye, (ushort)128);
            var samus = new SamusState { XPosition = 128, YPosition = 256 };
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
                    AssertEqual((ushort)(facingRight ? 152 : 104), particles[0].XPosition, "Eye particle facing-relative X");
                    AssertEqual((ushort)96, particles[0].YPosition, "Eye particle vertical origin");
                }
            }
        }
    }
}
