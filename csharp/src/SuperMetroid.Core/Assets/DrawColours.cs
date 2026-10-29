using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// What one sprite draw's pixels show in its frame (#1405). Each pixel takes the colours of one key: its ink row, the
/// row its image was coloured in, or, where the draw follows part rows, its part row. For each key the draw holds the
/// sixteen colours its pixels show, or none where they show the image's own colours, and whether they blend with what
/// lies behind them. Backends colour a draw from these alone.
/// </summary>
public sealed class DrawColours
{
    /// <summary>The most keys a draw holds: the part rows, the eight rows each with and without the fixed-row flag.</summary>
    public const int Keys = 2 * InkRows;

    /// <summary>The colours in one row.</summary>
    public const int RowColours = 16;

    /// <summary>The ink rows an image's pixels are coloured in.</summary>
    internal const int InkRows = SpriteImage.MaximumPaletteRow + 1;

    /// <summary>The colours of a frame: the background rows' colours, then the sprite rows'.</summary>
    private const int FrameColours = 2 * SpriteImage.SpriteColourCount;

    /// <summary>The lowest row whose pixels blend.</summary>
    private const int FirstBlendingRow = 4;

    /// <summary>For each key, the colours its pixels show, or null where they show the image's own.</summary>
    private readonly RgbColor[]?[] shown;

    /// <summary>One bit per key whose pixels blend.</summary>
    private readonly int blending;

    /// <summary>Keeps the resolved colours and blending.</summary>
    private DrawColours(ColourKeying keying, RgbColor[]?[] shown, int blending)
    {
        FollowsPartRows = keying == ColourKeying.PartRow;
        this.shown = shown;
        this.blending = blending;
    }

    /// <summary>
    /// Whether each pixel takes its part row's colours, as under a draw's override, rather than its ink row's.
    /// </summary>
    public bool FollowsPartRows { get; }

    /// <summary>Whether every pixel shows the image's own colours and blends as its own row does.</summary>
    public bool IsOwn => !FollowsPartRows && shown.All(row => row is null) && blending == OwnBlending;

    /// <summary>Whether the pixels of key <paramref name="key"/> show the image's own colours.</summary>
    public bool ShowsOwnColours(int key) => shown[CheckKey(key)] is null;

    /// <summary>The colour <paramref name="colour"/> of key <paramref name="key"/>'s pixels show.</summary>
    /// <exception cref="InvalidOperationException">The key's pixels show the image's own colours.</exception>
    public RgbColor Colour(int key, int colour) =>
        (shown[CheckKey(key)] ?? throw new InvalidOperationException($"Key {key} shows the image's own colours."))
        [Ensure.BetweenInclusive(0, RowColours - 1, colour)];

    /// <summary>Whether the pixels of key <paramref name="key"/> blend with what lies behind them.</summary>
    public bool Blends(int key) => (blending >> CheckKey(key) & 1) != 0;

    /// <summary>
    /// The colours <paramref name="draw"/> shows under <paramref name="frameColours"/>, a frame's 256 colours. A plain
    /// draw shows each ink row the frame uses, keeping its image's own colours while that row holds them. A draw with
    /// a drawn colouring shows, for each part row, the row the colouring's row leaves it.
    /// </summary>
    public static DrawColours Resolve(ImageSpriteDraw draw, ReadOnlySpan<RgbColor> frameColours)
    {
        Ensure.NotNull(draw);
        Ensure.LengthEqual(frameColours, FrameColours);
        var shown = new RgbColor[]?[Keys];
        int blending = 0;
        if (draw.Colouring is { } overriding)
        {
            for (int partRow = 0; partRow < Keys; partRow++)
            {
                int row = SpriteImage.OverriddenRow(partRow, overriding.Row);
                shown[partRow] = Row(frameColours, row).ToArray();
                if (row >= FirstBlendingRow)
                    blending |= 1 << partRow;
            }
            return new DrawColours(ColourKeying.PartRow, shown, blending);
        }
        ReadOnlySpan<RgbColor> standard = draw.Image.Sheet.StandardColours;
        for (int row = 0; row < InkRows; row++)
        {
            if (row >= FirstBlendingRow)
                blending |= 1 << row;
            if ((draw.Image.Rows & SpritePaletteRowSet.Rows[row]) == SpritePaletteRows.None)
                continue;
            ReadOnlySpan<RgbColor> colours = Row(frameColours, row);
            // Colour zero is transparent and never shown.
            if (!colours[1..].SequenceEqual(standard.Slice(row * RowColours, RowColours)[1..]))
                shown[row] = colours.ToArray();
        }
        return new DrawColours(ColourKeying.InkRow, shown, blending);
    }

