using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes Samus's body sheet (#1392, #1513): one full-body picture for every body an update draws, at every animation
/// frame the tables hold, over the definitions that frame selects. Each picture is the frame's top half, the chest cover
/// the forward-facing Power Suit body shows, and its bottom half unless the top half is the complete body, in that
/// drawing order; which halves and cover a frame shows is decided by the draw's own <see cref="SamusBodyDrawing"/>. The
/// tables are covered whole, since glitched animation frames index past a pose's program. Frames are coloured in the
/// Power Suit; the other suits, like palette effects, draw through the sheet's palette map.
/// </summary>
internal static class SamusBodySheetComposer
{
    /// <summary>
    /// <c>$90:868D</c>: the chest cover is standard sprite character $21 in Samus's palette row, in front of every
    /// background.
    /// </summary>
    private static readonly SpritePart ChestCoverAppearance = SpritePart.Single(character: 33, palette: 4, priority: 3);

    /// <summary>What the body sheet composes.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        SamusBodyCharacterArtwork body = inputs.SamusBody;
        var frames = new List<SheetFrameSource>();
        foreach ((SamusBodyFrameKey key, SpriteFrame frame) in Frames(body.Body))
        {
            SpriteArtwork characters = body.BodyArtwork(key.TopPicture, key.BottomPicture);
            // A part naming characters past the body's, the chest cover among them, reads the standard OBJ sheet there.
            if (frame.LastCharacter >= characters.CharacterCount)
                characters = SpriteArtwork.Overlay(new SpriteArtworkId(SpriteArtworkKind.SamusBodyOnStandardObjects, characters.Id.Variant),
                    inputs.StandardObjects, characters, 0);
            frames.Add(new(key.Name, frame, characters));
        }
        return new SpriteSheetSources(frames, inputs.SuitedColours(), ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades);
    }

    /// <summary>The names of the frames the body sheet holds.</summary>
    internal static IEnumerable<string> FrameNames(InstalledSamusBodyArtwork body) =>
        Frames(body).Select(frame => frame.Key.Name);

    /// <summary>
    /// Every distinct full-body frame the sheet holds, in table order. A frame that transfers no bottom half keeps the
    /// bottom characters an earlier frame selected, or none before any did, so it is covered over each of those. A body
    /// whose halves reach past the table draws nothing the sheet could hold: the draw rejects it.
    /// </summary>
    private static IEnumerable<KeyValuePair<SamusBodyFrameKey, SpriteFrame>> Frames(InstalledSamusBodyArtwork body)
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
            foreach (SamusMovementType movementType in SamusBodyDrawing.MovementTypesDrawnWith(pose))
            {
                SamusBodyFrame drawn = SamusBodyDrawing.Frame(movementType, pose, animationFrame, body);
                if (!Covers(spritemaps, drawn.Top) || (drawn.Bottom is { } lower && !Covers(spritemaps, lower)))
                    continue;
                foreach (SamusBodyPicture? bottom in bottoms)
                {
                    if (spritemaps.TryGetFrameKey(drawn.Top, drawn.Bottom, drawn.ChestCover, new SamusBodyPictures(top, bottom),
                            out SamusBodyFrameKey key) && names.Add(key.Name))
                        yield return KeyValuePair.Create(key, Compose(key.Name, spritemaps, drawn));
                }
            }
        }
    }

    /// <summary>Whether <paramref name="half"/>'s animation frame is one the table holds for its pose half.</summary>
    private static bool Covers(SamusSpritemapSources spritemaps, SamusBodyHalfFrame half) =>
        (uint)half.AnimationFrame < (uint)spritemaps.Frames(half).Count;

    /// <summary>
    /// <paramref name="drawn"/> as one whole frame: the top half's parts, the chest cover at its place, then the bottom
    /// half's parts, frontmost first as the draw emits them.
    /// </summary>
    private static SpriteFrame Compose(string name, SamusSpritemapSources spritemaps, SamusBodyFrame drawn)
    {
        ImmutableArray<SpriteFramePart>.Builder parts = ImmutableArray.CreateBuilder<SpriteFramePart>();
        if (spritemaps.TryGetFrame(drawn.Top, out SpriteFrame? top))
            parts.AddRange(top.Parts);
        if (drawn.ChestCover is { } chest)
            parts.Add(SpritePartFrames.Part(ChestCoverAppearance with { Offset = chest, Large = false }, 0));
        if (drawn.Bottom is { } lower && spritemaps.TryGetFrame(lower, out SpriteFrame? bottom))
            parts.AddRange(bottom.Parts);
        return new SpriteFrame(name, parts.ToImmutable());
    }

    /// <summary>Every animation frame the body shows installed pictures at, in pose and frame order.</summary>
    private static IEnumerable<SelectedFrame> Selections(InstalledSamusBodyArtwork body)
    {
        for (int pose = 0; pose < InstalledSamusBodyArtwork.PoseCount; pose++)
        {
            for (int animationFrame = 0; animationFrame < SamusBodySheetDefinitions.AnimationFrames; animationFrame++)
            {
                if (body.TryFrame(SamusPoseNumbering.FromNumber(pose), animationFrame, out SamusBodyFramePictures pictures))
                    yield return new SelectedFrame(SamusPoseNumbering.FromNumber(pose), animationFrame, pictures);
            }
        }
    }

    /// <summary>An animation frame the body shows installed pictures at.</summary>
    /// <param name="Pose">The pose.</param>
    /// <param name="AnimationFrame">The pose's animation frame.</param>
    /// <param name="Pictures">The half pictures the frame selects.</param>
    private readonly record struct SelectedFrame(SamusPoseId Pose, int AnimationFrame, SamusBodyFramePictures Pictures);
}
