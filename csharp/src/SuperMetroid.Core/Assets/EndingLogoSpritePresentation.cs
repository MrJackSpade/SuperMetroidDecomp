using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the eight final assembling-logo frames.</summary>
public sealed class EndingLogoSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Decoded OAM compositions keyed by their bank-$8C cinematic pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Creates a presentation from the validated compositions for all ending-logo frames.</summary>
    /// <param name="frames">Frame compositions indexed by the pointers used by the cinematic script.</param>
    private EndingLogoSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingLogoSpritePresentation), frames);

    /// <summary>Draws the ending-logo composition identified by its bank-$8C spritemap pointer.</summary>
    /// <param name="pointer">Bank-$8C pointer identifying one of the eight installed logo frames.</param>
    /// <param name="oam">Object-attribute buffer that receives the composition.</param>
    /// <param name="x">Horizontal origin in screen-space coordinates.</param>
    /// <param name="y">Vertical origin in screen-space coordinates.</param>
    /// <param name="paletteBits">SNES OAM attribute bits to combine with each part.</param>
    /// <param name="originIsOnScreen">Whether the origin is already in the visible coordinate range.</param>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending logo sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads and validates the eight named final-logo compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-logo sprite document.</param>
    /// <returns>The compiled ending-logo presentation.</returns>
    public static EndingLogoSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
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
        ArgumentNullException.ThrowIfNull(json);
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

    /// <summary>Number of published logo frames and the number of OAM parts in each S-letter segment.</summary>
    private const int FrameCount = 8, SParts = 14;

    /// <summary>Assembly stage used to size each circle composition in the native packed OAM records.</summary>
    private enum CircleStage
    {
        /// <summary>The first partial circle image.</summary>
        First,

        /// <summary>The second partial circle image.</summary>
        Second,

        /// <summary>The completed circle image.</summary>
        Complete
    }

    /// <summary>Gets the two letter segments followed by three right-wrap and three left-wrap stages.</summary>
    public static IReadOnlyList<EndingLogoSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Computes the bank-$8C pointer of one frame from the packed record sizes.</summary>
    /// <param name="index">Zero-based index among the eight published frame definitions.</param>
    /// <returns>The bank-local pointer to the frame's native OAM record.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the published frame range.</exception>
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

    /// <summary>Returns the number of OAM parts emitted at a particular circle assembly stage.</summary>
    /// <param name="stage">Circle progression stage being measured.</param>
    /// <returns>The number of packed OAM parts in that stage.</returns>
    private static int CircleParts(CircleStage stage) => stage switch
    {
        CircleStage.First => 12,
        CircleStage.Second => 18,
        CircleStage.Complete => 25,
        _ => throw new ArgumentOutOfRangeException(nameof(stage)),
    };
    /// <summary>Calculates the byte size of a packed OAM record including its part-count word.</summary>
    /// <param name="parts">Number of five-byte OAM parts stored after the count word.</param>
    /// <returns>Total record length in bytes.</returns>
    private static int RecordBytes(int parts) => sizeof(ushort) + parts * 5;

    /// <summary>Builds the named frame metadata at an index in the published ending-logo sequence.</summary>
    /// <param name="index">Zero-based frame index.</param>
    /// <returns>Name, native pointer, and stock OAM part count for that frame.</returns>
    private static EndingLogoSpriteFrameDefinition Get(int index)
    {
        ushort pointer = FramePointer(index);
        if (index < 2) return new(index == 0 ? "s-upper" : "s-lower", pointer, SParts);
        int stage = (index - 2) % 3;
        string side = index < 5 ? "right" : "left";
        string name = "circle-" + side + "-" + (stage + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return new(name, pointer, CircleParts((CircleStage)stage));
    }

    /// <summary>Calculates the eight frame definitions on demand without storing a second catalog.</summary>
    private sealed class FrameView : IReadOnlyList<EndingLogoSpriteFrameDefinition>
    {
        /// <summary>Gets the fixed number of frames in the ending-logo assembly sequence.</summary>
        public int Count => FrameCount;

        /// <summary>Gets the definition at its position in the cinematic assembly order.</summary>
        /// <param name="index">Zero-based frame index.</param>
        /// <returns>The corresponding frame name, pointer, and stock part count.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the published frame range.</exception>
        public EndingLogoSpriteFrameDefinition this[int index] => Get(index);

        /// <summary>Enumerates the frame definitions in the same order used by the cinematic script.</summary>
        /// <returns>An enumerator over the eight computed frame definitions.</returns>
        public IEnumerator<EndingLogoSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
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
