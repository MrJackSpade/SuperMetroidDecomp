namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>What a cue of the Work Robot's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum WorkRobotCueKind
{
    /// <summary>Counts the laser cooldown down (<c>DecrementLaserCooldown</c>, $A8:D16B).</summary>
    CountDownLaserCooldown,
    /// <summary>Plays the footstep sound when the Work Robot is on screen (<c>PlaySFXIfOnScreen</c>, $A8:D091).</summary>
    Footstep,
    /// <summary>Steps backward facing left; a wall redirects the walk (<c>FacingLeft_MoveBackward_HandleHittingWall</c>, $A8:CE85).</summary>
    LeftBackward,
    /// <summary>Steps backward facing left; a wall or ledge redirects the walk (<c>FacingLeft_MoveBackward_HandleWallOrFall</c>, $A8:CDEA).</summary>
    LeftBackwardWatchingLedges,
    /// <summary>Steps forward facing left; a wall redirects the walk (<c>FacingLeft_MoveForward_HandleHittingWall</c>, $A8:CDA4).</summary>
    LeftForward,
    /// <summary>Steps forward facing left; a wall or ledge redirects the walk (<c>FacingLeft_MoveForward_HandleWallOrFall</c>, $A8:CD09).</summary>
    LeftForwardWatchingLedges,
    /// <summary>Steps backward facing right; a wall redirects the walk (<c>FacingRight_MoveBackward_HandleHittingWall</c>, $A8:D04B).</summary>
    RightBackward,
    /// <summary>Steps backward facing right; a wall or ledge redirects the walk (<c>FacingRight_MoveBackward_HandleWallOrFall</c>, $A8:CFB0).</summary>
    RightBackwardWatchingLedges,
    /// <summary>Steps forward facing right; a wall redirects the walk (<c>FacingRight_MoveForward_HandleHittingWall</c>, $A8:CF6A).</summary>
    RightForward,
    /// <summary>Steps forward facing right; a wall or ledge redirects the walk (<c>FacingRight_MoveForward_HandleWallOrFall</c>, $A8:CECF).</summary>
    RightForwardWatchingLedges,
    /// <summary>Fires a down-left laser when the cooldown allows, continuing at its firing animation (<c>TryShootingLaserDownLeft</c>, $A8:D13D).</summary>
    TryLaserDownLeft,
    /// <summary>Fires a down-right laser when the cooldown allows, continuing at its firing animation (<c>TryShootingLaserDownRight</c>, $A8:D131).</summary>
    TryLaserDownRight,
    /// <summary>Fires a laser to the left when the cooldown allows, continuing at its firing animation (<c>TryShootingLaserLeft</c>, $A8:D107).</summary>
    TryLaserLeft,
    /// <summary>Fires a laser to the right when the cooldown allows, continuing at its firing animation (<c>TryShootingLaserRight</c>, $A8:D100).</summary>
    TryLaserRight,
    /// <summary>Fires an up-left laser when the cooldown allows, continuing at its firing animation (<c>TryShootingLaserUpLeft</c>, $A8:D0D2).</summary>
    TryLaserUpLeft,
    /// <summary>Fires an up-right laser when the cooldown allows, continuing at its firing animation (<c>TryShootingLaserUpRight</c>, $A8:D0C6).</summary>
    TryLaserUpRight,
    /// <summary>Continues at the left-facing walk (<c>Goto_FacingLeft_WalkingForwards</c>, $A8:D0C2).</summary>
    WalkLeft,
    /// <summary>Continues at the right-facing walk (<c>SetInstListTo_FacingRight_WalkingForwards</c>, $A8:CECB).</summary>
    WalkRight,
}
