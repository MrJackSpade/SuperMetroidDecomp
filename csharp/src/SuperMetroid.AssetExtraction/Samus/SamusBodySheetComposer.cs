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
            SpriteArtwork characters = body.BodyArtwork(key.TopRecord, key.BottomRecord);
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
        int?[] inheritedBottoms = [null, .. Selections(body)
            .Where(entry => entry.Selection.NamesBottom)
            .Select(entry => (int?)body.DefinitionRecord(SamusBodyHalf.Bottom, entry.Selection.BottomSet, entry.Selection.BottomPosition))
            .Distinct()];
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach ((SamusPoseId pose, int animationFrame, SamusBodyFrameSelection selection) in Selections(body))
        {
            int top = body.DefinitionRecord(SamusBodyHalf.Top, selection.TopSet, selection.TopPosition);
            int?[] bottoms = !selection.NamesBottom ? inheritedBottoms
                : [body.DefinitionRecord(SamusBodyHalf.Bottom, selection.BottomSet, selection.BottomPosition)];
            foreach (SamusBodyHalf half in Halves)
            {
                var halfFrame = new SamusBodyHalfFrame(pose, half, animationFrame);
                if (!spritemaps.TryGetFrame(halfFrame, out SpriteFrame? frame))
                    continue;
                foreach (int? bottom in bottoms)
                {
                    if (spritemaps.TryGetFrameKey(halfFrame, new SamusBodyRecords(top, bottom), out SamusBodyFrameKey key) &&
                        SamusBodySheets.Sheet(key) == sheet && names.Add(key.Name))
                        yield return KeyValuePair.Create(key, frame);
                }
            }
        }
    }

    /// <summary>Every animation frame the tables hold a selection for, in table order.</summary>
    private static IEnumerable<SelectedFrame> Selections(InstalledSamusBodyArtwork body)
    {
        for (int pose = 0; pose < InstalledSamusBodyArtwork.PoseCount; pose++)
        {
            for (int animationFrame = 0; animationFrame < SamusBodySheetDefinitions.AnimationFrames; animationFrame++)
            {
                if (body.TryFrame((SamusPoseId)pose, animationFrame, out SamusBodyFrameSelection selection))
                    yield return new SelectedFrame((SamusPoseId)pose, animationFrame, selection);
            }
        }
    }

    /// <summary>An animation frame the body tables select a picture for.</summary>
    /// <param name="Pose">The pose.</param>
    /// <param name="AnimationFrame">The pose's animation frame.</param>
    /// <param name="Selection">What the tables select.</param>
    private readonly record struct SelectedFrame(SamusPoseId Pose, int AnimationFrame, SamusBodyFrameSelection Selection);
}
