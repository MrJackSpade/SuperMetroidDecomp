using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the six atmospheric ending clouds.</summary>
public sealed class EndingCloudSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled cloud compositions keyed by their bank-$8C spritemap pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Accepts the complete validated six-frame cloud set.</summary>
    private EndingCloudSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingCloudSpritePresentation), frames);

    /// <summary>Draws the ending-cloud composition identified by its bank-$8C spritemap pointer.</summary>
    /// <param name="pointer">Bank-$8C pointer identifying one of the six installed cloud frames.</param>
    /// <param name="oam">Object-attribute buffer that receives the composition.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">SNES OAM attribute bits to combine with each part.</param>
    /// <param name="originIsOnScreen">Whether the origin is already in the visible coordinate range.</param>
    public void Draw(ushort pointer, OamBuffer oam, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending cloud sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, origin, paletteBits);
        else
            frame.DrawOffScreen(oam, origin, paletteBits);
    }

    /// <summary>Loads and validates the six named atmospheric cloud compositions from JSON.</summary>
    /// <param name="json">Stream containing an ending-cloud sprite document.</param>
    /// <returns>The compiled ending-cloud presentation.</returns>
    public static EndingCloudSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
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
        Ensure.NotNull(json);
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
    /// <summary>Four horizontal cloud records plus two side-cloud records.</summary>
    private const int FrameCount = 6;
    /// <summary>Native storage order, which differs from cinematic actor/list order.</summary>
    private enum Record
    {
        /// <summary>First horizontal 16-part native record; selected for the actor's lower pattern.</summary>
        BottomPattern,
        /// <summary>Second horizontal 16-part native record; selected for the actor's upper pattern.</summary>
        TopPattern,
        /// <summary>Third horizontal 16-part native record; selected for the actor's lower edge.</summary>
        BottomEdge,
        /// <summary>Fourth horizontal 16-part native record; selected for the actor's upper edge.</summary>
        TopEdge,
        /// <summary>First 32-part side-cloud record in the cartridge's storage order.</summary>
        Right,
        /// <summary>Second 32-part side-cloud record in the cartridge's storage order.</summary>
        Left,
    }
    /// <summary>Cloud roles in the order used by the ending actors.</summary>
    internal enum Role
    {
        /// <summary>Upper cloud pattern requested first by the ending actor lists.</summary>
        UpperPattern,
        /// <summary>Upper cloud edge paired with the first pattern.</summary>
        UpperEdge,
        /// <summary>Lower cloud edge drawn before its lower pattern.</summary>
        LowerEdge,
        /// <summary>Lower cloud pattern selected after the lower edge.</summary>
        LowerPattern,
        /// <summary>Right-hand side cloud, drawn from the first 32-part native record.</summary>
        Right,
        /// <summary>Left-hand side cloud, drawn from the second 32-part native record.</summary>
        Left,
    }

    /// <summary>Gets the ordered upper, lower, right, and left cloud roles used by the ending actors.</summary>
    public static IReadOnlyList<EndingCloudSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Maps an actor-list role to the corresponding native cloud record.</summary>
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

    /// <summary>Computes a cloud record pointer from preceding 16- and 32-part native records.</summary>
    private static EndingCloudSpriteFrameDefinition Define(string name, Record record)
    {
        int index = (int)record;
        int horizontalRecords = Math.Min(index, 4);
        int sideRecords = Math.Max(index - 4, 0);
        int offset = horizontalRecords * (2 + 16 * 5) + sideRecords * (2 + 32 * 5);
        return new(name, (ushort)(FirstRecord + offset), index < 4 ? 16 : 32);
    }

    /// <summary>Calculated view of the six cloud records in actor order.</summary>
    private sealed class FrameView : IReadOnlyList<EndingCloudSpriteFrameDefinition>
    {
        /// <summary>Four horizontal and two side-cloud actor roles.</summary>
        public int Count => FrameCount;
        /// <summary>Selects one cloud identity in cinematic actor order.</summary>
        public EndingCloudSpriteFrameDefinition this[int index] => Get(index);
        /// <summary>Enumerates all calculated cloud identities without caching an array.</summary>
        public IEnumerator<EndingCloudSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Uses the typed cloud enumeration for nongeneric callers.</summary>
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
