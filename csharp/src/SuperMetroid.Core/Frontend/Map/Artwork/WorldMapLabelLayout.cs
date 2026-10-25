using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Cosmetic world-map label anchors. Area availability and navigation are not content.</summary>
public sealed class WorldMapLabelLayout
{
    /// <summary>Drawing anchor for Crateria's world-map name.</summary>
    private readonly Position16<SpritePixels> crateria;
    /// <summary>Drawing anchor for Brinstar's world-map name.</summary>
    private readonly Position16<SpritePixels> brinstar;
    /// <summary>Drawing anchor for Norfair's world-map name.</summary>
    private readonly Position16<SpritePixels> norfair;
    /// <summary>Drawing anchor for the Wrecked Ship's world-map name.</summary>
    private readonly Position16<SpritePixels> wreckedShip;
    /// <summary>Drawing anchor for Maridia's world-map name.</summary>
    private readonly Position16<SpritePixels> maridia;
    /// <summary>Drawing anchor for Tourian's world-map name.</summary>
    private readonly Position16<SpritePixels> tourian;

    /// <summary>Retains six independently supplied label positions without deriving one area from another.</summary>
    private WorldMapLabelLayout(Position16<SpritePixels> crateria, Position16<SpritePixels> brinstar, Position16<SpritePixels> norfair,
        Position16<SpritePixels> wreckedShip, Position16<SpritePixels> maridia, Position16<SpritePixels> tourian)
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
    public Position16<SpritePixels> Get(int area) => ((uint)area < AreaIds.Zebes.Count ? AreaIds.Zebes[area] : AreaId.Ceres) switch
    {
        AreaId.Crateria => crateria,
        AreaId.Brinstar => brinstar,
        AreaId.Norfair => norfair,
        AreaId.WreckedShip => wreckedShip,
        AreaId.Maridia => maridia,
        AreaId.Tourian => tourian,
        AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(area)),
        _ => throw new ArgumentOutOfRangeException(nameof(area)),
    };

    /// <summary>Loads and validates label anchors for exactly the six Zebes world-map areas.</summary>
    /// <param name="json">Caller-owned stream containing a world-map label document.</param>
    /// <returns>The compiled cosmetic label layout.</returns>
    public static WorldMapLabelLayout Load(Stream json)
    {
        WorldMapLabelDocument document;
        try { document = JsonAssetDocument.Read<WorldMapLabelDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("World-map label layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid world-map label layout JSON.", error); }
        if (document.Version != WorldMapLabelFormat.Version || document.Areas is null || document.Areas.Count != 6)
            throw new InvalidDataException("World-map labels require version 1 and exactly the six Zebes areas.");
        Position16<SpritePixels> RequireArea(AreaId area)
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
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
