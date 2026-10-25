namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's bounce routine. The enemy uses the shared quadratic-speed table for a diminishing three-impact
/// settling animation.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum MaridiaLargeSnailBounceFunction : ushort
{
    /// <summary>Function_Oum_Falling, $A2:CF66: increases the quadratic-speed selector by $0180 per update up to $4000; a floor impact consumes a bounce, subtracts $1000, and selects rising.</summary>
    Falling = 1,
    /// <summary>Function_Oum_Rising, $A2:CFA9: subtracts $0180 per update and uses the quadratic table's negative 16.16 pixel displacement; signed selector underflow resets to zero and selects falling without moving that update.</summary>
    Rising,
}
