namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>How near Samus must come for a Ceres door to open, and how far she must go for it to close.</summary>
internal static partial class CeresDoorApproachDefinitions
{
    /// <summary>
    /// The distance in pixels, along either axis, at which Samus is distant from a door: a shut door opens only while she
    /// is nearer than this on both axes, and an open door closes once she is this far or farther on either.
    /// </summary>
    internal const int DistantPixels = 48;
}
