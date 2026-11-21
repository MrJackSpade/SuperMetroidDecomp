namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>What one way of colouring a sheet's pixels draws its characters in (<see cref="SpriteImageColourDefinitions"/>).</summary>
/// <param name="TwoBitCharacters">Whether the sheet's characters are two-bit, every pixel keeping its set's colour, colour zero included.</param>
/// <param name="SetColours">The colours one of the sheet's colour sets holds.</param>
internal sealed partial record SpriteImageColourDefinition(bool TwoBitCharacters, int SetColours);
