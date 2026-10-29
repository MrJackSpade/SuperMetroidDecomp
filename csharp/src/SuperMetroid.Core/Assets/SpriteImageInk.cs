namespace SuperMetroid.Core.Assets;

/// <summary>
/// How one pixel of a <see cref="SpriteImage"/> was inked beside its colour (#1403): the palette entry it shows, its
/// priority against the backgrounds and the part colour group a drawn colouring recolours.
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

    /// <summary>The palette index, the priority one field up and the part group's code two fields up.</summary>
    private readonly int fields;

    /// <summary>Records one pixel's ink.</summary>
    /// <param name="paletteIndex">The pixel's colour group's place times sixteen plus its colour; zero where it is transparent.</param>
    /// <param name="priority">The pixel's priority against the backgrounds 0..3, as the part that shows it had it.</param>
    /// <param name="partGroup">The colour group of the part the pixel belongs to, and whether that part keeps it under a drawn colouring.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value is negative or does not fit its field.</exception>
    public SpriteImageInk(int paletteIndex, int priority, PartColourGroup partGroup)
    {
        Ensure.AtLeastZero(paletteIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(paletteIndex, FieldMaximum);
        Ensure.AtLeastZero(priority);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(priority, FieldMaximum);
        fields = paletteIndex | priority << FieldBits | partGroup.Code << 2 * FieldBits;
    }

    /// <summary>The pixel's colour group's place times sixteen plus its colour; zero where it is transparent.</summary>
    public int PaletteIndex => fields & FieldMaximum;

    /// <summary>The pixel's priority against the backgrounds 0..3, as the part that shows it had it.</summary>
    public int Priority => fields >> FieldBits & FieldMaximum;

    /// <summary>The colour group of the part the pixel belongs to, and whether that part keeps it under a drawn colouring.</summary>
    public PartColourGroup PartGroup => PartColourGroup.FromCode(fields >> 2 * FieldBits & FieldMaximum);
}
