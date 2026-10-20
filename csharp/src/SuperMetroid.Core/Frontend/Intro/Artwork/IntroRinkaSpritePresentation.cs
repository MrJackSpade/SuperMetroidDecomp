using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable intro Rinka OAM art, without movement or hit behavior.</summary>
public sealed class IntroRinkaSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Three compiled OAM compositions keyed by their bank-$8C frame pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroRinkaSpritePresentation), frames);

    /// <summary>Retains the validated frame lookup produced by the JSON compiler.</summary>
    private IntroRinkaSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends an installed intro Rinka composition to OAM without changing its actor's movement, hit behavior, or animation timing.</summary>
    /// <param name="pointer">Native bank-$8C frame identity $8C8D, $8CA3, or $8CB9.</param>
    /// <param name="objects">Destination object-attribute buffer receiving the authored parts in their original order.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">Actor's OBJ palette bits, used only for parts configured to inherit a palette.</param>
    /// <param name="originIsOnScreen">Whether to use ordinary origin clipping; false selects the cinematic negative-origin Y-wrap clipping path.</param>
    /// <exception cref="InvalidDataException">The requested frame identity is not installed.</exception>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        SpriteComposition.DrawInstalled(frames, "Intro Rinka", pointer, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Validates and compiles all three named intro Rinka frames into independently owned visual compositions.</summary>
    /// <param name="json">Readable JSON stream at its current position; it remains open and caller-owned.</param>
    /// <returns>Artwork keyed by the original bank-$8C frame identities, unaffected by subsequent edits to authoring arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate properties, has an unsupported version or frame set, or contains invalid OAM visual fields.</exception>
    public static IntroRinkaSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroRinkaSpriteDocument document;
        document = JsonAssetDocument.Read<IntroRinkaSpriteDocument>(json, ArtworkJson.Options, "intro Rinka sprite");
        IReadOnlyList<SpritemapFrameDefinition> definitions = IntroRinkaSpriteDefinitions.Frames;
        if (document.Version != IntroRinkaSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Intro Rinkas require three named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (SpritemapFrameDefinition definition in definitions)
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
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
