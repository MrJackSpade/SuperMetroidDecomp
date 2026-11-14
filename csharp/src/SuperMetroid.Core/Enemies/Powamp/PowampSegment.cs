namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PowampAnimations"/>.
/// </summary>
internal enum PowampSegment
{
    /// <summary>The body fast animation.</summary>
    BodyFast,
    /// <summary>The body slow animation.</summary>
    BodySlow,
    /// <summary>The balloon inflating from deflated, running on into its inflating from halfway.</summary>
    BalloonInflatingFromDeflated,
    /// <summary>The balloon inflating from halfway to full, where it holds.</summary>
    BalloonInflatingFromHalfway,
    /// <summary>The balloon start sinking animation.</summary>
    BalloonStartSinking,
    /// <summary>The balloon deflated animation.</summary>
    BalloonDeflated,
}
