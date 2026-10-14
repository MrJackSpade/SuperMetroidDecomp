namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="PowampAnimations"/>.
/// </summary>
internal enum PowampProgram
{
    /// <summary><c>InstList_Powamp_Body_FastAnimation</c> at $A8:C163.</summary>
    BodyFast,
    /// <summary><c>InstList_Powamp_Body_SlowAnimation</c> at $A8:C173.</summary>
    BodySlow,
    /// <summary><c>InstList_Powamp_Balloon_Inflate_0</c> at $A8:C183.</summary>
    BalloonInflate0,
    /// <summary><c>InstList_Powamp_Balloon_Inflate_1</c> at $A8:C187.</summary>
    BalloonInflate1,
    /// <summary><c>InstList_Powamp_Balloon_StartSinking</c> at $A8:C191.</summary>
    BalloonStartSinking,
    /// <summary><c>InstList_Powamp_Balloon_Deflated</c> at $A8:C199.</summary>
    BalloonDeflated,
}
