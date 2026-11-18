namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How fast a wall space pirate's jumps between its walls turn, as its placement says (#2453). Its values are arbitrary.</summary>
internal enum WallSpacePirateJumpSpeed
{
    /// <summary>The jump turns as <see cref="SpacePirateMotionDefinitions.FastWallJump"/> says.</summary>
    Fast,

    /// <summary>The jump turns as <see cref="SpacePirateMotionDefinitions.SlowWallJump"/> says.</summary>
    Slow,
}
