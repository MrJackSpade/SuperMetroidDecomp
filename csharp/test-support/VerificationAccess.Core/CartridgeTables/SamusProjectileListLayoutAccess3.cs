using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Verification access to <see cref="SamusProjectileListLayout"/> members production does not use.</summary>
internal static class SamusProjectileListLayoutAccess3
{
    /// <summary>Exposes <see cref="SamusProjectileListLayout"/> state that only verification reads.</summary>
    extension(SamusProjectileListLayout self)
    {
        /// <summary>The frame each timed record's instruction pointer shows.</summary>
        internal IReadOnlyDictionary<int, SamusProjectileFrameId> RecordFrames => self.recordFrames;
    }
}
