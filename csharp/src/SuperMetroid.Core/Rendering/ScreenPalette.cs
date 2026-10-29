using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's own 256 colours (#1358, #1404): eight background rows of sixteen, then eight sprite rows, each row
/// the colours that one colour variant of the screen's pre-coloured tiles shows. The screen publishes them with its frame.
/// </summary>
public sealed class ScreenPalette : IColourSets
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
    internal readonly RgbColor[] colors = new RgbColor[ColorCount];

    /// <summary>The colours, in palette order: the background rows, then the sprite rows.</summary>
    public ReadOnlySpan<RgbColor> Colors => colors;

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

    /// <inheritdoc />
    public ColourRun Set(int set) => SpriteRow(set);

    /// <summary>The background rows, as the colour sets a screen's background images show.</summary>
    public IColourSets BackgroundSets => new Rows(BackgroundRow);

    /// <summary>The screen's colours as a frame shows them now.</summary>
    public BackgroundColours Shown() => new(ShownColourSets.Of(BackgroundSets), ShownColourSets.Of(this));

    /// <summary>The colour behind every layer, where nothing is drawn.</summary>
    public RgbColor Backdrop
    {
        get => colors[0];
        set => colors[0] = value;
    }

    /// <summary>Replaces every colour with <paramref name="screenColours"/>, a whole screen's colours.</summary>
    public void Load(ReadOnlySpan<RgbColor> screenColours)
    {
        Ensure.LengthEqual(screenColours, ColorCount);
        screenColours.CopyTo(colors);
    }

    /// <summary>Replaces every colour with <paramref name="screenColours"/> except those in <paramref name="kept"/>.</summary>
    public void LoadExcept(ReadOnlySpan<RgbColor> screenColours, params ReadOnlySpan<ColourRun> kept)
    {
        Ensure.LengthEqual(screenColours, ColorCount);
        for (int position = 0; position < ColorCount; position++)
        {
            if (!Keeps(kept, position))
                colors[position] = screenColours[position];
        }
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

    /// <summary>One kind of the screen's rows, as colour sets.</summary>
    /// <param name="row">The row of the kind each set is.</param>
    private sealed class Rows(Func<int, ColourRun> row) : IColourSets
    {
        /// <inheritdoc />
        public ColourRun Set(int set) => row(set);
    }
}
