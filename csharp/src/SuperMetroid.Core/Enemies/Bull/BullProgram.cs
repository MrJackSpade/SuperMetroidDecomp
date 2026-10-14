namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's normal and immune-shot animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="BullAnimations"/>.
/// </summary>
internal enum BullProgram
{
    /// <summary><c>InstList_Bull_Normal</c> at $A8:D841.</summary>
    Normal,
    /// <summary><c>InstList_Bull_Shot_0</c> at $A8:D855.</summary>
    Shot,
    /// <summary><c>InstList_Bull_Shot_1</c> at $A8:D859.</summary>
    ShotLoop,
}
