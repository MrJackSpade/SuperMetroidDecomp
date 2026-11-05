using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes Samus's body sheet (#1392, #1513): one full-body frame for every body the game can draw, and nothing else.
/// The bodies are the ones <see cref="SamusShownFrames"/> gives: each picture frame of each pose, the frames its own
/// animation shows a picture on, which every frame the body shows draws, with the bottom pictures it can show. Each
/// picture is the frame's top half, the chest cover the forward-facing Power Suit body shows, and its bottom half unless
/// the pose's own movement type makes the top half the complete body, in that drawing order; which halves and cover a
/// frame shows is decided by the draw's own <see cref="SamusBodyDrawing"/>, so every frame is a whole Samus. A body whose
/// selected characters are blank wherever its compositions read them shows nothing: the sheet holds it as a frame without
/// a picture. Bodies that show the same picture share it on the sheet. Frames are coloured in the Power Suit with the
/// power beam, and a frame a pose always drawn in another colouring shows in that colouring's first shades over the rows
/// it inks (<see cref="SamusBodyColouringDefinitions"/>); the other suits, like palette effects, draw through the sheet's
/// shades.
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
        var colourings = new Dictionary<string, PictureShade[]>(StringComparer.Ordinal);
        foreach (SheetBody body in Bodies(characters.Body, inputs.AnimationContinuation))
        {
            SamusBodyFrameKey key = body.Key;
            SpriteFrame frame = body.Frame;
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
            {
                frames.Add(source);
                if (body.Colouring is { } colouring)
                    colourings.Add(key.Name, colouring);
            }
            else
            {
                blank.Add(key.Name);
            }
        }
        return new SpriteSheetSources(frames, WithPoseColourings(inputs.SuitedColours(), frames, colourings, inputs.Shades),
            ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades, Blank: blank);
    }

    /// <summary>
    /// <paramref name="sheetColours"/> with each of <paramref name="frames"/> that <paramref name="colourings"/> names a
    /// colouring for coloured in it (#1513): the colouring's shades among <paramref name="shades"/> laid over the colour
    /// rows the frame's parts ink, so the frame's picture shows the colours the game always draws it in. Her draws lay
    /// those shades over the sheet at runtime as before, so what she shows is unchanged.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A colouring names a shade the sheet lacks, two frames need one colour in two colourings, or a frame drawn in no
    /// colouring inks a row a colouring recolours: the sheet's colours are one set for all its frames.
    /// </exception>
    private static RgbColor[] WithPoseColourings(RgbColor[] sheetColours, IReadOnlyList<SheetFrameSource> frames,
        Dictionary<string, PictureShade[]> colourings, IReadOnlyList<SheetShade> shades)
    {
        RgbColor[] shown = [.. sheetColours];
        var recoloured = new Dictionary<int, RgbColor>();
        foreach (SheetFrameSource frame in frames)
        {
            if (!colourings.TryGetValue(frame.Name, out PictureShade[]? colouring))
                continue;
            HashSet<int> rows = RowsOf(frame);
            var left = new Dictionary<int, RgbColor>();
            foreach (PictureShade name in colouring)
            {
                SheetShade shade = shades.FirstOrDefault(candidate => candidate.Shade == name)
                    ?? throw new InvalidDataException($"Samus's sheets have no shade {name.Effect} {name.Step} to colour body frame {frame.Name} in.");
                for (int index = 0; index < shade.Inks.Length; index++)
                {
                    if (rows.Contains(shade.Inks[index] / SamusSheetInks.GroupColours))
                        left[shade.Inks[index]] = shade.Colours[index];
                }
            }
            foreach (KeyValuePair<int, RgbColor> ink in left)
            {
                if (recoloured.TryGetValue(ink.Key, out RgbColor other) && other != ink.Value)
                    throw new InvalidDataException($"Samus's body frames are drawn with sprite colour {ink.Key} in two colourings, frame {frame.Name}'s among them.");
                recoloured[ink.Key] = ink.Value;
                shown[ink.Key] = ink.Value;
            }
        }
        HashSet<int> recolouredRows = [.. recoloured.Keys.Select(ink => ink / SamusSheetInks.GroupColours)];
        foreach (SheetFrameSource frame in frames)
        {
            if (!colourings.ContainsKey(frame.Name) && RowsOf(frame).Overlaps(recolouredRows))
                throw new InvalidDataException($"Samus's body frame {frame.Name} is drawn in no colouring but inks a colour row frames always drawn in one are pre-coloured in.");
        }
        return shown;
    }

    /// <summary>The colour rows <paramref name="frame"/>'s parts ink, each its own or the frame's.</summary>
    private static HashSet<int> RowsOf(SheetFrameSource frame) =>
        [.. frame.Frame.Parts.Select(part => part.Palette ?? frame.Palette ?? 0)];

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
    /// Every distinct full body the game can show, in pose and frame order, as its frame, with the colouring
    /// <see cref="SamusBodyColouringDefinitions"/> names for a pose that shows it. A body whose halves reach past the table
    /// draws nothing the sheet could hold: the draw rejects it.
    /// </summary>
    /// <exception cref="InvalidDataException">Poses with two different colourings show one body.</exception>
    private static List<SheetBody> Bodies(InstalledSamusBodyArtwork body, LaidOutProgramContinuation<SamusPoseId> continuation)
    {
        SamusSpritemapSources spritemaps = body.Spritemaps.Source;
        var bodies = new List<SheetBody>();
        var named = new Dictionary<string, SheetBody>(StringComparer.Ordinal);
        foreach (SamusShownBody shown in SamusShownFrames.Of(continuation).Bodies(body))
        {
            SamusBodyFrame drawn = shown.Frame;
            if (!Covers(spritemaps, drawn.Top) || (drawn.Bottom is { } lower && !Covers(spritemaps, lower)))
                continue;
            if (!spritemaps.TryGetFrameKey(drawn.Top, drawn.Bottom, drawn.ChestCover, shown.Pictures, out SamusBodyFrameKey key))
                continue;
            PictureShade[]? colouring = SamusBodyColouringDefinitions.Poses.GetValueOrDefault(drawn.Top.Pose);
            if (named.TryGetValue(key.Name, out SheetBody? known))
            {
                if (colouring is null)
                    continue;
                if (known.Colouring is { } other && !other.SequenceEqual(colouring))
                    throw new InvalidDataException($"Samus's body frame {key.Name} is shown by poses drawn in two colourings.");
                known.Colouring = colouring;
                continue;
            }
            var added = new SheetBody(key, Compose(key.Name, spritemaps, drawn)) { Colouring = colouring };
            named.Add(key.Name, added);
            bodies.Add(added);
        }
        return bodies;
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

    /// <summary>One distinct full body the sheet holds, and the colouring the poses showing it are always drawn in.</summary>
    /// <param name="key">The body's frame.</param>
    /// <param name="frame">The body as one whole frame.</param>
    private sealed class SheetBody(SamusBodyFrameKey key, SpriteFrame frame)
    {
        /// <summary>The body's frame.</summary>
        internal SamusBodyFrameKey Key { get; } = key;

        /// <summary>The body as one whole frame.</summary>
        internal SpriteFrame Frame { get; } = frame;

        /// <summary>The shades a pose showing the body is always drawn in, or null when none of them is.</summary>
        internal PictureShade[]? Colouring { get; set; }
    }
}
