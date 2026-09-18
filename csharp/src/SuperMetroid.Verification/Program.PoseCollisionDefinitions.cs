using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPoseCollisionDefinitions(SuperMetroidAddressSpace rom)
    {
        var forbidden = new SlopeHeightNoReadBus();
        var samus = new SamusState();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            ushort expected = rom.ReadByte(SamusMovementRomData.Poses.Definitions +
                pose * SamusMovementRomData.Poses.DefinitionByteCount + 6);
            ISnesAddressSpace source = forbidden;
            AssertEqual(expected, SamusState.ReadPoseYRadius(source, (byte)pose),
                "Prospective collision radius matches native pose, without authored ROM reads");
            samus.Pose = (byte)pose;
            samus.Kinematics.XRadius = samus.Kinematics.YRadius = ushort.MaxValue;
            samus.RefreshCollisionRadii(source);
            AssertEqual((ushort)5, samus.Kinematics.XRadius, "Every pose keeps native horizontal radius");
            AssertEqual(expected, samus.Kinematics.YRadius, "Live collision owner receives native vertical radius");
        }
    }
}
