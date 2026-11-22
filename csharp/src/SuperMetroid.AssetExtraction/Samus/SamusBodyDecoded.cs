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
/// <param name="Compositions">Each composition's sprite parts, frontmost first, by the composition's name.</param>
/// <param name="Explosion">The composition each suit-explosion sheet frame shows, or none for a frame that shows nothing.</param>
/// <param name="Atmosphere">The composition each diving-splash and bubbles sheet frame shows, or none for a frame that shows nothing.</param>
internal sealed partial record SamusBodyDecoded(
    IReadOnlyDictionary<SamusPoseId, int> ArtOrigins,
    IReadOnlyDictionary<SamusPoseId, IReadOnlyDictionary<int, SamusBodyDecodedFrame>> Frames,
    IReadOnlyDictionary<SamusSpritemapId, SpriteVisualPiece[]> Compositions,
    IReadOnlyDictionary<string, SamusSpritemapId?> Explosion,
    IReadOnlyDictionary<string, SamusSpritemapId?> Atmosphere)
{
    /// <summary>
    /// The body as the import reads it, with the dependent catalogs: each frame's halves show the compositions it decoded.
    /// </summary>
    /// <param name="atmosphere">The appearances of the atmospheric effects.</param>
    /// <param name="deathSteps">Her death steps.</param>
    /// <param name="armCannon">The arm cannon's descriptors, appearances and cover tiles.</param>
    internal SamusBodyArtworkParts Parts(SamusAtmosphericSourceArtwork atmosphere, SuperMetroid.Core.Samus.Death.SamusDeathSteps deathSteps,
        SamusArmCannonSourceArtwork armCannon)
    {
        var halves = new Dictionary<SamusBodyHalfFrame, SamusSpritemapId?>();
        foreach ((SamusPoseId pose, IReadOnlyDictionary<int, SamusBodyDecodedFrame> frames) in Frames)
        {
            foreach ((int frame, SamusBodyDecodedFrame decoded) in frames)
            {
                halves.Add(new SamusBodyHalfFrame(pose, SamusBodyHalf.Top, frame), decoded.TopComposition);
                halves.Add(new SamusBodyHalfFrame(pose, SamusBodyHalf.Bottom, frame), decoded.BottomComposition);
            }
        }
        var spritemaps = new SamusSpritemapSources(
            Compositions.ToDictionary(pair => pair.Key, pair => new SamusSpritemapDefinition(
                [.. pair.Value.Select(part => part.ToSpritePart(SpriteArtwork.CharactersPerRow))])),
            halves, Explosion, Atmosphere);
        return new SamusBodyArtworkParts { ArtOrigins = ArtOrigins, Frames = Frames, Spritemaps = spritemaps, Atmosphere = atmosphere, DeathSteps = deathSteps, ArmCannon = armCannon };
    }
}
