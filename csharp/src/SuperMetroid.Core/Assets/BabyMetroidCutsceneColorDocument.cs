namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON artwork for the Mother Brain cutscene Baby's initial image and six displayed death-fade images; array references and RGB5 entries remain caller-owned and mutable.</summary>
public sealed record BabyMetroidCutsceneColorDocument
{
    /// <summary>Schema version required to match <see cref="BabyMetroidCutsceneColorFormat.Version"/> when loading or writing.</summary>
    public required int Version { get; init; }
    /// <summary>Fifteen RGB5 entries in native initial-image order, mapping to OBJ palette seven colors 1..15 and excluding transparent color zero.</summary>
    public required PaletteRgb5[] Initial { get; init; }
    /// <summary>Six chronological RGB5 rows of fourteen colors each; row zero corresponds to displayed fade selector one, and each row replaces OBJ palette seven colors 1..14 without changing color 15.</summary>
    public required PaletteRgb5[][] Fade { get; init; }
}
