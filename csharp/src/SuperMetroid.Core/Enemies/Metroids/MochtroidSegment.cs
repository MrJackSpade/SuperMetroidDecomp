namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Mochtroid's free-flight and attached pulse loops (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="MochtroidAnimations"/>.
/// </summary>
internal enum MochtroidSegment
{
    /// <summary>Floating, its body squeezing and widening slowly, repeating.</summary>
    FreeFlight,
    /// <summary>Latched onto Samus, its body squeezing and widening quickly, repeating.</summary>
    Attached,
}
