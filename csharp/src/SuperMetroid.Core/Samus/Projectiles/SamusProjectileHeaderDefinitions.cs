namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The program each bank-$93 projectile header's list field selects (#1301), by header and direction: the ten fields a
/// directional header holds, and for the shorter headers the words beyond them that a glitched selection reads. A
/// zero field, marked in the header's empty mask, is no list (the safe left-facing Murder Beam); any other field that is
/// not a list fails loudly.
/// </summary>
internal static class SamusProjectileHeaderDefinitions
{
}
