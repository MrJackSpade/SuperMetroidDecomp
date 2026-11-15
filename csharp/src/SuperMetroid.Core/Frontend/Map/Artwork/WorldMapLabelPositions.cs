using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Cosmetic world-map label anchors. Area availability and navigation are not content.</summary>
public sealed class WorldMapLabelPositions
{
    /// <summary>Drawing anchor for Crateria's world-map name.</summary>
    private readonly Offset16<ScreenPixels> crateria;
    /// <summary>Drawing anchor for Brinstar's world-map name.</summary>
    private readonly Offset16<ScreenPixels> brinstar;
    /// <summary>Drawing anchor for Norfair's world-map name.</summary>
    private readonly Offset16<ScreenPixels> norfair;
    /// <summary>Drawing anchor for the Wrecked Ship's world-map name.</summary>
    private readonly Offset16<ScreenPixels> wreckedShip;
    /// <summary>Drawing anchor for Maridia's world-map name.</summary>
    private readonly Offset16<ScreenPixels> maridia;
    /// <summary>Drawing anchor for Tourian's world-map name.</summary>
    private readonly Offset16<ScreenPixels> tourian;

    /// <summary>Retains six independently supplied label positions without deriving one area from another.</summary>
    internal WorldMapLabelPositions(Offset16<ScreenPixels> crateria, Offset16<ScreenPixels> brinstar, Offset16<ScreenPixels> norfair,
        Offset16<ScreenPixels> wreckedShip, Offset16<ScreenPixels> maridia, Offset16<ScreenPixels> tourian)
    {
        this.crateria = crateria;
        this.brinstar = brinstar;
        this.norfair = norfair;
        this.wreckedShip = wreckedShip;
        this.maridia = maridia;
        this.tourian = tourian;
    }

    /// <summary>Selects the independently editable anchor belonging to a named Zebes area.</summary>
    /// <remarks>The six area identities select layout roles,
    /// not numerical samples. Preserve each imported/custom point without deriving it
    /// from another area or storing a positional lookup. Only the six Zebes areas have labels.</remarks>
    public Offset16<ScreenPixels> Get(AreaId area) => area switch
    {
        AreaId.Crateria => crateria,
        AreaId.Brinstar => brinstar,
        AreaId.Norfair => norfair,
        AreaId.WreckedShip => wreckedShip,
        AreaId.Maridia => maridia,
        AreaId.Tourian => tourian,
        AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(area), area, "Ceres has no world-map label."),
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Undefined area."),
    };

}
