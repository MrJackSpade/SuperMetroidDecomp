using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Which body halves a pose's picture frame shows, where the body is drawn relative to Samus, and the chest cover the
/// forward-facing Power Suit body shows: the decisions that make the one full-body picture Samus's body draws each update
/// (#1513). The draw and the import that composes the body sheet both make them here.
/// </summary>
/// <remarks>
/// Every decision follows the pose's own movement type, never the movement Samus's gameplay carries. The drained-Samus
/// commands write a drained pose without initializing it, so her movement stays the type of the pose she was last
/// initialized in, and the game draws the drained pose's halves by that carried type: a carried spin draws a drained body
/// without its legs, a carried ball draws only its top. Drawing half a body because of a carried movement type is a
/// presentation quirk the port does not replicate (owner ruling on #1513: no sprite sheet frame shows half a Samus); the
/// carried movement itself stays gameplay and still governs her movement, collisions and the wall contact's rewind.
/// </remarks>
internal static class SamusBodyDrawing
{
    /// <summary>
    /// The full body an update draws for <paramref name="pose"/> at picture frame <paramref name="pictureFrame"/>: its top
    /// half, its bottom half unless the pose's own movement type makes the top half the complete body, and the chest cover
    /// between them for the forward-facing Power Suit body. <see cref="SamusShownFrames.PictureFrame"/> gives the picture
    /// frame an animation frame shows.
    /// </summary>
    /// <exception cref="InvalidDataException">The pose has no movement type, or the body's placement lacks installed art.</exception>
    internal static SamusBodyFrame Frame(SamusPoseId pose, int pictureFrame, InstalledSamusBodyArtwork artwork)
    {
        Ensure.NotNull(artwork);
        return new SamusBodyFrame(
            new SamusBodyHalfFrame(pose, SamusBodyHalf.Top, pictureFrame),
            ShowsBottomHalf(pose, pictureFrame) ? new SamusBodyHalfFrame(pose, SamusBodyHalf.Bottom, pictureFrame) : null,
            ChestCover(pose, pictureFrame, artwork));
    }

    /// <summary>
    /// Whether the body's picture frame shows its bottom half, porting the draw's complete lower-half dispatcher for the
    /// pose's own movement type: a frame whose top half is the complete body shows none.
    /// </summary>
    /// <exception cref="InvalidDataException">The pose has no movement type.</exception>
    private static bool ShowsBottomHalf(SamusPoseId pose, int animationFrame)
    {
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(pose);
        // The draw suppresses the ordinary spin-jump bottom half for art frames 1..A;
        // those frames' top spritemaps contain the complete curled body. Frame zero and
        // frames B+ draw the split bottom. The admitted Screw/Space Jump records use their
        // separate rule and always draw the bottom half at every animation frame.
        bool ordinarySpinBottom = movementType != SamusMovementType.SpinJumping ||
            pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose or
                SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose ||
            animationFrame == 0 || animationFrame >= 11;

        // The Crystal-Flash-end body is movement type `$0A`'s only exception. The first three frames of
        // the `$D7/$D8` Crystal-Flash-end/fatal-damage body are complete top spritemaps;
        // all other knockback-family poses and later frames retain an ordinary lower half.
        bool knockbackBottom = movementType != SamusMovementType.Knockback ||
            pose is not (SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose) ||
            animationFrame >= 3;

        // The posture-transition draw is a pose-and-frame dispatcher, not a blanket type-$0F
        // policy. Basic crouch/stand transitions always use a lower half. Morph/unmorph
        // bodies below `$DB` never do. `$DB/$DC` draw it only on frame zero, `$DD-$F0`
        // only on frame two, and the aimed transition family `$F1+` always draws it.
        bool transitionBottom = movementType != SamusMovementType.PostureTransition ||
            !SamusPoseNumbering.Precedes(pose, SamusPoseId.CrouchingTransitionAimUpRightPose) ||
            pose is SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or
                SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose ||
            (pose is SamusPoseId.UnusedPoseDb or SamusPoseId.UnusedPoseDc &&
                animationFrame == 0) ||
            (!SamusPoseNumbering.Precedes(pose, SamusPoseId.UnusedPoseDd) &&
                SamusPoseNumbering.Precedes(pose, SamusPoseId.CrouchingTransitionAimUpRightPose) && animationFrame == 2);

        // The otherwise-unused movement type `$0D` still has executable game logic
        // in the draw: UnusedPose65 and UnusedPose66 draw a lower half only on frame zero, while every
        // other type-`$0D` pose follows the ordinary always-split return.
        bool unusedTypeDBottom = movementType != SamusMovementType.Unused0D ||
            pose is not (SamusPoseId.UnusedPose65 or SamusPoseId.UnusedPose66) ||
            animationFrame < 1;

        bool wallJumpBottom = movementType != SamusMovementType.WallJumping ||
            animationFrame < 3 || animationFrame >= 13;
        bool damageBoostBottom = movementType != SamusMovementType.DamageBoost ||
            animationFrame < 2 || animationFrame >= 9;
        // The draw suppresses the lower half for vertical shinesparks and for drained
        // crouch/fall byte indices zero and one. Every other type-$1B record draws it.
        bool specialType1BBottom = movementType != SamusMovementType.Special ||
            (pose is not (SamusPoseId.ShinesparkVerticalRightPose or SamusPoseId.ShinesparkVerticalLeftPose) &&
             (pose is not (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose) ||
              animationFrame >= 2));
        return movementType is not (
            SamusMovementType.MorphBallGround or
            SamusMovementType.UnusedGlitchBall or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or
            SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling) &&
            ordinarySpinBottom && knockbackBottom && transitionBottom && unusedTypeDBottom &&
            wallJumpBottom && damageBoostBottom && specialType1BBottom;
    }

