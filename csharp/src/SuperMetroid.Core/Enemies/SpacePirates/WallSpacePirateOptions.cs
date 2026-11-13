namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Independently tested options of a wall Space Pirate: the wall it starts on, and the slower jump and laser timing.
/// The values are arbitrary bits; the population entry names them.
/// </summary>
[Flags]
public enum WallSpacePirateOptions
{
    /// <summary>It starts on the left wall with the fast jump and laser.</summary>
    None = 0,
    /// <summary>Starts the downward-climbing animation and detection action on the right wall; clear selects the left wall.</summary>
    StartsOnRightWall = 1,
    /// <summary>
    /// The slow jump and laser timing: the jump turns two 256ths of a turn per update and ends two 256ths of a turn
    /// inside each of the fast jump's endpoint angles.
    /// </summary>
    SlowJumpAndLaser = 2,
}
