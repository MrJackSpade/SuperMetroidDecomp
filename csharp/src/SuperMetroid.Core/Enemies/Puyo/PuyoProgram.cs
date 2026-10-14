namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="PuyoAnimations"/>.
/// </summary>
internal enum PuyoProgram
{
    /// <summary><c>InstList_Puyo_GroundedDropping_Fast</c> at $A2:99AD.</summary>
    GroundedFast,
    /// <summary><c>InstList_Puyo_GroundedDropping_Medium</c> at $A2:99C1.</summary>
    GroundedMedium,
    /// <summary><c>InstList_Puyo_GroundedDropping_Slow</c> at $A2:99D5.</summary>
    GroundedSlow,
    /// <summary><c>InstList_Puyo_HoppingRight_0_HoppingLeft_4</c> at $A2:99E9.</summary>
    RightFrame0LeftFrame4,
    /// <summary><c>InstList_Puyo_HoppingRight_1_HoppingLeft_3</c> at $A2:99EF.</summary>
    RightFrame1LeftFrame3,
    /// <summary><c>InstList_Puyo_Hopping_2</c> at $A2:99F5.</summary>
    Frame2,
    /// <summary><c>InstList_Puyo_HoppingRight_3_HoppingLeft_1</c> at $A2:99FB.</summary>
    RightFrame3LeftFrame1,
    /// <summary><c>InstList_Puyo_HoppingRight_4_HoppingLeft_0</c> at $A2:9A01.</summary>
    RightFrame4LeftFrame0,
}
