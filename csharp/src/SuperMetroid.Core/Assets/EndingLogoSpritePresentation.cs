using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the eight final assembling-logo frames.</summary>
public sealed class EndingLogoSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled compositions indexed by the spritemap pointers used by the ending script.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Retains the validated frame set after related logo parts have been calculated.</summary>
    private EndingLogoSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingLogoSpritePresentation), frames);

    /// <summary>Draws the ending-logo composition identified by its bank-$8C spritemap pointer.</summary>
    /// <param name="pointer">Bank-$8C pointer identifying one of the eight installed logo frames.</param>
    /// <param name="oam">Object-attribute buffer that receives the composition.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">SNES OAM attribute bits to combine with each part.</param>
    /// <param name="originIsOnScreen">Whether the origin is already in the visible coordinate range.</param>
    public void Draw(ushort pointer, OamBuffer oam, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending logo sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, origin, paletteBits);
        else
            frame.DrawOffScreen(oam, origin, paletteBits);
    }

    /// <summary>Loads and validates the eight named final-logo compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-logo sprite document.</param>
    /// <returns>The compiled ending-logo presentation.</returns>
    public static EndingLogoSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingLogoSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingLogoSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending logo sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending logo sprite JSON.", error);
        }
        IReadOnlyList<EndingLogoSpriteFrameDefinition> definitions =
            EndingLogoSpriteDefinitions.Frames;
        if (document.Version != EndingLogoSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending logo requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingLogoSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending logo frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name));
        }
        SpriteComposition upper = frames[definitions[0].Pointer].CalculateIfMatching(new EndingLogoUpperParts());
        frames[definitions[0].Pointer] = upper;
        SpriteComposition completeRight = EndingLogoWrapParts.CalculateIfMatching(frames[definitions[4].Pointer]);
        frames[definitions[4].Pointer] = completeRight;
        for (int index = 0; index < definitions.Count; index++)
        {
            ushort pointer = definitions[index].Pointer;
            frames[pointer] = EndingLogoRelatedParts.CalculateIfMatching(index, frames[pointer], upper, completeRight);
        }
        return new EndingLogoSpritePresentation(frames);
    }

    /// <summary>Validates and writes an ending-logo sprite document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, EndingLogoSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>JSON schema for the editable final-logo sprite compositions.</summary>
public sealed record EndingLogoSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="EndingLogoSpriteFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the compositions keyed by the published frame names.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Cartridge OAM identities paired with the four $8B:EE5D..EE9A logo lists.</summary>
public static class EndingLogoSpriteDefinitions
{
    /// <summary>$8C:B97F, ScrewAttackSymbolUpperPart, followed by the lower S,
    /// three right-wrap and three left-wrap compositions.</summary>
    private const ushort FirstFrame = 0xb97f;
    /// <summary>Two letter sections and three stages for each side of the surrounding circle.</summary>
    private const int FrameCount = 8;
    /// <summary>The upper and lower S records each carry fourteen OAM parts.</summary>
    private const int SParts = 14;
    /// <summary>The right and left circle groups advance through matching partial and complete shapes.</summary>
    private enum CircleStage
    {
        /// <summary>First twelve-part section of one side of the circle.</summary>
        First,
        /// <summary>Second eighteen-part section of one side of the circle.</summary>
        Second,
        /// <summary>Complete 25-part section of one side of the circle.</summary>
        Complete,
    }

    /// <summary>Gets the two letter segments followed by three right-wrap and three left-wrap stages.</summary>
    public static IReadOnlyList<EndingLogoSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Walks the variable-size OAM records from $8C:B97F to locate one ending-logo frame.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 2) return (ushort)(FirstFrame + index * RecordBytes(SParts));
        int circle = (index - 2) / 3;
        int stage = (index - 2) % 3;
        int offset = 2 * RecordBytes(SParts);
        for (int preceding = 0; preceding < 3; preceding++)
            offset += circle * RecordBytes(CircleParts((CircleStage)preceding));
        for (int preceding = 0; preceding < stage; preceding++)
            offset += RecordBytes(CircleParts((CircleStage)preceding));
        return (ushort)(FirstFrame + offset);
    }

    /// <summary>Uses the cartridge's twelve, eighteen, and 25 part counts for each circle stage.</summary>
    private static int CircleParts(CircleStage stage) => stage switch
    {
        CircleStage.First => 12,
        CircleStage.Second => 18,
        CircleStage.Complete => 25,
        _ => throw new ArgumentOutOfRangeException(nameof(stage)),
    };
    /// <summary>Accounts for the part count word and five encoded bytes per OAM part.</summary>
    private static int RecordBytes(int parts) => sizeof(ushort) + parts * 5;

    /// <summary>Pairs the calculated cartridge pointer with the JSON name and expected part count.</summary>
    private static EndingLogoSpriteFrameDefinition Get(int index)
    {
        ushort pointer = FramePointer(index);
        if (index < 2) return new(index == 0 ? "s-upper" : "s-lower", pointer, SParts);
        int stage = (index - 2) % 3;
        string side = index < 5 ? "right" : "left";
        string name = "circle-" + side + "-" + (stage + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return new(name, pointer, CircleParts((CircleStage)stage));
    }

    /// <summary>Exposes the eight calculated frame identities without a stored pointer table.</summary>
    private sealed class FrameView : IReadOnlyList<EndingLogoSpriteFrameDefinition>
    {
        /// <summary>The script uses two S sections and six circle stages.</summary>
        public int Count => FrameCount;
        /// <summary>Resolves the selected stage to its cartridge pointer and JSON key.</summary>
        public EndingLogoSpriteFrameDefinition this[int index] => Get(index);
        /// <summary>Visits all frame definitions in cinematic instruction order.</summary>
        public IEnumerator<EndingLogoSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Shares the typed frame order with nongeneric collection callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

/// <summary>Identifies one editable final-logo composition and its stock cartridge shape.</summary>
/// <param name="Name">Stable JSON key for the frame.</param>
/// <param name="Pointer">Bank-$8C pointer used by the cinematic script.</param>
/// <param name="StockPartCount">Number of OAM parts in the cartridge composition.</param>
public readonly record struct EndingLogoSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

/// <summary>Defines the ending-logo sprite document contract.</summary>
public static class EndingLogoSpriteFormat
{
    /// <summary>Current ending-logo sprite schema version.</summary>
    public const int Version = 1;

    /// <summary>Canonical ending-logo sprite asset file name.</summary>
    public const string FileName = "ending-logo-sprites.json";
}
