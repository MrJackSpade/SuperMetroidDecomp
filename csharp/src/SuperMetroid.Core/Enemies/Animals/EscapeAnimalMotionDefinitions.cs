namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How the escaping Etecoons and Dachora read the rising acid and how an escaping Etecoon moves (#2279).</summary>
internal static class EscapeAnimalMotionDefinitions
{
    /// <summary>
    /// The room row at or below which the escaping animals find the acid still low.
    /// </summary>
    internal const int LowAcidSurfaceY = 206;

    /// <summary>The one whole pixel an escaping Etecoon probes downward each update, which settles it on the room's floor.</summary>
    internal static readonly SignedSixteenSixteen EtecoonGravity = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>The horizontal speed, 3.5 pixels per update, at which an escaping Etecoon leaves the room.</summary>
    internal static readonly SignedSixteenSixteen EtecoonExitSpeed = SignedSixteenSixteen.FromPixelFraction(7, 2);
}
