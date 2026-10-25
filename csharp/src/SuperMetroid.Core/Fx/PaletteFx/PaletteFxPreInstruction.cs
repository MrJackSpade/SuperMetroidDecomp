namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The per-frame condition a palette effect checks before its program runs.
/// Its values are arbitrary; verification's native view names each member's cartridge word (#1415).
/// </summary>
public enum PaletteFxPreInstruction : ushort
{
    /// <summary><c>PaletteFxPreInstruction_Null</c> at $8D:C526.</summary>
    Null = 1,
    /// <summary><c>PaletteFxPreInstruction_Cleared</c> at $8D:C5E3.</summary>
    Cleared,
    /// <summary><c>PaletteFxPreInstruction_Intro</c> at $8D:E20B.</summary>
    Intro,
    /// <summary><c>PaletteFxPreInstruction_DeleteWhenEnemyZeroDies</c> at $8D:E2E0.</summary>
    DeleteWhenEnemyZeroDies,
    /// <summary><c>PaletteFxPreInstruction_Heat</c> at $8D:E379.</summary>
    Heat,
    /// <summary><c>PaletteFxPreInstruction_SwitchAboveY380</c> at $8D:EC59.</summary>
    SwitchAboveY380,
    /// <summary><c>PaletteFxPreInstruction_SwitchAboveY380Second</c> at $8D:ED84.</summary>
    SwitchAboveY380Second,
    /// <summary><c>PaletteFxPreInstruction_DeleteWhenAreaMiniBossDies</c> at $8D:EEC5.</summary>
    DeleteWhenAreaMiniBossDies,
    /// <summary><c>PaletteFxPreInstruction_InspectAdjacentSlot</c> at $8D:F621.</summary>
    InspectAdjacentSlot,
}
