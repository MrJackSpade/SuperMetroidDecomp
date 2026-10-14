
namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Verification access to <see cref="SamusTileTransferState"/> members production does not use.</summary>
internal static class SamusTileTransferStateAccess
{
    /// <summary>Clears the pending Samus tile-transfer flags after a verification upload.</summary>
    extension(SamusTileTransferState self)
    {
        /// <summary>Clears both transfer flags as the game-state reset routines do.</summary>
        internal void ClearTransferFlags()
        {
            self.TopTransferEnabled = false;
            self.BottomTransferEnabled = false;
        }
    }
}
