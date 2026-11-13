using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Hud;

using SuperMetroid.Verification.Harness;

/// <summary>Checks that an item chosen for auto-cancel returns the HUD to the beams when its use ends (#1704).</summary>
internal static class AutoCancelledItemVerification
{
    /// <summary>
    /// The grapple's four endings (a firing cancellation, a release from a swing, a drop and a wall jump), each with the
    /// Grapple chosen while Item Cancel was held and chosen without it: as each ending runs, an auto-cancelled Grapple
    /// returns the HUD to the beams and spends the auto-cancel, and a Grapple chosen without it stays selected.
    /// </summary>
    internal static void VerifyGrappleEndReturnsAutoCancelledItem()
    {
        var air = new RoomLevelData(new Extent16<RoomBlocks>(16, 16),
            new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]),
            RoomLevelData.NothingBeyondGrid());
        foreach ((string ending, GrapplePhase phase, SamusPoseId pose) in new[]
        {
            ("firing cancellation", GrapplePhase.CancelPending, SamusPoseId.FacingRightNormalPose),
            ("release from a swing", GrapplePhase.ReleaseFromSwing, SamusPoseId.GrappleSwingRightPose),
            ("drop", GrapplePhase.Dropped, SamusPoseId.GrappleSwingRightPose),
            ("wall jump", GrapplePhase.WallJumping, SamusPoseId.GrappleWallContactLeftPose),
        })
        foreach (bool autoCancelled in new[] { true, false })
        {
            SamusState samus = InstalledFixtures.CreateSamus();
            samus.Pose = pose;
            samus.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.EquippedItems = SamusEquipmentFlags.GrappleBeam;
            samus.SelectedHudItem = HudItem.Grapple;
            samus.AutoCancelHudItemIndex = autoCancelled ? HudItem.Grapple : HudItem.Beam;
            samus.Grapple.Phase = phase;
            samus.LiquidPhysics.BeginFrameSoundRequests();

            GrappleMovementResult ended = phase == GrapplePhase.CancelPending
                ? SamusGrappleMovement.CompleteFiringCancellation(air, samus)
                : SamusGrappleMovement.Step(new SamusTerrain { Level = air }, samus, 0, 0);

            AssertEqual(GrapplePhase.Inactive, ended.Phase, $"the grapple's {ending} ends it");
            AssertEqual(autoCancelled ? HudItem.Beam : HudItem.Grapple, samus.SelectedHudItem,
                autoCancelled
                    ? $"the grapple's {ending} returns an auto-cancelled Grapple to the beams"
                    : $"the grapple's {ending} keeps a Grapple chosen without Item Cancel");
            AssertEqual(HudItem.Beam, samus.AutoCancelHudItemIndex, $"the grapple's {ending} leaves no auto-cancel");
        }
        Console.WriteLine("  Auto-cancel: each of the grapple's four endings returns an auto-cancelled Grapple to the beams.");
    }

    /// <summary>
    /// X-ray chosen while Item Cancel was held and chosen without it, its beam run from its start through its warm-up,
    /// widening and restoration with Dash released: when the beam ends, an auto-cancelled X-ray returns the HUD to the
    /// beams and spends the auto-cancel, and an X-ray chosen without it stays selected.
    /// </summary>
    internal static void VerifyXrayEndReturnsAutoCancelledItem()
    {
        foreach (bool autoCancelled in new[] { true, false })
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
                SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = SamusPoseId.FacingRightNormalPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                Health = 99,
                MaxHealth = 99,
                EquippedItems = SamusEquipmentFlags.XrayScope,
                SelectedHudItem = HudItem.XRay,
                AutoCancelHudItemIndex = autoCancelled ? HudItem.XRay : HudItem.Beam,
            };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            AssertTrue(samus.Xray.TryBegin(samus, SamusMovementType.Standing,
                new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "the fixture starts X-ray");
            while (!samus.Xray.WarmUp.IsOver)
                samus.Xray.StepBeam(samus, controllerInput: 0);
            while (samus.Xray.BeamPhase != XrayBeamPhase.Finish)
            {
                samus.Xray.StepBeam(samus, controllerInput: 0);
                AssertEqual(HudItem.XRay, samus.SelectedHudItem, "X-ray stays selected while its beam runs");
            }
            samus.Xray.StepBeam(samus, controllerInput: 0);

            AssertTrue(!samus.Xray.IsActive, "the beam's end ends X-ray");
            AssertEqual(autoCancelled ? HudItem.Beam : HudItem.XRay, samus.SelectedHudItem,
                autoCancelled
                    ? "X-ray's end returns an auto-cancelled X-ray to the beams"
                    : "X-ray's end keeps an X-ray chosen without Item Cancel");
            AssertEqual(HudItem.Beam, samus.AutoCancelHudItemIndex, "X-ray's end leaves no auto-cancel");
        }
        Console.WriteLine("  Auto-cancel: X-ray's end returns an auto-cancelled X-ray to the beams.");
    }

    /// <summary>Runs the auto-cancelled item checks.</summary>
    [VerificationCommand("--auto-cancelled-item")]
    internal static int AutoCancelledItemCommand()
    {
        Suite(nameof(VerifyGrappleEndReturnsAutoCancelledItem), () => VerifyGrappleEndReturnsAutoCancelledItem());
        Suite(nameof(VerifyXrayEndReturnsAutoCancelledItem), () => VerifyXrayEndReturnsAutoCancelledItem());
        return 0;
    }
}
