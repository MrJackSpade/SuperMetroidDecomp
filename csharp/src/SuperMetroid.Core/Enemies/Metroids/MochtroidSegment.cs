namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Mochtroid's free-flight and attached pulse loops (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="MochtroidAnimations"/>.
/// </summary>
internal enum MochtroidSegment
{
    /// <summary>The free flight animation.</summary>
    FreeFlight,
    /// <summary>The attached animation.</summary>
    Attached,
}
