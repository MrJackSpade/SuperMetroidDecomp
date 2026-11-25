using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's layers and the actors gameplay shows (#1407): chosen on each update, shown from the accepted frame
/// that latches them. An update without a frame boundary leaves the shown layers in place.
/// </summary>
internal sealed partial class GameplayScreenLayers
{
    /// <summary>The layers an ordinary room shows: both backgrounds and the sprites.</summary>
    private const ScreenLayers OrdinaryLayers = ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.BossBody | ScreenLayers.Sprites;

    /// <summary>The layers the update chose for the next accepted frame.</summary>
    internal ScreenLayers Chosen { get; private set; } = OrdinaryLayers;

    /// <summary>The layers the most recent accepted frame shows.</summary>
    public ScreenLayers Displayed { get; private set; }

    /// <summary>
    /// Chooses the room's own layers for the next accepted frame, as the cartridge's layer blending does at the top of every
    /// update: foreground and the sprites alone while the room's boss body hides its own layer and the background behind it
    /// (#2571), and the waterfall rooms' layers when their effect's blend lays the background behind the room (#1545). A
    /// door transition's own layers are not chosen here: the frame shows them over these while the door's display runs
    /// (<see cref="ActiveRoomState.DoorTransitionShownLayers"/>), and the transition chooses its destination's own layers on
    /// each of its updates once the destination has loaded, so they are ready when the door hands the screen back (#3047).
    /// </summary>
    internal void Choose(RoomScope room)
    {
        Chosen = room.FxOverlay.Blend is OverlayBehindRoomBlend { BackgroundBehind: true }
            ? WaterfallRoomDisplayRules.ShownLayers
            : ScreenLayers.Foreground | ScreenLayers.Sprites |
                (room.Enemies.DisplaySources.BossBody?.HidesBackgroundLayer == true
                    ? ScreenLayers.None
                    : ScreenLayers.Background | ScreenLayers.BossBody);
    }

    /// <summary>The accepted frame shows the chosen layers.</summary>
    internal void Latch() => Displayed = Chosen;
}
