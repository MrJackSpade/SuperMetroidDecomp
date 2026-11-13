namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The Shaktool's saw-hand, arm and head animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="ShaktoolAnimations"/>.
/// </summary>
internal enum ShaktoolSegment
{
    /// <summary>The saw hand attack primary piece animation. No authored spawn or branch enters it.</summary>
    SawHandAttackPrimaryPiece,
    /// <summary>The saw hand attack final piece animation. No authored spawn or branch enters it.</summary>
    SawHandAttackFinalPiece,
    /// <summary>The saw hand head bob primary piece animation.</summary>
    SawHandHeadBobPrimaryPiece,
    /// <summary>The saw hand head bob final piece animation.</summary>
    SawHandHeadBobFinalPiece,
    /// <summary>The saw hand primary piece animation.</summary>
    SawHandPrimaryPiece,
    /// <summary>The saw hand final piece animation.</summary>
    SawHandFinalPiece,
    /// <summary>The arm piece attack back animation. No authored spawn or branch enters it.</summary>
    ArmPieceAttackBack,
    /// <summary>The arm piece attack front animation. No authored spawn or branch enters it.</summary>
    ArmPieceAttackFront,
    /// <summary>The arm piece head bob back animation.</summary>
    ArmPieceHeadBobBack,
    /// <summary>The arm piece head bob front animation.</summary>
    ArmPieceHeadBobFront,
    /// <summary>The arm piece normal animation.</summary>
    ArmPieceNormal,
    /// <summary>The head attack animation. No authored spawn or branch enters it.</summary>
    HeadAttack,
    /// <summary>The head head bob animation.</summary>
    HeadHeadBob,
    /// <summary>The head aiming left animation.</summary>
    HeadAimingLeft,
    /// <summary>The head aiming up left animation.</summary>
    HeadAimingUpLeft,
    /// <summary>The head aiming up animation.</summary>
    HeadAimingUp,
    /// <summary>The head aiming up right animation.</summary>
    HeadAimingUpRight,
    /// <summary>The head aiming right animation.</summary>
    HeadAimingRight,
    /// <summary>The head aiming down right animation.</summary>
    HeadAimingDownRight,
    /// <summary>The head aiming down animation.</summary>
    HeadAimingDown,
    /// <summary>The head aiming down left animation.</summary>
    HeadAimingDownLeft,
}
