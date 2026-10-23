using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>The background colours room FX writes (#1405).</summary>
public static class RoomFxColourDefinitions
{
    /// <summary>The three colours a palette blend writes: layer 3's sixth row, colours one to three.</summary>
    public static readonly BackgroundColourRun Layer3Blend = new(1, 9, 3);

    /// <summary>The one colour the empty blend clears: the blend's third.</summary>
    public static readonly BackgroundColourRun Layer3Empty = new(1, 11, 1);
}
