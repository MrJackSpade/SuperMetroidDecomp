using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Poses;

/// <summary>Compares compiled pose collision radii with retail definitions without letting gameplay read the cartridge.</summary>
internal static class PoseCollisionDefinitionsVerification
{
    /// <summary>Checks every byte-valued pose's vertical radius and live horizontal-radius refresh against native data.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for pose collision definitions")]
    internal static void VerifyPoseCollisionDefinitions(SuperMetroidAddressSpace rom)
    {
        var samus = new SamusState();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            ushort expected = rom.ReadCartridgeByte(SamusSourceDefinitions.PoseDefinitions.Value +
                pose * SamusSourceDefinitions.PoseDefinitionByteCount + 6);
            AssertEqual(expected, SamusPoseMetadata.ReadPoseYRadius((SamusPoseId)pose),
                "Prospective collision radius matches native pose, without authored ROM reads");
            samus.Pose = (SamusPoseId)pose;
            samus.Kinematics.Radius = new(ushort.MaxValue, ushort.MaxValue);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            AssertEqual((ushort)5, samus.Kinematics.Radius.X, "Every pose keeps native horizontal radius");
            AssertEqual(expected, samus.Kinematics.Radius.Y, "Live collision owner receives native vertical radius");
        }
    }
}
