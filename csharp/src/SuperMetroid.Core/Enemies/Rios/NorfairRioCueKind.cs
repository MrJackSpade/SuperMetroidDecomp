namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>What a cue of the Norfair Rio's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum NorfairRioCueKind
{
    /// <summary>Places the flames level with the Norfair Rio (<c>Instruction_Geruta_SetFlamesYOffset_0</c>, $A2:C20B).</summary>
    FlamesAt0,
    /// <summary>Places the flames 12 pixels below the Norfair Rio (<c>Instruction_Geruta_SetFlamesYOffset_C</c>, $A2:C1EA).</summary>
    FlamesAt12,
    /// <summary>Places the flames 4 pixels below the Norfair Rio (<c>Instruction_Geruta_SetFlamesYOffset_4</c>, $A2:C200).</summary>
    FlamesAt4,
    /// <summary>Places the flames 8 pixels below the Norfair Rio (<c>Instruction_Geruta_SetFlamesYOffset_8</c>, $A2:C1D4).</summary>
    FlamesAt8,
    /// <summary>Places the flames 12 pixels above the Norfair Rio (<c>Instruction_Geruta_SetFlamesYOffset_negativeC</c>, $A2:C1F5).</summary>
    FlamesAtMinus12,
    /// <summary>Places the flames 16 pixels above the Norfair Rio (<c>Instruction_Geruta_SetFlamesYOffset_negative10</c>, $A2:C22C).</summary>
    FlamesAtMinus16,
    /// <summary>Places the flames 4 pixels above the Norfair Rio (<c>Instruction_Geruta_SetFlamesYOffset_negative4</c>, $A2:C216).</summary>
    FlamesAtMinus4,
    /// <summary>Tells main AI the swoop's start animation has finished (<c>Instruction_Geruta_SetFinishedSwoopStartAnimationFlag</c>, $A2:C1C9).</summary>
    SwoopStartFinished,
}
