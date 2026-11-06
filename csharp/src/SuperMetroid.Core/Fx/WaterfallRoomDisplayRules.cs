using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>Which layers the waterfall rooms (the Colosseum and Halfie Climb) show directly while their waterfall blends.</summary>
public static class WaterfallRoomDisplayRules
{
    /// <summary>The foreground and the actors are the layers the screen shows directly; the background shows only through the waterfall's blend.</summary>
    public const ScreenLayers ShownLayers = ScreenLayers.Foreground | ScreenLayers.Sprites;
}
