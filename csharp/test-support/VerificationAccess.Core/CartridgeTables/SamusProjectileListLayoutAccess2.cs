using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Verification access to <see cref="SamusProjectileListLayout"/> members production does not use.</summary>
internal static class SamusProjectileListLayoutAccess2
{
    /// <summary>Exposes <see cref="SamusProjectileListLayout"/> state that only verification reads.</summary>
    extension(SamusProjectileListLayout self)
    {
        /// <summary>The word just before <paramref name="program"/>'s list.</summary>
        internal ushort Prefix(SamusProjectileSegment program) => checked((ushort)self.positions[program].PrecedingWord);
    }
}
