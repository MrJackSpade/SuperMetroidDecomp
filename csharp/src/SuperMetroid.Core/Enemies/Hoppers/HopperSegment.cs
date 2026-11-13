namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="HopperAnimations"/>.
/// </summary>
internal enum HopperSegment
{
    /// <summary>The sidehopper floor jumping animation.</summary>
    SidehopperFloorJumping,
    /// <summary>The sidehopper floor landed animation.</summary>
    SidehopperFloorLanded,
    /// <summary>The sidehopper ceiling jumping animation.</summary>
    SidehopperCeilingJumping,
    /// <summary>The sidehopper ceiling landed animation.</summary>
    SidehopperCeilingLanded,
    /// <summary>The dessgeega floor jumping animation.</summary>
    DessgeegaFloorJumping,
    /// <summary>The dessgeega floor landed animation.</summary>
    DessgeegaFloorLanded,
    /// <summary>The dessgeega ceiling jumping animation.</summary>
    DessgeegaCeilingJumping,
    /// <summary>The dessgeega ceiling landed animation.</summary>
    DessgeegaCeilingLanded,
    /// <summary>The large sidehopper floor jumping animation.</summary>
    LargeSidehopperFloorJumping,
    /// <summary>The large sidehopper floor landed animation.</summary>
    LargeSidehopperFloorLanded,
    /// <summary>The large sidehopper ceiling jumping animation.</summary>
    LargeSidehopperCeilingJumping,
    /// <summary>The large sidehopper ceiling landed animation.</summary>
    LargeSidehopperCeilingLanded,
    /// <summary>The large dessgeega floor jumping animation.</summary>
    LargeDessgeegaFloorJumping,
    /// <summary>The large dessgeega floor landed animation.</summary>
    LargeDessgeegaFloorLanded,
    /// <summary>The large dessgeega ceiling jumping animation.</summary>
    LargeDessgeegaCeilingJumping,
    /// <summary>The large dessgeega ceiling landed animation.</summary>
    LargeDessgeegaCeilingLanded,
}
