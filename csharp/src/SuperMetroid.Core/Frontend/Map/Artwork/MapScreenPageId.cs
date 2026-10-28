using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>One of the thirteen map-screen tilemap pages: the shared world foreground, or an area's world background or room frame.</summary>
public abstract record MapScreenPageId
{
    /// <summary>Closes the hierarchy to the three page kinds below.</summary>
    private MapScreenPageId()
    {
    }

    /// <summary>The page's key in the map-screen document.</summary>
    public abstract string Name { get; }

    /// <summary>The world map's foreground, shared by every area.</summary>
    public sealed record WorldForeground : MapScreenPageId
    {
        /// <summary>The only foreground page.</summary>
        public static WorldForeground Instance { get; } = new();

        /// <summary>Creates the only foreground page.</summary>
        private WorldForeground()
        {
        }

        /// <inheritdoc/>
        public override string Name => "World.Foreground";
    }

    /// <summary>A playable area's world-map background.</summary>
    public sealed record WorldBackground : MapScreenPageId
    {
        /// <summary>Selects <paramref name="area"/>'s world-map background.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not a playable Zebes area.</exception>
        public WorldBackground(AreaId area) => Area = MapScreens.RequireZebesArea(area);

        /// <summary>The playable area whose background this is.</summary>
        public AreaId Area { get; }

        /// <inheritdoc/>
        public override string Name => "World." + Area;
    }

    /// <summary>A playable area's room-map frame.</summary>
    public sealed record RoomFrame : MapScreenPageId
    {
        /// <summary>Selects <paramref name="area"/>'s room-map frame.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not a playable Zebes area.</exception>
        public RoomFrame(AreaId area) => Area = MapScreens.RequireZebesArea(area);

        /// <summary>The playable area whose frame this is.</summary>
        public AreaId Area { get; }

        /// <inheritdoc/>
        public override string Name => "Room." + Area;
    }
}
