namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The Shaktool's saw-hand, arm and head animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="ShaktoolAnimations"/>.
/// </summary>
internal enum ShaktoolSegment
{
    /// <summary>The saw hand head bob primary piece animation.</summary>
    SawHandHeadBobPrimaryPiece,
    /// <summary>The saw hand head bob final piece animation.</summary>
    SawHandHeadBobFinalPiece,
    /// <summary>The saw hand primary piece animation.</summary>
    SawHandPrimaryPiece,
    /// <summary>The saw hand final piece animation.</summary>
    SawHandFinalPiece,
    /// <summary>The arm piece head bob back animation.</summary>
    ArmPieceHeadBobBack,
    /// <summary>The arm piece head bob front animation.</summary>
    ArmPieceHeadBobFront,
    /// <summary>The arm piece normal animation.</summary>
    ArmPieceNormal,
    /// <summary>The head head bob animation.</summary>
    HeadHeadBob,
    /// <summary>
    /// The head aiming left, where its head bob rests until the chain moves again; while the chain orients it, the head
    /// shows the picture its orientation chooses instead (#2284).
    /// </summary>
    HeadAimingLeft,
    /// <summary>The head aiming down, its first pose until the chain first orients it.</summary>
    HeadAimingDown,
}
