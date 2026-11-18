namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Where the escape room's acid trigger is in its course (<see cref="EscapeAcidRiseObject"/>, #2482).</summary>
internal enum EscapeAcidRisePhase
{
    /// <summary>Spawned: it begins watching for Samus from its next update.</summary>
    Starting,
    /// <summary>Watching for Samus passing its trigger point.</summary>
    Watching,
}
