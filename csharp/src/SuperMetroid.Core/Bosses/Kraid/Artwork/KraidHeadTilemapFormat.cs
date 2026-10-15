namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Supported JSON version and fixed head-frame dimensions matching the $0160-word native copy at $A7:AF3D.</summary>
public static class KraidHeadTilemapFormat
{
    /// <summary>Supported Kraid head tilemap schema revision one.</summary>
    public const int Version = 1;
    /// <summary>Thirty-two tile-reference columns in each head-frame row.</summary>
    public const int Width = 32;
    /// <summary>Eleven tile-reference rows in each head frame; with <see cref="Width"/> this yields 352 native words.</summary>
    public const int Height = 11;
}
