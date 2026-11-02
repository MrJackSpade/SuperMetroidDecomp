namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// The installed four-bit OBJ characters the Ceres and Zebes escape timers show (#1409), compiled by the import pipeline
/// from the editable timer typeface.
/// </summary>
public class EscapeTimerTileAtlas
{
    /// <summary>The 25 timer characters in four-bit planar load order.</summary>
    internal readonly byte[] characters;

    /// <summary>Retains the compiled timer characters.</summary>
    /// <param name="characters">The 25 timer characters in transfer order.</param>
    internal EscapeTimerTileAtlas(byte[] characters)
    {
        this.characters = characters;
    }
}
