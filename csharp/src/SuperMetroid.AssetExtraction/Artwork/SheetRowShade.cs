using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A shade the import pre-colours a sheet in as the cartridge shows its frames when their draw selects another palette
/// row (#1512): a hit flash's, a frozen enemy's, a Ninja Pirate's gold wall flash. Each pixel of a part that takes its
/// draw's row shows the colour of its own colour in the row its part's own bits combine with <see cref="Row"/> into,
/// among <see cref="SpriteColours"/>; a part that keeps its own row is left as it is. The layout derives the inks from
/// the pixels it paints: every ink whose pixels the draw takes from another row, whether or not the colour there equals
/// its standard one, so a shade laid before the selection never shows through it; an ink whose row the selection leaves
/// as it is stays out, as the draw shows what that row holds. It rejects a sheet where one ink would need two colours.
/// </summary>
/// <param name="Shade">The shade's name, which the game's draws give.</param>
/// <param name="Row">The palette row the draw selects.</param>
/// <param name="SpriteColours">The eight sprite rows' colours, sixteen each, as the selected row shows them.</param>
internal sealed record SheetRowShade(PictureShade Shade, int Row, RgbColor[] SpriteColours);
