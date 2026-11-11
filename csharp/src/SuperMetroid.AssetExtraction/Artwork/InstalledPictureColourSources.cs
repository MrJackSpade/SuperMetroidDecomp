using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where the colours of a picture the import composed came from (#1512, #1544, #1633): the half of the colour table it was
/// composed against, the background sets of its colour groups, group after group, or none, the palette bits a sprite
/// sheet's draws OR into each part's own row, and, for each colour key of the picture's visible pixels and backdrop, the
/// composed palette indices its pixels of that key were composed as. The import's knowledge, kept beside a composed picture; never installed.
/// </summary>
/// <param name="Palette">The half of the colour table the sheet was composed against.</param>
/// <param name="Sets">The sets, one per colour group, or empty for a sheet that names none.</param>
/// <param name="PaletteBits">The palette bits ORed into each sprite part's own row; zero for every other sheet.</param>
/// <param name="ComposedIndices">
/// For each colour key of the picture's visible pixels and its backdrop, the composed palette indices, in rising order, of
/// its opaque pixels of that key, and of its backdrop where that is the key's. A clear pixel is never keyed.
/// </param>
internal sealed record InstalledPictureColourSources(SpriteImageColours Palette, ImmutableArray<InstalledBackgroundSet> Sets, int PaletteBits,
    ImmutableDictionary<ColourKey, ImmutableArray<int>> ComposedIndices);
