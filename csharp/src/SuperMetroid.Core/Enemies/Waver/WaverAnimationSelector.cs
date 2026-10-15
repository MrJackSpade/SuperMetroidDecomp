namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Composable Waver animation selector stored in variables C/F. The cartridge toggles
/// bit zero for facing and sets/clears bit one for the temporary spin family.
/// </summary>
[Flags]
public enum WaverAnimationSelector : ushort
{
    /// <summary>Neither bit set: steady left-facing art, selected by the first pointer at $A3:86DB.</summary>
    None = 0,
    /// <summary>Bit 0 selects right-facing art; horizontal collision toggles it, and it combines with Spinning to select the right spin program.</summary>
    FacingRight = 1,
    /// <summary>Bit 1 selects temporary spin art at $A3:86DF/$86E1 while preserving facing; AI clears it when $A3:86E3's animation command reports spin completion.</summary>
    Spinning = 2,
}
