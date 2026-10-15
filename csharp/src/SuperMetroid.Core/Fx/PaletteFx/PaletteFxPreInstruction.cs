namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Native per-frame callbacks installed by palette-FX streams.</summary>
public enum PaletteFxPreInstruction : ushort
{
    /// <summary><c>PaletteFxPreInstruction_Null</c> at $8D:C526.</summary>
    Null = 0xc526,
    /// <summary><c>PaletteFxPreInstruction_Cleared</c> at $8D:C5E3.</summary>
    Cleared = 0xc5e3,
    /// <summary><c>PaletteFxPreInstruction_Intro</c> at $8D:E20B.</summary>
    Intro = 0xe20b,
    /// <summary><c>PaletteFxPreInstruction_DeleteWhenEnemyZeroDies</c> at $8D:E2E0.</summary>
    DeleteWhenEnemyZeroDies = 0xe2e0,
    /// <summary><c>PaletteFxPreInstruction_Heat</c> at $8D:E379.</summary>
    Heat = 0xe379,
    /// <summary><c>PaletteFxPreInstruction_SwitchAboveY380</c> at $8D:EC59.</summary>
    SwitchAboveY380 = 0xec59,
    /// <summary><c>PaletteFxPreInstruction_SwitchAboveY380Second</c> at $8D:ED84.</summary>
    SwitchAboveY380Second = 0xed84,
    /// <summary><c>PaletteFxPreInstruction_DeleteWhenAreaMiniBossDies</c> at $8D:EEC5.</summary>
    DeleteWhenAreaMiniBossDies = 0xeec5,
    /// <summary><c>PaletteFxPreInstruction_InspectAdjacentSlot</c> at $8D:F621.</summary>
    InspectAdjacentSlot = 0xf621,
}
