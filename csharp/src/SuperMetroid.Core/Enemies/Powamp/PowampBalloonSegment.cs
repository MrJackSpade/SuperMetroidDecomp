namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's balloon inflate and sink animations (#641), each showing the balloon's poses (#2553), so every frame the
/// balloon shows is the pose it stands at. Their steps are <see cref="PowampAnimations.BalloonSegments"/>.
/// </summary>
internal enum PowampBalloonSegment
{
    /// <summary>The balloon inflating from deflated, running on into its inflating from halfway.</summary>
    InflatingFromDeflated,
    /// <summary>The balloon inflating from halfway to full, where it holds.</summary>
    InflatingFromHalfway,
    /// <summary>Its balloon full, then half full as it starts to sink, then small.</summary>
    StartSinking,
    /// <summary>Its balloon small, held while it sinks.</summary>
    Deflated,
}
