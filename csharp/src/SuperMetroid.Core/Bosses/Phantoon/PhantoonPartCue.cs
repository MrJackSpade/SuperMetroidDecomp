namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The gameplay cues Phantoon's part animations publish: the behaviours their animations call.</summary>
internal enum PhantoonPartCue
{
    /// <summary>Queues the next materialization sound.</summary>
    PlayMaterializationSound,
    /// <summary>Gives the body its eye hitbox and starts the eye tracking Samus.</summary>
    BeginEyeTracking,
    /// <summary>Picks the next round's flame and swoop pattern.</summary>
    PickNewPattern,
    /// <summary>Spawns a casual flame from the mouth.</summary>
    SpawnCasualFlame,
}