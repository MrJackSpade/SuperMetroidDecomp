namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// What a cue of the ninja Space Pirates' animations does: reset the speed, face Samus, start a divekick, or begin one of
/// the moves the Pirate's main AI then runs.
/// </summary>
internal enum NinjaSpacePirateCueKind
{
    /// <summary>Resets the speed.</summary>
    ResetSpeed,
    /// <summary>Continues at the active stance facing Samus.</summary>
    StartActive,
    /// <summary>Starts the divekick's jump, either way.</summary>
    StartDivekick,
    /// <summary>Stands still while the animation poses the Pirate: a claw throw, a kick, a flinch or a landing.</summary>
    StandStill,
    /// <summary>Waits at its post for Samus to come within reach.</summary>
    WaitForSamus,
    /// <summary>Takes its stance and chooses its next attack.</summary>
    ChooseAttack,
    /// <summary>Takes off on a spin jump to the left post.</summary>
    SpinJumpLeft,
    /// <summary>Takes off on a spin jump to the right post.</summary>
    SpinJumpRight,
    /// <summary>Lands at the jump's peak, ready to divekick.</summary>
    ReadyDivekick,
    /// <summary>Jumps up to divekick to the left.</summary>
    JumpToDivekickLeft,
    /// <summary>Jumps up to divekick to the right.</summary>
    JumpToDivekickRight,
    /// <summary>Dives down to the left.</summary>
    DiveLeft,
    /// <summary>Dives down to the right.</summary>
    DiveRight,
    /// <summary>Walks back left to its post after the divekick.</summary>
    WalkBackToLeftPost,
    /// <summary>Walks back right to its post after the divekick.</summary>
    WalkBackToRightPost,
}
