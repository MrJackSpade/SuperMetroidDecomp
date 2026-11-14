using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>
/// Confirms #1535: every pose the Baby's Hyper Beam freeze and rainbow stop can install by a frame read past a
/// transition's program is a pose Samus can hold, and every frame such a pose shows resolves to a picture on the installed
/// body sheet; the Draygon-held ball, held without Draygon, runs the Draygon-held movement and stays where it is.
/// </summary>
internal static class SamusOverreadHeldPoseFramesVerification
{
    /// <summary>
    /// The poses the over-read installs, derived from the programs' layout (#1535): the glitch balls, which the read past the
    /// right-facing morph runs into the glitch-ball morphing programs' transitions to install; their knockbacks, which damage
    /// to a glitch ball installs; the Draygon-held ball, which the left-facing unmorph frozen for the Hyper Beam reads into
    /// the Draygon-held morphing program to install; and its stand-up, which Up selects from the ball.
    /// </summary>
    private static readonly SamusPoseId[] OverreadHeldPoses =
    [
        SamusPoseId.GlitchBallRightPose,
        SamusPoseId.GlitchBallLeftPose,
        SamusPoseId.GlitchBallKnockbackRightPose,
        SamusPoseId.GlitchBallKnockbackLeftPose,
        SamusPoseId.DraygonHeldBallLeftPose,
        SamusPoseId.DraygonHeldStandingUpLeftPose,
    ];

    /// <summary>Checks every over-read pose's frames against the installed body sheet.</summary>
    internal static void VerifyOverreadHeldPoseFrames()
    {
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.SamusBody);
        SamusBodySources body = InstalledFixtures.SamusBody;
        SamusSpritemapSources spritemaps = body.Spritemaps;
        SamusShownFrames shown = SamusShownFrames.Of(InstalledFixtures.FramesBeyondAnimation);
        SamusShownBody[] bodies = [.. shown.Bodies(body)];
        int resolved = 0;
        foreach (SamusPoseId pose in OverreadHeldPoses)
        {
            AssertTrue(!SamusUnreachablePoseDefinitions.Poses.Contains(pose), $"{pose} is held, so it is not listed as never held");
            AssertTrue(SamusShownFrames.Poses.Contains(pose), $"{pose} is one of the poses the body can show");
            AssertTrue(shown.pictured[pose].Count > 0, $"{pose} pictures at least one frame");
            foreach (int frame in shown.pictured[pose])
            {
                AssertTrue(body.TryPictures(pose, frame, out _), $"{pose} frame {frame} has installed pictures");
                SamusShownBody[] drawnAs = [.. bodies.Where(drawn => drawn.Frame.Pose == pose && drawn.Frame.PictureFrame == frame)];
                AssertTrue(drawnAs.Length > 0, $"{pose} frame {frame} is drawn as a body");
                foreach (SamusShownBody drawn in drawnAs)
                {
                    AssertTrue(spritemaps.PictureKey(drawn.Composition, drawn.Pictures) is not null,
                        $"{pose} frame {frame} is composed of a body picture");
                    // ShowsNothing throws when the sheet holds no such frame at all.
                    AssertTrue(!sheet.ShowsNothing(drawn.Frame.Name), $"{pose} frame {frame} is pictured on the body sheet as {drawn.Frame.Name}");
                    resolved++;
                }
            }
        }

        // The Draygon-held ball has the Draygon-held movement type, whose normal entry only clears the vertical collision
        // result, so without Draygon placing her she does not move.
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.DraygonHeldBallLeftPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)),
            SolidVerticalCollisionResult = SamusVerticalCollisionResult.Landed,
        };
        SamusDraygonGrabbedState.StepMovement(samus);
        AssertEqual(SamusVerticalCollisionResult.None, samus.SolidVerticalCollisionResult, "the Draygon-held ball's movement clears the vertical collision result");
        AssertEqual(0x0080, samus.Position.X.Whole, "the Draygon-held ball does not move horizontally");
        AssertEqual(0x0100, samus.Position.Y.Whole, "the Draygon-held ball does not move vertically");
        Console.WriteLine($"Over-read held poses: {OverreadHeldPoses.Length} poses resolve {resolved} drawn bodies on the body sheet.");
    }

    /// <summary>Runs the over-read held poses' frame check.</summary>
    [VerificationCommand("--samus-overread-held-pose-frames")]
    internal static int SamusOverreadHeldPoseFramesCommand()
    {
        Suite(nameof(VerifyOverreadHeldPoseFrames), VerifyOverreadHeldPoseFrames);
        return 0;
    }
}
