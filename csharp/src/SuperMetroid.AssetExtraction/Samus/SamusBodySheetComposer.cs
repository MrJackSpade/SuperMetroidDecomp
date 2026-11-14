using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes Samus's body sheet (#1392, #1513, #1585): one whole picture for every pose and picture frame the game can
/// draw, named by that pose and frame, and nothing else. The bodies are the ones <see cref="SamusShownFrames"/> gives:
/// each picture frame of each pose, the frames its own animation shows a picture on, which every frame the body shows
/// draws. Each picture is composed of the cartridge's halves its own frame selects: the frame's top half, the chest cover
/// the forward-facing Power Suit body shows, and its bottom half unless the pose's own movement type makes the top half
/// the complete body, in that drawing order (<see cref="SamusBodyCompositions"/>), so every frame is a whole Samus and
/// nothing an earlier frame showed carries into it. A picture that inks nothing is installed as an empty frame, which
/// draws nothing. Bodies that show the same picture share it on the sheet. Frames are coloured in the Power Suit with the
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
    /// show nothing the bodies whose compositions ink no pixel (#1513, #1585).
    /// </summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        SamusBodyCharacterArtwork characters = inputs.SamusBody;
        var frames = new List<SheetFrameSource>();
        var blank = new List<string>();
        var overlaid = new Dictionary<SpriteArtwork, SpriteArtwork>(ReferenceEqualityComparer.Instance);
        var colourings = new Dictionary<string, PictureShade[]>(StringComparer.Ordinal);
        foreach (SheetBody body in Bodies(characters.Body, characters.Body.InstalledBody(inputs.FramesBeyondAnimation)))
        {
            if (body.Picture is not { } picture)
            {
                blank.Add(body.Name);
                continue;
            }
            SpriteFrame frame = picture.Frame;
            SpriteArtwork read = characters.BodyArtwork(picture.Key.TopPicture, picture.Key.BottomPicture);
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
            var source = new SheetFrameSource(body.Name, new SpriteFrame(body.Name, frame.Parts), read);
            if (source.ShowsAnything())
            {
                frames.Add(source);
                if (picture.Colouring is { } colouring)
                    colourings.Add(body.Name, colouring);
            }
            else
            {
                blank.Add(body.Name);
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
    /// every body frame the installed body names (#1638), which reads no import data.
    /// </summary>
    /// <param name="content">The installation's content directory.</param>
    internal static IReadOnlyList<string> FrameNames(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        return [.. SamusBodyArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.SamusBodyDirectoryName))
            .Frames.Select(frame => frame.Name)];
    }

    /// <summary>
    /// Every body frame the body sheet pictures, in pose and frame order, with the picture it shows, which bodies composed
    /// alike share with the colouring <see cref="SamusBodyColouringDefinitions"/> names for any pose that shows it.
    /// </summary>
    /// <exception cref="InvalidDataException">Poses with two different colourings show one picture.</exception>
    private static List<SheetBody> Bodies(SamusBodySources body, InstalledSamusBody installed)
    {
        SamusSpritemapSources spritemaps = body.Spritemaps;
        var bodies = new List<SheetBody>();
        var pictures = new Dictionary<SamusBodyPictureKey, SheetPicture>();
        foreach (SamusShownBody shown in body.SheetBodies(installed))
        {
            SamusBodyComposition drawn = shown.Composition;
            string name = shown.Frame.Name;
            // A body with no composition and no chest cover shows nothing (owner decision on #1318).
            if (spritemaps.PictureKey(drawn, shown.Pictures) is not { } key)
            {
                bodies.Add(new SheetBody(name, null));
                continue;
            }
            PictureShade[]? colouring = SamusBodyColouringDefinitions.Poses.GetValueOrDefault(shown.Frame.Pose);
            if (!pictures.TryGetValue(key, out SheetPicture? picture))
            {
                picture = new SheetPicture(key, Compose(name, spritemaps, drawn));
                pictures.Add(key, picture);
            }
            if (colouring is not null)
            {
                if (picture.Colouring is { } other && !other.SequenceEqual(colouring))
                    throw new InvalidDataException($"Samus's body picture {name} is shown by poses drawn in two colourings.");
                picture.Colouring = colouring;
            }
            bodies.Add(new SheetBody(name, picture));
        }
        return bodies;
    }

    /// <summary>
    /// <paramref name="drawn"/> as one whole frame: the top half's parts, the chest cover at its place, then the bottom
    /// half's parts, frontmost first as the draw emits them.
    /// </summary>
    private static SpriteFrame Compose(string name, SamusSpritemapSources spritemaps, SamusBodyComposition drawn)
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

    /// <summary>One pose and picture frame the sheet names, and the picture it shows, or none for one that shows nothing.</summary>
    /// <param name="Name">The body's name on the sheet.</param>
    /// <param name="Picture">The picture the body shows, or null when it has no composition and no chest cover.</param>
    private sealed record SheetBody(string Name, SheetPicture? Picture);

    /// <summary>One distinct whole picture the sheet holds, and the colouring the poses showing it are always drawn in.</summary>
    /// <param name="key">What the picture is composed of.</param>
    /// <param name="frame">The picture as one whole frame.</param>
    private sealed class SheetPicture(SamusBodyPictureKey key, SpriteFrame frame)
    {
        /// <summary>What the picture is composed of.</summary>
        internal SamusBodyPictureKey Key { get; } = key;

        /// <summary>The picture as one whole frame.</summary>
        internal SpriteFrame Frame { get; } = frame;

        /// <summary>The shades a pose showing the picture is always drawn in, or null when none of them is.</summary>
        internal PictureShade[]? Colouring { get; set; }
    }
}
