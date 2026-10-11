using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifySparkCrashAlignment()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (SamusPoseId pose in new SamusPoseId[] { SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkDiagonalLeftPose })
        {
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(700, 35)) };
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x1234);
            samus.RefreshCollisionRadii(bus);
            var finish = typeof(SamusShinesparkState).GetMethod("FinishCrash",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            finish.Invoke(samus.Shinespark, [samus, (ushort)5, null]);
            AssertEqual((ushort)35, samus.Position.Y.Whole, "Movement queues standing alignment until after animation");
            SamusShinesparkState.ApplyCrashFinishPose(bus, samus);
            AssertEqual((ushort)19, samus.Kinematics.Radius.Y, "Standing alignment does not publish next-frame radius early");
            AssertEqual((ushort)33, samus.Position.Y.Whole, "Native diagonal crash-to-standing feet alignment");
            AssertEqual((ushort)0x1234, samus.Kinematics.Position.Y.Fraction, "Crash alignment preserves current fraction");
            var previous = new SamusCameraPoint(699, 0x5678, 40, 0xabcd);
            AssertEqual(previous with { YPosition = 38 }, samus.ApplyPreviousPositionWrites(previous),
                "Native alignment shifts previous whole Y, not replacing it with current Y");
            AssertEqual(previous, samus.ApplyPreviousPositionWrites(previous), "Alignment checkpoint consumed once");
        }
    }
}
