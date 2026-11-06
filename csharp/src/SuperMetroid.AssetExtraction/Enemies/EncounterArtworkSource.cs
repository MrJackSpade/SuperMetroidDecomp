namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// What last put a sprite character's artwork in place while an encounter's artwork sequence plays (#1513): the room's
/// load, or the pages a step copies. A frame holds its own artwork only where every character it reads was last put in
/// place by one of the sources its encounter draws it from; a character another source wrote over shows that source's
/// artwork, which is not the frame's.
/// </summary>
/// <remarks>The values are arbitrary.</remarks>
internal enum EncounterArtworkSource
{
    /// <summary>The room's load, which leaves the characters every frame shows before any step.</summary>
    RoomLoad,

    /// <summary>A step of an encounter whose every page holds the artwork of the frames it draws.</summary>
    EncounterPages,

    /// <summary>A page of Mother Brain's phase-two legs.</summary>
    MotherBrainLegs,

    /// <summary>A page of the Baby Metroid that drains her.</summary>
    MotherBrainBabyMetroid,

    /// <summary>A page of her attack sprites: her dead brain's faces, her onion rings and her bombs.</summary>
    MotherBrainAttack,

    /// <summary>A page of the escape door's explosion.</summary>
    MotherBrainExplodedDoor,

    /// <summary>A page of her corpse.</summary>
    MotherBrainCorpse,

    /// <summary>A page of the escape timer's digits.</summary>
    EscapeTimer,

    /// <summary>A page of the escape message's letters.</summary>
    EscapeText,
}
