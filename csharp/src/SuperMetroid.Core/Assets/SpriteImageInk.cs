namespace SuperMetroid.Core.Assets;

/// <summary>
/// How one pixel of a <see cref="SpriteImage"/> was inked beside its colour (#1403): the palette entry it shows, its
/// priority against the backgrounds and the part row a draw's palette override recolours.
/// </summary>
/// <remarks>
/// Installed images hold billions of pixels, so the three values share one word per pixel, a byte each, instead of
/// a word each.
/// </remarks>
public readonly record struct SpriteImageInk
{
    /// <summary>The largest value one of the three fields holds.</summary>
    private const int FieldMaximum = byte.MaxValue;

    /// <summary>The bits each field occupies in the shared word.</summary>
    private const int FieldBits = 8;

    /// <summary>The palette index, the priority one field up and the part row two fields up.</summary>
    private readonly int fields;

    /// <summary>Records one pixel's ink.</summary>
    /// <param name="paletteIndex">The pixel's palette row times sixteen plus its colour; zero where it is transparent.</param>
    /// <param name="priority">The pixel's priority against the backgrounds 0..3, as the part that shows it had it.</param>
    /// <param name="partRow">
    /// The pixel's part palette row 0..7, before a draw's palette bits were ORed into it, with
    /// <see cref="SpriteImage.FixedPartRow"/> where the part keeps its own row under a draw's override.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">A value is negative or does not fit its field.</exception>
    public SpriteImageInk(int paletteIndex, int priority, int partRow)
    {
        Ensure.AtLeastZero(paletteIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(paletteIndex, FieldMaximum);
        Ensure.AtLeastZero(priority);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(priority, FieldMaximum);
        Ensure.AtLeastZero(partRow);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(partRow, FieldMaximum);
        fields = paletteIndex | priority << FieldBits | partRow << 2 * FieldBits;
    }

    /// <summary>The pixel's palette row times sixteen plus its colour; zero where it is transparent.</summary>
    public int PaletteIndex => fields & FieldMaximum;

    /// <summary>The pixel's priority against the backgrounds 0..3, as the part that shows it had it.</summary>
    public int Priority => fields >> FieldBits & FieldMaximum;

    /// <summary>The pixel's part palette row, with <see cref="SpriteImage.FixedPartRow"/> where the part keeps its own row.</summary>
    public int PartRow => fields >> 2 * FieldBits & FieldMaximum;
}
