using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's own 256 colours (#1358, #1404): eight background rows of sixteen, then eight sprite rows, each row
/// the colours that one colour variant of the screen's pre-coloured tiles shows. The screen publishes them with its frame.
/// </summary>
/// <remarks>The shared palette effects write through <see cref="IPaletteColors"/> until #1405 types them.</remarks>
public sealed class ScreenPalette : IPaletteColors
{
    /// <summary>The number of colours a screen palette holds.</summary>
    public const int ColorCount = 2 * RowsPerKind * ColoursPerRow;

    /// <summary>The colours in one row.</summary>
    public const int ColoursPerRow = 16;

    /// <summary>The rows of each kind: as many background rows as sprite rows.</summary>
    public const int RowsPerKind = 8;

    /// <summary>The colours of each kind.</summary>
    private const int KindColours = RowsPerKind * ColoursPerRow;

    /// <summary>The colours, background rows first.</summary>
    private readonly Bgr555[] colors = new Bgr555[ColorCount];

    /// <inheritdoc />
    public ReadOnlySpan<Bgr555> Colors => colors;

    /// <summary>Every colour of the screen.</summary>
    public ColourRun All => new(colors, 0, ColorCount);

    /// <summary>The background rows' colours.</summary>
    public ColourRun Background => new(colors, 0, KindColours);

    /// <summary>The sprite rows' colours.</summary>
    public ColourRun Sprites => new(colors, KindColours, KindColours);

    /// <summary>Background row <paramref name="row"/>, zero through seven.</summary>
    public ColourRun BackgroundRow(int row) =>
        Background.Slice(Ensure.BetweenInclusive(0, RowsPerKind - 1, row) * ColoursPerRow, ColoursPerRow);

    /// <summary>Sprite row <paramref name="row"/>, zero through seven.</summary>
    public ColourRun SpriteRow(int row) =>
        Sprites.Slice(Ensure.BetweenInclusive(0, RowsPerKind - 1, row) * ColoursPerRow, ColoursPerRow);

    /// <summary>The colour behind every layer, where nothing is drawn.</summary>
    public Bgr555 Backdrop
    {
        get => colors[0];
        set => colors[0] = value;
    }

    /// <summary>Replaces every colour with <paramref name="screenColours"/>, a whole screen's colours.</summary>
    public void Load(ReadOnlySpan<Bgr555> screenColours)
    {
        Ensure.LengthEqual(screenColours, ColorCount);
        screenColours.CopyTo(colors);
    }

    /// <summary>Replaces every colour with <paramref name="screenColours"/> except those in <paramref name="kept"/>.</summary>
    public void LoadExcept(ReadOnlySpan<Bgr555> screenColours, params ReadOnlySpan<ColourRun> kept)
    {
        Ensure.LengthEqual(screenColours, ColorCount);
        for (int position = 0; position < ColorCount; position++)
        {
            if (!Keeps(kept, position))
                colors[position] = screenColours[position];
        }
    }

    /// <inheritdoc />
    public void SetColor(int index, Bgr555 color)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        colors[index] = color;
    }

    /// <inheritdoc />
    public Rgba32 GetRgba(int index)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors[index].ToRgba32();
    }

    /// <inheritdoc />
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
    {
        if ((bytes.Length & 1) != 0)
            throw new ArgumentException("Palette data must contain complete two-byte colours.", nameof(bytes));
        int count = bytes.Length / 2;
        if (destinationIndex < 0 || destinationIndex + count > ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destinationIndex), "A palette load must remain within 256 colours.");
        for (int color = 0; color < count; color++)
            colors[destinationIndex + color] = Bgr555.FromCgramPortWord(SnesLittleEndian.Word(bytes[color * 2], bytes[color * 2 + 1]));
    }

    /// <summary>Whether a run in <paramref name="kept"/> holds the colour at <paramref name="position"/>.</summary>
    private static bool Keeps(ReadOnlySpan<ColourRun> kept, int position)
    {
        foreach (ColourRun run in kept)
        {
            if (run.Covers(position))
                return true;
        }
        return false;
    }
}
