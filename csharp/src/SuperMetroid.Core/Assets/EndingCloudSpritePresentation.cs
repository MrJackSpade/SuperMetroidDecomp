using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the six atmospheric ending clouds.</summary>
public sealed class EndingCloudSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled compositions indexed by the bank-$8C pointers used by the ending actors.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Creates a presentation from compositions already validated against the stock cloud layouts.</summary>
    /// <param name="frames">Compiled frames keyed by their cinematic spritemap pointers.</param>
    private EndingCloudSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingCloudSpritePresentation), frames);

    /// <summary>Draws the ending-cloud composition identified by its bank-$8C spritemap pointer.</summary>
    /// <param name="pointer">Bank-$8C pointer identifying one of the six installed cloud frames.</param>
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
                $"Ending cloud sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads and validates the six named atmospheric cloud compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-cloud sprite document.</param>
    /// <returns>The compiled ending-cloud presentation.</returns>
    public static EndingCloudSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingCloudSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingCloudSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending cloud sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending cloud sprite JSON.", error);
        }
        IReadOnlyList<EndingCloudSpriteFrameDefinition> definitions =
            EndingCloudSpriteDefinitions.Frames;
        if (document.Version != EndingCloudSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Ending clouds require exactly six named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        for (int index = 0; index < definitions.Count; index++)
        {
            EndingCloudSpriteFrameDefinition definition = definitions[index];
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Ending cloud sprite {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroCinematicSpriteCompiler.Compile(visual, definition.Name)
                    .CalculateIfMatching(new EndingCloudGridParts((EndingCloudSpriteDefinitions.Role)index)));
        }
        return new EndingCloudSpritePresentation(frames);
    }

    /// <summary>Validates and writes an ending-cloud sprite document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, EndingCloudSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>JSON schema for the editable atmospheric ending-cloud compositions.</summary>
public sealed record EndingCloudSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="EndingCloudSpriteFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the compositions keyed by the published cloud-frame names.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>
/// Cartridge identities are ordered by the six consecutive bank-$8B instruction
/// lists at $ECED, $ECF5, $ECFD, $ED05, $ED0D, and $ED15.
/// </summary>
public static class EndingCloudSpriteDefinitions
{
    /// <summary>$8C:B6F3, EndingCutsceneBottomCloudsPattern; four16-part records
    /// precede the right and left32-part records.</summary>
    private const ushort FirstRecord = 0xb6f3;
    /// <summary>Number of ordered cloud frames exposed to the editable presentation.</summary>
    private const int FrameCount = 6;
    // Native storage order is distinct from the actor/list order.
    /// <summary>Record order in the cartridge's cloud spritemap data, which differs from actor draw order.</summary>
    private enum Record
    {
        /// <summary>Bottom cloud pattern in the native storage sequence.</summary>
        BottomPattern,
        /// <summary>Top cloud pattern in the native storage sequence.</summary>
        TopPattern,
        /// <summary>Bottom cloud edge in the native storage sequence.</summary>
        BottomEdge,
        /// <summary>Top cloud edge in the native storage sequence.</summary>
        TopEdge,
        /// <summary>Right-side cloud composition, stored as a 32-part record.</summary>
        Right,
        /// <summary>Left-side cloud composition, stored as a 32-part record.</summary>
        Left
    }

    /// <summary>Presentation order expected by the six ending actors and their instruction lists.</summary>
    internal enum Role
    {
        /// <summary>First upper-cloud pattern frame.</summary>
        UpperPattern,
        /// <summary>Second upper-cloud edge frame.</summary>
        UpperEdge,
        /// <summary>First lower-cloud edge frame.</summary>
        LowerEdge,
        /// <summary>Second lower-cloud pattern frame.</summary>
        LowerPattern,
        /// <summary>Right-side cloud frame.</summary>
        Right,
        /// <summary>Left-side cloud frame.</summary>
        Left
    }

    /// <summary>Gets the ordered upper, lower, right, and left cloud roles used by the ending actors.</summary>
    public static IReadOnlyList<EndingCloudSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Maps a presentation-order index to the stable JSON name and cartridge record for that cloud actor.</summary>
    /// <param name="index">Zero-based actor/list order from upper pattern through left-side cloud.</param>
    /// <returns>The frame identity and stock layout assigned to the actor.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the six published roles.</exception>
    private static EndingCloudSpriteFrameDefinition Get(int index) => (Role)index switch
    {
        Role.UpperPattern => Define("scene-b-upper-a", Record.TopPattern),
        Role.UpperEdge => Define("scene-b-upper-b", Record.TopEdge),
        Role.LowerEdge => Define("scene-b-lower-a", Record.BottomEdge),
        Role.LowerPattern => Define("scene-b-lower-b", Record.BottomPattern),
        Role.Right => Define("scene-a-right", Record.Right),
        Role.Left => Define("scene-a-left", Record.Left),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Calculates a frame's bank-$8C pointer and part count from its position in native spritemap storage.</summary>
    /// <param name="name">Stable JSON key exposed to editable assets.</param>
    /// <param name="record">The corresponding native storage record.</param>
    /// <returns>The identity and stock composition size for that record.</returns>
    private static EndingCloudSpriteFrameDefinition Define(string name, Record record)
    {
        int index = (int)record;
        int horizontalRecords = Math.Min(index, 4);
        int sideRecords = Math.Max(index - 4, 0);
        int offset = horizontalRecords * (2 + 16 * 5) + sideRecords * (2 + 32 * 5);
        return new(name, (ushort)(FirstRecord + offset), index < 4 ? 16 : 32);
    }

    /// <summary>Provides indexed and sequential access to the six calculated cloud frame identities.</summary>
    private sealed class FrameView : IReadOnlyList<EndingCloudSpriteFrameDefinition>
    {
        /// <summary>Gets the fixed number of cloud frame identities.</summary>
        public int Count => FrameCount;

        /// <summary>Gets the frame identity assigned to one ending actor position.</summary>
        /// <param name="index">Zero-based index in published actor/list order.</param>
        /// <returns>The corresponding stable frame definition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the published frame range.</exception>
        public EndingCloudSpriteFrameDefinition this[int index] => Get(index);

        /// <summary>Enumerates the six frame identities in the order consumed by the ending actors.</summary>
        /// <returns>An enumerator over the stable frame definitions.</returns>
        public IEnumerator<EndingCloudSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

/// <summary>Identifies one editable ending-cloud composition and its stock cartridge shape.</summary>
/// <param name="Name">Stable JSON key for the frame.</param>
/// <param name="Pointer">Bank-$8C pointer used by the cinematic instruction list.</param>
/// <param name="StockPartCount">Number of OAM parts in the cartridge composition.</param>
public readonly record struct EndingCloudSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

/// <summary>Defines the ending-cloud sprite document contract.</summary>
public static class EndingCloudSpriteFormat
{
    /// <summary>Current ending-cloud sprite schema version.</summary>
    public const int Version = 1;

    /// <summary>Canonical ending-cloud sprite asset file name.</summary>
    public const string FileName = "ending-cloud-sprites.json";
}
