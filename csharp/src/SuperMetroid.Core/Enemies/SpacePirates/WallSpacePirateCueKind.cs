namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a cue of the wall Space Pirates' animations does: one per kind of step its lists use.</summary>
internal enum WallSpacePirateCueKind
{
    /// <summary>Picks a random climb direction and continues at that left-wall climb.</summary>
    ChooseLeftWallDirection,
    /// <summary>Picks a random climb direction and continues at that right-wall climb.</summary>
    ChooseRightWallDirection,
    /// <summary>Fires a laser left.</summary>
    FireLaserLeft,
    /// <summary>Fires a laser right.</summary>
    FireLaserRight,
    /// <summary>Plays the attack sound.</summary>
    PlayAttackSound,
    /// <summary>Prepares the jump to the left wall.</summary>
    PrepareJumpLeft,
    /// <summary>Prepares the jump to the right wall.</summary>
    PrepareJumpRight,
}
