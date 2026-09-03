namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$8D commands executed by palette-FX instruction streams.
/// Values remain cartridge pointers so debugger state matches the retail disassembly.
/// </summary>
public static class PaletteFxInstructionCodes
{
    /// <summary><c>Instruction_PaletteFx_Wait</c> at $8D:C595.</summary>
    public const ushort Wait = 0xc595;
    /// <summary><c>Instruction_PaletteFx_ColorPlus2</c> at $8D:C599.</summary>
    public const ushort ColorPlus2 = 0xc599;
    /// <summary><c>Instruction_PaletteFx_ColorPlus3</c> at $8D:C5A2.</summary>
    public const ushort ColorPlus3 = 0xc5a2;
    /// <summary><c>Instruction_PaletteFx_ColorPlus4</c> at $8D:C5AB.</summary>
    public const ushort ColorPlus4 = 0xc5ab;
    /// <summary><c>Instruction_PaletteFx_ColorPlus8</c> at $8D:C5B4.</summary>
    public const ushort ColorPlus8 = 0xc5b4;
    /// <summary><c>Instruction_PaletteFx_ColorPlus9</c> at $8D:C5BD.</summary>
    public const ushort ColorPlus9 = 0xc5bd;
    /// <summary><c>Instruction_PaletteFx_ColorPlus15</c> at $8D:C5C6.</summary>
    public const ushort ColorPlus15 = 0xc5c6;
    /// <summary><c>Instruction_PaletteFx_Delete</c> at $8D:C5CF.</summary>
    public const ushort Delete = 0xc5cf;
    /// <summary><c>Instruction_PaletteFx_SetPreInstruction</c> at $8D:C5D4.</summary>
    public const ushort SetPreInstruction = 0xc5d4;
    /// <summary><c>Instruction_PaletteFx_ClearPreInstruction</c> at $8D:C5DD.</summary>
    public const ushort ClearPreInstruction = 0xc5dd;
    /// <summary><c>Instruction_PaletteFx_Goto</c> at $8D:C61E.</summary>
    public const ushort Goto = 0xc61e;
    /// <summary><c>Instruction_PaletteFx_DecrementTimerAndGoto</c> at $8D:C639.</summary>
    public const ushort DecrementTimerAndGoto = 0xc639;
    /// <summary><c>Instruction_PaletteFx_SetTimer</c> at $8D:C648.</summary>
    public const ushort SetTimer = 0xc648;
    /// <summary><c>Instruction_PaletteFx_SetColorIndex</c> at $8D:C655.</summary>
    public const ushort SetColorIndex = 0xc655;
    /// <summary><c>Instruction_PaletteFx_QueueMusic</c> at $8D:C65E.</summary>
    public const ushort QueueMusic = 0xc65e;
    /// <summary><c>Instruction_PaletteFx_QueueSfx1</c> at $8D:C66A.</summary>
    public const ushort QueueSfx1 = 0xc66a;
    /// <summary><c>Instruction_PaletteFx_QueueSfx2</c> at $8D:C673.</summary>
    public const ushort QueueSfx2 = 0xc673;
    /// <summary><c>Instruction_PaletteFx_QueueSfx3</c> at $8D:C67C.</summary>
    public const ushort QueueSfx3 = 0xc67c;
    /// <summary><c>Instruction_PaletteFx_SetPaletteFxIndex</c> at $8D:F1C6.</summary>
    public const ushort SetPaletteFxIndex = 0xf1c6;
}

/// <summary>Native setup callbacks stored in bank-$8D palette-FX definitions.</summary>
public static class PaletteFxSetupCodes
{
    /// <summary><c>PaletteFxSetup_Null</c> at $8D:C685.</summary>
    public const ushort Null = 0xc685;
    /// <summary><c>PaletteFxSetup_Intro</c> at $8D:E204.</summary>
    public const ushort Intro = 0xe204;
    /// <summary><c>PaletteFxSetup_Norfair</c> at $8D:E440.</summary>
    public const ushort Norfair = 0xe440;
    /// <summary><c>PaletteFxSetup_Brinstar</c> at $8D:F730.</summary>
    public const ushort Brinstar = 0xf730;
}

/// <summary>Native per-frame callbacks installed by palette-FX streams.</summary>
public static class PaletteFxPreInstructionCodes
{
    /// <summary><c>PaletteFxPreInstruction_Null</c> at $8D:C526.</summary>
    public const ushort Null = 0xc526;
    /// <summary><c>PaletteFxPreInstruction_Cleared</c> at $8D:C5E3.</summary>
    public const ushort Cleared = 0xc5e3;
    /// <summary><c>PaletteFxPreInstruction_Intro</c> at $8D:E20B.</summary>
    public const ushort Intro = 0xe20b;
    /// <summary><c>PaletteFxPreInstruction_DeleteWhenEnemyZeroDies</c> at $8D:E2E0.</summary>
    public const ushort DeleteWhenEnemyZeroDies = 0xe2e0;
    /// <summary><c>PaletteFxPreInstruction_Heat</c> at $8D:E379.</summary>
    public const ushort Heat = 0xe379;
    /// <summary><c>PaletteFxPreInstruction_SwitchAboveY380</c> at $8D:EC59.</summary>
    public const ushort SwitchAboveY380 = 0xec59;
    /// <summary><c>PaletteFxPreInstruction_SwitchAboveY380Second</c> at $8D:ED84.</summary>
    public const ushort SwitchAboveY380Second = 0xed84;
    /// <summary><c>PaletteFxPreInstruction_DeleteWhenAreaMiniBossDies</c> at $8D:EEC5.</summary>
    public const ushort DeleteWhenAreaMiniBossDies = 0xeec5;
    /// <summary><c>PaletteFxPreInstruction_InspectAdjacentSlot</c> at $8D:F621.</summary>
    public const ushort InspectAdjacentSlot = 0xf621;
}

/// <summary>Instruction-list entry points selected directly by palette-FX callbacks.</summary>
public static class PaletteFxInstructionListPointers
{
    /// <summary>Power-suit Norfair heat list at $8D:E45E.</summary>
    public const ushort NorfairPowerSuit = 0xe45e;
    /// <summary>Varia-suit Norfair heat list at $8D:E68A.</summary>
    public const ushort NorfairVariaSuit = 0xe68a;
    /// <summary>Gravity-suit Norfair heat list at $8D:E8B6.</summary>
    public const ushort NorfairGravitySuit = 0xe8b6;
    /// <summary>First vertical-switch continuation list at $8D:EB43.</summary>
    public const ushort AboveY380 = 0xeb43;
    /// <summary>Second vertical-switch continuation list at $8D:EC76.</summary>
    public const ushort AboveY380Second = 0xec76;
}
