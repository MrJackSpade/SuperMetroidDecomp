using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the 56 completion-message glyph frames.</summary>
public sealed class EndingCompletionTextSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled phrase prefixes, digits, and colon keyed by their bank-$8C spritemap pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Retains the complete validated composition set independently of authoring arrays.</summary>
    private EndingCompletionTextSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingCompletionTextSpritePresentation), frames);

    /// <summary>Appends the selected completion-text composition's parts in authored OAM order, preserving inherited palettes and the native on-screen/off-screen clipping path.</summary>
    /// <param name="pointer">Bank-$8C spritemap identity for a phrase prefix, digit, or colon; an uninstalled identity is rejected.</param>
    /// <param name="objects">Destination OAM buffer for this frame's text parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">Packed OBJ palette bits used only by parts whose editable palette is null.</param>
    /// <param name="originIsOnScreen">Selects ordinary origin clipping when true or the native negative-origin Y-wrap path when false.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        SpriteComposition.DrawInstalled(frames, "Ending completion text", pointer, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Loads version-1 <c>ending-completion-text-sprites.json</c>, requiring all 56 named compositions and validating each ordered part's offsets, tile region, size, palette, and priority.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled editable compositions, using calculated stock letter parts only when every supplied visual field matches.</returns>
    public static EndingCompletionTextSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingCompletionTextSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingCompletionTextSpriteDocument>(
                ArtworkJson.Options) ??
                throw new InvalidDataException("Ending completion text sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending completion text sprite JSON.", error);
        }
        IReadOnlyList<SpriteFrameDefinition> definitions =
            EndingCompletionTextSpriteDefinitions.Frames;
        if (document.Version != EndingCompletionTextSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending completion text requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        for (int index = 0; index < definitions.Count; index++)
        {
            SpriteFrameDefinition definition = definitions[index];
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending completion text frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name)
                    .CalculateIfMatching(new EndingCompletionTextParts(index)));
        }
        return new EndingCompletionTextSpritePresentation(frames);
    }

    /// <summary>Serializes completion-text compositions as UTF-8 JSON and validates the complete named-frame set and OAM fields before writing any bytes.</summary>
    /// <param name="json">Destination written at its current position and left open.</param>
    /// <param name="document">Versioned phrase-prefix, digit, and colon compositions to serialize.</param>
    public static void Write(Stream json, EndingCompletionTextSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
