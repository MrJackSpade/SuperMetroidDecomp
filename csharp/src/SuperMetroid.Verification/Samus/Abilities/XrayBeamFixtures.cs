using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Samus states the X-ray beam's timing checks start from.</summary>
internal static class XrayBeamFixtures
{
    /// <summary>A standing, right-facing Samus whose X-ray has just been admitted.</summary>
    internal static SamusState AdmittedStanding()
    {
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(),
            new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(samus.Xray.TryBegin(samus, previousMovementType: SamusMovementType.Standing,
            conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "X-ray admitted");
        return samus;
    }
}
