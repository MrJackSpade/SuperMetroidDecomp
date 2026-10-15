namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Native bank-$8D commands executed by palette-FX instruction streams.
/// Values are the cartridge pointers so debugger state matches the retail disassembly.
/// </summary>
public enum PaletteFxInstruction : ushort
{
    /// <summary><c>Instruction_PaletteFx_Wait</c> at $8D:C595.</summary>
    Wait = 0xc595,
    /// <summary><c>Instruction_PaletteFx_ColorPlus2</c> at $8D:C599.</summary>
    ColorPlus2 = 0xc599,
    /// <summary><c>Instruction_PaletteFx_ColorPlus3</c> at $8D:C5A2.</summary>
    ColorPlus3 = 0xc5a2,
    /// <summary><c>Instruction_PaletteFx_ColorPlus4</c> at $8D:C5AB.</summary>
    ColorPlus4 = 0xc5ab,
    /// <summary><c>Instruction_PaletteFx_ColorPlus8</c> at $8D:C5B4.</summary>
    ColorPlus8 = 0xc5b4,
    /// <summary><c>Instruction_PaletteFx_ColorPlus9</c> at $8D:C5BD.</summary>
    ColorPlus9 = 0xc5bd,
    /// <summary><c>Instruction_PaletteFx_ColorPlus15</c> at $8D:C5C6.</summary>
    ColorPlus15 = 0xc5c6,
    /// <summary><c>Instruction_PaletteFx_Delete</c> at $8D:C5CF.</summary>
    Delete = 0xc5cf,
    /// <summary><c>Instruction_PaletteFx_SetPreInstruction</c> at $8D:C5D4.</summary>
    SetPreInstruction = 0xc5d4,
    /// <summary><c>Instruction_PaletteFx_ClearPreInstruction</c> at $8D:C5DD.</summary>
    ClearPreInstruction = 0xc5dd,
    /// <summary><c>Instruction_PaletteFx_Goto</c> at $8D:C61E.</summary>
    Goto = 0xc61e,
    /// <summary><c>Instruction_PaletteFx_DecrementTimerAndGoto</c> at $8D:C639.</summary>
    DecrementTimerAndGoto = 0xc639,
    /// <summary><c>Instruction_PaletteFx_SetTimer</c> at $8D:C648.</summary>
    SetTimer = 0xc648,
    /// <summary><c>Instruction_PaletteFx_SetColorIndex</c> at $8D:C655.</summary>
    SetColorIndex = 0xc655,
    /// <summary><c>Instruction_PaletteFx_QueueMusic</c> at $8D:C65E.</summary>
    QueueMusic = 0xc65e,
    /// <summary><c>Instruction_PaletteFx_QueueSfx1</c> at $8D:C66A.</summary>
    QueueSfx1 = 0xc66a,
    /// <summary><c>Instruction_PaletteFx_QueueSfx2</c> at $8D:C673.</summary>
    QueueSfx2 = 0xc673,
    /// <summary><c>Instruction_PaletteFx_QueueSfx3</c> at $8D:C67C.</summary>
    QueueSfx3 = 0xc67c,
    /// <summary><c>Instruction_PaletteFx_SetPaletteFxIndex</c> at $8D:F1C6.</summary>
    SetPaletteFxIndex = 0xf1c6,
}
