namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Verification access to <see cref="SamusSuitPickupState"/> members production does not use.</summary>
internal static class SamusSuitPickupStateAccess
{
    /// <summary>Exposes <see cref="SamusSuitPickupState"/> state that only verification reads.</summary>
    extension(SamusSuitPickupState self)
    {
        /// <summary>Value-only render input while the transformation is active; null once it ends.</summary>
        public SamusSuitPickupFlash? Flash => self.IsActive
            ? new(self.Beam, self.FixedColorRed, self.FixedColorGreen, self.FixedColorBlue)
            : null;
    }
}
