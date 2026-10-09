using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable intro Mother Brain OAM art, separate from AI and instruction timing.</summary>
public sealed class IntroMotherBrainSpritePresentation
{
    /// <summary>Installed visual compositions indexed by their native bank-$8C frame pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroMotherBrainSpritePresentation), frames);

    /// <summary>Creates the presentation from validated, independently owned frame compositions.</summary>
    /// <param name="frames">Compiled Mother Brain compositions keyed by their original bank-$8C frame identities.</param>
    private IntroMotherBrainSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends one installed Mother Brain composition through the on-screen OAM path used by the opening flashback, without advancing animation.</summary>
    /// <param name="pointer">Native bank-$8C frame identity $8C00, $8C2F, or $8C5E; $8C8D belongs to Rinka, not Mother Brain.</param>
    /// <param name="oam">Destination object-attribute buffer; authored part order is preserved for sprite overlap.</param>
    /// <param name="x">Horizontal composition origin in screen-space pixels.</param>
    /// <param name="y">Vertical composition origin in screen-space pixels.</param>
    /// <param name="paletteBits">Actor's OBJ palette bits, applied only to parts configured to inherit the palette.</param>
    /// <exception cref="InvalidDataException">The requested frame identity is not installed.</exception>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException($"Intro Mother Brain frame $8C:{pointer:X4} is not installed.");
        frame.DrawOnScreen(oam, x, y, paletteBits);
    }

    /// <summary>Validates and compiles the three named intro Mother Brain frames into independently owned visual compositions.</summary>
    /// <param name="json">Readable JSON stream at its current position; it remains open and caller-owned.</param>
    /// <returns>Artwork keyed by the original bank-$8C identities, independent of subsequent edits to authoring arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate properties, has an unsupported version or frame set, or contains invalid OAM visual fields.</exception>
    public static IntroMotherBrainSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroMotherBrainSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroMotherBrainSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Intro Mother Brain sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid intro Mother Brain sprite JSON.", error);
        }
        IReadOnlyList<IntroMotherBrainSpriteFrameDefinition> definitions =
            IntroMotherBrainSpriteDefinitions.Frames;
        if (document.Version != IntroMotherBrainSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro Mother Brain requires three named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroMotherBrainSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Intro Mother Brain frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroMotherBrainParts.CalculateIfMatching(definition.Pointer,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroMotherBrainSpritePresentation(frames);
    }

    /// <summary>Serializes and validates the complete Mother Brain visual document before writing any bytes to the destination.</summary>
    /// <param name="json">Writable destination at its current position; it remains open and caller-owned.</param>
    /// <param name="document">Editable artwork containing the supported version and all three canonical frame names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document does not satisfy the loader's schema and visual-field constraints.</exception>
    public static void Write(Stream json, IntroMotherBrainSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Mutable JSON authoring data for the intro flashback's Mother Brain, distinct from boss gameplay and the separate explosion sprites.</summary>
public sealed record IntroMotherBrainSpriteDocument
{
    /// <summary>Gets the schema revision, which must equal <see cref="IntroMotherBrainSpriteFormat.Version"/> when loaded or written.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the caller-owned ordered parts keyed by mother-brain-frame-0, mother-brain-frame-1, and mother-brain-frame-2.</summary>
    /// <remarks>Stock frames are 48-by-48-pixel compositions of nine 16-pixel OBJs. Editable parts use pixel offsets and tile coordinates in a 16-column by 32-row atlas; null palettes inherit the actor's selector. Loading copies compiled values, not these mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity for the three intro Mother Brain visual frames.</summary>
public static class IntroMotherBrainSpriteFormat
{
    /// <summary>Supported JSON visual-schema revision; does not control the flashback actor's instruction timing or behavior.</summary>
    public const int Version = 1;
    /// <summary>Installation-relative JSON filename selected for the intro Mother Brain's three OAM compositions.</summary>
    public const string FileName = "intro-mother-brain-sprites.json";
}
