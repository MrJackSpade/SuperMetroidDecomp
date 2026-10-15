namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship hull's and entrance pad's typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="GunshipAnimations"/>.
/// </summary>
internal enum GunshipProgram
{
    /// <summary><c>InstList_ShipEntrancePad_Opening_0</c> at $A2:A5BE.</summary>
    EntrancePadOpening,
    /// <summary><c>InstList_ShipEntrancePad_Opening_1</c> at $A2:A5E6.</summary>
    EntrancePadOpen,
    /// <summary><c>InstList_ShipEntrancePad_Closing</c> at $A2:A5EE.</summary>
    EntrancePadClosing,
    /// <summary><c>InstList_ShipEntrancePad_Closed</c> at $A2:A60E.</summary>
    BottomEntrancePad,
    /// <summary><c>InstList_ShipTop</c> at $A2:A616.</summary>
    TopHull,
    /// <summary><c>InstList_ShipBottom</c> at $A2:A61C.</summary>
    BottomHull,
}
