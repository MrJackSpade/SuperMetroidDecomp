namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>What starting a flicker step does to a Wrecked Ship ghost's visibility. Its values are arbitrary.</summary>
public enum WreckedShipGhostFlickerVisibility
{
    /// <summary>The step leaves the ghost as it was: still hidden if it has not yet been shown.</summary>
    Unchanged,

    /// <summary>The step makes the ghost visible.</summary>
    Shown,
}
