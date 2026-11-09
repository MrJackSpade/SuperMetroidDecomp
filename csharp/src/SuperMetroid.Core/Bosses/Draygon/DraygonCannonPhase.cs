namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Where a Draygon wall cannon is in its course (<see cref="DraygonCannonObject"/>).</summary>
internal enum DraygonCannonPhase
{
    /// <summary>Spawned whole: its first update arms its missile wake and shows the shield.</summary>
    StartingWhole,
    /// <summary>Spawned destroyed: its first update destroys it and starts its smoke.</summary>
    StartingDestroyed,
    /// <summary>Showing its shield and resting until a missile wakes it.</summary>
    Shielded,
    /// <summary>A missile woke it: the update counts the hit.</summary>
    Struck,
    /// <summary>Flashing its shield after a hit that did not destroy it.</summary>
    Flashing,
    /// <summary>Destroyed, smoking for good.</summary>
    Smoking,
}
