namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The thirty-one title-sequence OBJ compositions: each title card's reveal frames, the Super Metroid logo and the
/// Nintendo copyright. Each reveal shows one more letter than the frame before it, except the penultimate Metroid 3
/// frame, which adds the space before the numeral. Values are arbitrary; installed title artwork is keyed by these
/// names, and only the extraction pipeline knows which cartridge spritemap each one came from.
/// </summary>
public enum TitleSprite
{
    /// <summary>The year card with one letter shown.</summary>
    YearReveal1,
    /// <summary>The year card with two letters shown.</summary>
    YearReveal2,
    /// <summary>The year card with three letters shown.</summary>
    YearReveal3,
    /// <summary>The complete year card.</summary>
    YearReveal4,
    /// <summary>The Nintendo card with one letter shown.</summary>
    NintendoReveal1,
    /// <summary>The Nintendo card with two letters shown.</summary>
    NintendoReveal2,
    /// <summary>The Nintendo card with three letters shown.</summary>
    NintendoReveal3,
    /// <summary>The Nintendo card with four letters shown.</summary>
    NintendoReveal4,
    /// <summary>The Nintendo card with five letters shown.</summary>
    NintendoReveal5,
    /// <summary>The Nintendo card with six letters shown.</summary>
    NintendoReveal6,
    /// <summary>The Nintendo card with seven letters shown.</summary>
    NintendoReveal7,
    /// <summary>The complete Nintendo card.</summary>
    NintendoReveal8,
    /// <summary>The Presents card with one letter shown.</summary>
    PresentsReveal1,
    /// <summary>The Presents card with two letters shown.</summary>
    PresentsReveal2,
    /// <summary>The Presents card with three letters shown.</summary>
    PresentsReveal3,
    /// <summary>The Presents card with four letters shown.</summary>
    PresentsReveal4,
    /// <summary>The Presents card with five letters shown.</summary>
    PresentsReveal5,
    /// <summary>The Presents card with six letters shown.</summary>
    PresentsReveal6,
    /// <summary>The Presents card with seven letters shown.</summary>
    PresentsReveal7,
    /// <summary>The complete Presents card.</summary>
    PresentsReveal8,
    /// <summary>The Metroid 3 card with one letter shown.</summary>
    MetroidThreeReveal1,
    /// <summary>The Metroid 3 card with two letters shown.</summary>
    MetroidThreeReveal2,
    /// <summary>The Metroid 3 card with three letters shown.</summary>
    MetroidThreeReveal3,
    /// <summary>The Metroid 3 card with four letters shown.</summary>
    MetroidThreeReveal4,
    /// <summary>The Metroid 3 card with five letters shown.</summary>
    MetroidThreeReveal5,
    /// <summary>The Metroid 3 card with six letters shown.</summary>
    MetroidThreeReveal6,
    /// <summary>The Metroid 3 card with all seven letters shown.</summary>
    MetroidThreeReveal7,
    /// <summary>The Metroid 3 card with its letters and the space before the numeral.</summary>
    MetroidThreeReveal8,
    /// <summary>The complete Metroid 3 card, including its numeral.</summary>
    MetroidThreeReveal9,
    /// <summary>The Super Metroid title logo held on the title screen.</summary>
    SuperMetroidLogo,
    /// <summary>The Nintendo copyright line drawn beneath the title logo.</summary>
    NintendoCopyright,
}
