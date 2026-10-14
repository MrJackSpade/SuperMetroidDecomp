namespace SuperMetroid.Core.Game;

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
