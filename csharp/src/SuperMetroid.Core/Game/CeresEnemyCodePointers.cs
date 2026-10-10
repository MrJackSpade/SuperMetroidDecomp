namespace SuperMetroid.Core.Game;

/// <summary>The Ceres Baby Metroid's bank-$A6 behavior function, kept in Ridley's extended workspace.</summary>
public enum CeresBabyFunction : ushort
{
    /// <summary>Outside Ceres the Baby workspace is never initialized and holds zero.</summary>
    None = 0,
    /// <summary><c>UpdateBabyMetroidPosition_CarriedInArms</c> at $A6:BE9C.</summary>
    CarriedInArms = 0xbe9c,
    /// <summary><c>UpdateBabyMetroidPosition_CarriedInFeet</c> at $A6:BEB3.</summary>
    CarriedInFeet = 0xbeb3,
    /// <summary><c>DropBabyMetroid</c> at $A6:BECA.</summary>
    Drop = 0xbeca,
    /// <summary><c>BabyMetroidDropped</c> at $A6:BEDC.</summary>
    Dropped = 0xbedc,
    /// <summary><c>RTS_A6BF19</c> at $A6:BF19: the idle function after landing.</summary>
    Idle = 0xbf19,
}

/// <summary>The Ceres Baby Metroid cutscene animation-list instructions in bank $A6.</summary>
internal enum CeresBabyInstruction : ushort
{
    /// <summary><c>Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX</c> at $A6:BFC9.</summary>
    PlayCrySfxOrGoto = 0xbfc9,
    /// <summary><c>Instruction_BabyMetroidCutscene_UpdateColors</c> at $A6:BFE1.</summary>
    UpdateColors = 0xbfe1,
    /// <summary><c>Instruction_BabyMetroidCutscene_GotoXIfNotFalling</c> at $A6:BFF2.</summary>
    GotoIfNotFalling = 0xbff2,
    /// <summary><c>Instruction_BabyMetroidCutscene_GotoX</c> at $A6:BFF8.</summary>
    Goto = 0xbff8,
}

/// <summary>The Ceres steam's private bank-$A6 animation instructions.</summary>
internal enum CeresSteamInstruction : ushort
{
    /// <summary><c>Instruction_CeresSteam_SetToIntangibleAndInvisible</c> at $A6:F11D.</summary>
    SetToIntangibleAndInvisible = 0xf11d,
    /// <summary><c>Instruction_CeresSteam_DecActivationTimer_Decide_GotoYOrY2</c> at $A6:F127.</summary>
    DecrementActivationTimerGotoYOrY2 = 0xf127,
    /// <summary><c>Instruction_CeresSteam_SetToTangibleAndVisible</c> at $A6:F135.</summary>
    SetToTangibleAndVisible = 0xf135,
}

/// <summary>The Ceres door's private bank-$A6 animation instructions.</summary>
internal enum CeresDoorInstruction : ushort
{
    /// <summary><c>Inst_CeresDoor_GotoYIfSamusIsNotWithing30Pixels</c> at $A6:F63E.</summary>
    GotoYIfSamusIsDistant = 0xf63e,
    /// <summary><c>Instruction_CeresDoor_GotoYIfAreaBossIsAlive</c> at $A6:F66A.</summary>
    GotoYIfAreaBossIsAlive = 0xf66a,
    /// <summary><c>Instruction_CeresDoor_GotoYIfCeresRidleyHasNotEscaped</c> at $A6:F678.</summary>
    GotoYIfCeresRidleyHasNotEscaped = 0xf678,
    /// <summary><c>Instruction_CeresDoor_SetAsIntangible</c> at $A6:F68B.</summary>
    SetAsIntangible = 0xf68b,
    /// <summary><c>Instruction_CeresDoor_SetAsTangible</c> at $A6:F695.</summary>
    SetAsTangible = 0xf695,
    /// <summary><c>Instruction_CeresDoor_SetDrawnByRidleyFlag</c> at $A6:F69F.</summary>
    SetDrawnByRidleyFlag = 0xf69f,
    /// <summary><c>Instruction_CeresDoor_SetAsInvisible</c> at $A6:F6A6.</summary>
    SetAsInvisible = 0xf6a6,
    /// <summary><c>Instruction_CeresDoor_SetAsVisible_ClearDrawnByRidleyFlag</c> at $A6:F6B0.</summary>
    SetAsVisibleClearDrawnByRidleyFlag = 0xf6b0,
    /// <summary><c>Instruction_CeresDoor_SetAsVisible</c> at $A6:F6B3.</summary>
    SetAsVisible = 0xf6b3,
    /// <summary><c>Instruction_CeresDoor_QueueOpeningSFX</c> at $A6:F6BD.</summary>
    QueueOpeningSFX = 0xf6bd,
}

/// <summary>Sound identifiers queued by <see cref="CeresDoorInstruction"/> routines.</summary>
internal static class CeresDoorInstructionSounds
{
    /// <summary>Library-three door-opening sound $2C queued by <see cref="CeresDoorInstruction.QueueOpeningSFX"/>.</summary>
    internal const ushort Opening = 0x002c;
}
