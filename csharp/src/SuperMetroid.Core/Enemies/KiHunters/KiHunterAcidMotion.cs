namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// How Ki-Hunter acid moves itself (#1843): one launch pass in its direction, then its fall.
/// </summary>
internal enum KiHunterAcidMotion
{
    /// <summary>Shifts the acid out of the mouth leftward for one pass, then sets it falling.</summary>
    LaunchingLeft,

    /// <summary>Shifts the acid out of the mouth rightward for one pass, then sets it falling.</summary>
    LaunchingRight,

    /// <summary>Flies Ki-Hunter acid under gravity until it strikes the room.</summary>
    Falling,
}
