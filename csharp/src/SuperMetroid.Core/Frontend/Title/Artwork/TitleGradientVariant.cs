namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>One title zoom bucket corresponding to a pointer in native TitleSequenceHDMATables at $8C:BC5D, expanded into screen scanlines.</summary>
public sealed record TitleGradientVariant
{
    /// <summary>Zero through fifteen; native selects this with zoom bits four through seven.</summary>
    public required int ZoomHighNibble { get; init; }
    /// <summary>Exactly 224 nonnull scanlines indexed by visible screen Y from 0 through 223, not HDMA commands or run lengths.</summary>
    public required TitleGradientScanline[] Lines { get; init; }
}
