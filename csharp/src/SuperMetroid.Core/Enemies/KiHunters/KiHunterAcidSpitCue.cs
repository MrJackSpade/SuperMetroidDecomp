namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// A cue of the Ki-Hunter acid's animations (#1843).
/// </summary>
internal enum KiHunterAcidSpitCue
{
    /// <summary>Launches the acid leftward.</summary>
    LaunchLeft,

    /// <summary>Launches the acid rightward.</summary>
    LaunchRight,

    /// <summary>Stops the acid's movement as its splash plays.</summary>
    StopMoving,

    /// <summary>Deletes the acid.</summary>
    Delete,
}
