using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SharedSlopeHeightsVerification;

/// <summary>Represents pose collision definitions verification.</summary>
internal static class PoseCollisionDefinitionsVerification
{
    /// <summary>Verifies pose collision definitions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for pose collision definitions")]
    internal static void VerifyPoseCollisionDefinitions(SuperMetroidAddressSpace rom)
    {
        var forbidden = NewSlopeHeightNoReadBus();
        var samus = new SamusState();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            ushort expected = rom.ReadCartridgeByte(SamusMovementRomData.Poses.Definitions +
                pose * SamusMovementRomData.Poses.DefinitionByteCount + 6);
            ISnesAddressSpace source = forbidden;
            AssertEqual(expected, SamusPoseMetadata.ReadPoseYRadius((SamusPoseId)pose),
                "Prospective collision radius matches native pose, without authored ROM reads");
            samus.Pose = (SamusPoseId)pose;
            samus.Kinematics.Radius = new(ushort.MaxValue, ushort.MaxValue);
            samus.PoseMetadata.RefreshCollisionRadii(source);
            AssertEqual((ushort)5, samus.Kinematics.Radius.X, "Every pose keeps native horizontal radius");
            AssertEqual(expected, samus.Kinematics.Radius.Y, "Live collision owner receives native vertical radius");
        }
    }
}
