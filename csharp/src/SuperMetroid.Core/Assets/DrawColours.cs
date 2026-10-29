using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// What one sprite draw's pixels show in its frame (#1405, #1464). Each pixel takes the colours of one key: the colour
/// group its ink was coloured from, or, where the draw follows part groups, its <see cref="PartColourGroup"/>'s code.
/// For each key the draw holds the sixteen colours its pixels show, or none where they show the image's own colours, and
/// whether they blend with what lies behind them. Backends colour a draw from these alone.
/// </summary>
public sealed class DrawColours
{
    /// <summary>The most keys a draw holds: one per part group.</summary>
    public const int Keys = PartColourGroup.Codes;

    /// <summary>The colours of one key.</summary>
    public const int KeyColours = ImageColourGroup.Colours;

    /// <summary>For each key, the colours its pixels show, or null where they show the image's own.</summary>
    private readonly RgbColor[]?[] shown;

    /// <summary>One bit per key whose pixels blend.</summary>
    private readonly int blending;

    /// <summary>Keeps the resolved colours and blending.</summary>
    private DrawColours(ColourKeying keying, RgbColor[]?[] shown, int blending)
    {
        FollowsPartGroups = keying == ColourKeying.PartGroup;
        this.shown = shown;
        this.blending = blending;
    }

    /// <summary>
    /// Whether each pixel takes its part colour group's colours, as under a drawn colouring, rather than its ink's group's.
    /// </summary>
    public bool FollowsPartGroups { get; }

    /// <summary>The keys the draw's pixels take: every part group's, or every ink colour group's.</summary>
    public int KeyCount => FollowsPartGroups ? Keys : ImageColourGroup.Count;

    /// <summary>Whether every pixel shows the image's own colours and blends as its own group does.</summary>
    public bool IsOwn => !FollowsPartGroups && shown.All(group => group is null) && blending == OwnBlending;

    /// <summary>Whether the pixels of key <paramref name="key"/> show the image's own colours.</summary>
    public bool ShowsOwnColours(int key) => shown[CheckKey(key)] is null;

    /// <summary>The colour <paramref name="colour"/> of key <paramref name="key"/>'s pixels show.</summary>
    /// <exception cref="InvalidOperationException">The key's pixels show the image's own colours.</exception>
    public RgbColor Colour(int key, int colour) =>
        (shown[CheckKey(key)] ?? throw new InvalidOperationException($"Key {key} shows the image's own colours."))
        [Ensure.BetweenInclusive(0, KeyColours - 1, colour)];

    /// <summary>Whether the pixels of key <paramref name="key"/> blend with what lies behind them.</summary>
    public bool Blends(int key) => (blending >> CheckKey(key) & 1) != 0;

    /// <summary>
    /// The colours <paramref name="draw"/> shows under <paramref name="spriteSets"/>, a frame's sprite sets. A plain
    /// draw shows each colour group the frame uses, keeping its image's own colours while that group's store holds them.
    /// A draw with a drawn colouring shows, for each part group, what the colouring's placement shows over it (#1464).
    /// </summary>
    public static DrawColours Resolve(ImageSpriteDraw draw, ShownColourSets spriteSets)
    {
        Ensure.NotNull(draw);
        Ensure.NotNull(spriteSets);
        var shown = new RgbColor[]?[Keys];
        int blending = 0;
        if (draw.Colouring is { } overriding)
        {
            foreach (PartColourGroup part in PartColourGroup.Every)
            {
                shown[part.Code] = overriding.Placement.ShownOver(overriding.Shows, part, spriteSets, out bool blends).ToArray();
                if (blends)
                    blending |= 1 << part.Code;
            }
            return new DrawColours(ColourKeying.PartGroup, shown, blending);
        }
        foreach (ImageColourGroup group in ImageColourGroup.Every)
        {
            if (group.Store.Blends)
                blending |= 1 << group.Number;
            if (!draw.Image.ColourGroups.Contains(group))
                continue;
            ReadOnlySpan<RgbColor> colours = spriteSets.Set(group.Store.Set);
            // Colour zero is transparent and never shown.
            if (!colours[1..].SequenceEqual(draw.Image.Sheet.StandardColoursOf(group)[1..]))
                shown[group.Number] = colours.ToArray();
        }
        return new DrawColours(ColourKeying.InkGroup, shown, blending);
    }

    /// <summary>
    /// The colours <paramref name="draw"/> shows where <paramref name="owner"/> holds the colours of some of its colour
    /// groups: each group the frame uses shows the owner's colours over the image's own, and blends as its own group does.
    /// </summary>
    /// <exception cref="InvalidOperationException">The draw has a drawn colouring, which an owner's draws never take.</exception>
    public static DrawColours Owned(ImageSpriteDraw draw, IImageGroupColours owner)
    {
        Ensure.NotNull(draw);
        Ensure.NotNull(owner);
        if (draw.Colouring is not null)
            throw new InvalidOperationException("An owner colours its draws itself; a drawn colouring shows colours another load placed.");
        var shown = new RgbColor[]?[Keys];
        Span<RgbColor> colours = stackalloc RgbColor[KeyColours];
        foreach (ImageColourGroup group in ImageColourGroup.Every)
        {
            if (!draw.Image.ColourGroups.Contains(group))
                continue;
            ReadOnlySpan<RgbColor> own = draw.Image.Sheet.StandardColoursOf(group);
            own.CopyTo(colours);
            owner.Overlay(group, colours);
            // Colour zero is transparent and never shown.
            if (!colours[1..].SequenceEqual(own[1..]))
                shown[group.Number] = colours.ToArray();
        }
        return new DrawColours(ColourKeying.InkGroup, shown, OwnBlending);
    }

    /// <summary>
    /// <paramref name="draws"/> with the colours of those no owner coloured resolved under <paramref name="spriteSets"/>,
    /// a frame's sprite sets, as the frame publishes them. An owner's draws keep the colours it gave them.
    /// </summary>
    /// <exception cref="InvalidDataException">A draw is not an image draw.</exception>
    public static ImmutableArray<SpriteDraw> ResolveAll(ImmutableArray<SpriteDraw> draws, ShownColourSets spriteSets)
    {
        var resolved = ImmutableArray.CreateBuilder<SpriteDraw>(draws.Length);
        foreach (SpriteDraw draw in draws)
        {
            var image = draw as ImageSpriteDraw ?? throw new InvalidDataException($"A {draw.GetType().Name} has no image to colour.");
            resolved.Add(image.Colours is null ? image with { Colours = Resolve(image, spriteSets) } : image);
        }
        return resolved.MoveToImmutable();
    }

    /// <summary>The blending bits of an image's own colour groups: those whose stores blend.</summary>
    private static readonly int OwnBlending = OwnBlendingBits();

    /// <summary>Computes <see cref="OwnBlending"/>.</summary>
    private static int OwnBlendingBits()
    {
        int bits = 0;
        foreach (ImageColourGroup group in ImageColourGroup.Every)
        {
            if (group.Store.Blends)
                bits |= 1 << group.Number;
        }
        return bits;
    }

    /// <summary>Checks that <paramref name="key"/> is one of the draw's.</summary>
    private int CheckKey(int key) => Ensure.BetweenInclusive(0, KeyCount - 1, key);
}
