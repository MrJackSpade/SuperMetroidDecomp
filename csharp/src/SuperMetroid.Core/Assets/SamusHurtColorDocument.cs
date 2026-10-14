namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON payload for the two complete Samus OBJ palettes; initialized array references and their RGB5 entries remain caller-owned and mutable.</summary>
public sealed record SamusHurtColorDocument
{
    /// <summary>Schema version required to match <see cref="SamusHurtColorFormat.Version"/> when loading or writing.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen RGB5 colors in native hurt-flash pen order, including transparent slot zero; replaces Samus OBJ palette four on odd flash calls.</summary>
    public required PaletteRgb5[] Hurt { get; init; }
    /// <summary>Sixteen RGB5 colors in native intro pen order, including transparent slot zero; supplies cinematic restoration rather than the equipment-dependent normal suit palette.</summary>
    public required PaletteRgb5[] Intro { get; init; }
}
