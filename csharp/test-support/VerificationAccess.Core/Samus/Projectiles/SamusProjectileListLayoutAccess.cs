namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusProjectileListLayout"/> members production does not use.</summary>
internal static class SamusProjectileListLayoutAccess
{
    /// <summary>Exposes <see cref="SamusProjectileListLayout"/> state that only verification reads.</summary>
    extension(SamusProjectileListLayout self)
    {
        /// <summary>Each step's instruction pointer, by program.</summary>
        internal IReadOnlyDictionary<SamusProjectileProgram, ushort[]> StepPositions => self.stepPositions;
    }
}
