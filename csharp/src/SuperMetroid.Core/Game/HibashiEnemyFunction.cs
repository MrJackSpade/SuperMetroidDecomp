namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 function word stored in Hibashi variable A. The graphics part switches
/// between one inactive timer and one instruction-driven active eruption.
/// </summary>
public enum HibashiEnemyFunction : ushort
{
    /// <summary>$A6:902F, Function_Hibashi_Inactive: decrement the graphics owner's wait word and, on signed underflow, restart eruption art and enable the following invisible collision part.</summary>
    Inactive = 0x902f,
    /// <summary>$A6:9062, Function_Hibashi_Active: let instruction-driven art/hitbox commands run until the finish flag is set, then reload the population wait and return to inactive.</summary>
    Active = 0x9062,
}
