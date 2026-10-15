namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Gameplay cues published by the head animation.</summary>
internal enum BotwoonHeadCue
{
    /// <summary>Sets the head's collision radius for its facing (<c>Instruction_Botwoon_EnemyRadius_*</c>).</summary>
    ApplyFacingRadius,
    /// <summary>Queues the spit sound (<c>Instruction_Botwoon_QueueSpitSFX</c>).</summary>
    QueueSpitSound,
    /// <summary>Marks the frame the spit volley leaves the mouth (<c>Instruction_Botwoon_SetSpittingFlag</c>).</summary>
    ReachSpitFrame,
}
