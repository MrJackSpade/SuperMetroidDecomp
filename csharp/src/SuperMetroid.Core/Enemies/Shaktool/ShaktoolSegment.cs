namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The Shaktool's saw-hand, arm and head animations (#641).
/// Their steps are <see cref="ShaktoolAnimations"/>.
/// </summary>
internal enum ShaktoolSegment
{
    /// <summary>The saw hand's inner piece pausing while the head bobs, then spinning.</summary>
    SawHandHeadBobPrimaryPiece,
    /// <summary>
    /// The saw hand's outer piece pausing while the head bobs, then setting the Shaktool moving again and spinning.
    /// </summary>
    SawHandHeadBobFinalPiece,
    /// <summary>The saw hand's inner piece turning a twelfth of a turn at a time, slowly, repeating.</summary>
    SawHandPrimaryPiece,
    /// <summary>The saw hand's outer piece turning a twelfth of a turn at a time, quickly, repeating.</summary>
    SawHandFinalPiece,
    /// <summary>A back arm piece lowering and rising again while the head bobs, then held.</summary>
    ArmPieceHeadBobBack,
    /// <summary>A front arm piece lowering and rising again a little later while the head bobs, then held.</summary>
    ArmPieceHeadBobFront,
    /// <summary>An arm piece, held.</summary>
    ArmPieceNormal,
    /// <summary>The head lowering and rising as it bobs, then aiming left.</summary>
    HeadHeadBob,
    /// <summary>
    /// The head aiming left, where its head bob rests until the chain moves again; while the chain orients it, the head
    /// shows the picture its orientation chooses instead (#2284).
    /// </summary>
    HeadAimingLeft,
    /// <summary>The head aiming down, its first pose until the chain first orients it.</summary>
    HeadAimingDown,
}
