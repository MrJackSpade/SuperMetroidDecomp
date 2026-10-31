namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Proven parameter-one bits consumed by wall Space Pirate code. The low bit chooses the
/// starting wall; the high bit independently selects the slower retail jump/laser timing.
/// The values are arbitrary bits; the population entry names them.
/// </summary>
[Flags]
public enum WallSpacePirateParameterFlags
{
    /// <summary>No named parameter meaning is set.</summary>
    None = 0,
    /// <summary>Parameter-one bit zero tested by $B2:EF9F: starts the downward-climbing list and detection function on the right wall; clearing it selects the left wall.</summary>
    StartsOnRightWall = 1,
    /// <summary>Parameter-one bit fifteen preserves the native slow jump settings: two byte-angle units per update with right/left endpoint angles $BE/$42 instead of the fast $C0/$40 endpoints.</summary>
    SlowJumpAndLaser = 2,
}
