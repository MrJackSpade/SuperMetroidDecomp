namespace SuperMetroid.Core.Assets;

/// <summary>
/// How one pixel of a <see cref="SpriteImage"/> was inked beside its colour (#1403): the palette entry it shows and its
/// priority against the backgrounds.
/// </summary>
/// <remarks>
/// Installed images hold billions of pixels, so the two values share one word per pixel, a byte each, instead of a word
/// each.
/// </remarks>
public readonly record struct SpriteImageInk
{
    /// <summary>The largest value one of the two fields holds.</summary>
    private const int FieldMaximum = byte.MaxValue;

    /// <summary>The bits each field occupies in the shared word.</summary>
    private const int FieldBits = 8;

    /// <summary>The palette index, with the priority one field up.</summary>
    private readonly int fields;

    /// <summary>Records one pixel's ink.</summary>
    /// <param name="paletteIndex">The pixel's colour group's place times sixteen plus its colour; zero where it is transparent.</param>
    /// <param name="priority">The pixel's priority against the backgrounds 0..3, as the part that shows it had it.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value is negative or does not fit its field.</exception>
    public SpriteImageInk(int paletteIndex, int priority)
    {
        Ensure.AtLeastZero(paletteIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(paletteIndex, FieldMaximum);
        Ensure.AtLeastZero(priority);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(priority, FieldMaximum);
        fields = paletteIndex | priority << FieldBits;
    }

    /// <summary>The pixel's colour group's place times sixteen plus its colour; zero where it is transparent.</summary>
    public int PaletteIndex => fields & FieldMaximum;

    /// <summary>The pixel's priority against the backgrounds 0..3, as the part that shows it had it.</summary>
    public int Priority => fields >> FieldBits & FieldMaximum;
}
