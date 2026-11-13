namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PowampAnimations"/>.
/// </summary>
internal enum PowampSegment
{
    /// <summary>The body fast animation.</summary>
    BodyFast,
    /// <summary>The body slow animation.</summary>
    BodySlow,
    /// <summary>The balloon inflate 0 animation.</summary>
    BalloonInflate0,
    /// <summary>The balloon inflate 1 animation.</summary>
    BalloonInflate1,
    /// <summary>The balloon start sinking animation.</summary>
    BalloonStartSinking,
    /// <summary>The balloon deflated animation.</summary>
    BalloonDeflated,
}
