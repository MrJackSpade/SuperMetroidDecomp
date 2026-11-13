namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Which hopper an enemy definition is: its size and species, which choose its animations and sounds.</summary>
public enum HopperKind
{
    /// <summary>The small Sidehopper.</summary>
    Sidehopper,
    /// <summary>The large Sidehopper, whose animations the Tourian Sidehopper shares.</summary>
    LargeSidehopper,
    /// <summary>The large Dessgeega.</summary>
    LargeDessgeega,
    /// <summary>The small Dessgeega.</summary>
    Dessgeega,
}
