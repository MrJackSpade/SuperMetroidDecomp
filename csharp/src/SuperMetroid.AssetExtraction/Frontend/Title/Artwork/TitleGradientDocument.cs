
namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Editable title-gradient schema: sixteen gradients, one per zoom range, each described by its stops.</summary>
public sealed record TitleGradientDocument
{
    /// <summary>Schema revision; loading requires <see cref="TitleGradientFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen nonnull variants in order of their zoom ranges, from no zoom up, the first also shown at the floor's own size; this array and its nested stop arrays remain caller-mutable until compilation.</summary>
    public required TitleGradientVariant[] Variants { get; init; }
}
