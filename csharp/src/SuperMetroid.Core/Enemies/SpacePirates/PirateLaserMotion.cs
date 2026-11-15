namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// How a Space Pirate's or Mother Brain's laser moves itself once fired (#1843).
/// </summary>
internal enum PirateLaserMotion
{
    /// <summary>Flies a Space Pirate or Mother Brain laser left until it leaves the camera.</summary>
    FlyingLeft,

    /// <summary>Flies a Space Pirate or Mother Brain laser right until it leaves the camera.</summary>
    FlyingRight,
}
