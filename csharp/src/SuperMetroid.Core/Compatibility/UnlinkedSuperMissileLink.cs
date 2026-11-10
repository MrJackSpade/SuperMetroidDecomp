namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The place a Super Missile names as its collision link before it has one (#1590): Samus's first shot place. A Super
/// Missile that ignites with no free place for its link keeps naming the first place, so once ignited it moves whatever
/// projectile holds the first place as its link, and deletes it when it explodes at a low speed; one that an enemy
/// reflects before it ignites deletes the projectile holding the first place, unless that is the Super Missile itself.
/// The link is itself a Super Missile, and names the first place too.
/// </summary>
/// <remarks>
/// Consulted when a Super Missile is fired and when its link is created, which name this place; read by the link's
/// movement after ignition and by an enemy's reflection. A Super Missile that ignites with a free place names its link's
/// place instead, and a reflection leaves it naming none until it re-ignites. Evidence: the original Super Missile setup
/// and its link's setup clear the link word, whose zero indexes the first shot place, and the link movement and the
/// reflection index the shots by it without testing it.
/// </remarks>
[CompatibilityService("a Super Missile with no collision link of its own treats the projectile in Samus's first shot place as its link")]
public static class UnlinkedSuperMissileLink
{
    /// <summary>The zero-based shot place an unlinked Super Missile names as its link.</summary>
    public const int Place = 0;
}
