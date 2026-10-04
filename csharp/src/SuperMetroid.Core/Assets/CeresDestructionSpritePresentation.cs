using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions unique to the station blast and Zebes reveal.</summary>
public sealed class CeresDestructionSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(CeresDestructionSpritePresentation), frames);

    private CeresDestructionSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    public bool Contains(ushort pointer) => frames.ContainsKey(pointer);

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ceres destruction sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    public static CeresDestructionSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresDestructionSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CeresDestructionSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ceres destruction sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres destruction sprite JSON.", error);
        }
        IReadOnlyList<CeresDestructionSpriteFrameDefinition> definitions =
            CeresDestructionSpriteDefinitions.Frames;
        if (document.Version != CeresDestructionSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ceres destruction requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (CeresDestructionSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ceres destruction sprite {definition.Name} is missing.");
            var compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = IntroMotherBrainExplosionParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresStationBlastParts.CalculateIfMatching(definition.Pointer, compiled);
            frames.Add(definition.Pointer, compiled);
        }
        return new CeresDestructionSpritePresentation(frames);
    }

    public static void Write(Stream json, CeresDestructionSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Routes the scene's shared approach frames and unique destruction frames.</summary>
internal sealed class CeresSceneSpritePresentation(
    CeresFlightSpritePresentation flight,
    CeresDestructionSpritePresentation destruction) : IIntroCinematicSpritePresentation
{
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        IIntroCinematicSpritePresentation owner = destruction.Contains(pointer)
            ? destruction : flight;
        owner.Draw(pointer, oam, x, y, paletteBits, originIsOnScreen);
    }
}

public sealed record CeresDestructionSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

public readonly record struct CeresDestructionSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class CeresDestructionSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ceres-destruction-sprites.json";
}
