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
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(),
            new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(samus.Xray.TryBeginAndActivate(samus, previousMovementType: SamusMovementType.Standing,
            conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay, PowerBombExplosionActive = false, ProjectileCooldownTimer = 0, BombCounter = 0 }), "X-ray admitted");
        return samus;
    }
}
