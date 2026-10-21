namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>A colour within one of Kraid's paint bands.</summary>
/// <param name="Band">The band.</param>
/// <param name="Color">The color.</param>
internal readonly record struct KraidPaintCell(int Band, int Color);
