namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// Which value of another enemy an Evir body's initialization takes its first bobbing half-cycle from. Each member
/// reads the value's upper eight bits and halves them.
/// </summary>
public enum EvirHalfCycleSourceField
{
    /// <summary>The enemy's direction-change period as an Evir body; an Evir's arms and an enemy not yet loaded give zero.</summary>
    DirectionChangePeriod,
    /// <summary>The enemy's sprite palette selection; halving its upper eight bits gives the palette number.</summary>
    Palette,
}
