namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses (#641), each showing installed frames. Their steps are <see cref="PowampAnimations.BodySegments"/>.
/// </summary>
internal enum PowampBodySegment
{
    /// <summary>Its body's tail swinging left, down and right quickly, repeating.</summary>
    BodyFast,
    /// <summary>Its body's tail swinging left, down and right slowly, repeating.</summary>
    BodySlow,
}
