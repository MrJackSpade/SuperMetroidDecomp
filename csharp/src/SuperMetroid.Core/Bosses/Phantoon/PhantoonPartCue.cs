namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The gameplay cues Phantoon's part programs publish: the routines their native lists call through <c>Instruction_CommonA7_CallFunctionInY</c>.</summary>
internal enum PhantoonPartCue
{
    /// <summary>Queues the next materialization sound (<c>PlayPhantoonMaterializationSFX</c>, $A7:CEED).</summary>
    PlayMaterializationSound,
    /// <summary>Gives the body its eye hitbox and starts the eye tracking Samus (<c>SetupEyeOpenPhantoonState</c>, $A7:D03F).</summary>
    BeginEyeTracking,
    /// <summary>Picks the next round's flame and swoop pattern (<c>PickNewPhantoonPattern</c>, $A7:D076).</summary>
    PickNewPattern,
    /// <summary>Spawns a casual flame from the mouth (<c>SpawnCasualFlame</c>, $A7:CF5E).</summary>
    SpawnCasualFlame,
}