using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes Samus's body sheets (#1392): every spritemap a pose's top or bottom base reaches at any animation frame the
/// tables hold, over the definitions that frame selects, named by the records whose characters it reads. The tables
/// are covered whole, since glitched animation frames index past a pose's program. Frames are coloured in the Power
/// Suit; the other suits, like palette effects, draw through the sheet's palette map.
/// </summary>
internal static class SamusBodySheetComposer
{
    /// <summary>The body halves each animation frame shows a picture of, in drawing order.</summary>
    private static readonly SamusBodyHalf[] Halves = [SamusBodyHalf.Top, SamusBodyHalf.Bottom];

    /// <summary>What one body sheet composes.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs, SpriteSheetId sheet)
    {
        Ensure.NotNull(inputs);
        SamusBodyCharacterArtwork body = inputs.SamusBody;
        var frames = new List<SheetFrameSource>();
        foreach ((SamusBodyFrameKey key, SpriteFrame frame) in Frames(body.Body, sheet))
        {
            SpriteArtwork characters = body.BodyArtwork(key.TopPicture, key.BottomPicture);
            // A spritemap naming characters past the body's reads the standard OBJ sheet there.
            if (frame.LastCharacter >= characters.CharacterCount)
                characters = SpriteArtwork.Overlay(new SpriteArtworkId(SpriteArtworkKind.SamusBodyOnStandardObjects, characters.Id.Variant),
                    inputs.StandardObjects, characters, 0);
            frames.Add(new(key.Name, frame, characters));
        }
        return new SpriteSheetSources(frames, inputs.SuitedColours(), ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection));
    }

    /// <summary>The names of the frames a body sheet holds.</summary>
    internal static IEnumerable<string> FrameNames(InstalledSamusBodyArtwork body, SpriteSheetId sheet) =>
        Frames(body, sheet).Select(frame => frame.Key.Name);

    /// <summary>
    /// Every distinct frame a body sheet holds, in table order. A frame that transfers no bottom half keeps the bottom
    /// characters an earlier frame selected, or none before any did, so it is covered over each of those.
    /// </summary>
    private static IEnumerable<KeyValuePair<SamusBodyFrameKey, SpriteFrame>> Frames(InstalledSamusBodyArtwork body, SpriteSheetId sheet)
    {
        SamusSpritemapSources spritemaps = body.Spritemaps.Source;
        SamusBodyPicture?[] inheritedBottoms = [null, .. Selections(body)
            .Where(entry => entry.Pictures.Bottom is not null)
            .Select(entry => entry.Pictures.Bottom)
            .Distinct()];
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach ((SamusPoseId pose, int animationFrame, SamusBodyFramePictures pictures) in Selections(body))
        {
            SamusBodyPicture top = pictures.Top;
            SamusBodyPicture?[] bottoms = pictures.Bottom is null ? inheritedBottoms : [pictures.Bottom];
            foreach (SamusBodyHalf half in Halves)
            {
                var halfFrame = new SamusBodyHalfFrame(pose, half, animationFrame);
                if (!spritemaps.TryGetFrame(halfFrame, out SpriteFrame? frame))
                    continue;
                foreach (SamusBodyPicture? bottom in bottoms)
                {
                    if (spritemaps.TryGetFrameKey(halfFrame, new SamusBodyPictures(top, bottom), out SamusBodyFrameKey key) &&
                        SamusBodySheets.Sheet(key) == sheet && names.Add(key.Name))
                        yield return KeyValuePair.Create(key, frame);
                }
            }
        }
    }

    /// <summary>Every animation frame the body shows installed pictures at, in pose and frame order.</summary>
    private static IEnumerable<SelectedFrame> Selections(InstalledSamusBodyArtwork body)
    {
        for (int pose = 0; pose < InstalledSamusBodyArtwork.PoseCount; pose++)
        {
            for (int animationFrame = 0; animationFrame < SamusBodySheetDefinitions.AnimationFrames; animationFrame++)
            {
                if (body.TryFrame((SamusPoseId)pose, animationFrame, out SamusBodyFramePictures pictures))
                    yield return new SelectedFrame((SamusPoseId)pose, animationFrame, pictures);
            }
        }
    }

    /// <summary>An animation frame the body shows installed pictures at.</summary>
    /// <param name="Pose">The pose.</param>
    /// <param name="AnimationFrame">The pose's animation frame.</param>
    /// <param name="Pictures">The half pictures the frame selects.</param>
    private readonly record struct SelectedFrame(SamusPoseId Pose, int AnimationFrame, SamusBodyFramePictures Pictures);
}
