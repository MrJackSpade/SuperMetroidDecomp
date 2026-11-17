namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter's acid-spit animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="KiHunterAcidSpitAnimations"/>.
/// </summary>
internal enum KiHunterAcidSpitSegment
{
    /// <summary>
    /// Acid spat left: a droplet stretching to a streak and a blob that spatters as it is launched, then a ball.
    /// </summary>
    Left,
    /// <summary>
    /// The acid splashing where it hit the floor: a mound spreading wide and flat and fading before it vanishes.
    /// </summary>
    HitFloor,
    /// <summary>
    /// Acid spat right: a droplet stretching to a streak and a blob that spatters as it is launched, then a ball.
    /// </summary>
    Right,
}
