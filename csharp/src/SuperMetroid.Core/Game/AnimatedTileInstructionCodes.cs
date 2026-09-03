namespace SuperMetroid.Core.Game;

/// <summary>Proven retail commands in bank-$87 animated-tile instruction streams.</summary>
public static class AnimatedTileInstructionCodes
{
    /// <summary><c>Instruction_AnimatedTilesObject_Delete</c> at $87:80B2.</summary>
    public const ushort Delete = 0x80b2;
    /// <summary><c>Instruction_AnimatedTilesObject_GotoY</c> at $87:80B7.</summary>
    public const ushort Goto = 0x80b7;
    /// <summary><c>Instruction_AnimatedTilesObject_GotoYIfEventYSet</c> at $87:813F.</summary>
    public const ushort GotoIfEventSet = 0x813f;
    /// <summary><c>Instruction_AnimatedTilesObject_SetEventY</c> at $87:8150.</summary>
    public const ushort SetEvent = 0x8150;
    /// <summary><c>Instruction_AnimatedTilesObject_WaitUntilAreaBossIsDead</c> at $87:81BA.</summary>
    public const ushort WaitUntilAreaBossIsDead = 0x81ba;
    /// <summary><c>Instruction_AnimTilesObject_GotoYIfAnyBossBitsYSetForAreaY</c> at $87:8303.</summary>
    public const ushort GotoIfAnyBossBitsSetForArea = 0x8303;
    /// <summary><c>Instruction_AnimTilesObject_SpawnTourianStatueEyeGlowParamY</c> at $87:8320.</summary>
    public const ushort SpawnTourianStatueEyeGlow = 0x8320;
    /// <summary><c>Instruction_AnimTilesObject_SpawnTourianStatuesSoulParamY</c> at $87:832F.</summary>
    public const ushort SpawnTourianStatueSoul = 0x832f;
    /// <summary><c>Instruction_AnimatedTilesObject_GotoYIfTourianStatueBusy</c> at $87:833E.</summary>
    public const ushort GotoIfTourianStatueBusy = 0x833e;
    /// <summary><c>Instruction_AnimatedTilesObject_TourianStatueSetAnimStateY</c> at $87:8349.</summary>
    public const ushort SetTourianStatueAnimationState = 0x8349;
    /// <summary><c>Instruction_AnimatedTilesObject_TourianStatueResetAnimStateY</c> at $87:8352.</summary>
    public const ushort ResetTourianStatueAnimationState = 0x8352;
    /// <summary><c>Instruction_AnimatedTilesObject_Clear3ColorsOfPaletteData</c> at $87:835B.</summary>
    public const ushort ClearThreePaletteColors = 0x835b;
    /// <summary><c>Instruction_AnimatedTilesObject_SpawnPaletteFXObjectInY</c> at $87:8372.</summary>
    public const ushort SpawnPaletteFxObject = 0x8372;
    /// <summary><c>Instruction_AnimatedTilesObject_Write8ColorsOfTargetPaletteD</c> at $87:837F.</summary>
    public const ushort WriteEightTargetPaletteColors = 0x837f;
}

/// <summary>Bank-$87 object headers selected directly by translated room setup code.</summary>
public static class AnimatedTileObjectPointers
{
    /// <summary>Wrecked Ship rightward treadmill object at $87:8275.</summary>
    public const ushort WreckedShipTreadmillRightwards = 0x8275;
    /// <summary>Wrecked Ship leftward treadmill object at $87:827B.</summary>
    public const ushort WreckedShipTreadmillLeftwards = 0x827b;
}

/// <summary>Named entry points within the translated Wrecked Ship instruction streams.</summary>
public static class AnimatedTileInstructionListPointers
{
    /// <summary>Rightward treadmill boss-wait entry at $87:81E1.</summary>
    public const ushort WreckedShipTreadmillRightwardsWait = 0x81e1;
    /// <summary>Rightward treadmill four-frame loop at $87:81E3.</summary>
    public const ushort WreckedShipTreadmillRightwardsLoop = 0x81e3;
    /// <summary>Leftward treadmill boss-wait entry at $87:81F7.</summary>
    public const ushort WreckedShipTreadmillLeftwardsWait = 0x81f7;
    /// <summary>Leftward treadmill four-frame loop at $87:81F9.</summary>
    public const ushort WreckedShipTreadmillLeftwardsLoop = 0x81f9;
}
