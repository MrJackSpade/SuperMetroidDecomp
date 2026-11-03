using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The shades the import pre-colours the reserve-tank arrow's pictures in (#1512): its solid bevel colours and each phase
/// of its pulse. Which inks each recolours, and to what, is the import's.
/// </summary>
public static class PauseReserveArrowShades
{
    /// <summary>The arrow's bevels in their solid colours, shown while the transfer control is selected or the arrow is dimmed.</summary>
    public static PictureShade Solid { get; } = new("PauseReserveArrowSolid", 0);

    /// <summary>The arrow's bevels at phase <paramref name="frame"/> of the pulse shown while AUTO is selected.</summary>
    public static PictureShade Pulse(int frame) => new("PauseReserveArrowPulse", frame);
}
