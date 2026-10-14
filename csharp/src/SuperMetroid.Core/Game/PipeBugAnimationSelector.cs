namespace SuperMetroid.Core.Game;

/// <summary>
/// Composable Brinstar Pipe Bug animation selector stored in the native list-table index.
/// Bit zero selects shooting and bit one selects right-facing.
/// </summary>
[Flags]
public enum PipeBugAnimationSelector : ushort
{
    /// <summary>Neither bit set: left-facing rise animation at entry zero of $B3:882B InstListPointers_Zeb or $B3:8833 InstListPointers_Zebbo.</summary>
    None = 0,
    /// <summary>Bit 0 selects the shooting animation rather than rising; preserves the facing bit and does not itself request movement or a projectile.</summary>
    Shooting = 1,
    /// <summary>Bit 1 selects the right-facing pair of animation entries; combine with Shooting for right-facing shooting, or leave it clear for left-facing art.</summary>
    FacingRight = 2,
}
