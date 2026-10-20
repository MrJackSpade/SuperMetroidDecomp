namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="MotherBrainGlassAnimation"/> members production does not use.</summary>
internal static class MotherBrainGlassAnimationAccess
{
    /// <summary>Exposes <see cref="MotherBrainGlassAnimation"/> state that only verification reads.</summary>
    extension(MotherBrainGlassAnimation self)
    {
        /// <summary>The program played.</summary>
        internal MotherBrainGlassProgram Program => self.Cursor.Program;
    }
}
