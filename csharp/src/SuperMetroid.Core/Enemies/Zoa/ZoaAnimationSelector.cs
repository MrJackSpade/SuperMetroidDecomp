namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Composable Zoa animation selector: whether the Zoa is rising rather than shooting, and whether it faces right
/// rather than left. The two together choose one of its four animations.
/// </summary>
[Flags]
public enum ZoaAnimationSelector
{
    /// <summary>Neither flag set: left-facing shooting art.</summary>
    None = 0,
    /// <summary>Selects rising art rather than shooting; AI clears it on reaching Samus's height while retaining facing.</summary>
    Rising = 1 << 0,
    /// <summary>Selects right-facing shooting art, or right-facing rising when combined with Rising; its absence selects left-facing art.</summary>
    FacingRight = 1 << 1,
}
