namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="HopperAnimations"/>.
/// </summary>
internal enum HopperProgram
{
    /// <summary><c>InstList_Sidehopper_Hopping_UpsideUp</c> at $A3:AA76.</summary>
    SidehopperFloorJumping,
    /// <summary>The native list at $A3:AA82.</summary>
    SidehopperFloorLanded,
    /// <summary>The native list at $A3:AA9C.</summary>
    SidehopperCeilingJumping,
    /// <summary>The native list at $A3:AAA8.</summary>
    SidehopperCeilingLanded,
    /// <summary><c>InstList_Dessgeega_Hopping_UpsideUp</c> at $A3:AFA5.</summary>
    DessgeegaFloorJumping,
    /// <summary>The native list at $A3:AFAD.</summary>
    DessgeegaFloorLanded,
    /// <summary>The native list at $A3:AFC3.</summary>
    DessgeegaCeilingJumping,
    /// <summary>The native list at $A3:AFCB.</summary>
    DessgeegaCeilingLanded,
    /// <summary><c>InstList_SidehopperLarge_Hopping_UpsideUp</c> at $A3:B0C5.</summary>
    LargeSidehopperFloorJumping,
    /// <summary>The native list at $A3:B0D1.</summary>
    LargeSidehopperFloorLanded,
    /// <summary>The native list at $A3:B0EB.</summary>
    LargeSidehopperCeilingJumping,
    /// <summary>The native list at $A3:B0F7.</summary>
    LargeSidehopperCeilingLanded,
    /// <summary><c>InstList_DessgeegaLarge_Hopping_UpsideUp</c> at $A3:B237.</summary>
    LargeDessgeegaFloorJumping,
    /// <summary>The native list at $A3:B23F.</summary>
    LargeDessgeegaFloorLanded,
    /// <summary>The native list at $A3:B255.</summary>
    LargeDessgeegaCeilingJumping,
    /// <summary>The native list at $A3:B25D.</summary>
    LargeDessgeegaCeilingLanded,
}
