namespace SuperMetroid.Core.Game;

/// <summary>Proven retail commands in bank-$87 animated-tile instruction streams.</summary>
public enum AnimatedTileInstruction : ushort
{
    /// <summary><c>Instruction_AnimatedTilesObject_Delete</c> at $87:80B2.</summary>
    Delete = 0x80b2,
    /// <summary><c>Instruction_AnimatedTilesObject_GotoY</c> at $87:80B7.</summary>
    Goto = 0x80b7,
    /// <summary><c>Instruction_AnimatedTilesObject_GotoYIfEventYSet</c> at $87:813F.</summary>
    GotoIfEventSet = 0x813f,
    /// <summary><c>Instruction_AnimatedTilesObject_SetEventY</c> at $87:8150.</summary>
    SetEvent = 0x8150,
    /// <summary><c>Instruction_AnimatedTilesObject_WaitUntilAreaBossIsDead</c> at $87:81BA.</summary>
    WaitUntilAreaBossIsDead = 0x81ba,
    /// <summary><c>Instruction_AnimTilesObject_GotoYIfAnyBossBitsYSetForAreaY</c> at $87:8303.</summary>
    GotoIfAnyBossBitsSetForArea = 0x8303,
    /// <summary><c>Instruction_AnimTilesObject_SpawnTourianStatueEyeGlowParamY</c> at $87:8320.</summary>
    SpawnTourianStatueEyeGlow = 0x8320,
    /// <summary><c>Instruction_AnimTilesObject_SpawnTourianStatuesSoulParamY</c> at $87:832F.</summary>
    SpawnTourianStatueSoul = 0x832f,
    /// <summary><c>Instruction_AnimatedTilesObject_GotoYIfTourianStatueBusy</c> at $87:833E.</summary>
    GotoIfTourianStatueBusy = 0x833e,
    /// <summary><c>Instruction_AnimatedTilesObject_TourianStatueSetAnimStateY</c> at $87:8349.</summary>
    SetTourianStatueAnimationState = 0x8349,
    /// <summary><c>Instruction_AnimatedTilesObject_TourianStatueResetAnimStateY</c> at $87:8352.</summary>
    ResetTourianStatueAnimationState = 0x8352,
    /// <summary><c>Instruction_AnimatedTilesObject_Clear3ColorsOfPaletteData</c> at $87:835B.</summary>
    ClearThreePaletteColors = 0x835b,
    /// <summary><c>Instruction_AnimatedTilesObject_SpawnPaletteFXObjectInY</c> at $87:8372.</summary>
    SpawnPaletteFxObject = 0x8372,
    /// <summary><c>Instruction_AnimatedTilesObject_Write8ColorsOfTargetPaletteD</c> at $87:837F.</summary>
    WriteEightTargetPaletteColors = 0x837f,
}
