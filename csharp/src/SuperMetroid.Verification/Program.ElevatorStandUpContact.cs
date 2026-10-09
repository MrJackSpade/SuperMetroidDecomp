using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// <c>$91:F404</c> installs the new pose before <c>$91:FDAE</c> probes the larger body, so
    /// an elevator pseudo-door met by those probes sees the new pose against
    /// <c>$94:938B</c>'s below-$09 gate. In the 13% movie Samus stands up ($28 to $02) on
    /// Kraid's lair elevator; native arms it in that frame and Down boards it next frame.
    /// </summary>
    private static void VerifyElevatorStandUpContact()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const int Width = 4, Height = 6, DoorRow = 4;
        var blocks = new ushort[Width * Height];
        var behaviors = new byte[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            // Tourian's elevator list: BTS 2 is the solid pseudo-door that arms the actor.
            blocks[DoorRow * Width + x] = 0x9000;
            behaviors[DoorRow * Width + x] = 2;
        }
        var level = new RoomLevelData(Width, Height, blocks, behaviors, new ushort[Width * Height], [],
            doorListPointer: 0xdad5);

        var samus = new SamusState { Pose = (byte)SamusPoseId.CrouchingLeftPose, XPosition = 24 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        // Feet on the elevator's top surface; the standing body grows five pixels upward.
        samus.YPosition = (ushort)(DoorRow * 16 - samus.Kinematics.YRadius);
        AssertTrue(level.ConsumeElevatorDoorContact() is false, "no contact before the stand-up");

        AssertTrue(samus.TryApplyDirectCrouchToStandingTransition(bus, level, SamusPoseIds.FacingLeftNormalPose, 0),
            "the standing body fits above the elevator");
        AssertEqual(SamusPoseIds.FacingLeftNormalPose, samus.Pose, "Samus stands");
        AssertTrue(level.ConsumeElevatorDoorContact(),
            "the probe for the new standing pose ($02 < $09) arms the elevator");
        Console.WriteLine("  Elevator stand-up contact: standing up on the elevator arms it in the same frame.");
    }
}
