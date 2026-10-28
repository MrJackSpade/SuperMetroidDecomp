namespace SuperMetroid.Core.Fx;

/// <summary>
/// The layer-blending configuration a room-FX record or an effect selects. Its values are arbitrary; the import decodes
/// the record's dispatcher byte (#1415).
/// </summary>
public enum LayerBlendingConfiguration
{
    /// <summary>
    /// <c>LayerBlending_Config</c> zero: <c>LayerBlending_Handler</c> branches past initialization
    /// and the configuration table (<c>$88:8008</c> <c>BEQ</c>), leaving the layer registers as set.
    /// </summary>
    Unconfigured = 0,

    /// <summary>Bank-$88 dispatcher offset <c>$02</c>, used by ordinary gameplay without a special room effect.</summary>
    NormalGameplay = 1,

    /// <summary>Dispatcher offset <c>$04</c>, which omits Phantoon's body before its fade-in.</summary>
    PhantoonHidden,

    /// <summary>Dispatcher offset <c>$06</c>, an unused configuration with semi-transparent sprites.</summary>
    UnusedSemiTransparentSprites,

    /// <summary>Dispatcher offset <c>$08</c>, Wrecked Ship with its power off.</summary>
    WreckedShipPowerOff,

    /// <summary>Dispatcher offset <c>$0A</c>, which composites the drifting-spore BG3 atmosphere.</summary>
    Spores,

    /// <summary>Dispatcher offset <c>$0C</c>, which applies the Fireflea room's darkness and light effects.</summary>
    Fireflea,

    /// <summary>Dispatcher offset <c>$0E</c>, which adds the full-screen rain BG3 plane.</summary>
    Rain,

    /// <summary>Dispatcher offset <c>$10</c>, the Morph Ball eye room.</summary>
    MorphBallEye,

    /// <summary>Dispatcher offset <c>$12</c>, the suit-pickup rooms.</summary>
    SuitPickup,

    /// <summary>Dispatcher offset <c>$14</c>, which subtracts the water BG3 plane from the gameplay scene.</summary>
    WaterSubtractive,

    /// <summary>Dispatcher offset <c>$16</c>, the subtractive water variant whose window setup supports a waterfall.</summary>
    WaterfallSubtractive,

    /// <summary>Dispatcher offset <c>$18</c>, which adds a liquid or fog BG3 plane to the gameplay scene.</summary>
    LiquidOrFogAdditive,

    /// <summary>Dispatcher offset <c>$1A</c>, which presents Phantoon through the configured translucent composition.</summary>
    PhantoonSemiTransparent,

    /// <summary>Dispatcher offset <c>$1C</c>, an unused half-additive configuration with reversed backgrounds.</summary>
    UnusedHalfAdditiveReversedBackgrounds,

    /// <summary>Dispatcher offset <c>$1E</c>, which adds the lava or acid BG3 plane and fixed-color effect.</summary>
    LavaAcidAdditive,

    /// <summary>Dispatcher offset <c>$20</c>, an alternate normal-gameplay configuration.</summary>
    NormalGameplayAlternate,

    /// <summary>Dispatcher offset <c>$22</c>, an unused subtractive water configuration.</summary>
    UnusedWaterSubtractive,

    /// <summary>Dispatcher offset <c>$24</c>, Mother Brain's window configuration.</summary>
    MotherBrainWindow,

    /// <summary>Dispatcher offset <c>$26</c>, an unused half-additive configuration.</summary>
    UnusedHalfAdditive,

    /// <summary>Dispatcher offset <c>$28</c>, one of two native backdrop modes that advances the visor palette animation.</summary>
    VisorBackdrop28,

    /// <summary>Dispatcher offset <c>$2A</c>, the second native backdrop mode that advances the visor palette animation.</summary>
    VisorBackdrop2A,

    /// <summary>Dispatcher offset <c>$2C</c>, haze rooms and the Torizo rooms.</summary>
    HazeOrTorizo,

    /// <summary>Dispatcher offset <c>$2E</c>, an unused subtractive configuration.</summary>
    UnusedSubtractive,

    /// <summary>Dispatcher offset <c>$30</c>, which adds the full-screen fog BG3 plane.</summary>
    FogAdditive,

    /// <summary>Dispatcher offset <c>$32</c>, an unused subtractive background configuration.</summary>
    UnusedSubtractiveBackground,

    /// <summary>Dispatcher offset <c>$34</c>, installed during Mother Brain's second-phase room transformation.</summary>
    MotherBrainPhaseTwo,
}
