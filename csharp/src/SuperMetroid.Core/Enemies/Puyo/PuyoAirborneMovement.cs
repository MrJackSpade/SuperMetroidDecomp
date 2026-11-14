namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's airborne behaviours; each hop names the one it runs.
/// Its values are arbitrary.
/// </summary>
public enum PuyoAirborneMovement
{
    /// <summary>Moves the small normal hop and selects the slow grounded animation on landing or terrain collision.</summary>
    NormalShortHop,
    /// <summary>Moves the taller normal hop and selects the medium grounded animation on landing or terrain collision.</summary>
    NormalBigHop,
    /// <summary>Moves as the big hop with increased horizontal speed and selects the fast grounded animation on landing or terrain collision.</summary>
    NormalLongHop,
    /// <summary>Moves the 128-pixel-height hop; normal landing restores small-hop selection, while wall or ceiling collision retains dropping recovery.</summary>
    GiantHop,
    /// <summary>Falls at the drop's own speed until terrain contact selects a random small or big post-drop bounce.</summary>
    Dropping,
    /// <summary>Moves either post-drop bounce and selects a giant hop for the next grounded launch when the hopping animation is cleared by terrain contact.</summary>
    Dropped,
}
