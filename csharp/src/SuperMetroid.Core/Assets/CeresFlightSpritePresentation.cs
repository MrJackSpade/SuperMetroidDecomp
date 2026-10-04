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
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(CeresFlightSpritePresentation), frames);

    private CeresFlightSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

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
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresStationParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresSmallAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            frames.Add(definition.Pointer, compiled);
        }
        return new CeresFlightSpritePresentation(frames);
    }

    public static void Write(Stream json, CeresFlightSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record CeresFlightSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>One bank-$8C visual spritemap and its expected retail OAM entry count.</summary>
public readonly record struct CeresFlightSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

/// <summary>Installed visual-composition file for the approach-to-Ceres actor set.</summary>
public static class CeresFlightSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ceres-flight-sprites.json";
}
