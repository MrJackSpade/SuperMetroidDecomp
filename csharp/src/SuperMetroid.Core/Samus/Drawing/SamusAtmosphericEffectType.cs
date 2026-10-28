namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The graphics type of one shared atmospheric slot. The slot's packed frame/type word still stores the type number,
/// so the members keep the numbers that word holds; <see cref="Inactive"/> is the cleared slot.
/// </summary>
public enum SamusAtmosphericEffectType
{
    /// <summary>The slot holds no effect.</summary>
    Inactive = 0,
    /// <summary>Footstep splash drawn from the footstep small-sprite list.</summary>
    FootstepSplash = 1,
    /// <summary>Footstep splash whose sprite list is absent, so it draws nothing.</summary>
    AlternateFootstepSplash = 2,
    /// <summary>Diving splash pinned to the liquid surface and drawn through Samus's spritemaps.</summary>
    DivingSplash = 3,
    /// <summary>Lava-surface spray drifting up and outward.</summary>
    LavaSurfaceDamage = 4,
    /// <summary>Breath bubbles drawn through Samus's spritemaps.</summary>
    Bubbles = 5,
    /// <summary>Rising dust with lengthening frame holds.</summary>
    Dust = 6,
    /// <summary>Second rising-dust type sharing the dust cadence and sprites.</summary>
    AlternateDust = 7,
}
