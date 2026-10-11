namespace SuperMetroid.Core.Game;

/// <summary>
/// Verified special-Samus-palette handler indices. Values one through six remain unnamed
/// until their producers are translated; this ordinary enum can still carry them losslessly.
/// </summary>
public enum SamusSpecialPaletteType : ushort
{
    /// <summary>No special palette handler owns Samus's suit colors.</summary>
    None = 0,

    /// <summary>Crystal Flash owns the special-palette timer and suit-color sequence.</summary>
    CrystalFlash = 7,

    /// <summary>X-ray mode owns the special-palette timer and animated visor colors.</summary>
    Xray = 8,
}

/// <summary>
/// Verified room layer-blending configuration indices consulted by translated rendering.
/// The two backdrop entries are kept distinct by native value without claiming an
/// unverified visual distinction between them.
/// </summary>
public enum LayerBlendingConfiguration : ushort
{
    /// <summary>
    /// <c>LayerBlending_Config</c> zero: <c>LayerBlending_Handler</c> branches past initialization
    /// and the configuration table (<c>$88:8008</c> <c>BEQ</c>), leaving the layer registers as set.
    /// </summary>
    Unconfigured = 0x0000,

    /// <summary>Bank-$88 dispatcher offset <c>$02</c>, used by ordinary gameplay without a special room effect.</summary>
    NormalGameplay = 0x0002,

    /// <summary>Dispatcher offset <c>$04</c>, which omits Phantoon's body before its fade-in.</summary>
    PhantoonHidden = 0x0004,

    /// <summary>Dispatcher offset <c>$06</c>, an unused configuration with semi-transparent sprites.</summary>
    UnusedSemiTransparentSprites = 0x0006,

    /// <summary>Dispatcher offset <c>$08</c>, Wrecked Ship with its power off.</summary>
    WreckedShipPowerOff = 0x0008,

    /// <summary>Dispatcher offset <c>$0A</c>, which composites the drifting-spore BG3 atmosphere.</summary>
    Spores = 0x000a,

    /// <summary>Dispatcher offset <c>$0C</c>, which applies the Fireflea room's darkness and light effects.</summary>
    Fireflea = 0x000c,

    /// <summary>Dispatcher offset <c>$0E</c>, which adds the full-screen rain BG3 plane.</summary>
    Rain = 0x000e,

    /// <summary>Dispatcher offset <c>$10</c>, the Morph Ball eye room.</summary>
    MorphBallEye = 0x0010,

    /// <summary>Dispatcher offset <c>$12</c>, the suit-pickup rooms.</summary>
    SuitPickup = 0x0012,

    /// <summary>Dispatcher offset <c>$14</c>, which subtracts the water BG3 plane from the gameplay scene.</summary>
    WaterSubtractive = 0x0014,

    /// <summary>Dispatcher offset <c>$16</c>, the subtractive water variant whose window setup supports a waterfall.</summary>
    WaterfallSubtractive = 0x0016,

    /// <summary>Dispatcher offset <c>$18</c>, which adds a liquid or fog BG3 plane to the gameplay scene.</summary>
    LiquidOrFogAdditive = 0x0018,

    /// <summary>Dispatcher offset <c>$1A</c>, which presents Phantoon through the configured translucent composition.</summary>
    PhantoonSemiTransparent = 0x001a,

    /// <summary>Dispatcher offset <c>$1C</c>, an unused half-additive configuration with reversed backgrounds.</summary>
    UnusedHalfAdditiveReversedBackgrounds = 0x001c,

    /// <summary>Dispatcher offset <c>$1E</c>, which adds the lava or acid BG3 plane and fixed-color effect.</summary>
    LavaAcidAdditive = 0x001e,

    /// <summary>Dispatcher offset <c>$20</c>, an alternate normal-gameplay configuration.</summary>
    NormalGameplayAlternate = 0x0020,

    /// <summary>Dispatcher offset <c>$22</c>, an unused subtractive water configuration.</summary>
    UnusedWaterSubtractive = 0x0022,

    /// <summary>Dispatcher offset <c>$24</c>, Mother Brain's window configuration.</summary>
    MotherBrainWindow = 0x0024,

    /// <summary>Dispatcher offset <c>$26</c>, an unused half-additive configuration.</summary>
    UnusedHalfAdditive = 0x0026,

    /// <summary>Dispatcher offset <c>$28</c>, one of two native backdrop modes that advances the visor palette animation.</summary>
    VisorBackdrop28 = 0x0028,

    /// <summary>Dispatcher offset <c>$2A</c>, the second native backdrop mode that advances the visor palette animation.</summary>
    VisorBackdrop2A = 0x002a,

    /// <summary>Dispatcher offset <c>$2C</c>, haze rooms and the Torizo rooms.</summary>
    HazeOrTorizo = 0x002c,

    /// <summary>Dispatcher offset <c>$2E</c>, an unused subtractive configuration.</summary>
    UnusedSubtractive = 0x002e,

    /// <summary>Dispatcher offset <c>$30</c>, which adds the full-screen fog BG3 plane.</summary>
    FogAdditive = 0x0030,

    /// <summary>Dispatcher offset <c>$32</c>, an unused subtractive background configuration.</summary>
    UnusedSubtractiveBackground = 0x0032,

    /// <summary>Dispatcher offset <c>$34</c>, installed during Mother Brain's second-phase room transformation.</summary>
    MotherBrainPhaseTwo = 0x0034,
}

/// <summary>Conventionally named values in the native room scroll-zone byte grid.</summary>
public enum RoomScrollState : byte
{
    /// <summary>An impassable camera boundary; camera motion may not enter this screen cell.</summary>
    RedBoundary = 0,

    /// <summary>A traversable screen cell using the cartridge's blue vertical-alignment behavior.</summary>
    Blue = 1,

    /// <summary>A traversable screen cell using the alternate green boundary behavior installed by room scripts.</summary>
    Green = 2,
}

/// <summary>Semantic queries for native rendering dispatcher values.</summary>
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
