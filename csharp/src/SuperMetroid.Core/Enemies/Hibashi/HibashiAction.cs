namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The behaviour Hibashi's main AI runs. The flame switches between one inactive timer and one
/// animation-driven active eruption.
/// Its values are arbitrary.
/// </summary>
public enum HibashiAction
{
    /// <summary>Wait out the flame's inactive wait, then restart the eruption art and enable the following invisible collision part.</summary>
    Inactive,
    /// <summary>Let the animation's art and hitbox cues run until the finish flag is set, then reload the population wait and return to inactive.</summary>
    Active,
}
