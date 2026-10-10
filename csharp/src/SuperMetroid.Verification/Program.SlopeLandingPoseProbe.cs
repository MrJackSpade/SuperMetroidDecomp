using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// #1275 "Samus drunk", updates 44757-44759 in Ceres room $DFD7: Samus falls at X $69 onto
    /// a stair of horizontally flipped slopes. The fall is clipped one pixel onto slope $54,
    /// but the landing pose's two extra radius pixels then meet slope $55 below, and
    /// <c>$91:FF49</c> re-probes the remaining zero distance. <c>$94:96E3</c> still scans that
    /// zero probe, finds slope $54 under the bottom boundary and restores the falling pose.
    /// One frame later Samus is a pixel deeper and the landing fits by moving her up.
    /// </summary>
    private static void VerifySlopeLandingZeroDistanceProbe()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const int Width = 16, Height = 32;
        var foreground = new ushort[Width * Height];
        var behavior = new byte[Width * Height];
        foreach (var (x, y, bts) in new[] { (5, 23, 0x55), (6, 23, 0x54), (5, 24, 0x53), (6, 24, 0x55), (7, 24, 0x54),
            (6, 25, 0x53), (7, 25, 0x55), (8, 25, 0x54) })
        {
            foreground[y * Width + x] = 0x1000;
            behavior[y * Width + x] = (byte)bts;
        }
        RoomLevelData level = CreateRoom(Width, Height, foreground, behavior);

        SamusState Falling(ushort y, ushort ySubposition)
        {
            var samus = new SamusState { Pose = (byte)SamusPoseId.NeutralJumpRightPose, XPosition = 0x69, YPosition = y };
            samus.Kinematics.YSubposition = ySubposition;
            samus.RefreshCollisionRadii(bus);
            return samus;
        }

        SamusState clipped = Falling(0x16d, 0x2fff);
        AssertTrue(!clipped.TryApplyAerialLanding(bus, level, wasSpinning: false, controllerInput: 0, nmiFrameCounter: 0),
            "a zero-distance changed-pose probe still meets slope $54 and rejects the landing");
        AssertEqual((byte)SamusPoseId.NeutralJumpRightPose, clipped.Pose, "the falling pose is retained");
        AssertEqual((ushort)0x16d, clipped.YPosition, "the rejected landing leaves Y unchanged");

        SamusState deeper = Falling(0x16e, 0xb7ff);
        AssertTrue(deeper.TryApplyAerialLanding(bus, level, wasSpinning: false, controllerInput: 0, nmiFrameCounter: 0),
            "one pixel deeper the landing fits");
        AssertEqual((byte)SamusPoseId.NormalLandingRightPose, deeper.Pose, "native update 44759 lands");
        AssertEqual((ushort)0x16d, deeper.YPosition, "the landing moves Samus up clear of slope $55");
        Console.WriteLine("Slope landing: a zero-distance pose-change probe rejects the landing on the clipped frame; the next frame lands.");
    }
}
