namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Skultera's swimming and turning animations as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="SkulteraAnimations"/>.
/// </summary>
internal enum SkulteraProgram
{
    /// <summary>The native list at $A3:902A.</summary>
    SwimmingLeft,
    /// <summary>The loop at $A3:902C.</summary>
    SwimmingLeftLoop,
    /// <summary>The native list at $A3:903C.</summary>
    TurningRight,
    /// <summary>The native list at $A3:9060.</summary>
    SwimmingRight,
    /// <summary>The loop at $A3:9062.</summary>
    SwimmingRightLoop,
    /// <summary>The native list at $A3:9072.</summary>
    TurningLeft,
}