    /// <summary>
    /// How far above Samus's drawn point the body's origin sits for <paramref name="pose"/> at picture frame
    /// <paramref name="animationFrame"/>, by the pose's own movement type: movement type zero uses the standing position
    /// selector, `$0F/$1B` use their explicit table-backed selectors, and every other pose the pose's graphics offset. The
    /// origin goes with the picture: a mapped frame is placed as the picture frame it shows.
    /// </summary>
    /// <exception cref="InvalidDataException">The pose has no movement type, or the frame's offset lacks installed art.</exception>
    internal static int OriginRise(SamusPoseId pose, int animationFrame, InstalledSamusBodyArtwork artwork)
    {
        Ensure.NotNull(artwork);
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(pose);
        if (movementType == SamusMovementType.Standing &&
            pose is SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose &&
            animationFrame >= 2)
        {
            // The front-facing draw uses the ordinary pose graphics offset for front-facing
            // frames zero/one, then pins every later frame exactly one pixel above Samus's
            // center.
            return 1;
        }
        if (movementType == SamusMovementType.Standing &&
            SamusPoseNumbering.InRange(pose, SamusPoseId.NormalLandingRightPose, SamusPoseId.SpinLandingLeftPose))
        {
            // The landing offset indexes sixteen packed bytes. The game reads a word there, whose high byte, the next
            // frame's byte, the drawn position never shows (#1397), so the body is placed by the frame's own byte (#1500).
            int landingOffsetIndex = SamusPoseNumbering.Offset(pose, SamusPoseId.NormalLandingRightPose) * 4 + animationFrame;
            if (!artwork.TryLandingYOffset(landingOffsetIndex, out int landingOffset))
                throw new InvalidDataException($"Samus landing offset {landingOffsetIndex} lacks installed art.");
            return landingOffset;
        }
        if (movementType == SamusMovementType.PostureTransition &&
            !SamusPoseNumbering.Precedes(pose, SamusPoseId.CrouchingTransitionRightPose) &&
            SamusPoseNumbering.Precedes(pose, SamusPoseId.MorphBallGroundLeftPose))
        {
            // The posture-transition offset indexes a signed byte by `2*(pose-$35)+animation frame` instead
            // of using the pose-definition graphics offset. Installed visual bytes
            // retain ordinary crouch/morph/stand/unmorph values and the four
            // valid-but-unused zero records `$39/$3A/$3F/$40`. An out-of-range
            // selection rejects; neighboring bytes are not a runtime fallback.
            int postureIndex = SamusPoseNumbering.Offset(pose, SamusPoseId.CrouchingTransitionRightPose) * 2 + animationFrame;
            if (!artwork.TryPostureYOffset(postureIndex, out int transitionOffset))
                throw new InvalidDataException($"Samus posture offset {postureIndex} lacks installed art.");
            return -transitionOffset;
        }
        if (pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
        {
            // The drained-body offset indexes a shared 32-byte table directly with the
            // animation byte index. Several indices intentionally name command operands,
            // because the external controller can publish those literal indices for a
            // visible frame. The installed 32-byte table preserves that odd layout.
            if (!artwork.TryDrainedYOffset(animationFrame, out int drainedOffset))
                throw new InvalidDataException($"Samus drained offset {animationFrame} lacks installed art.");
            return -drainedOffset;
        }
        // The drained-body draw replaces the usual pose graphics offset with -3 after
        // standing drained art reaches byte index five.
        if (pose is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose && animationFrame >= 5)
            return 3;
        // The draw sign-extends the byte at pose-definition offset four. FacingRightNormalPose stores
        // +6, moving the art origin six pixels above Samus's world-space center.
        return artwork.GraphicsYOffset(pose);
    }

    /// <summary>
    /// Where the chest cover sits relative to the body's origin, or none for a pose without it. The game draws one
    /// small sprite between the top and bottom halves when the unsuited Samus faces the screen; it is not a visor, but
    /// covers the left side of the Power Suit's chest. The suited forward body has that shape in its own art and shows
    /// no cover. The game places it seven pixels left of and seventeen above Samus's world point, which is the body's
    /// drawn point offset by its origin rise.
    /// </summary>
    /// <remarks>
    /// The game positions the cover from the unrotated world point while Ceres's turning floor rotates the body's;
    /// the full-body picture keeps the cover where it sits on the body, a presentation quirk the port does not replicate
    /// (owner ruling on #1513).
    /// </remarks>
    private static Offset16<ScreenPixels>? ChestCover(SamusPoseId pose, int animationFrame, InstalledSamusBodyArtwork artwork) =>
        pose == SamusPoseId.ForwardFacingPowerSuitPose
            ? Offset16<ScreenPixels>.Of(-7, OriginRise(pose, animationFrame, artwork) - 17)
            : null;
}
