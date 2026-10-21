using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Hardware;

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
        /// <summary>
        /// Ports <c>TransferSamusTilesToVRAM</c> at <c>$80:9376</c> for one accepted NMI.
        /// </summary>
        public void TransferToVram(SnesVram vram)
        {
            Ensure.NotNull(vram);

            // Samus has a dedicated DMA path instead of using the ordinary seven-byte VRAM
            // queue. The four destinations form two interleaved character regions selected by
            // the pose spritemaps' tile numbers under gameplay OBSEL=$03.
            SamusBodyArtworkCatalog installed = self.Artwork ?? throw new InvalidOperationException(
                "Samus tile transfer requires installed body artwork.");
            var samusVram = new SamusVram(vram);
            if (self.TopTransferEnabled)
                SamusTileTransferStateConstants.ExecuteInstalledDefinition(installed.DefinitionAt(self.TopDefinitionRecord),
                    samusVram.BodyTopFirst, samusVram.BodyTopSecond);
            if (self.BottomTransferEnabled)
                SamusTileTransferStateConstants.ExecuteInstalledDefinition(installed.DefinitionAt(self.BottomDefinitionRecord),
                    samusVram.BodyBottomFirst, samusVram.BodyBottomSecond);

            // These flags are intentionally not cleared. The original NMI routine leaves them
            // set, and Samus_Draw refreshes the selected definitions during each main-loop pass.
        }
    }
}
