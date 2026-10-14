namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>What a cue of the Hibashi's programs does: one per bank-$A6 instruction its lists use.</summary>
internal enum HibashiCueKind
{
    /// <summary>Sets the collision part's Y offset and half-height for the eruption frame in Value (<c>Instruction_Hibashi_ActivityFrame0</c> through <c>Instruction_Hibashi_ActivityFrame15</c>, $A6:8E13-$A6:8FBD; the routine's position is the frame).</summary>
    ActivityFrame,
    /// <summary>Ends the eruption and hands the collision part back to waiting (<c>FinishActivity</c>, $A6:8FD1).</summary>
    FinishActivity,
    /// <summary>Plays the eruption sound (<c>PlaySFX</c>, $A6:8DAF).</summary>
    PlayEruptionSound,
}
