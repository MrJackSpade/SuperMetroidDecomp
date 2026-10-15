namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Independently tested meanings in walking Pirate population parameter one. No other bits
/// are named: unknown/modded values remain losslessly available through the raw slot word.
/// </summary>
[Flags]
public enum WalkingSpacePirateParameterFlags : ushort
{
    /// <summary>Population parameter-one bit 0, tested by <c>InitAI_PirateWalking</c> at $B2:FD0B: selects the right-facing initial animation; clear selects left, independently of bit 15.</summary>
    StartsFacingRight = 0x0001,
    /// <summary>Population parameter-one bit 15: $B2:FD3B enables the projectile-flinch check after normal AI, and the inherited laser parameter selects two rather than four horizontal pixels per projectile update.</summary>
    SlowLaserAndProjectileFlinch = 0x8000,
}
