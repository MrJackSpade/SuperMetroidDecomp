using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>The background colours room FX writes (#1405).</summary>
public static class RoomFxColourDefinitions
{
    /// <summary>The three colours a palette blend writes into the HUD's effect colours.</summary>
    public static readonly BackgroundColourRun Layer3Blend = new(BackgroundColourSet.HudItemsAndEffects, 9, 3);

    /// <summary>The one colour the empty blend clears: the blend's third.</summary>
    public static readonly BackgroundColourRun Layer3Empty = new(BackgroundColourSet.HudItemsAndEffects, 11, 1);
}
