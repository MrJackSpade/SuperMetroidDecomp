namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Dachora's run, idle, blink, shinespark charge, echo and fall animations as typed programs (#641), one per bank-$A7 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="DachoraAnimations"/>.
/// </summary>
internal enum DachoraProgram
{
    /// <summary><c>InstList_Dachora_RunningLeft</c> at $A7:F345.</summary>
    RunningLeft,
    /// <summary><c>InstList_Dachora_RunningLeft_FastAnimation</c> at $A7:F361.</summary>
    RunningLeftFast,
    /// <summary><c>InstList_Dachora_RunningLeft_VeryFastAnimation</c> at $A7:F37D.</summary>
    RunningLeftVeryFast,
    /// <summary><c>InstList_Dachora_Idling_FacingLeft</c> at $A7:F399.</summary>
    IdleLeft,
    /// <summary><c>InstList_Dachora_Blinking_FacingLeft</c> at $A7:F3C9.</summary>
    BlinkLeft,
    /// <summary><c>InstList_Dachora_Echo_FacingLeft</c> at $A7:F3F7.</summary>
    EchoLeft,
    /// <summary><c>InstList_Dachora_Falling_FacingLeft</c> at $A7:F3FF.</summary>
    FallingLeft,
    /// <summary><c>InstList_Dachora_RunningRight</c> at $A7:F407.</summary>
    RunningRight,
    /// <summary><c>InstList_Dachora_RunningRight_FastAnimation</c> at $A7:F423.</summary>
    RunningRightFast,
    /// <summary><c>InstList_Dachora_RunningRight_VeryFastAnimation</c> at $A7:F43F.</summary>
    RunningRightVeryFast,
    /// <summary><c>InstList_Dachora_Idling_FacingRight</c> at $A7:F45B.</summary>
    IdleRight,
    /// <summary><c>InstList_Dachora_Blinking_FacingRight</c> at $A7:F48B.</summary>
    BlinkRight,
    /// <summary><c>InstList_Dachora_ChargeShinespark_FacingRight</c> at $A7:F4B3.</summary>
    ChargeRight,
    /// <summary><c>InstList_Dachora_Echo_FacingRight</c> at $A7:F4B9.</summary>
    EchoRight,
    /// <summary><c>InstList_Dachora_Falling_FacingRight</c> at $A7:F4C1.</summary>
    FallingRight,
}
