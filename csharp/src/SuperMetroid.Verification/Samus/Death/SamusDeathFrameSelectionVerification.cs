using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Death;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Confirms which death picture Samus shows on each update (#1406): the death sequence selects the explosion frame of
/// its pre-coloured sheet at the update the cartridge queued its uploads and drew that frame, with no upload left.
/// </summary>
internal static class SamusDeathFrameSelectionVerification
{
    /// <summary>The update on which the suit explosion first shows frame zero: after sixteen pose calls and sixty flash calls.</summary>
    private const int FirstExplosionUpdate = 76;

    /// <summary>
    /// The first update showing each explosion frame. Frame zero shows for twenty updates (its 21-call hold is stepped once
    /// on the update it begins), each later frame for its authored dwell of 6, 3, 4, 5, 5, 6, 6 and 80 updates.
    /// </summary>
    private static readonly int[] FrameStarts = [FirstExplosionUpdate, 96, 102, 105, 109, 114, 119, 125, 131];

    /// <summary>The update on which the sequence completes without drawing a tenth frame.</summary>
    private const int CompletionUpdate = 211;

    /// <summary>Steps the death sequence facing each way and asserts the phase, the selected explosion frame and the sheet frame drawn on every update.</summary>
    internal static void VerifySamusDeathFrameSelection()
    {
        foreach (bool facingLeft in new[] { false, true })
        {
            SamusState samus = (new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            }.InPose(facingLeft ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.DeathSequence.Begin(samus, cameraPosition: new(0, 0));
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.SamusDeath);
            HorizontalDirection facing = facingLeft ? HorizontalDirection.Left : HorizontalDirection.Right;
            var samusColours = new SamusColours();
            for (int update = 1; update <= CompletionUpdate; update++)
            {
                SamusDeathSequenceStepResult step = samus.DeathSequence.Step(samus, (SamusDeathSequencePhase.Begun)samus.DeathSequence.Phase, samusColours,
                    InstalledFixtures.SamusBody.Gameplay.DeathSteps);
                string context = $"death facing {facing}, update {update}";
                int? expectedFrame = ExpectedFrame(update);
                AssertEqual(ExpectedPhase(update), samus.DeathSequence.Phase, context + ": phase");
                AssertEqual(expectedFrame is int expected ? SamusDeathSheetDefinitions.Explosion[expected] : null,
                    samus.DeathSequence.ExplosionFrame, context + ": selected explosion frame");
                AssertEqual(update < FirstExplosionUpdate, step.Draws == SamusDeathDraw.Pose, context + ": draws the death pose");
                AssertEqual(expectedFrame is not null, step.Draws == SamusDeathDraw.Explosion, context + ": draws the explosion");
                AssertEqual(update == CompletionUpdate, step.Completed, context + ": completes");

                var sprites = new SpriteFrameBuffer();
                sprites.BeginFrame();
                samus.DeathSequence.DrawExplosion(sprites, InstalledFixtures.SpriteSheets);
                sprites.FinalizeFrame();
                if (expectedFrame is int frame)
                {
                    SpriteSheetFrame expectedImage = sheet.Frame(SamusDeathSheetDefinitions.ExplosionFrame(facing, SamusDeathSheetDefinitions.Explosion[frame]));
                    AssertTrue(sprites.Draws.Length == 1 && sprites.Draws[0] is SamusColouredDraw image &&
                            ReferenceEquals(expectedImage, image.Image),
                        context + $": draws sheet frame {SamusDeathSheetDefinitions.ExplosionFrame(facing, SamusDeathSheetDefinitions.Explosion[frame])}");
                }
                else
                {
                    AssertEqual(0, sprites.Draws.Length, context + ": draws no explosion");
                }
            }
            AssertEqual(new SamusDeathSequencePhase.Begun.Complete(), samus.DeathSequence.Phase, $"death facing {facing}: completes");
        }
        Console.WriteLine($"Samus death frame selection: phase, explosion frame and sheet frame match on all {CompletionUpdate} updates facing each way.");
    }

    /// <summary>The explosion frame shown on <paramref name="update"/>, or null before the explosion and on completion.</summary>
    private static int? ExpectedFrame(int update)
    {
        if (update is < FirstExplosionUpdate or >= CompletionUpdate)
            return null;
        int frame = 0;
        while (frame + 1 < FrameStarts.Length && update >= FrameStarts[frame + 1])
            frame++;
        return frame;
    }

    /// <summary>The phase after <paramref name="update"/>: sixteen pose calls, sixty flash calls, the explosion, then completion.</summary>
    private static SamusDeathSequencePhase ExpectedPhase(int update) => update switch
    {
        < 16 => new SamusDeathSequencePhase.Begun.PreFlashing(),
        < FirstExplosionUpdate => new SamusDeathSequencePhase.Begun.Flashing(),
        < CompletionUpdate => new SamusDeathSequencePhase.Begun.SuitExplosion(),
        _ => new SamusDeathSequencePhase.Begun.Complete(),
    };

    /// <summary>Runs the death frame-selection fixture.</summary>
    [VerificationCommand("--samus-death-frame-selection")]
    internal static int SamusDeathFrameSelectionCommand()
    {
        Suite(nameof(VerifySamusDeathFrameSelection), VerifySamusDeathFrameSelection);
        return 0;
    }
}
