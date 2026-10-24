namespace SuperMetroid.Core.Fx;

/// <summary>
/// The fixed colour the room's colour math blends with (#1407): Fireflea darkness writes it and X-ray blending reads it.
/// It persists across rooms, as the cartridge's does.
/// </summary>
public sealed class RoomFixedColor
{
    /// <summary>The colour.</summary>
    public Bgr555 Color { get; internal set; }
}
