using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One colour an installed shade shows (#1633): the pixels of <see cref="Colour"/> in recolour region <see cref="Region"/>
/// show <see cref="Shows"/>.
/// </summary>
public sealed record InstalledColourSubstitutionDocument
{
    /// <summary>The colour the recoloured pixels hold, as <c>#rrggbb</c>.</summary>
    public required string Colour { get; init; }

    /// <summary>The recolour region among the image's pixels of that colour; absent for region zero.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Region { get; init; }

    /// <summary>The colour they show, as <c>#rrggbb</c>.</summary>
    public required string Shows { get; init; }

    /// <summary>The manifest entry of <paramref name="substitution"/>.</summary>
    internal static InstalledColourSubstitutionDocument Of(KeyedSubstitution substitution) => new()
    {
        Colour = InstalledColourText.Format(substitution.Replaced.Colour),
        Region = substitution.Replaced.Region,
        Shows = InstalledColourText.Format(substitution.Shown),
    };

    /// <summary>The substitution the entry names.</summary>
    /// <param name="context">Names the manifest entry in a failure.</param>
    /// <exception cref="InvalidDataException">A colour is not written as <c>#rrggbb</c> or the region is negative.</exception>
    internal KeyedSubstitution ToSubstitution(string context) => Region >= 0
        ? new KeyedSubstitution(new ColourKey(InstalledColourText.Parse(Colour, context), Region), InstalledColourText.Parse(Shows, context))
        : throw new InvalidDataException($"{context} names recolour region {Region}, below zero.");
}
