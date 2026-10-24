using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What one band of Crocomire's melt image shows: its pass's image after a number of dissolve steps.</summary>
/// <param name="Pass">The melt image the band shows.</param>
/// <param name="Step">Dissolve steps taken when the band was shown, zero through <see cref="CrocomireMeltingDefinitions.ColumnCount"/>.</param>
public readonly record struct CrocomireMeltBandPicture(CrocomireMeltingIllustration Pass, int Step);
