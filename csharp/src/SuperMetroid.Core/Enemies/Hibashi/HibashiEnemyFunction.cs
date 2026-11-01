namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The routine Hibashi's main AI runs. The graphics part switches between one inactive timer and one
/// instruction-driven active eruption.
/// Its values are arbitrary.
/// </summary>
public enum HibashiEnemyFunction
{
    /// <summary>Decrement the graphics owner's wait word and, on signed underflow, restart eruption art and enable the following invisible collision part.</summary>
    Inactive = 1,
    /// <summary>Let instruction-driven art/hitbox commands run until the finish flag is set, then reload the population wait and return to inactive.</summary>
    Active,
}
