namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// Which value of another enemy an Evir body's initialization takes its first bobbing half-cycle from. Each member
/// reads the value's upper eight bits and halves them.
/// </summary>
public enum EvirHalfCycleSourceField
{
    /// <summary>The enemy's second population parameter; for an Evir body its upper eight bits are the direction-change period, and an Evir's arms hold zero.</summary>
    PopulationParameter2,
    /// <summary>The enemy's sprite palette selection; halving its upper eight bits gives the palette number.</summary>
    Palette,
}
