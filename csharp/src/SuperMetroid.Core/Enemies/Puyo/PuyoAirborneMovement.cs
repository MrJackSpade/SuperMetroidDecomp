namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's airborne behaviours; entries zero through six of its hop table select them.
/// Its values are arbitrary.
/// </summary>
public enum PuyoAirborneMovement
{
    /// <summary>Moves the small normal hop and selects the slow grounded animation on landing or terrain collision.</summary>
    NormalShortHop = 1,
    /// <summary>Moves the taller normal hop and selects the medium grounded animation on landing or terrain collision.</summary>
    NormalBigHop,
    /// <summary>Moves hop-table record two with increased horizontal speed and selects the fast grounded animation on landing or terrain collision.</summary>
    NormalLongHop,
    /// <summary>Moves the 128-pixel-height hop; normal landing restores small-hop selection, while wall or ceiling collision retains dropping recovery.</summary>
    GiantHop,
    /// <summary>Falls vertically at the hop record's packed 8.8 speed until terrain contact selects a random small or big post-drop bounce.</summary>
    Dropping,
    /// <summary>Moves either post-drop bounce and selects a giant hop for the next grounded launch when the hopping animation is cleared by terrain contact.</summary>
    Dropped,
}
