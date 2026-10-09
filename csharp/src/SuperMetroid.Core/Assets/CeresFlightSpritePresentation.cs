using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable visual compositions for the six star, station, asteroid, and vortex
/// frames shared by the Ceres approach and destruction scenes. Animation timing
/// and actor motion remain in their compiled owners.
/// </summary>
public sealed class CeresFlightSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled visual compositions keyed by their native bank-$8C spritemap pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(CeresFlightSpritePresentation), frames);

    /// <summary>Creates a presentation from the frame compositions validated and compiled by <see cref="Load"/>.</summary>
    /// <param name="frames">Compiled compositions indexed by each frame's native bank-$8C pointer.</param>
    private CeresFlightSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends an installed Ceres flight composition to OAM in authored part order, choosing the caller's native origin-clipping path without advancing actor motion or animation.</summary>
    /// <param name="pointer">Bank-$8C spritemap identity for one of the six installed star, station, asteroid, or vortex drawings.</param>
    /// <param name="oam">Destination OAM buffer for the ordered sprite parts.</param>
    /// <param name="x">Composition origin's screen X coordinate in native wrapped pixels.</param>
    /// <param name="y">Composition origin's screen Y coordinate in native wrapped pixels.</param>
    /// <param name="paletteBits">Encoded OBJ palette bits applied only to parts authored to inherit the drawing owner's palette.</param>
    /// <param name="originIsOnScreen">True for ordinary on-screen origin clipping; false for the native off-screen path with opposite Y-wrap clipping for negative origins.</param>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ceres flight sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads exactly the six named visual compositions, rejecting duplicate properties, unsupported versions, missing frames, and invalid OAM parts; compiles selected artwork without importing actor timing or motion.</summary>
    /// <param name="json">Caller-owned UTF-8 JSON stream containing the versioned Ceres flight sprite document.</param>
    /// <returns>The immutable bank-$8C-keyed presentation shared by approach and destruction scenes.</returns>
    public static CeresFlightSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresFlightSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CeresFlightSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ceres flight sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres flight sprite JSON.", error);
        }
        IReadOnlyList<CeresFlightSpriteFrameDefinition> definitions =
            CeresFlightSpriteDefinitions.Frames;
        if (document.Version != CeresFlightSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Ceres flight requires exactly six named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (CeresFlightSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Ceres flight sprite {definition.Name} is missing.");
            var compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = CeresStarPointParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresStationParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresSmallAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresVortexParts.CalculateIfMatching(definition.Pointer, compiled,
                frames.GetValueOrDefault(CeresFlightSpriteDefinitions.Stars));
            frames.Add(definition.Pointer, compiled);
        }
        return new CeresFlightSpritePresentation(frames);
    }

    /// <summary>Serializes a sprite document using the presentation JSON options and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream for the validated UTF-8 JSON.</param>
    /// <param name="document">Document containing all six required named visual frames.</param>
    public static void Write(Stream json, CeresFlightSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable JSON schema for Ceres approach/destruction OAM compositions; scene scripts retain actor placement, motion, and frame-selection timing.</summary>
public sealed record CeresFlightSpriteDocument
{
    /// <summary>Schema revision, which must equal <see cref="CeresFlightSpriteFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly six ordered-part compositions named <c>stars</c>, <c>large-asteroid</c>, <c>station-under-attack</c>, <c>small-asteroid</c>, <c>vortex-even</c>, and <c>vortex-odd</c>, each bounded by the 128-part OAM limit.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>One bank-$8C visual spritemap and its expected retail OAM entry count.</summary>
/// <param name="Name">Stable asset name identifying the visual composition.</param>
/// <param name="Pointer">Native 16-bit spritemap pointer in bank $8C, used as the runtime frame identity.</param>
/// <param name="StockPartCount">Retail composition's OAM part count for source comparison, not a required count for independently edited artwork.</param>
public readonly record struct CeresFlightSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

/// <summary>Installed visual-composition file for the approach-to-Ceres actor set.</summary>
public static class CeresFlightSpriteFormat
{
    /// <summary>Supported revision of the six-composition Ceres flight sprite JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Asset filename for the editable OAM compositions shared by the approach and destruction scenes.</summary>
    public const string FileName = "ceres-flight-sprites.json";
}
