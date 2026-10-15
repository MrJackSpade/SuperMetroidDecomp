namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Gameplay cues of the hand beam's program.</summary>
internal enum MotherBrainHandBeamCue
{
    /// <summary><c>Spawn_MotherBrainRedBeam_Fired</c> ($86:C7FB): spawns the next fired beam along the aim.</summary>
    SpawnNextBeam,
    /// <summary>Deletes the beam after its third stage.</summary>
    Delete,
}
