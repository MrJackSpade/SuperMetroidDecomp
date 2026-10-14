using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The gameplay state and presentation a pause menu opens over (#633).</summary>
internal sealed record PauseMenuEntry
{
    /// <summary>Samus.</summary>
    public required SamusState Samus { get; init; }

    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }

    /// <summary>Area index.</summary>
    public required AreaId AreaIndex { get; init; }

    /// <summary>Room map position.</summary>
    public required Position16<MapTiles> RoomMapPosition { get; init; }

    /// <summary>Audio.</summary>
    public CartridgeAudioState? Audio { get; init; } = null;

    /// <summary>Gameplay vram.</summary>
    public SnesVram? GameplayVram { get; init; } = null;

    /// <summary>Map reveal mode.</summary>
    public MapRevealMode MapRevealMode { get; init; } = MapRevealMode.None;

    /// <summary>Map presentation.</summary>
    public AreaMapPresentationCatalog? MapPresentation { get; init; } = null;
}
