namespace SuperMetroid.Core.Game;

/// <summary>
/// Proven parameter-one bits consumed by wall Space Pirate code. The low bit chooses the
/// starting wall; the high bit independently selects the slower retail jump/laser timing.
/// No other parameter-one bits are interpreted by <c>$B2:EF9F-$F11F</c>.
/// </summary>
[Flags]
public enum WallSpacePirateParameterFlags : ushort
{
    /// <summary>Parameter-one bit zero tested by $B2:EF9F: starts the downward-climbing list and detection function on the right wall; clearing it selects the left wall.</summary>
    StartsOnRightWall = 0x0001,
    /// <summary>Parameter-one bit fifteen preserves the native slow jump settings: two byte-angle units per update with right/left endpoint angles $BE/$42 instead of the fast $C0/$40 endpoints.</summary>
    SlowJumpAndLaser = 0x8000,
}
