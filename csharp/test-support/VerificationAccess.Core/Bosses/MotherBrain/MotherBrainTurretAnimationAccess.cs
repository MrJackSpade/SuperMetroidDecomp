namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="MotherBrainTurretAnimation"/> members production does not use.</summary>
internal static class MotherBrainTurretAnimationAccess
{
    /// <summary>Exposes <see cref="MotherBrainTurretAnimation"/> state that only verification reads.</summary>
    extension(MotherBrainTurretAnimation self)
    {
        /// <summary>The program played.</summary>
        internal MotherBrainTurretProgram Program => self.Cursor.Program.Program;
    }
}
