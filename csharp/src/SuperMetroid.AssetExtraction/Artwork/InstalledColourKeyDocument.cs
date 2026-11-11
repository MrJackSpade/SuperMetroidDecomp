using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A colour key in an installed sheet manifest (#1633): a colour the image's pixels hold and its recolour region.</summary>
public sealed record InstalledColourKeyDocument
{
    /// <summary>The colour, as <c>#rrggbb</c>.</summary>
    public required string Colour { get; init; }

    /// <summary>The recolour region among the image's pixels of that colour (<see cref="ColourKey.Region"/>).</summary>
    public required int Region { get; init; }

    /// <summary>The manifest entry of <paramref name="key"/>.</summary>
    internal static InstalledColourKeyDocument Of(ColourKey key) => new() { Colour = InstalledColourText.Format(key.Colour), Region = key.Region };

    /// <summary>The key the entry names.</summary>
    /// <param name="context">Names the manifest entry in a failure.</param>
    /// <exception cref="InvalidDataException">The colour is not written as <c>#rrggbb</c> or the region is negative.</exception>
    internal ColourKey ToKey(string context) => Region >= 0
        ? new ColourKey(InstalledColourText.Parse(Colour, context), Region)
        : throw new InvalidDataException($"{context} names recolour region {Region}, below zero.");
}
