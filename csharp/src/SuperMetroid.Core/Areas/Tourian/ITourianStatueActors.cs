using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The entrance statue's state and the unlock effects the statue sequence drives.</summary>
internal interface ITourianStatueActors
{
    /// <summary>Which statues are animating, whether a release is in progress, and whether all four have released.</summary>
    TourianStatueAnimation Animation { get; }

    /// <summary>The statue's vertical offset as it sinks.</summary>
    int VerticalOffset { get; set; }

    /// <summary>Whether the sequence has finished.</summary>
    bool Finished { get; set; }

    /// <summary>The water surface the unlock effects follow; only its Y axis is meaningful.</summary>
    Position16<RoomPixels> WaterSurface { get; set; }

    /// <summary>Spawns the glow of a statue's <paramref name="eye"/>.</summary>
    void SpawnEyeGlow(TourianStatueEye eye);

    /// <summary>Spawns a statue's released soul at its <paramref name="eye"/>.</summary>
    void SpawnSoul(TourianStatueEye eye);

    /// <summary>Spawns the descent's dust.</summary>
    void SpawnDescentDust();

    /// <summary>
    /// Shows a released statue's look, its blacked-out eyes or its grey, as the shade the import pre-coloured it in (#1512).
    /// </summary>
    void ShowStatueLook(PictureShade look);
}
