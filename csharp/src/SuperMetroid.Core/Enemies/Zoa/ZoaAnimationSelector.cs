namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Composable Zoa animation selector stored in variables C/D. Native AI uses bit zero
/// for rising versus shooting and bit one for right-facing versus left-facing.
/// </summary>
[Flags]
public enum ZoaAnimationSelector
{
    /// <summary>Neither bit set: left-facing shooting art, entry zero of $A3:B40D InstListPointers_Zoa.</summary>
    None = 0,
    /// <summary>Bit 0 selects rising art at $A3:B40F/$B413 rather than shooting; AI clears it on reaching Samus's height while retaining facing.</summary>
    Rising = 1,
    /// <summary>Bit 1 selects right-facing shooting art, or right-facing rising when combined with Rising; absence of this bit selects left-facing art.</summary>
    FacingRight = 2,
}
