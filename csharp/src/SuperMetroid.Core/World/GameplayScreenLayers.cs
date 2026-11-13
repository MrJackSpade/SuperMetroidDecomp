using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's layers and the actors gameplay shows (#1407): chosen on each main-loop pass, shown from the accepted frame
/// that latches them. An update without a frame boundary leaves the shown layers in place.
/// </summary>
internal sealed class GameplayScreenLayers
{
    /// <summary>The layers an ordinary room shows: both backgrounds and the sprites.</summary>
    private const ScreenLayers OrdinaryLayers = ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites;

    /// <summary>Supplies door-transition screen-layer overrides.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Binds the room whose door transitions override the layers.</summary>
    internal GameplayScreenLayers(ActiveRoomState room)
    {
        _room = room;
    }

    /// <summary>The layers the main loop chose for the next accepted frame.</summary>
    internal ScreenLayers Chosen { get; private set; } = OrdinaryLayers;

    /// <summary>The layers the most recent accepted frame shows.</summary>
    public ScreenLayers Displayed { get; private set; }

    /// <summary>
    /// Chooses the layers before projectile frame behaviours run: a door transition's own layers, foreground and the sprites alone
    /// while Mother Brain's death hides background, and the waterfall rooms' layers when their effect's blend lays the
    /// background behind the room (#1545).
    /// </summary>
    internal void Choose(RoomScope room)
    {
        Chosen = _room.DoorTransitionShownLayers ??
            (ScreenLayers.Foreground | ScreenLayers.Sprites |
                (room.Enemies.DisplaySources.MotherBrain?.DeathHidesBodyLayer == true
                    ? ScreenLayers.None
                    : ScreenLayers.Background));
        if (_room.DoorTransitionShownLayers is null &&
            room.FxOverlay.Blend is { BackgroundBehind: true })
        {
            Chosen = WaterfallRoomDisplayRules.ShownLayers;
        }
    }

    /// <summary>The accepted frame shows the chosen layers.</summary>
    internal void Latch() => Displayed = Chosen;
}
