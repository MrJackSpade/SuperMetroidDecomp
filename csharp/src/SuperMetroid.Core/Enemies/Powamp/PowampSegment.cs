namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations (#641).
/// Their steps are <see cref="PowampAnimations"/>.
/// </summary>
internal enum PowampSegment
{
    /// <summary>Its body's tail swinging left, down and right quickly, repeating.</summary>
    BodyFast,
    /// <summary>Its body's tail swinging left, down and right slowly, repeating.</summary>
    BodySlow,
    /// <summary>The balloon inflating from deflated, running on into its inflating from halfway.</summary>
    BalloonInflatingFromDeflated,
    /// <summary>The balloon inflating from halfway to full, where it holds.</summary>
    BalloonInflatingFromHalfway,
    /// <summary>Its balloon full, then half full as it starts to sink, then small.</summary>
    BalloonStartSinking,
    /// <summary>Its balloon small, held while it sinks.</summary>
    BalloonDeflated,
}
