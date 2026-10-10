namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A5 functions the Draygon eye runs from its enemy variable A.</summary>
internal enum DraygonEyeFunction : ushort
{
    /// <summary><c>RTS_A5804B</c> at $A5:804B: the eye is inert.</summary>
    Inert = 0x804b,

    /// <summary><c>Function_DraygonEye_FacingLeft</c> at $A5:C48D.</summary>
    FacingLeft = 0xc48d,

    /// <summary><c>Function_DraygonEye_FacingRight</c> at $A5:C513.</summary>
    FacingRight = 0xc513,
}

/// <summary>Draygon's private bank-$A5 instruction opcodes.</summary>
internal enum DraygonInstruction : ushort
{
    /// <summary><c>Instruction_Draygon_SetInstList_Body_Eye_Tail_Arms</c> at $A5:94DD.</summary>
    Draygon_SetInstList_Body_Eye_Tail_Arms = 0x94dd,

    /// <summary><c>Instruction_Draygon_RoomLoadingInterruptCmd_BeginHUDDraw</c> at $A5:9895.</summary>
    Draygon_RoomLoadingInterruptCmd_BeginHUDDraw = 0x9895,

    /// <summary><c>Instruction_Draygon_RoomLoadingInterruptCmd_BeginHUDDraw_dup</c> at $A5:9C8A.</summary>
    Draygon_RoomLoadingInterruptCmd_BeginHUDDrawDuplicate = 0x9c8a,

    /// <summary><c>Instruction_Draygon_EyeFunctionInY</c> at $A5:C47B.</summary>
    Draygon_EyeFunctionInY = 0xc47b,

    /// <summary><c>Instruction_Draygon_FunctionInY</c> at $A5:9736.</summary>
    Draygon_FunctionInY = 0x9736,

    /// <summary><c>Instruction_DraygonBody_DisplaceGraphics</c> at $A5:9E0A.</summary>
    DraygonBody_DisplaceGraphics = 0x9e0a,

    /// <summary><c>Instruction_Draygon_QueueSFXInY_Lib2_Max6</c> at $A5:9F60.</summary>
    Draygon_QueueSFXInY_Lib2_Max6 = 0x9f60,

    /// <summary><c>Instruction_Draygon_QueueSFXInY_Lib3_Max6</c> at $A5:9F6E.</summary>
    Draygon_QueueSFXInY_Lib3_Max6 = 0x9f6e,

    /// <summary><c>Inst_Draygon_SpawnDyingDraygonSpriteObject_BigDustCloud</c> at $A5:973F.</summary>
    Draygon_SpawnDyingDraygonSpriteObject_BigDustCloud = 0x973f,

    /// <summary><c>Inst_Draygon_SpawnDyingDraygonSpriteObject_SmallExplosion</c> at $A5:9752.</summary>
    Draygon_SpawnDyingDraygonSpriteObject_SmallExplosion = 0x9752,

    /// <summary><c>Inst_Draygon_SpawnDyingDraygonSpriteObject_BigExplosion</c> at $A5:9765.</summary>
    Draygon_SpawnDyingDraygonSpriteObject_BigExplosion = 0x9765,

    /// <summary><c>Inst_Draygon_SpawnDyingDraygonSpriteObject_BreathBubbles</c> at $A5:9778.</summary>
    Draygon_SpawnDyingDraygonSpriteObject_BreathBubbles = 0x9778,

    /// <summary><c>Instruction_Draygon_ParalyseDraygonTailAndArms</c> at $A5:98D3.</summary>
    Draygon_ParalyseDraygonTailAndArms = 0x98d3,

    /// <summary><c>Instruction_DraygonBody_SetAsIntangible</c> at $A5:98EF.</summary>
    DraygonBody_SetAsIntangible = 0x98ef,

    /// <summary><c>Instruction_Draygon_BodyFunctionInY</c> at $A5:9F57.</summary>
    Draygon_BodyFunctionInY = 0x9f57,

    /// <summary><c>Instruction_DraygonTail_TailWhipHit</c> at $A5:9B9A.</summary>
    DraygonTail_TailWhipHit = 0x9b9a,

    /// <summary><c>Instruction_Draygon_SpawnGoop_Leftwards</c> at $A5:9F7C.</summary>
    Draygon_SpawnGoop_Leftwards = 0x9f7c,

    /// <summary><c>Instruction_Draygon_SpawnGoop_Rightwards</c> at $A5:9FAE.</summary>
    Draygon_SpawnGoop_Rightwards = 0x9fae,

}
