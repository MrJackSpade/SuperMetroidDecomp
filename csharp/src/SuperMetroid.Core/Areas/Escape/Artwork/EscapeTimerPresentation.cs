using SuperMetroid.Core.Hardware;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>Editable escape-timer spritemap compositions, palette and screen-relative anchors.</summary>
public sealed class EscapeTimerPresentation
{
    /// <summary>Only edited timer compositions; stock frames are calculated when absent.</summary>
    internal readonly Dictionary<string, SpriteComposition> frames;
    /// <summary>Only edited screen-relative offsets; stock anchors are calculated when absent.</summary>
    internal readonly Dictionary<string, Offset16<OamPixels>> anchors;

    /// <summary>Whole frames over the timer artwork, one per composition name.</summary>
    private readonly Dictionary<string, SpriteFrame> spriteFrames;

    /// <summary>Retains validated visual overrides and converts every composition to a whole frame over the timer artwork.</summary>
    private EscapeTimerPresentation(Dictionary<string, SpriteComposition> frames,
        Dictionary<string, Offset16<OamPixels>> anchors, int digitSpacing, int palette)
    {
        this.frames = frames;
        this.anchors = anchors;
        DigitSpacing = digitSpacing;
        Palette = Ensure.BetweenInclusive(0, byte.MaxValue, palette);
        spriteFrames = EscapeTimerPresentationDefinitions.FrameNames.ToDictionary(name => name,
            name => (frames.TryGetValue(name, out var supplied) ? supplied : EscapeTimerPresentationDefinitions.DefaultFrame(name))
                .ToFrame(name, EscapeTimerPresentationDefinitions.FirstCharacter), StringComparer.Ordinal);
    }

    /// <summary>Horizontal pixel displacement from each pair's tens-digit origin to its ones-digit origin; stock spacing is eight pixels.</summary>
    public int DigitSpacing { get; }
    /// <summary>The shared OBJ palette 0..7, applied only to parts that inherit their palette.</summary>
    public int Palette { get; }

    /// <summary>Publishes the timer as whole-frame draws while retaining live BCD/countdown state.</summary>
    /// <param name="timer">Live timer supplying packed-BCD minutes, seconds and centiseconds and the integer pixel origin of its moving position.</param>
    /// <param name="sprites">Frame receiving the TIME label and then the tens and ones digit of each pair, after any sprites already emitted.</param>
    /// <param name="artwork">The installed timer characters the frames index.</param>
    /// <remarks>The caller controls visibility. This method neither checks whether the timer is active nor advances its countdown.</remarks>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidOperationException">A timer byte contains a nondecimal BCD nibble; earlier draws may already have been added.</exception>
    public void Draw(EscapeTimer timer, SpriteFrameBuffer sprites, SpriteArtwork artwork)
    {
        Ensure.NotNull(timer);
        Ensure.NotNull(sprites);
        Ensure.NotNull(artwork);
        DrawFrame(EscapeTimerPresentationDefinitions.LabelFrame, Anchor("Label"));
        DrawPair(timer.Minutes, Anchor("Minutes"));
        DrawPair(timer.Seconds, Anchor("Seconds"));
        DrawPair(timer.Centiseconds, Anchor("Centiseconds"));

        void DrawPair(PackedBcd pair, Offset16<OamPixels> anchor)
        {
            if (!pair.IsValid)
                throw new InvalidOperationException($"Cannot draw invalid packed-BCD timer byte ${pair.Raw:X2}.");
            DrawFrame(EscapeTimerPresentationDefinitions.DigitFrame(pair.Tens), anchor);
            DrawFrame(EscapeTimerPresentationDefinitions.DigitFrame(pair.Ones),
                anchor + new Offset16<OamPixels>(checked((short)DigitSpacing), 0));
        }

        void DrawFrame(string name, Offset16<OamPixels> anchor) =>
            sprites.Add(new SpriteDraw(new SpriteSource(SpriteSourceKind.EscapeTimer), spriteFrames[name], artwork,
                timer.PixelPosition + new Offset16<ScreenPixels>(anchor.X, anchor.Y), SnesTileFlipFlags.None, Palette,
                EscapeTimerPresentationDefinitions.TimerPriority));
    }

    /// <summary>Resolves an edited timer anchor or its calculated stock position.</summary>
    private Offset16<OamPixels> Anchor(string name) => anchors.TryGetValue(name, out var supplied)
        ? supplied : EscapeTimerPresentationDefinitions.DefaultAnchor(name);

