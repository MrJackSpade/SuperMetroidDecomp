using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>CGRAM ranges written by the Ceres and Norfair Ridley palette routines (#630).</summary>
[PpuLayoutBoundary("catalog: Ridley palettes")]
public static class RidleyCgramLayout
{
    /// <summary>CGADD $A0-$BF (native byte offset $0140): the door/container and initial Baby OBJ palettes 2-3 from $A6:E16F.</summary>
    public static readonly PpuLayoutCell CeresStart = PpuMemoryLayout.Colors(nameof(CeresStart),
        CeresRidleyPaletteRomData.StartCgramIndex, CeresRidleyPaletteRomData.StartColorCount, "Bgr555 OBJ palettes 2-3");

    /// <summary>CGADD $FC-$FE: Ceres Ridley's three eye colours, OBJ palette 7 colours $C-$E ($A6:E2AA rows).</summary>
    public static readonly PpuLayoutCell CeresEyeFade = PpuMemoryLayout.Colors(nameof(CeresEyeFade),
        CeresRidleyPaletteRomData.EyeFadeCgramIndex, CeresRidleyPaletteRomData.EyeFadeColorCount, "Bgr555 eye shade");

    /// <summary>CGADD $91-$9B (byte offset $0122): OBJ palette 1 colours 1-$B receiving the $A6:E30A body fade.</summary>
    public static readonly PpuLayoutCell CeresBodyFadeShadow = PpuMemoryLayout.Colors(nameof(CeresBodyFadeShadow),
        CeresRidleyPaletteRomData.BodyFadeBgCgramIndex, CeresRidleyPaletteRomData.BodyFadeColorCount, "Bgr555 body fade");

    /// <summary>CGADD $F1-$FB (byte offset $01E2): OBJ palette 7 colours 1-$B receiving the $A6:E30A body fade.</summary>
    public static readonly PpuLayoutCell CeresBodyFade = PpuMemoryLayout.Colors(nameof(CeresBodyFade),
        CeresRidleyPaletteRomData.BodyFadeObjCgramIndex, CeresRidleyPaletteRomData.BodyFadeColorCount, "Bgr555 body fade");

    /// <summary>CGADD $F1-$FE (byte offset $01E2): OBJ palette 7 colours 1-$E, the shared $A6:E46A health shade.</summary>
    public static readonly PpuLayoutCell Health = PpuMemoryLayout.Colors(nameof(Health),
        CeresRidleyPaletteRomData.HealthCgramIndex, CeresRidleyPaletteRomData.HealthColorCount, "Bgr555 health shade");

    /// <summary>CGADD $61-$63 (byte offset $00C2): BG palette 6 colours 1-3, the $A6:C1DF EMERGENCY-text cycle.</summary>
    public static readonly PpuLayoutCell CeresAlarm = PpuMemoryLayout.Colors(nameof(CeresAlarm),
        CeresRidleyPaletteRomData.AlarmCgramIndex, CeresRidleyPaletteRomData.AlarmColorCount, "Bgr555 alarm text");

    /// <summary>CGADD $51-$5F (byte offset $00A2): BG palette 5 colours 1-$F, the $A6:A9E3 getaway backdrop.</summary>
    public static readonly PpuLayoutCell CeresRetreatBackground = PpuMemoryLayout.Colors(nameof(CeresRetreatBackground),
        CeresRidleyPaletteRomData.RetreatBgCgramIndex, CeresRidleyPaletteRomData.RetreatBgColorCount, "Bgr555 getaway backdrop");

    /// <summary>CGADD $21-$28 (byte offset $0042): BG palette 2 colours 1-8 receiving the $A6:AA01 shared retreat shade.</summary>
    public static readonly PpuLayoutCell CeresRetreatSharedBackground = PpuMemoryLayout.Colors(nameof(CeresRetreatSharedBackground),
        CeresRidleyPaletteRomData.RetreatSharedBgCgramIndex, CeresRidleyPaletteRomData.RetreatSharedColorCount, "Bgr555 retreat shade");

    /// <summary>CGADD $F1-$F8 (byte offset $01E2): OBJ palette 7 colours 1-8 receiving the $A6:AA01 shared retreat shade.</summary>
    public static readonly PpuLayoutCell CeresRetreatSharedObject = PpuMemoryLayout.Colors(nameof(CeresRetreatSharedObject),
        CeresRidleyPaletteRomData.RetreatSharedObjCgramIndex, CeresRidleyPaletteRomData.RetreatSharedColorCount, "Bgr555 retreat shade");

    /// <summary>CGADD $B1-$BF (byte offset $0162): OBJ palette 3 colours 1-$F, the Ceres Baby shade written by $A6:BFE1.</summary>
    public static readonly PpuLayoutCell CeresBaby = PpuMemoryLayout.Colors(nameof(CeresBaby),
        CeresRidleyPaletteRomData.BabyCgramIndex, CeresRidleyPaletteRomData.BabyColorCount, "Bgr555 Baby shade");

    /// <summary>CGADD $51-$5F (byte offset $00A2): BG palette 5 colours 1-$F, the Mode 7 zoom shade written by $A6:B0EF.</summary>
    public static readonly PpuLayoutCell CeresMode7Zoom = PpuMemoryLayout.Colors(nameof(CeresMode7Zoom),
        CeresRidleyPaletteRomData.Mode7ZoomCgramIndex, CeresRidleyPaletteRomData.Mode7ZoomColorCount, "Bgr555 zoom shade");

    /// <summary>CGADD $A0-$BF (byte offset $0140): Norfair Ridley's initial OBJ palettes 2-3.</summary>
    public static readonly PpuLayoutCell NorfairInitial = PpuMemoryLayout.Colors(nameof(NorfairInitial),
        NorfairRidleyPaletteRomData.InitialCgramIndex, NorfairRidleyPaletteRomData.InitialColorCount, "Bgr555 OBJ palettes 2-3");

    /// <summary>CGADD $71-$7E (byte offset $00E2): BG palette 7 colours 1-$E, the Norfair reveal fade.</summary>
    public static readonly PpuLayoutCell NorfairReveal = PpuMemoryLayout.Colors(nameof(NorfairReveal),
        NorfairRidleyPaletteRomData.RevealCgramIndex, NorfairRidleyPaletteRomData.RevealColorCount, "Bgr555 reveal fade");
}
