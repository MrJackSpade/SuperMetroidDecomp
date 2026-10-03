using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Cosmetic world-map label anchors. Area availability and navigation are not content.</summary>
public sealed class WorldMapLabelLayout
{
    private readonly MapLabelPoint crateria;
    private readonly MapLabelPoint brinstar;
    private readonly MapLabelPoint norfair;
    private readonly MapLabelPoint wreckedShip;
    private readonly MapLabelPoint maridia;
    private readonly MapLabelPoint tourian;

    private WorldMapLabelLayout(MapLabelPoint crateria, MapLabelPoint brinstar, MapLabelPoint norfair,
        MapLabelPoint wreckedShip, MapLabelPoint maridia, MapLabelPoint tourian)
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
    public MapLabelPoint Get(int area) => area switch
    {
        (int)AreaId.Crateria => crateria,
        (int)AreaId.Brinstar => brinstar,
        (int)AreaId.Norfair => norfair,
        (int)AreaId.WreckedShip => wreckedShip,
        (int)AreaId.Maridia => maridia,
        (int)AreaId.Tourian => tourian,
        _ => throw new ArgumentOutOfRangeException(nameof(area)),
    };

    public static WorldMapLabelLayout Load(Stream json)
    {
        WorldMapLabelDocument document;
        try { document = JsonAssetDocument.Read<WorldMapLabelDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("World-map label layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid world-map label layout JSON.", error); }
        if (document.Version != WorldMapLabelFormat.Version || document.Areas is null || document.Areas.Count != 6)
            throw new InvalidDataException("World-map labels require version 1 and exactly the six Zebes areas.");
        MapLabelPoint RequireArea(AreaId area)
        {
            string name = area.ToString();
            if (!document.Areas.TryGetValue(name, out var point) || point is null ||
                point.X is < 1 or > 255 || point.Y is < 1 or > 223)
                throw new InvalidDataException($"World-map label {name} requires X=1..255 and Y=1..223.");
            return point;
        }
        return new(RequireArea(AreaId.Crateria), RequireArea(AreaId.Brinstar), RequireArea(AreaId.Norfair),
            RequireArea(AreaId.WreckedShip), RequireArea(AreaId.Maridia), RequireArea(AreaId.Tourian));
    }

    public static void Write(Stream json, WorldMapLabelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record MapLabelPoint(int X, int Y);
public sealed record WorldMapLabelDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, MapLabelPoint> Areas { get; init; }
}
public static class WorldMapLabelFormat
{
    public const int Version = 1;
    public const string FileName = "world-map-labels.json";
}
