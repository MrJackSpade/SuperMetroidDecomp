namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// The installed sheets of sprite characters Mother Brain's encounter copies over her room's load as it plays out (#1406,
/// #1538): the import composes her frames' finished pictures from them, and gameplay never names them.
/// </summary>
public enum MotherBrainSpriteSheet
{
    /// <summary>Her phase-two legs.</summary>
    Legs,

    /// <summary>The Baby Metroid that drains her.</summary>
    BabyMetroid,

    /// <summary>Her attack sprites, which her legs cover and her rise and the Baby's cutscene restore.</summary>
    Attack,

    /// <summary>The escape door's explosion.</summary>
    ExplodedDoor,

    /// <summary>Her corpse.</summary>
    Corpse,

    /// <summary>The escape timer's digits.</summary>
    EscapeTimer,

    /// <summary>The escape message's letters.</summary>
    EscapeText,
}
