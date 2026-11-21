namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Editable health-band palette JSON, separate from rainbow cycling and death-fade artwork; array references, rows, and RGB5 entries remain caller-owned and mutable.</summary>
public sealed partial record MotherBrainHealthPaletteDocument
{
    /// <summary>Schema version required to match <see cref="MotherBrainHealthPaletteFile.Version"/> during loading or writing.</summary>
    public required int Version { get; init; }
    /// <summary>
    /// Each of her four health bands' colours, keyed by the band's installed key: fifteen body BG and brain/neck OBJ colors
    /// 1..15 (native <c>$AD:E6A2</c>) and fifteen rear-leg OBJ colors 1..15 (native <c>$AD:E742</c>).
    /// </summary>
    public required Dictionary<string, MotherBrainBodyColoursDocument> HealthBands { get; init; }
}
