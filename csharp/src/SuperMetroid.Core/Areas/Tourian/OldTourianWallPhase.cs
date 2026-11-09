namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Where the old Tourian escape shaft's fake wall is in its course (<see cref="OldTourianWallObject"/>).</summary>
internal enum OldTourianWallPhase
{
    /// <summary>Spawned: it begins watching for Samus from its next update.</summary>
    Starting,
    /// <summary>Watching for Samus below and right of its target.</summary>
    Waiting,
    /// <summary>Blown open: it leaves on its next update.</summary>
    Opened,
}
