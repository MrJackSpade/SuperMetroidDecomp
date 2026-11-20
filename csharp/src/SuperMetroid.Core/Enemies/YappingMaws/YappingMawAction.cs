namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The behaviour Yapping Maw's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum YappingMawAction
{
    /// <summary>Samples Samus distance/angle, refreshes the grab cooldown inside <see cref="YappingMawAttackDefinitions.GrabSafetyDistance"/>, and starts an attack only below the population activation threshold.</summary>
    WaitingForSamus,
    /// <summary>Starts the curl from rest, takes its reach and bend from where Samus is, and starts the attack animation for its eight-way aim with where it holds a caught Samus.</summary>
    BeginExtension,
    /// <summary>Solves all four curled link positions, advances the quadratic angular motion, carries any captive with the mouth, and enters the hold after retraction when the curl passes below zero.</summary>
    ExtendingOrRetracting,
    /// <summary>Holds a caught Samus in the mouth for its hold after retraction (<see cref="YappingMawAttackDefinitions.RetractedHold"/>), releasing her input on the last and returning to the wait, unless her death keeps the Maw holding.</summary>
    RetractedDelay,
}
