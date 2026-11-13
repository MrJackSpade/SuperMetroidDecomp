using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Which halves of the cartridge's split body a pose's picture frame is composed of, and the chest cover the
/// forward-facing Power Suit body shows: the decisions the import makes, once, to compose the whole picture Samus's body
/// draws for each pose and picture frame (#1513, #1585).
/// </summary>
/// <remarks>
/// Every decision follows the pose's own movement type, never the movement Samus's gameplay carries. The drained-Samus
/// commands write a drained pose without initializing it, so her movement stays the type of the pose she was last
/// initialized in, and the game draws the drained pose's halves by that carried type: a carried spin draws a drained body
/// without its legs, a carried ball draws only its top. Drawing half a body because of a carried movement type is a
/// presentation quirk the port does not replicate (owner ruling on #1513: no sprite sheet frame shows half a Samus); the
/// carried movement itself stays gameplay and still governs her movement, collisions and the wall contact's rewind.
/// </remarks>
internal static class SamusBodyCompositions
{
    /// <summary>
    /// The halves the whole picture of <paramref name="pose"/> at picture frame <paramref name="pictureFrame"/> is composed
    /// of: its top half, its bottom half unless the pose's own movement type makes the top half the complete body, and the
    /// chest cover between them for the forward-facing Power Suit body.
    /// </summary>
    /// <exception cref="InvalidDataException">The pose has no movement type, or the forward-facing Power Suit frame is not installed.</exception>
    internal static SamusBodyComposition Of(SamusPoseId pose, int pictureFrame, SamusBodySources artwork)
    {
        Ensure.NotNull(artwork);
        return new SamusBodyComposition(
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
        // The draw suppresses the ordinary spin-jump bottom half for frames one through ten, whose top halves are the
        // complete curled body. Frame zero and frames from eleven draw the split bottom. The Space Jump and Screw Attack
        // poses always draw the bottom half.
        bool ordinarySpinBottom = movementType != SamusMovementType.SpinJumping ||
            pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose or
                SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose ||
            animationFrame == 0 || animationFrame >= 11;

        // The death-sequence body is the knockback movement's only exception: its first three frames' top halves are the
        // complete body. All other knockback poses and later frames keep an ordinary lower half.
        bool knockbackBottom = movementType != SamusMovementType.Knockback ||
            pose is not (SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose) ||
            animationFrame >= 3;

        // The posture-transition draw is a pose-and-frame dispatcher, not one rule for the whole movement type. The
        // crouch and stand transitions and the aimed transitions always draw a lower half; the morph and unmorph bodies'
        // top halves are the complete curling body. The other posture transitions are never given to Samus, so no frame
        // of theirs is drawn.
        bool transitionBottom = movementType != SamusMovementType.PostureTransition ||
            pose is not (SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or
                SamusPoseId.UnmorphingTransitionRightPose or SamusPoseId.UnmorphingTransitionLeftPose);

        bool wallJumpBottom = movementType != SamusMovementType.WallJumping ||
            animationFrame < 3 || animationFrame >= 13;
        bool damageBoostBottom = movementType != SamusMovementType.DamageBoost ||
            animationFrame < 2 || animationFrame >= 9;
        // The draw suppresses the lower half for vertical shinesparks and for the drained crouch's frames zero and one.
        // Every other special-movement pose draws it.
        bool specialMovementBottom = movementType != SamusMovementType.Special ||
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
            ordinarySpinBottom && knockbackBottom && transitionBottom &&
            wallJumpBottom && damageBoostBottom && specialMovementBottom;
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
    /// the whole picture keeps the cover where it sits on the body, a presentation quirk the port does not replicate
    /// (owner ruling on #1513).
    /// </remarks>
    private static Offset16<ScreenPixels>? ChestCover(SamusPoseId pose, int animationFrame, SamusBodySources artwork) =>
        pose == SamusPoseId.ForwardFacingPowerSuitPose
            ? Offset16<ScreenPixels>.Of(-7, artwork.Gameplay.OriginRise(pose, animationFrame) - 17)
            : null;
}
