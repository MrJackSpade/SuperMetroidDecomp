using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifySparkCrashAlignment()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (byte pose in new byte[] { 0xcd, 0xce })
        {
            var samus = new SamusState { Pose = pose, XPosition = 700, YPosition = 35 };
            samus.Kinematics.YSubposition = 0x1234;
            samus.RefreshCollisionRadii(bus);
            var finish = typeof(SamusShinesparkState).GetMethod("FinishCrash",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            finish.Invoke(samus.Shinespark, [bus, samus, ShinesparkPhase.Diagonal, (ushort)5, null]);
            AssertEqual((ushort)33, samus.YPosition, "Native diagonal crash-to-standing feet alignment");
            AssertEqual((ushort)0x1234, samus.Kinematics.YSubposition, "Crash alignment preserves current fraction");
            var previous = new SamusCameraPoint(699, 0x5678, 40, 0xabcd);
            AssertEqual(previous with { YPosition = 38 }, samus.ApplyPoseCollisionCameraCheckpoint(previous),
                "Native alignment shifts previous whole Y, not replacing it with current Y");
            AssertEqual(previous, samus.ApplyPoseCollisionCameraCheckpoint(previous), "Alignment checkpoint consumed once");
        }
    }
}
