using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Verification.Samus.Poses;

/// <summary>Compares compiled pose collision radii with retail definitions without letting gameplay read the cartridge.</summary>
internal static class PoseCollisionDefinitionsVerification
{
    /// <summary>Checks every byte-valued pose's vertical radius and live horizontal-radius refresh against native data.</summary>
    internal static void VerifyPoseCollisionDefinitions(CartridgeImportAddressSpace rom)
    {
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            ushort expected = rom.ReadCartridgeByte(SamusSourceDefinitions.PoseDefinitions.Value +
                pose * SamusSourceDefinitions.PoseDefinitionByteCount + 6);
            AssertEqual(expected, SamusPoseMetadata.ReadPoseYRadius(SamusPoseNumbering.FromNumber(pose)),
                "Prospective collision radius matches native pose, without authored ROM reads");
            samus.Pose = SamusPoseNumbering.FromNumber(pose);
            samus.Kinematics.Radius = new(ushort.MaxValue, ushort.MaxValue);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            AssertEqual((ushort)5, samus.Kinematics.Radius.X, "Every pose keeps native horizontal radius");
            AssertEqual(expected, samus.Kinematics.Radius.Y, "Live collision owner receives native vertical radius");
        }
    }
}
