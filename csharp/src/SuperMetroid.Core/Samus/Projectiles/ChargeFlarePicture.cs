namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A picture a charge-flare component's animation shows (#1747, #2324): one of the main flare's four, from a small ember to
/// a starburst, or one of a spark ring's six, from its widest to its narrowest as the sparks draw in to the muzzle, which
/// the draw shows in the composition of Samus's facing. Its values are arbitrary.
/// </summary>
public enum ChargeFlarePicture
{
    /// <summary>The main flare as a small orange ember.</summary>
    Ember,
    /// <summary>The main flare as a small yellow glow.</summary>
    Glow,
    /// <summary>The main flare as a white-hot ball in an orange rim.</summary>
    WhiteBall,
    /// <summary>The main flare as a white-hot starburst with rays.</summary>
    Starburst,
    /// <summary>A spark ring at its widest, its sparks faint amber.</summary>
    SparksWidest,
    /// <summary>A spark ring drawn in from its widest, its sparks yellow.</summary>
    SparksWider,
    /// <summary>A spark ring drawn in to halfway, its sparks yellow.</summary>
    SparksWide,
    /// <summary>A spark ring drawn in past halfway, its sparks pale.</summary>
    SparksNarrow,
    /// <summary>A spark ring drawn in close to the muzzle, its sparks pale.</summary>
    SparksNarrower,
    /// <summary>A spark ring at its narrowest, at the muzzle, its sparks pale.</summary>
    SparksNarrowest,
}
