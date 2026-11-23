namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A picture a spark ring of the charge flare shows (#1747, #2324, #2553), from its widest to its narrowest as the sparks
/// draw in to the muzzle, in the composition of Samus's facing. Its values are arbitrary.
/// </summary>
public enum SparkPicture
{
    /// <summary>The ring at its widest, its sparks faint amber.</summary>
    Widest,
    /// <summary>The ring drawn in from its widest, its sparks yellow.</summary>
    Wider,
    /// <summary>The ring drawn in to halfway, its sparks yellow.</summary>
    Wide,
    /// <summary>The ring drawn in past halfway, its sparks pale.</summary>
    Narrow,
    /// <summary>The ring drawn in close to the muzzle, its sparks pale.</summary>
    Narrower,
    /// <summary>The ring at its narrowest, at the muzzle, its sparks pale.</summary>
    Narrowest,
}
