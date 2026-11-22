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

    /// <summary>The installed place of a Zebes area's label.</summary>
    /// <remarks>Each of the six Zebes areas' labels is drawn at its own installed place; only they have
    /// labels.</remarks>
    public Offset16<ScreenPixels> Get(ZebesArea area) => Labels[area];

    /// <summary>Where each of the six Zebes areas' labels lies; Ceres has none.</summary>
    private IdentityTable<ZebesArea, Offset16<ScreenPixels>> Labels => field ??= new(new Dictionary<ZebesArea, Offset16<ScreenPixels>>
    {
        [ZebesArea.Crateria] = crateria,
        [ZebesArea.Brinstar] = brinstar,
        [ZebesArea.Norfair] = norfair,
        [ZebesArea.WreckedShip] = wreckedShip,
        [ZebesArea.Maridia] = maridia,
        [ZebesArea.Tourian] = tourian,
    });

}
