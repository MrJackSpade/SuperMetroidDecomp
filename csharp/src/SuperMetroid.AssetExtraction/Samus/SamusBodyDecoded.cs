using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body as <see cref="SamusBodyTableDecoding"/> decodes it from the cartridge's tables (#2556), which the import
/// stores as the body's import data: no record number, selector or table entry survives into it.
/// </summary>
/// <param name="ArtOrigins">Each pose with a record's art origin.</param>
/// <param name="Frames">
/// Each pose with a record's picture frames whose half pictures, placement and compositions the tables hold, by the frame's
/// place in the game's layout of the pose's animation.
/// </param>
/// <param name="Pictures">The characters each half picture transfers, by the picture's name.</param>
/// <param name="Compositions">Each composition's sprite parts, frontmost first, by the composition's name.</param>
/// <param name="Explosion">The composition each suit-explosion sheet frame shows, or none for a frame that shows nothing.</param>
/// <param name="Atmosphere">The composition each diving-splash and bubbles sheet frame shows, or none for a frame that shows nothing.</param>
internal sealed record SamusBodyDecoded(
    IReadOnlyDictionary<SamusPoseId, int> ArtOrigins,
    IReadOnlyDictionary<SamusPoseId, Dictionary<int, SamusBodyDecodedFrame>> Frames,
    IReadOnlyDictionary<SamusBodyPicture, SamusBodyTileTransfer> Pictures,
    IReadOnlyDictionary<SamusSpritemapId, SpriteVisualPart[]> Compositions,
    IReadOnlyDictionary<string, SamusSpritemapId?> Explosion,
    IReadOnlyDictionary<string, SamusSpritemapId?> Atmosphere);
