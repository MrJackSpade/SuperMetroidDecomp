namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The cues Ridley's fireball and afterburn animations publish.</summary>
internal enum RidleyFireballCue
{
    /// <summary>Stops the projectile's motion.</summary>
    ClearFrameBehavior,
    /// <summary>Starts the fireball's flight: installs its moving frame behaviour, as the animation's flight cue does.</summary>
    BeginFlight,
    /// <summary>Emits the right and left afterburns.</summary>
    SpawnHorizontalAfterburns,
    /// <summary>Emits the up and down afterburns.</summary>
    SpawnVerticalAfterburns,
    /// <summary>Extends the afterburn chain.</summary>
    SpawnNextAfterburn,
    /// <summary>Deletes the projectile.</summary>
    Delete,
}
