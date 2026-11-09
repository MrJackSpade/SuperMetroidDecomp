namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Where the Lower Norfair Chozo hand is in its course (<see cref="LowerNorfairChozoHandObject"/>).</summary>
internal enum LowerNorfairChozoHandPhase
{
    /// <summary>Spawned: its first update checks whether the acid is already lowered.</summary>
    Starting,
    /// <summary>The acid was not lowered: it waits for its trigger block to clear.</summary>
    Waiting,
    /// <summary>The acid was lowered and the cleared hand drawn: it leaves on its next update.</summary>
    Lowered,
}
