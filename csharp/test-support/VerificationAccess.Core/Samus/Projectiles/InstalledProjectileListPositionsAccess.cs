namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="InstalledProjectileListPositions"/> members production does not use.</summary>
internal static class InstalledProjectileListPositionsAccess
{
    /// <summary>Exposes <see cref="InstalledProjectileListPositions"/> state that only verification reads.</summary>
    extension(InstalledProjectileListPositions self)
    {
        /// <summary>Each step's instruction pointer, by program.</summary>
        internal IReadOnlyDictionary<SamusProjectileProgram, ushort[]> StepPositions => self.stepPositions;
    }
}
