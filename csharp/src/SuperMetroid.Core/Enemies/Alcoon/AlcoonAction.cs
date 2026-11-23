namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The behaviour Alcoon's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum AlcoonAction
{
    /// <summary>Starts the emergence jump when Samus is strictly within its emergence reach across (<see cref="AlcoonMotionDefinitions.EmergeXDistance"/>) and of the previously discovered landing Y (<see cref="AlcoonMotionDefinitions.EmergeYDistance"/>).</summary>
    WaitingForSamus,
    /// <summary>Integrates upward vertical motion with half-pixel acceleration until it stops rising, then selects the falling pose.</summary>
    EmergingRising,
    /// <summary>Descends through the terrain collision mover, then faces Samus and begins walking with one walking cycle remaining.</summary>
    EmergingFalling,
    /// <summary>Follows the floor, hides after travelling its walking range from spawn (<see cref="AlcoonMotionDefinitions.HideXDistance"/>), or requests a volley once its walk cycles are walked (<see cref="AlcoonEnemyState.WalkCyclesBeforeVolley"/>) and Samus is ahead; animation cues perform horizontal movement.</summary>
    WalkingAndFiring,
    /// <summary>Performs no main-AI movement while the fire-volley animation runs, until its cue restores walking.</summary>
    WaitingForFireAnimation,
    /// <summary>Integrates the retreat jump's upward motion until acceleration stops it rising.</summary>
    HidingRising,
    /// <summary>Falls without terrain collision until reaching spawn Y, then restores the whole spawn coordinates and proximity-wait state, keeping where it lies within a pixel.</summary>
    HidingFalling,
}
