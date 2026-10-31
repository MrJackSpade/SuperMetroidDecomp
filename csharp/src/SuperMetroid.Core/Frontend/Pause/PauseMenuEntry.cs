using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The gameplay state and presentation a pause menu opens over (#633).</summary>
internal sealed record PauseMenuEntry
{
    /// <summary>Samus.</summary>
    public required SamusState Samus { get; init; }

    /// <summary>System.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>Area index.</summary>
    public required AreaId AreaIndex { get; init; }

    /// <summary>Room map position.</summary>
    public required Position16<MapTiles> RoomMapPosition { get; init; }

    /// <summary>Audio.</summary>
    public AudioQueues? Audio { get; init; } = null;

    /// <summary>The HUD gameplay displayed at its last frame (#1351, #1466), which pause keeps resident.</summary>
    public ShownHud? GameplayHud { get; init; } = null;

    /// <summary>Map reveal mode.</summary>
    public MapRevealMode MapRevealMode { get; init; } = MapRevealMode.None;

    /// <summary>Map presentation.</summary>
    public AreaMapPresentation? MapPresentation { get; init; } = null;

    /// <summary>The installed sprite sheets the map's sprites draw from.</summary>
    public InstalledSpriteSheets? SpriteSheets { get; init; } = null;
}
