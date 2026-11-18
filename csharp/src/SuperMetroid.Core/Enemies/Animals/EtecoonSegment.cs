namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Etecoon's look, run, wall-jump, hop and flex animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="EtecoonAnimations"/>.
/// </summary>
internal enum EtecoonSegment
{
    /// <summary>Looking right at Samus for a moment before it runs off left.</summary>
    LookRightAtSamusAndRunLeft,
    /// <summary>Turning to the left as it starts to run, then running left.</summary>
    BeginRunningLeft,
    /// <summary>Running left, stretching, bunching and pushing off, repeating.</summary>
    RunningLeft,
    /// <summary>The wall jump left's leap.</summary>
    WallJumpLeft,
    /// <summary>The spin a wall jump left plays after its leap, for good.</summary>
    JumpingLeft,
    /// <summary>Crouched to hop facing left, held.</summary>
    HoppingFacingLeft,
    /// <summary>The hop facing left, up to its crest and down again, which hitting a ceiling enters at the crest.</summary>
    ContinueHoppingFacingLeft,
    /// <summary>Clinging to a wall facing left, held, ready to jump off it.</summary>
    WallJumpLeftEligible,
    /// <summary>Looking left at Samus for a moment before it runs off right.</summary>
    LookLeftAtSamusAndRunRight,
    /// <summary>Turning to the right as it starts to run, then running right.</summary>
    BeginRunningRight,
    /// <summary>Running right, stretching, bunching and pushing off, repeating.</summary>
    RunningRight,
    /// <summary>Leaping right off a wall, then spinning as it flies on.</summary>
    WallJumpRight,
    /// <summary>Spinning a quarter turn at a time as it jumps right, repeating.</summary>
    JumpingRight,
    /// <summary>Crouched to hop facing right, held.</summary>
    HoppingFacingRight,
    /// <summary>
    /// Hopping facing right: raising its arms, flinging them out at the top of the hop and lowering them again.
    /// </summary>
    ContinueHoppingFacingRight,
    /// <summary>Clinging to a wall facing right, held, ready to jump off it.</summary>
    WallJumpRightEligible,
    /// <summary>Crouched to hop facing right, the pose it starts in.</summary>
    StartingCrouch,
    /// <summary>The flex's six hop poses, which the flex plays four times.</summary>
    Flexing,
    /// <summary>The flex's two closing poses, the last held.</summary>
    FlexEnd,
}
