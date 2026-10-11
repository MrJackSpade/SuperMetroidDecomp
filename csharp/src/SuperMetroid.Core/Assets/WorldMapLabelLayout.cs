using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Cosmetic world-map label anchors. Area availability and navigation are not content.</summary>
public sealed class WorldMapLabelLayout
{
    private readonly Point32<ScreenPixels> crateria;
    private readonly Point32<ScreenPixels> brinstar;
    private readonly Point32<ScreenPixels> norfair;
    private readonly Point32<ScreenPixels> wreckedShip;
    private readonly Point32<ScreenPixels> maridia;
    private readonly Point32<ScreenPixels> tourian;

    private WorldMapLabelLayout(Point32<ScreenPixels> crateria, Point32<ScreenPixels> brinstar, Point32<ScreenPixels> norfair,
        Point32<ScreenPixels> wreckedShip, Point32<ScreenPixels> maridia, Point32<ScreenPixels> tourian)
    {
        this.crateria = crateria;
        this.brinstar = brinstar;
        this.norfair = norfair;
        this.wreckedShip = wreckedShip;
        this.maridia = maridia;
        this.tourian = tourian;
    }

    /// <summary>Selects the independently editable anchor belonging to a named Zebes area.</summary>
    /// <remarks>Reviewed for #1165: the six native area identities select layout roles,
    /// not numerical samples. Preserve each imported/custom point without deriving it
    /// from another area or storing a positional lookup. Only area IDs0..5 are valid.</remarks>
    public Point32<ScreenPixels> Get(int area) => area switch
    {
        (int)AreaId.Crateria => crateria,
        (int)AreaId.Brinstar => brinstar,
        (int)AreaId.Norfair => norfair,
        (int)AreaId.WreckedShip => wreckedShip,
        (int)AreaId.Maridia => maridia,
        (int)AreaId.Tourian => tourian,
        _ => throw new ArgumentOutOfRangeException(nameof(area)),
    };

    /// <summary>Loads and validates label anchors for exactly the six Zebes world-map areas.</summary>
    /// <param name="json">Caller-owned stream containing a world-map label document.</param>
    /// <returns>The compiled cosmetic label layout.</returns>
    public static WorldMapLabelLayout Load(Stream json)
    {
        WorldMapLabelDocument document;
        try { document = JsonAssetDocument.Read<WorldMapLabelDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("World-map label layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid world-map label layout JSON.", error); }
        if (document.Version != WorldMapLabelFormat.Version || document.Areas is null || document.Areas.Count != 6)
            throw new InvalidDataException("World-map labels require version 1 and exactly the six Zebes areas.");
        Point32<ScreenPixels> RequireArea(AreaId area)
        {
            string name = area.ToString();
            if (!document.Areas.TryGetValue(name, out var point) ||
                point.X is < 1 or > 255 || point.Y is < 1 or > 223)
                throw new InvalidDataException($"World-map label {name} requires X=1..255 and Y=1..223.");
            return point;
        }
        return new(RequireArea(AreaId.Crateria), RequireArea(AreaId.Brinstar), RequireArea(AreaId.Norfair),
            RequireArea(AreaId.WreckedShip), RequireArea(AreaId.Maridia), RequireArea(AreaId.Tourian));
    }

    /// <summary>Validates and writes a world-map label document as JSON.</summary>
    /// <param name="json">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream json, WorldMapLabelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>JSON schema for the six named Zebes world-map label anchors.</summary>
public sealed record WorldMapLabelDocument
{
    /// <summary>Gets the schema version required by <see cref="WorldMapLabelFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the label anchors keyed by the six case-sensitive area names.</summary>
    public required Dictionary<string, Point32<ScreenPixels>> Areas { get; init; }
}

/// <summary>Defines the installed world-map label resource contract.</summary>
public static class WorldMapLabelFormat
{
    /// <summary>Supported world-map label schema version.</summary>
    public const int Version = 1;

    /// <summary>Canonical world-map label asset file name.</summary>
    public const string FileName = "world-map-labels.json";
}
