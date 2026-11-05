using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes Samus's body sheet (#1392, #1513): one full-body frame for every body the game can draw, and nothing else.
/// The bodies are the ones <see cref="SamusShownFrames"/> gives: each frame each pose can show, with the movement types it
/// is drawn with and the bottom pictures it can show. Each picture is the frame's top half, the chest cover the
/// forward-facing Power Suit body shows, and its bottom half unless the top half is the complete body, in that drawing
/// order; which halves and cover a frame shows is decided by the draw's own <see cref="SamusBodyDrawing"/>. A body whose
/// selected characters are blank wherever its compositions read them shows nothing: the sheet holds it as a frame without
/// a picture. Bodies that show the same picture share it on the sheet. Frames are coloured in the Power Suit; the other
/// suits, like palette effects, draw through the sheet's palette map.
/// </summary>
internal static class SamusBodySheetComposer
{
    /// <summary>
    /// <c>$90:868D</c>: the chest cover is standard sprite character $21 in Samus's palette row, in front of every
    /// background.
    /// </summary>
    private static readonly SpritePart ChestCoverAppearance = SpritePart.Single(character: 33, palette: 4, priority: 3);

    /// <summary>
    /// What the body sheet composes: the picture of every body the game can show that shows anything, and as frames that
    /// show nothing the bodies whose selected characters are blank wherever their compositions read them (#1513).
    /// </summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        SamusBodyCharacterArtwork characters = inputs.SamusBody;
        var frames = new List<SheetFrameSource>();
        var blank = new List<string>();
        var overlaid = new Dictionary<SpriteArtwork, SpriteArtwork>(ReferenceEqualityComparer.Instance);
        foreach ((SamusBodyFrameKey key, SpriteFrame frame) in Bodies(characters.Body, inputs.AnimationContinuation))
        {
            SpriteArtwork read = characters.BodyArtwork(key.TopPicture, key.BottomPicture);
            // A part naming characters past the body's, the chest cover among them, reads the standard OBJ sheet there.
            if (frame.LastCharacter >= read.CharacterCount)
            {
                if (!overlaid.TryGetValue(read, out SpriteArtwork? onStandardObjects))
                {
                    onStandardObjects = SpriteArtwork.Overlay(new SpriteArtworkId(SpriteArtworkKind.SamusBodyOnStandardObjects, read.Id.Variant),
                        inputs.StandardObjects, read, 0);
                    overlaid.Add(read, onStandardObjects);
                }
                read = onStandardObjects;
            }
            var source = new SheetFrameSource(key.Name, frame, read);
            if (source.ShowsAnything())
                frames.Add(source);
            else
                blank.Add(key.Name);
        }
        return new SpriteSheetSources(frames, inputs.SuitedColours(), ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades,
            Blank: blank);
    }

    /// <summary>
    /// The names of the frames the body sheet installed under <paramref name="content"/> holds, pictured or showing nothing:
    /// every body the game can show with the installed body.
    /// </summary>
    /// <param name="content">The installation's content directory.</param>
    internal static IReadOnlyList<string> FrameNames(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        return [.. Bodies(
            SamusBodyArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.SamusBodyDirectoryName)),
            SamusOverreadFiles.LoadAnimationContinuation(Path.Combine(content, GameInstallationLayout.SamusOverreadDirectoryName)))
            .Select(body => body.Key.Name)];
    }

    /// <summary>
    /// Every distinct full body the game can show, in pose and frame order, as its frame. A body whose halves reach past the
    /// table draws nothing the sheet could hold: the draw rejects it.
    /// </summary>
    private static IEnumerable<KeyValuePair<SamusBodyFrameKey, SpriteFrame>> Bodies(InstalledSamusBodyArtwork body,
        LaidOutProgramContinuation<SamusPoseId> continuation)
    {
        SamusSpritemapSources spritemaps = body.Spritemaps.Source;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (SamusShownBody shown in SamusShownFrames.Of(continuation).Bodies(body))
        {
            SamusBodyFrame drawn = shown.Frame;
            if (!Covers(spritemaps, drawn.Top) || (drawn.Bottom is { } lower && !Covers(spritemaps, lower)))
                continue;
            if (spritemaps.TryGetFrameKey(drawn.Top, drawn.Bottom, drawn.ChestCover, shown.Pictures, out SamusBodyFrameKey key) &&
                names.Add(key.Name))
                yield return KeyValuePair.Create(key, Compose(key.Name, spritemaps, drawn));
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
}
