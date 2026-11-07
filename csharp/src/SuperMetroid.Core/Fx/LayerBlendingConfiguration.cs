namespace SuperMetroid.Core.Fx;

/// <summary>
/// The layer-blending configuration a room-FX record or an effect selects. Its values are arbitrary; the import decodes
/// the record's dispatcher byte (#1415).
/// </summary>
public enum LayerBlendingConfiguration
{
    /// <summary>
    /// No configuration: the layer-blending handler branches past initialization
    /// and the configuration table, leaving the layer settings as they are.
    /// </summary>
    Unconfigured = 0,

    /// <summary>The configuration used by ordinary gameplay without a special room effect.</summary>
    NormalGameplay = 1,

    /// <summary>The configuration that omits Phantoon's body before its fade-in.</summary>
    PhantoonHidden,

    /// <summary>An unused configuration with semi-transparent sprites.</summary>
    UnusedSemiTransparentSprites,

    /// <summary>Wrecked Ship with its power off.</summary>
    WreckedShipPowerOff,

    /// <summary>The configuration that composites the drifting-spore effect overlay atmosphere.</summary>
    Spores,

    /// <summary>The configuration that applies the Fireflea room's darkness and light effects.</summary>
    Fireflea,

    /// <summary>The configuration that adds the full-screen rain effect overlay plane.</summary>
    Rain,

    /// <summary>The Morph Ball eye room.</summary>
    MorphBallEye,

    /// <summary>The suit-pickup rooms.</summary>
    SuitPickup,

    /// <summary>The configuration that subtracts the water effect overlay plane from the gameplay scene.</summary>
    WaterSubtractive,

    /// <summary>The subtractive water variant whose window setup supports a waterfall.</summary>
    WaterfallSubtractive,

    /// <summary>The configuration that adds a liquid or fog effect overlay plane to the gameplay scene.</summary>
    LiquidOrFogAdditive,

    /// <summary>The configuration that presents Phantoon through the configured translucent composition.</summary>
    PhantoonSemiTransparent,

    /// <summary>An unused half-additive configuration with reversed backgrounds.</summary>
    UnusedHalfAdditiveReversedBackgrounds,

    /// <summary>The configuration that adds the lava or acid effect overlay plane and fixed-color effect.</summary>
    LavaAcidAdditive,

    /// <summary>An alternate normal-gameplay configuration.</summary>
    NormalGameplayAlternate,

    /// <summary>An unused subtractive water configuration.</summary>
    UnusedWaterSubtractive,

    /// <summary>Mother Brain's window configuration.</summary>
    MotherBrainWindow,

    /// <summary>An unused half-additive configuration.</summary>
    UnusedHalfAdditive,

    /// <summary>The first of two backdrop modes that advance the visor palette animation.</summary>
    VisorBackdrop28,

    /// <summary>The second of two backdrop modes that advance the visor palette animation.</summary>
    VisorBackdrop2A,

    /// <summary>Haze rooms and the Torizo rooms.</summary>
    HazeOrTorizo,

    /// <summary>An unused subtractive configuration.</summary>
    UnusedSubtractive,

    /// <summary>The configuration that adds the full-screen fog effect overlay plane.</summary>
    FogAdditive,

    /// <summary>An unused subtractive background configuration.</summary>
    UnusedSubtractiveBackground,

    /// <summary>The configuration installed during Mother Brain's second-phase room transformation.</summary>
    MotherBrainPhaseTwo,
}
