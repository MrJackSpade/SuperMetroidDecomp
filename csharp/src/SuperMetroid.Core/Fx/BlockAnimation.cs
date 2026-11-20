namespace SuperMetroid.Core.Fx;

/// <summary>The block animations rooms, room effects and the Tourian statues animate their pictures with.
/// Its values are arbitrary: nothing reads them.
/// </summary>
public enum BlockAnimation
{
    /// <summary>No block animation is loaded.</summary>
    None,
    /// <summary>The Phantoon statue in the Tourian statue room.</summary>
    TourianStatuePhantoon,
    /// <summary>The Ridley statue in the Tourian statue room.</summary>
    TourianStatueRidley,
    /// <summary>The Kraid statue in the Tourian statue room.</summary>
    TourianStatueKraid,
    /// <summary>The Draygon statue in the Tourian statue room.</summary>
    TourianStatueDraygon,
    /// <summary>Horizontal spikes, which a room's effect can animate in every area.</summary>
    HorizontalSpikes,
    /// <summary>The Maridia sand ceiling, which only a Maridia room's effect animates.</summary>
    MaridiaSandCeiling,
    /// <summary>Falling Maridia sand, which only a Maridia room's effect animates.</summary>
    MaridiaSandFalling,
    /// <summary>The Wrecked Ship's rightward treadmill, which only a Wrecked Ship room's effect animates.</summary>
    WreckedShipTreadmillRightwards,
    /// <summary>The Wrecked Ship's leftward treadmill, which only a Wrecked Ship room's effect animates.</summary>
    WreckedShipTreadmillLeftwards,

    /// <summary>
    /// The lava animation, spawned by the lava room effect.
    /// Its loop animates the liquid's pictures.
    /// </summary>
    Lava,

    /// <summary>
    /// The acid animation, spawned by the acid room effect.
    /// Its loop animates the liquid's pictures.
    /// </summary>
    Acid,

    /// <summary>
    /// The rain animation, spawned by the rain room effect.
    /// Its loop animates the rain's pictures.
    /// </summary>
    Rain,

    /// <summary>
    /// The spore animation, spawned by the spore room effect.
    /// Its loop animates the spores' pictures.
    /// </summary>
    Spores,
}
