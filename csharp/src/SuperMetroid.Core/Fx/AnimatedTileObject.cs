namespace SuperMetroid.Core.Fx;

/// <summary>The animated-tile objects rooms, room effects and the Tourian statues animate their characters with.
/// Its values are arbitrary; verification's view names each member's original identity (#1415).
/// </summary>
public enum AnimatedTileObject
{
    /// <summary>No animated-tile object is loaded.</summary>
    None = 0,
    /// <summary>The Phantoon statue in the Tourian statue room.</summary>
    TourianStatuePhantoon = 1,
    /// <summary>The Ridley statue in the Tourian statue room.</summary>
    TourianStatueRidley,
    /// <summary>The Kraid statue in the Tourian statue room.</summary>
    TourianStatueKraid,
    /// <summary>The Draygon statue in the Tourian statue room.</summary>
    TourianStatueDraygon,
    /// <summary>The empty object an unused area bit selects.</summary>
    Empty,
    /// <summary>Horizontal spikes, gated by the common area flag set's first flag.</summary>
    HorizontalSpikes,
    /// <summary>Vertical spikes, gated by the common area flag set's second flag.</summary>
    VerticalSpikes,
    /// <summary>The Crateria lake, gated by a Crateria area flag.</summary>
    CrateriaLake,
    /// <summary>The unused Crateria lava object, preserved for its Crateria area flag.</summary>
    UnusedCrateriaLava,
    /// <summary>The Brinstar plant, gated by a Brinstar area flag.</summary>
    BrinstarPlant,
    /// <summary>The Wrecked Ship screen, gated by a Wrecked Ship area flag.</summary>
    WreckedShipScreen,
    /// <summary>The Maridia sand ceiling, selected by room FX animation bits.</summary>
    MaridiaSandCeiling,
    /// <summary>Falling Maridia sand, selected by room FX animation bits.</summary>
    MaridiaSandFalling,
    /// <summary>Wrecked Ship rightward treadmill object.</summary>
    WreckedShipTreadmillRightwards,
    /// <summary>Wrecked Ship leftward treadmill object.</summary>
    WreckedShipTreadmillLeftwards,

    /// <summary>
    /// The lava animation, spawned by the lava room effect.
    /// Its five-frame loop repaints the liquid's characters.
    /// </summary>
    Lava,

    /// <summary>
    /// The acid animation, spawned by the acid room effect.
    /// Its five-frame loop repaints the liquid's characters.
    /// </summary>
    Acid,

    /// <summary>
    /// The rain animation, spawned by the rain room effect.
    /// Its five-frame loop repaints the rain's characters.
    /// </summary>
    Rain,

    /// <summary>
    /// The spore animation, spawned by the spore room effect.
    /// Three ten-tick frames repaint the spores' characters.
    /// </summary>
    Spores,
}
