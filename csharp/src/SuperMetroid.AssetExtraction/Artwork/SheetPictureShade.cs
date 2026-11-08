using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What one shade a sheet is pre-coloured in shows of the colour groups a colouring covers (#1513): the colour of each of
/// those groups' inks it recolours, and whether its draw blends over every part.
/// </summary>
/// <param name="Blends">Whether a draw showing the shade takes part in colour math over every part.</param>
/// <param name="Colours">The colour each recoloured ink of the covered groups shows, by ink; an ink it leaves out keeps what lies under it.</param>
internal sealed record SheetPictureShade(bool Blends, SortedDictionary<int, RgbColor> Colours);
