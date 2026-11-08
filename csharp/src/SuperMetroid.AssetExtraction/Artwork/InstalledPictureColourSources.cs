using System.Collections.Immutable;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where a loaded picture's manifest says its colours were installed from (#1512, #1544): the half of the colour table it
/// was composed against, the background sets of its colour groups, group after group, or none, the palette bits a sprite
/// sheet's draws OR into each ink's own row, and the composed palette index each of its own colours came from.
/// </summary>
/// <param name="Palette">The half of the colour table the sheet was composed against.</param>
/// <param name="Sets">The sets, one per colour group, or empty for a sheet that names none.</param>
/// <param name="PaletteBits">The palette bits ORed into each sprite ink's own row; zero for every other sheet.</param>
/// <param name="Inks">The composed palette index of each of the picture's own colours, in their order.</param>
internal sealed record InstalledPictureColourSources(SpriteImageColours Palette, ImmutableArray<InstalledBackgroundSet> Sets, int PaletteBits,
    ImmutableArray<int> Inks);
