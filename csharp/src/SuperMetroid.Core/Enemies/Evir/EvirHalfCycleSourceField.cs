namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// Which value of another enemy an Evir body's initialization takes its first bobbing half-cycle from. Each member
/// reads the value's upper eight bits and halves them.
/// </summary>
public enum EvirHalfCycleSourceField
{
    /// <summary>The enemy's direction-change period as an Evir body; an Evir's arms and an enemy not yet loaded give zero.</summary>
    DirectionChangePeriod,

    /// <summary>
    /// The colours the enemy draws in, which the game reads as a number: for an Evir's spit, the half-cycle
    /// <see cref="EvirInitDefinitions.SpitColoursHalfCycle"/> the retail rooms give; for an enemy not yet loaded, zero.
    /// </summary>
    SpitColours,
}
