using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable intro Rinka OAM art, without movement or hit behavior.</summary>
public sealed class IntroRinkaSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroRinkaSpritePresentation), frames);

    private IntroRinkaSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends an installed intro Rinka composition to OAM without changing its actor's movement, hit behavior, or animation timing.</summary>
    /// <param name="pointer">Native bank-$8C frame identity $8C8D, $8CA3, or $8CB9.</param>
    /// <param name="oam">Destination object-attribute buffer receiving the authored parts in their original order.</param>
    /// <param name="x">Horizontal screen-space origin in pixels, including wrapping unsigned representations of negative coordinates.</param>
    /// <param name="y">Vertical screen-space origin in pixels, including wrapping unsigned representations of negative coordinates.</param>
    /// <param name="paletteBits">Actor's OBJ palette bits, used only for parts configured to inherit a palette.</param>
    /// <param name="originIsOnScreen">Whether to use ordinary origin clipping; false selects the cinematic negative-origin Y-wrap clipping path.</param>
    /// <exception cref="InvalidDataException">The requested frame identity is not installed.</exception>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException($"Intro Rinka frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Validates and compiles all three named intro Rinka frames into independently owned visual compositions.</summary>
    /// <param name="json">Readable JSON stream at its current position; it remains open and caller-owned.</param>
    /// <returns>Artwork keyed by the original bank-$8C frame identities, unaffected by subsequent edits to authoring arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate properties, has an unsupported version or frame set, or contains invalid OAM visual fields.</exception>
    public static IntroRinkaSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroRinkaSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroRinkaSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Intro Rinka sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid intro Rinka sprite JSON.", error);
        }
        IReadOnlyList<IntroRinkaSpriteFrameDefinition> definitions = IntroRinkaSpriteDefinitions.Frames;
        if (document.Version != IntroRinkaSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro Rinkas require three named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroRinkaSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Intro Rinka frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroRinkaParts.CalculateIfMatching(definition.Pointer,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroRinkaSpritePresentation(frames);
    }

    /// <summary>Serializes and validates the complete Rinka visual document before writing any bytes to the destination.</summary>
    /// <param name="json">Writable destination at its current position; it remains open and caller-owned.</param>
    /// <param name="document">Editable artwork containing the supported version and all three canonical frame names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document does not satisfy the loader's schema and visual-field constraints.</exception>
    public static void Write(Stream json, IntroRinkaSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Mutable JSON authoring data for the opening flashback's Rinka sprites, not the room-enemy Rinka AI or its physical definitions.</summary>
public sealed record IntroRinkaSpriteDocument
{
    /// <summary>Gets the schema revision, which must equal <see cref="IntroRinkaSpriteFormat.Version"/> when loaded or written.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the caller-owned ordered parts keyed by rinka-0, rinka-1, and rinka-2.</summary>
    /// <remarks>Stock frames mirror one 8-pixel tile into a 16-by-16-pixel four-part composition, ordered bottom-right, bottom-left, top-right, then top-left. Editable parts use pixel offsets and a 16-column by 32-row tile atlas; null palettes inherit the actor's selector. Loading copies compiled values, not these mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed visual resource for the three intro Rinka frames.</summary>
public static class IntroRinkaSpriteFormat
{
    /// <summary>Supported JSON visual-schema revision; actor motion and instruction timing remain in cinematic definitions.</summary>
    public const int Version = 1;
    /// <summary>Installation-relative JSON filename selected for the three intro Rinka OAM compositions.</summary>
    public const string FileName = "intro-rinka-sprites.json";
}
