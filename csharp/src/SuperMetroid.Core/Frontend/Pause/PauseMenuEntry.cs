using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The gameplay state and presentation a pause menu opens over (#633).</summary>
internal sealed record PauseMenuEntry
{
    /// <summary>Samus, whose equipment and reserves the pause screens show and change.</summary>
    public required SamusState Samus { get; init; }

    /// <summary>The game state the session shares, whose collected maps and explored tiles the map shows.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>The area Samus is in.</summary>
    public required AreaId Area { get; init; }

    /// <summary>The room's place on the area map, which the map centres on and marks.</summary>
    public required Position16<MapTiles> RoomMapPosition { get; init; }

    /// <summary>The queues the pause screens' sounds go to.</summary>
    public required AudioQueues Audio { get; init; }

    /// <summary>The HUD gameplay displayed at its last frame (#1351, #1466), which pause keeps resident.</summary>
    public ShownHud? GameplayHud { get; init; } = null;

    /// <summary>How much of each area map the map assist shows.</summary>
    public MapRevealMode MapRevealMode { get; init; } = MapRevealMode.None;

    /// <summary>The installed screen content the pause screens draw their maps and selectors from.</summary>
    public required InstalledScreenContent ScreenContent { get; init; }

    /// <summary>The installed sprite sheets the map's sprites draw from.</summary>
    public required InstalledSpriteSheets SpriteSheets { get; init; }
}
