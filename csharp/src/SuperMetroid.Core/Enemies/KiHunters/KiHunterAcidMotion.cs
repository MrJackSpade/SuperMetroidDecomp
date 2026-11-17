namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// How Ki-Hunter acid moves itself (#1843): one launch pass the way it was spat, then its fall. The acid's state keeps
/// the way (#2338).
/// </summary>
internal enum KiHunterAcidMotion
{
    /// <summary>Shifts the acid out of the mouth the way it was spat for one pass, then sets it falling.</summary>
    Launching,

    /// <summary>Flies Ki-Hunter acid under gravity until it strikes the room.</summary>
    Falling,
}
