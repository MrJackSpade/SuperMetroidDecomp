using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Verification access to <see cref="SamusBodyFrameState"/> for checks that still picture sprites through emulated character memory.</summary>
internal static class SamusBodyFrameStateAccess
{
    /// <summary>Lays the shown body pictures where a sprite-memory picture of the body's draws reads them.</summary>
    extension(SamusBodyFrameState self)
    {
        /// <summary>
        /// Loads the body characters the shown definitions make (<see cref="SamusBodyArtworkCatalog.BodyArtwork"/>) at
        /// the start of the sprite characters, which the body's draws name from character zero.
        /// </summary>
        [PpuLayoutBoundary("exact hardware test: lays the shown body characters into the emulated sprite characters")]
        internal void LoadShownInto(SnesVram vram)
        {
            Ensure.NotNull(vram);
            SamusBodyArtworkCatalog installed = self.Artwork ?? throw new InvalidOperationException(
                "Picturing Samus's body requires installed body artwork.");
            vram.LoadBytes(GameplayVramLayoutConstants.ObjectCharacters.Offset * sizeof(ushort),
                installed.BodyArtwork(self.Shown.Top, self.Shown.Bottom).Characters);
        }
    }
}
