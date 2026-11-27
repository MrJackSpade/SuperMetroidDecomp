namespace Engine.Actors;

/// <summary>
/// How the player's body is read for the blocks it is in: at which heights down its middle column a block counts as
/// one they are in. The engine has no reading of its own: the game supplies one through its
/// <see cref="Core.GameDefinition"/>.
/// </summary>
public interface IBodyReadings
{
    /// <summary>
    /// The heights, in room pixels, at which the player is in whatever block lies there, in the order those blocks
    /// are taken: of the lowest row of their body, its middle and its top row, the ones that count for a body lying
    /// so in a room whose tiles are <paramref name="tileSize"/> pixels a side.
    /// </summary>
    /// <param name="bottom">The lowest row of the body.</param>
    /// <param name="middle">The row of the body's middle.</param>
    /// <param name="top">The top row of the body.</param>
    /// <param name="tileSize">The side of a tile of the room's terrain, in pixels.</param>
    IEnumerable<int> HeightsStoodAt(int bottom, int middle, int top, int tileSize);
}
