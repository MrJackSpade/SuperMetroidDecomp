namespace SuperMetroid.Core.Game;

/// <summary>
/// Verified special-Samus-palette handler indices. Values one through six remain unnamed
/// until their producers are translated; this ordinary enum can still carry them losslessly.
/// </summary>
public enum SamusSpecialPaletteType : ushort
{
    None = 0,
    CrystalFlash = 7,
    Xray = 8,
}

/// <summary>
/// Verified room layer-blending configuration indices consulted by translated rendering.
/// The two backdrop entries are kept distinct by native value without claiming an
/// unverified visual distinction between them.
/// </summary>
public enum LayerBlendingConfiguration : ushort
{
    PowerBombOnly = 0x0000,
    NormalGameplay = 0x0002,
    PhantoonHidden = 0x0004,
    UnusedSemiTransparentSprites = 0x0006,
    WreckedShipPowerOff = 0x0008,
    Spores = 0x000a,
    Fireflea = 0x000c,
    Rain = 0x000e,
    MorphBallEye = 0x0010,
    SuitPickup = 0x0012,
    WaterSubtractive = 0x0014,
    WaterfallSubtractive = 0x0016,
    LiquidOrFogAdditive = 0x0018,
    PhantoonSemiTransparent = 0x001a,
    UnusedHalfAdditiveReversedBackgrounds = 0x001c,
    LavaAcidAdditive = 0x001e,
    NormalGameplayAlternate = 0x0020,
    UnusedWaterSubtractive = 0x0022,
    MotherBrainWindow = 0x0024,
    UnusedHalfAdditive = 0x0026,
    VisorBackdrop28 = 0x0028,
    VisorBackdrop2A = 0x002a,
    HazeOrTorizo = 0x002c,
    UnusedSubtractive = 0x002e,
    FogAdditive = 0x0030,
    UnusedSubtractiveBackground = 0x0032,
    MotherBrainPhaseTwo = 0x0034,
}

/// <summary>Conventionally named values in the native room scroll-zone byte grid.</summary>
public enum RoomScrollState : byte
{
    RedBoundary = 0,
    Blue = 1,
    Green = 2,
}

public static class RenderingDiscriminantExtensions
{
    /// <summary>
    /// True only for the two native backdrop-color-math configurations that animate
    /// Samus's visor. Other raw configuration indices remain unnamed and return false.
    /// </summary>
    public static bool AnimatesVisor(this LayerBlendingConfiguration configuration) =>
        configuration is
            LayerBlendingConfiguration.VisorBackdrop28 or
            LayerBlendingConfiguration.VisorBackdrop2A;
}
