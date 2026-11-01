namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The gameplay cues Phantoon's part programs publish: the routines their lists call through <c>Instruction_CommonA7_CallFunctionInY</c>.</summary>
internal enum PhantoonPartCue
{
    /// <summary>Queues the next materialization sound (<c>PlayPhantoonMaterializationSFX</c>).</summary>
    PlayMaterializationSound,
    /// <summary>Gives the body its eye hitbox and starts the eye tracking Samus (<c>SetupEyeOpenPhantoonState</c>).</summary>
    BeginEyeTracking,
    /// <summary>Picks the next round's flame and swoop pattern (<c>PickNewPhantoonPattern</c>).</summary>
    PickNewPattern,
    /// <summary>Spawns a casual flame from the mouth (<c>SpawnCasualFlame</c>).</summary>
    SpawnCasualFlame,
}