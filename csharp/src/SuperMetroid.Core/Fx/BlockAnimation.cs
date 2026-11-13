namespace SuperMetroid.Core.Fx;

/// <summary>The block animations rooms, room effects and the Tourian statues animate their pictures with.
/// Its values are arbitrary; verification's view names each member's original identity (#1415).
/// </summary>
public enum BlockAnimation
{
    /// <summary>No block animation is loaded.</summary>
    None = 0,
    /// <summary>The Phantoon statue in the Tourian statue room.</summary>
    TourianStatuePhantoon = 1,
    /// <summary>The Ridley statue in the Tourian statue room.</summary>
    TourianStatueRidley,
    /// <summary>The Kraid statue in the Tourian statue room.</summary>
    TourianStatueKraid,
    /// <summary>The Draygon statue in the Tourian statue room.</summary>
    TourianStatueDraygon,
    /// <summary>Horizontal spikes, gated by the common area flag set's first flag.</summary>
    HorizontalSpikes,
    /// <summary>Vertical spikes, gated by the common area flag set's second flag.</summary>
    VerticalSpikes,
    /// <summary>The Crateria lake, gated by a Crateria area flag.</summary>
    CrateriaLake,
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
    /// Its five-frame loop animates the liquid's pictures.
    /// </summary>
    Lava,

    /// <summary>
    /// The acid animation, spawned by the acid room effect.
    /// Its five-frame loop animates the liquid's pictures.
    /// </summary>
    Acid,

    /// <summary>
    /// The rain animation, spawned by the rain room effect.
    /// Its five-frame loop animates the rain's pictures.
    /// </summary>
    Rain,

    /// <summary>
    /// The spore animation, spawned by the spore room effect.
    /// Three ten-tick frames animate the spores' pictures.
    /// </summary>
    Spores,
}
