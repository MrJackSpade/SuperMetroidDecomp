namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values of <see cref="SamusMovementRomData.Banks"/> that only verification reads.</summary>
internal static class SamusMovementRomDataBanksConstants
{
    /// <summary>Verification's view of the pose bank.</summary>
    extension(SamusMovementRomData.Banks)
    {
        /// <summary>Bank $91, which holds pose, animation, and transition tables.</summary>
        internal static int Pose => 0x910000;
    }
}
