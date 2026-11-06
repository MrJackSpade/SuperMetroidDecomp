using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx;

/// <summary>
/// The fixed colour the room's blends combine with (#1407): Fireflea darkness writes it and X-ray blending reads it.
/// It persists across rooms, as the game's does.
/// </summary>
public sealed class RoomFixedColor
{
    /// <summary>The colour.</summary>
    public RgbColor Color { get; internal set; }
}
