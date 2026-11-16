namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// What a cue of the wall Space Pirates' animations does: climb a random way, fire a laser left or right, play the attack
/// sound, prepare a jump to the other wall, or begin the cling, attack or jump the Pirate's main AI then runs.
/// </summary>
internal enum WallSpacePirateCueKind
{
    /// <summary>Clings to the left wall, watching Samus as it climbs.</summary>
    ClingToLeftWall,
    /// <summary>Clings to the right wall, watching Samus as it climbs.</summary>
    ClingToRightWall,
    /// <summary>Holds on its wall while its attack fires and readies the jump to the other wall.</summary>
    Attack,
    /// <summary>Jumps across to the left wall.</summary>
    JumpToLeftWall,
    /// <summary>Jumps across to the right wall.</summary>
    JumpToRightWall,
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
