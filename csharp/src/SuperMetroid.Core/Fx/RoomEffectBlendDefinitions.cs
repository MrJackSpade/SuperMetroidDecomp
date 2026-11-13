using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>The blends the room effects show (#1545), each the description a room's effect names.</summary>
public static class RoomEffectBlendDefinitions
{
    /// <summary>The overlay lies over the room and brightens it: lava, acid, clear water, the fog and the rain.</summary>
    public static RoomEffectBlend OverlayBrightens { get; } = new() { Operation = BlendOperation.Add };

    /// <summary>The overlay lies over the room and darkens it: the water of most water rooms.</summary>
    public static RoomEffectBlend OverlayDarkens { get; } = new() { Operation = BlendOperation.Subtract };

    /// <summary>
    /// The spores lie behind the room and brighten the background, the actors taking part in blends and the backdrop; the
    /// foreground keeps its colour, so the spores never brighten the room's terrain.
    /// </summary>
    public static RoomEffectBlend SporesBrightenBehind { get; } = new()
    {
        Operation = BlendOperation.Add,
        BlendsBehind = BlendedLayers.Background | BlendedLayers.Actors | BlendedLayers.Backdrop,
    };

    /// <summary>
    /// The waterfall, the background and the room's water lie behind the foreground, the actors taking part in blends and
    /// the backdrop, and darken them: the Colosseum's and Halfie Climb's waterfalls.
    /// </summary>
    public static RoomEffectBlend WaterfallDarkensBehind { get; } = new()
    {
        Operation = BlendOperation.Subtract,
        BlendsBehind = BlendedLayers.Foreground | BlendedLayers.Actors | BlendedLayers.Backdrop,
        BackgroundBehind = true,
    };
}
