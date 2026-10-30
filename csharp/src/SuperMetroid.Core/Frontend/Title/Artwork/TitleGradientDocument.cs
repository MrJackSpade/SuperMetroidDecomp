namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Editable title-gradient schema: sixteen gradients, one per zoom bucket, each described by its stops.</summary>
public sealed record TitleGradientDocument
{
    /// <summary>Schema revision; loading requires <see cref="TitleGradientFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen nonnull variants in zoom-bucket order 0 through 15; this array and its nested stop arrays remain caller-mutable until compilation.</summary>
    public required TitleGradientVariant[] Variants { get; init; }
}
