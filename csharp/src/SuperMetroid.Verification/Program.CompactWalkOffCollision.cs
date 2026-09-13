using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompactWalkOffCollision()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var words = new ushort[16 * 32];
        for (int x = 0; x < 16; x++) words[16 * 16 + x] = 0x8000;
        var level = new RoomLevelData(16, 32, words, new byte[words.Length], new ushort[words.Length], new byte[8]);
        foreach (bool left in new[] { false, true })
        {
            // Native soft-unmorph comparison: the neighboring ordinary landing's
            // grounding probe leaves a compact body near the floor. Falling expands
            // that body, so F404 must shift its center and clamp its fractional word.
            var samus = new SamusState
            {
                Pose = left ? SamusPoseIds.CrouchingLeftPose : SamusPoseIds.CrouchingRightPose,
                XPosition = 80, YPosition = 240,
            };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.Kinematics.YSubposition = 0x6400;
            samus.Kinematics.YSpeed = 2;
            samus.Kinematics.YSubspeed = 0xf400;
            byte target = left ? SamusPoseIds.FallingLeftPose : SamusPoseIds.FallingRightPose;
            samus.ApplyWalkedOffFloorTransition(bus, level, target);
            AssertEqual(0x00edffffu, samus.Kinematics.YFixed, "compact walk-off uses native floor center/subpixel correction");
            AssertEqual(target, samus.Pose, "compact walk-off selects ordinary falling art");
            AssertEqual(16, samus.Kinematics.YRadius, "compact walk-off retains live crouch radius until alpha");
            samus.RefreshCollisionRadii(bus);
            AssertEqual(19, samus.Kinematics.YRadius, "compact walk-off next alpha publishes falling radius");
            AssertEqual(0x00edffffu, samus.Kinematics.YFixed, "radius publication does not repeat compact correction");
            AssertEqual(0, samus.Kinematics.YSpeed, "accepted falling command clears whole vertical speed");
            AssertEqual(0, samus.Kinematics.YSubspeed, "accepted falling command clears fractional vertical speed");
            AssertEqual(2, samus.Kinematics.YDirection, "accepted falling command starts downward motion");
        }
    }
}
