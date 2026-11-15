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
    /// <summary>Moves one and a half marks back along the speed curve per update and rises by the curve's speed; a place stepped back past the start resets to the start and selects falling without moving that update.</summary>
    Rising,
}
