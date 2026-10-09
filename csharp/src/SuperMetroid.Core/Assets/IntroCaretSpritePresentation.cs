using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable visible OAM composition of the intro caret, independent of its blink script.</summary>
public sealed class IntroCaretSpritePresentation
{
    /// <summary>Compiled visible compositions keyed by their native spritemap pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroCaretSpritePresentation), frames);

    /// <summary>Creates a presentation from the already validated native-frame lookup.</summary>
    /// <param name="frames">Compositions indexed by the native pointer used by the cinematic draw script.</param>
    private IntroCaretSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends the installed visible caret composition to screen-space OAM; a missing frame is rejected, and blink visibility must be handled by the cinematic script rather than by this draw call.</summary>
    /// <param name="pointer">Bank-$8C spritemap identity; the installed visible caret is <c>$8D68</c>, while zero denotes script-controlled invisibility and is not a drawable frame.</param>
    /// <param name="oam">Destination OAM buffer, preserving the authored order of sprite parts.</param>
    /// <param name="x">Caret anchor's screen X coordinate in pixels, using native word wrapping.</param>
    /// <param name="y">Caret anchor's screen Y coordinate in pixels, using native word wrapping.</param>
    /// <param name="paletteBits">Encoded OBJ palette bits applied only to parts whose palette is inherited rather than explicitly authored.</param>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException($"Opening caret frame $8C:{pointer:X4} is not installed.");
        frame.DrawOnScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads and validates the visible caret's OAM composition, rejecting duplicate properties, unsupported schemas, missing frame names, and invalid sprite parts; version-one documents retain only their <c>caret-still</c> composition.</summary>
    /// <param name="json">UTF-8 JSON stream containing the current one-frame schema or the supported four-name legacy schema.</param>
    /// <returns>The validated presentation installed under native visible-frame identity <c>$8C:8D68</c>.</returns>
    public static IntroCaretSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroCaretSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroCaretSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Opening caret composition JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid opening caret composition JSON.", error);
        }
        var definitions = IntroCaretSpriteDefinitions.Frames;
        bool previousVersion = document.Version == IntroCaretSpriteFormat.PreviousVersion;
        if (document.Frames is null ||
            (previousVersion
                ? document.Frames.Count != IntroCaretSpriteDefinitions.PreviousFrameNames.Length ||
                    IntroCaretSpriteDefinitions.PreviousFrameNames.Any(name =>
                        !document.Frames.TryGetValue(name, out SpriteVisualPart[]? parts) ||
                        parts is null || parts.Length > IntroCaretSpriteDefinitions.MaximumParts)
                : document.Version != IntroCaretSpriteFormat.Version ||
                    document.Frames.Count != definitions.Count))
            throw new InvalidDataException("Opening caret requires its one visible sprite frame.");

        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroCaretFrameDefinition definition in definitions)
        {
            string sourceName = previousVersion
                ? IntroCaretSpriteDefinitions.PreviousFrameNames[0] : definition.Name;
            if (!document.Frames.TryGetValue(sourceName, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > IntroCaretSpriteDefinitions.MaximumParts)
                throw new InvalidDataException($"Opening caret frame {definition.Name} is missing or too large.");
            frames.Add(definition.Pointer,
                IntroScientistParts.CalculateIfMatching(definition.Pointer,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroCaretSpritePresentation(frames);
    }

    /// <summary>Serializes a caret document using the presentation JSON options and validates it through <see cref="Load"/> before writing any bytes to the destination stream.</summary>
    /// <param name="json">Destination stream for the validated UTF-8 JSON.</param>
    /// <param name="document">Current or supported legacy document to serialize.</param>
    public static void Write(Stream json, IntroCaretSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable JSON schema for ordered visible-caret sprite parts; positioning and blink timing remain cinematic-script behavior, not authored frame sequences.</summary>
public sealed record IntroCaretSpriteDocument
{
    /// <summary>Schema revision: current <see cref="IntroCaretSpriteFormat.Version"/> or the supported <see cref="IntroCaretSpriteFormat.PreviousVersion"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Current schema requires <c>caret-visible</c> with at most 128 ordered OAM parts; legacy schema requires <c>caret-still</c> and three <c>caret-blink-1..3</c> names, but only the still composition is installed.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity and schema version for opening caret compositions.</summary>
public static class IntroCaretSpriteFormat
{
    /// <summary>Current schema revision, containing only the visible <c>caret-visible</c> composition; the native script supplies the hidden blink phase.</summary>
    public const int Version = 2;
    /// <summary>Supported legacy revision with four mistakenly separate frame names; loading uses <c>caret-still</c> and discards the three blink compositions.</summary>
    public const int PreviousVersion = 1;
    /// <summary>Asset filename for the opening cinematic's editable caret sprite composition.</summary>
    public const string FileName = "intro-caret-sprites.json";
}