    /// <summary>
    /// The colours <paramref name="draw"/> shows where <paramref name="owner"/> holds the colours of some of its ink rows:
    /// each ink row the frame uses shows the owner's colours over the image's own, and blends as its own row does.
    /// </summary>
    /// <exception cref="InvalidOperationException">The draw has an override, which an owner's draws never take.</exception>
    public static DrawColours Owned(ImageSpriteDraw draw, IInkRowColours owner)
    {
        Ensure.NotNull(draw);
        Ensure.NotNull(owner);
        if (draw.Colouring is not null)
            throw new InvalidOperationException("An owner colours its draws itself; a drawn colouring selects colours from a row.");
        ReadOnlySpan<RgbColor> standard = draw.Image.Sheet.StandardColours;
        var shown = new RgbColor[]?[Keys];
        Span<RgbColor> colours = stackalloc RgbColor[RowColours];
        for (int row = 0; row < InkRows; row++)
        {
            if ((draw.Image.Rows & SpritePaletteRowSet.Rows[row]) == SpritePaletteRows.None)
                continue;
            ReadOnlySpan<RgbColor> own = standard.Slice(row * RowColours, RowColours);
            own.CopyTo(colours);
            owner.Overlay(row, colours);
            // Colour zero is transparent and never shown.
            if (!colours[1..].SequenceEqual(own[1..]))
                shown[row] = colours.ToArray();
        }
        return new DrawColours(ColourKeying.InkRow, shown, OwnBlending);
    }

    /// <summary>
    /// <paramref name="draws"/> with the colours of those no owner coloured resolved under <paramref name="frameColours"/>,
    /// a frame's 256 colours, as the frame publishes them. An owner's draws keep the colours it gave them.
    /// </summary>
    /// <exception cref="InvalidDataException">A draw is not an image draw.</exception>
    public static ImmutableArray<SpriteDraw> ResolveAll(ImmutableArray<SpriteDraw> draws, ReadOnlySpan<RgbColor> frameColours)
    {
        var resolved = ImmutableArray.CreateBuilder<SpriteDraw>(draws.Length);
        foreach (SpriteDraw draw in draws)
        {
            var image = draw as ImageSpriteDraw ?? throw new InvalidDataException($"A {draw.GetType().Name} has no image to colour.");
            resolved.Add(image.Colours is null ? image with { Colours = Resolve(image, frameColours) } : image);
        }
        return resolved.MoveToImmutable();
    }

    /// <summary>The blending bits of an image's own rows.</summary>
    private static readonly int OwnBlending = OwnBlendingBits();

    /// <summary>Computes <see cref="OwnBlending"/>.</summary>
    private static int OwnBlendingBits()
    {
        int bits = 0;
        for (int row = FirstBlendingRow; row < InkRows; row++)
            bits |= 1 << row;
        return bits;
    }

    /// <summary>The sixteen colours of sprite row <paramref name="row"/> in <paramref name="frameColours"/>.</summary>
    private static ReadOnlySpan<RgbColor> Row(ReadOnlySpan<RgbColor> frameColours, int row) =>
        frameColours.Slice(SpriteImage.SpriteColourCount + row * RowColours, RowColours);

    /// <summary>Checks that <paramref name="key"/> is one of the draw's.</summary>
    private int CheckKey(int key) => Ensure.BetweenInclusive(0, (FollowsPartRows ? Keys : InkRows) - 1, key);
}
