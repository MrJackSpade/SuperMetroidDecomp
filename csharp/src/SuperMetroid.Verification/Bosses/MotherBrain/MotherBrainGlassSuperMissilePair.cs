using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The list, radius and pre of one mother brain glass super missile pair.</summary>
/// <param name="List">The list.</param>
/// <param name="Radius">The radius.</param>
/// <param name="Pre">The pre.</param>
internal readonly record struct MotherBrainGlassSuperMissilePair(ushort List, ushort Radius, SamusProjectilePreInstruction Pre);
