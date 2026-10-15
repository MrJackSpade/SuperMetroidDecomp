namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Lower Norfair and Wrecked Ship Chozo statues' idle and activation animations as typed programs (#641), one per bank-$AA list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="ChozoStatueAnimations"/>.
/// </summary>
internal enum ChozoStatueProgram
{
    /// <summary><c>InstList_Chozo_LowerNorfair_Initial</c> at $AA:E39D.</summary>
    LowerNorfairInitial,
    /// <summary><c>InstList_Chozo_LowerNorfair_Activated_0</c> at $AA:E3A7.</summary>
    LowerNorfairActivated,
    /// <summary>The loop at $AA:E3E5.</summary>
    LowerNorfairActivatedLoop,
    /// <summary><c>InstList_Chozo_WreckedShip_Initial</c> at $AA:E457.</summary>
    WreckedShipInitial,
    /// <summary><c>InstList_Chozo_WreckedShip_Activated_0</c> at $AA:E461.</summary>
    WreckedShipActivated,
    /// <summary>The loop at $AA:E49D.</summary>
    WreckedShipActivatedLoop,
    /// <summary>The loop at $AA:E4C1.</summary>
    WreckedShipActivatedLoop2,
}
