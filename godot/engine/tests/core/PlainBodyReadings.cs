using Engine.Actors;

namespace Engine.Tests.Core;

/// <summary>
/// The stand-in game's reading of the player's body for the blocks it is in: its feet, its middle and its head, each
/// where it lies in a row of tiles the ones before it did not.
/// </summary>
public sealed class PlainBodyReadings : IBodyReadings
{
    /// <inheritdoc/>
    public IEnumerable<int> HeightsStoodAt(int bottom, int middle, int top, int tileSize)
    {
        var rows = new HashSet<int>();
        foreach (int height in (int[])[bottom, middle, top])
        {
            if (rows.Add((int)Math.Floor((double)height / tileSize)))
                yield return height;
        }
    }
}
