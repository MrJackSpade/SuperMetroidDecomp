namespace SuperMetroid.Core.Assets;

/// <summary>Editable expanded title-gradient schema; native HDMA runs are represented by sixteen complete screen-height color/control arrays.</summary>
public sealed record TitleGradientDocument
{
    /// <summary>Schema revision; loading requires <see cref="TitleGradientFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen nonnull variants in zoom-nibble order 0 through 15; this array and its nested line arrays remain caller-mutable until compilation.</summary>
    public required TitleGradientVariant[] Variants { get; init; }
}
