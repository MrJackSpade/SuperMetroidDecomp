using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>The installed per-area map views that exploration, saves and the HUD minimap read.</summary>
public interface IAreaMapSource
{
    /// <summary>The validated map view of <paramref name="area"/>.</summary>
    IAreaMapView Get(AreaId area);
}
