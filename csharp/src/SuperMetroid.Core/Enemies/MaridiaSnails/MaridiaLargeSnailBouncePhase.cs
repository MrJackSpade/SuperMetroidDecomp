namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's bounce behaviour. The enemy uses the shared quadratic-speed table for a diminishing three-impact
/// settling animation.
/// Its values are arbitrary.
/// </summary>
public enum MaridiaLargeSnailBouncePhase
{
    /// <summary>Increases the quadratic-speed selector by 384 per update up to 16384; a floor impact consumes a bounce, subtracts 4096, and selects rising.</summary>
    Falling = 1,
    /// <summary>Subtracts 384 per update and uses the quadratic table's negative 1/65536-pixel displacement; a selector stepped past zero resets to zero and selects falling without moving that update.</summary>
    Rising,
}
