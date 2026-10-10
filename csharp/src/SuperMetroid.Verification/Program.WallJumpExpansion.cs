using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks wall-jump pose and collision-radius expansion, preserving facing and X while applying ceiling correction and vertical subpixel clamping.</summary>
    /// <param name="bus">Address space used to read the native collision radii for spin and wall-jump poses.</param>
    private static void VerifyWallJumpExpansion(ISnesAddressSpace bus)
    {
        foreach (bool ceiling in new[] { false, true })
        foreach (bool left in new[] { false, true })
        {
            var entries = new ushort[16 * 32];
            if (ceiling) entries[6 * 16 + (left ? 7 : 8)] = 0x8000;
            var room = CreateRoom(16, 32, entries, new byte[entries.Length]);
            var samus = new SamusState
            {
                Pose = left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose,
                XPosition = left ? (ushort)119 : (ushort)137,
                YPosition = 127,
            };
            samus.Kinematics.YSubposition = 0x1000;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            ushort beforeX = samus.XPosition;
            samus.ApplyWallJumpTrigger(bus, room);
            AssertEqual(left ? SamusPoseIds.WallJumpLeftPose : SamusPoseIds.WallJumpRightPose,
                samus.Pose, "walljump expansion selects facing-preserving pose");
            AssertEqual(beforeX, samus.XPosition, "vertical expansion preserves X");
            AssertEqual(ceiling ? 131 : 127, samus.YPosition, "walljump ceiling expansion center correction");
            AssertEqual(ceiling ? 0 : 0x1000, samus.Kinematics.YSubposition,
                "walljump collision clamps fractional Y; air preserves it");
            // $91:FDAE keeps the spin radius until alpha's SetSamusRadius next frame.
            AssertEqual(SamusState.ReadPoseYRadius(bus, left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose),
                samus.Kinematics.YRadius, "walljump keeps the spin radius through its frame");
            AssertEqual(19, SamusState.ReadPoseYRadius(bus, samus.Pose), "walljump expanded collision radius");
        }
    }
}
