namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a cue of the Rinkas' programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum RinkaCueKind
{
    /// <summary>Makes the Rinka visible and tangible and starts its aim delay (<c>Instruction_Rinka_FireRinka</c>, $A2:B9C7).</summary>
    Fire,
    /// <summary>Makes the Rinka intangible and invisible (<c>Instruction_Rinka_SetAsIntangibleAndInvisible</c>, $A2:B9B3).</summary>
    Hide,
    /// <summary>Makes the Rinka intangible and invisible and keeps it processing off screen (<c>Instruction_Rinka_SetAsIntangibleInvisibleAndActiveOffScreen</c>, $A2:B9BD).</summary>
    HideAndProcessOffScreen,
}
