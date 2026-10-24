namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The sheets Mother Brain's encounter shows its sprites pages of after the room loads, in the order its frames' pictures
/// name them.
/// </summary>
public enum MotherBrainSpritePageSheet
{
    /// <summary>Her phase-two legs, shown page by page while she rises from the fake death.</summary>
    Legs,

    /// <summary>The Baby Metroid, shown page by page as it arrives to drain her.</summary>
    BabyMetroid,

    /// <summary>Her attack sprites, shown again after the leg pages and after the Baby Metroid's cutscene.</summary>
    Attack,

    /// <summary>The escape door's explosion, shown as the escape starts.</summary>
    ExplodedDoor,

    /// <summary>Her corpse, shown page by page once her head has fallen.</summary>
    Corpse,

    /// <summary>The escape timer's digits, shown before the escape message.</summary>
    EscapeTimer,

    /// <summary>The escape message's letters.</summary>
    EscapeText,
}
