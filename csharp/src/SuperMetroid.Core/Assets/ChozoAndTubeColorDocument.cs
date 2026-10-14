namespace SuperMetroid.Core.Assets;

/// <summary>Editable tube-crack and Chozo-statue RGB5 images, each retaining two ordered sixteen-color OBJ palettes including their transparent-slot words.</summary>
public sealed record ChozoAndTubeColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>32 ordered tube-crack inks corresponding to $AA:E2DD–$E31C, filling OBJ palettes 1 and 2; stock halves repeat but edits need not.</summary>
    public required PaletteRgb5[] TubeCracks { get; init; }
    /// <summary>32 ordered Wrecked Ship Chozo inks corresponding to $AA:E31D–$E35C, filling OBJ palettes 1 and 2.</summary>
    public required PaletteRgb5[] WreckedShip { get; init; }
    /// <summary>32 ordered Lower Norfair Chozo inks corresponding to $AA:E35D–$E39C, filling OBJ palettes 1 and 2 independently of the Wrecked Ship image.</summary>
    public required PaletteRgb5[] LowerNorfair { get; init; }
}
