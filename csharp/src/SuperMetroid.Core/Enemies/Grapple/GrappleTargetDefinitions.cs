namespace SuperMetroid.Core.Enemies.Grapple;

/// <summary>How the grapple beam's point reaches an enemy (#2149).</summary>
internal static partial class GrappleTargetDefinitions
{
    /// <summary>
    /// The beam point's own radius, in pixels: the point reaches an enemy whose ordinary radius plus this one, exclusive,
    /// spans the distance between them on both axes.
    /// </summary>
    internal const int BeamPointRadius = 8;
}
