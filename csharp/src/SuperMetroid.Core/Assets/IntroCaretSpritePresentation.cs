using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable visible OAM composition of the intro caret, independent of its blink script.</summary>
public sealed class IntroCaretSpritePresentation
{
    /// <summary>Compiled visible composition keyed by its native bank-$8C frame pointer.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroCaretSpritePresentation), frames);

    /// <summary>Retains the validated visible-frame lookup from either supported schema.</summary>
    private IntroCaretSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends the installed visible caret composition to screen-space OAM; a missing frame is rejected, and blink visibility must be handled by the cinematic script rather than by this draw call.</summary>
    /// <param name="pointer">Bank-$8C spritemap identity; the installed visible caret is <c>$8D68</c>, while zero denotes script-controlled invisibility and is not a drawable frame.</param>
    /// <param name="oam">Destination OAM buffer, preserving the authored order of sprite parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">Encoded OBJ palette bits applied only to parts whose palette is inherited rather than explicitly authored.</param>
    public void Draw(ushort pointer, OamBuffer oam, Position16<OamPixels> origin, ushort paletteBits)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException($"Opening caret frame $8C:{pointer:X4} is not installed.");
        frame.DrawOnScreen(oam, origin, paletteBits);
    }

    /// <summary>Loads and validates the visible caret's OAM composition, rejecting duplicate properties, unsupported schemas, missing frame names, and invalid sprite parts; version-one documents retain only their <c>caret-still</c> composition.</summary>
    /// <param name="json">UTF-8 JSON stream containing the current one-frame schema or the supported four-name legacy schema.</param>
    /// <returns>The validated presentation installed under native visible-frame identity <c>$8C:8D68</c>.</returns>
    public static IntroCaretSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
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
        var version = (IntroCaretSpriteVersion)document.Version;
        bool previousVersion = version == IntroCaretSpriteVersion.Previous;
        if (document.Frames is null ||
            (previousVersion
                ? document.Frames.Count != IntroCaretSpriteDefinitions.PreviousFrameNames.Length ||
                    IntroCaretSpriteDefinitions.PreviousFrameNames.Any(name =>
                        !document.Frames.TryGetValue(name, out SpriteVisualPart[]? parts) ||
                        parts is null || parts.Length > IntroCaretSpriteDefinitions.MaximumParts)
                : version != IntroCaretSpriteVersion.Current ||
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
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
