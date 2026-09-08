using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombJumpWallContact()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var words = new ushort[16 * 16];
        for (int row = 0; row < 10; row++) words[row * 16 + 4] = 0x8000;
        var level = new RoomLevelData(16, 16, words, new byte[words.Length], new ushort[words.Length], new byte[8]);
        var samus = new SamusState { Pose = SamusPoseIds.MorphBallGroundRightPose, XPosition = 59, YPosition = 80 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.Kinematics.XSubposition = 0xF000;
        samus.Kinematics.YSubacceleration = 0x4000;
        samus.RequestMorphedBombJump(3);
        SamusBombJumpMovement.Start(bus, samus);
        var result = SamusBombJumpMovement.Step(bus, level, samus, 0, new RoomPlmSystem());
        AssertTrue(result.Horizontal is { Collided: true }, $"diagonal bomb jump hits the shaft wall (X={samus.Kinematics.XFixed:X8}, radius={samus.Kinematics.XRadius}, move={result.Horizontal})");
        AssertTrue(result.Vertical is { Collided: false }, "shaft wall leaves vertical travel clear");
        // Original $90:E032 bytes: GrapplePoseAudit bomb-wall. A horizontal collision
        // must not end the special handler; the native branch tests the later Y result.
        AssertTrue(!result.Ended && samus.BombJumpActive, "wall-only collision retains native bomb-jump handler");
        AssertEqual(0x0803, samus.BombJumpDirection, "native bomb-wall direction remains armed");
        AssertEqual(59, samus.XPosition, "native bomb-wall X endpoint");
        AssertEqual(77, samus.YPosition, "native bomb-wall Y endpoint");
        AssertEqual(0x4000, samus.Kinematics.YSubposition, "native bomb-wall Y fraction");
        AssertEqual(2, samus.Kinematics.YSpeed, "native bomb-wall next speed");
        AssertEqual(0x8000, samus.Kinematics.YSubspeed, "native bomb-wall next subspeed");
        Console.WriteLine("  Bomb jump: wall-only collision preserves native ascent and handler lifetime.");
    }
}