    /// <summary>Validates an authored timer layout and compiles its ordered sprite parts and pixel anchors independently of the mutable document.</summary>
    /// <param name="json">UTF-8 JSON read from the current position to the end; the caller retains ownership of the stream.</param>
    /// <returns>A presentation that retains edited compositions and anchors, using calculated native geometry for entries that exactly match the stock layout.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed or ambiguous, required fields or exact frame/anchor names are missing, or values exceed the supported schema and OAM ranges.</exception>
    public static EscapeTimerPresentation Load(Stream json)
    {
        EscapeTimerPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<EscapeTimerPresentationDocument>(json,
                ArtworkJson.Options) ??
                throw new InvalidDataException("Escape timer presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid escape timer presentation JSON.", error);
        }

        if (document.Version != EscapeTimerPresentationDefinitions.Version ||
            document.Frames is null || document.Anchors is null ||
            document.DigitSpacing is < -256 or > 255 || (uint)document.Palette > 7)
            throw new InvalidDataException("Escape timer presentation requires version 1, valid anchors, frames, spacing and palette.");

        string[] expectedFrames = [EscapeTimerPresentationDefinitions.LabelFrame,
            .. Enumerable.Range(0, 10).Select(EscapeTimerPresentationDefinitions.DigitFrame)];
        if (document.Frames.Count != expectedFrames.Length ||
            expectedFrames.Any(name => !document.Frames.ContainsKey(name)))
            throw new InvalidDataException("Escape timer presentation requires Label and Digit.0 through Digit.9 exactly once.");
        if (document.Anchors.Count != EscapeTimerPresentationDefinitions.AnchorNames.Length ||
            EscapeTimerPresentationDefinitions.AnchorNames.Any(name => !document.Anchors.ContainsKey(name)))
            throw new InvalidDataException("Escape timer presentation requires Label, Minutes, Seconds and Centiseconds anchors exactly once.");

        var frames = new Dictionary<string, SpriteComposition>(StringComparer.Ordinal);
        foreach (string name in expectedFrames)
        {
            EscapeTimerVisualPart[] parts = document.Frames[name] ??
                throw new InvalidDataException($"Escape timer frame {name} is null.");
            if (parts.Length > EscapeTimerPresentationDefinitions.MaximumParts)
                throw new InvalidDataException($"Escape timer frame {name} exceeds OAM capacity.");
            var compiled = new CompiledSpritePart[parts.Length];
            for (int index = 0; index < parts.Length; index++)
            {
                EscapeTimerVisualPart part = parts[index] ??
                    throw new InvalidDataException($"Escape timer frame {name} part {index} is null.");
                if (part.OffsetX is < -256 or > 255 || part.OffsetY is < -128 or > 127 ||
                    part.Size is not (8 or 16) || part.TileNumber < EscapeTimerPresentationDefinitions.FirstCharacter ||
                    EscapeTimerPresentationDefinitions.LastCharacterRead(part.TileNumber, part.Size) >
                        EscapeTimerPresentationDefinitions.FirstCharacter + EscapeTimerTileAtlasFormat.TileCount - 1 ||
                    (uint)part.Priority > 3 || part.Palette is < 0 or > 7)
                    throw new InvalidDataException($"Escape timer frame {name} part {index} has invalid OAM presentation data.");
                var attributes = SnesObjAttributeWord.Create(part.TileNumber, part.Palette ?? 0,
                    part.Priority, (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (part.FlipY ? SnesTileFlipFlags.Vertical : 0));
                compiled[index] = new(SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                    unchecked((byte)(sbyte)part.OffsetY), attributes, part.Palette is null);
            }
            if (!compiled.SequenceEqual(EscapeTimerPresentationDefinitions.DefaultParts(name)))
                frames.Add(name, new SpriteComposition(compiled));
        }

        var anchors = new Dictionary<string, Offset16<OamPixels>>(StringComparer.Ordinal);
        foreach (string name in EscapeTimerPresentationDefinitions.AnchorNames)
        {
            Offset16<OamPixels> point = document.Anchors[name];
            if (point.X is < -256 or > 255 || point.Y is < -224 or > 223)
                throw new InvalidDataException($"Escape timer anchor {name} is outside the supported screen-relative range.");
            if (point != EscapeTimerPresentationDefinitions.DefaultAnchor(name)) anchors.Add(name, point);
        }
        return new(frames, anchors, document.DigitSpacing, document.Palette);
    }

    /// <summary>Serializes and validates a timer document before writing any of its UTF-8 JSON bytes.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored layout whose collections are read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails the same schema, composition and anchor checks as <see cref="Load"/>.</exception>
    public static void Write(Stream output, EscapeTimerPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}
