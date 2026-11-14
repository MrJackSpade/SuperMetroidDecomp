namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's bounce behaviour. The enemy uses the shared quadratic speed curve for a diminishing three-impact
/// settling animation.
/// Its values are arbitrary.
/// </summary>
public enum MaridiaLargeSnailBouncePhase
{
    /// <summary>Moves one and a half marks along the speed curve per update up to mark 64; a floor impact consumes a bounce, moves back 16 marks, and selects rising.</summary>
    Falling,
    /// <summary>Subtracts 384 per update and uses the quadratic speed curve's negative 1/65536-pixel displacement; a selector stepped past zero resets to zero and selects falling without moving that update.</summary>
    Rising,
}
