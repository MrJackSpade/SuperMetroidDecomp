
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Weapon dispatch to Samus projectile headers. The bank-$93 selector tables lie one after another, uncharged beams,
/// charged beams, non-beam kinds, echo and trail kinds, special attacks and the Super Missile link, so an index past one
/// table's end selects from the next.
/// </summary>
internal static class SamusProjectileSelectionDefinitions
{
    /// <summary>Entries in all the selector tables together.</summary>
    internal const int EntryCount = 56;
}
