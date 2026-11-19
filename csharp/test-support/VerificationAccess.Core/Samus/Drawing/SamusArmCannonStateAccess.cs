namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Verification access to <see cref="SamusArmCannonState"/> members production does not use.</summary>
internal static class SamusArmCannonStateAccess
{
    /// <summary>Exposes <see cref="SamusArmCannonState"/> state that only verification reads.</summary>
    extension(SamusArmCannonState self)
    {
        /// <summary>The updates the HUD selection has held still, 0 through 2: its stability wait's age.</summary>
        public int StableSelectionUpdates => self.stability.Age;
    }
}
