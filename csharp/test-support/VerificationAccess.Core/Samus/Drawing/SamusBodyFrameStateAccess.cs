using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Verification access to <see cref="SamusBodyFrameState"/> for checks that still picture sprites through emulated character memory.</summary>
internal static class SamusBodyFrameStateAccess
{
    /// <summary>Lays the shown body pictures where a sprite-memory picture of the body's draws reads them.</summary>
    extension(SamusBodyFrameState self)
    {
        /// <summary>
        /// Loads the body characters the shown definitions make (<see cref="SuperMetroid.AssetExtraction.Samus.SamusBodyCharacterArtwork.BodyArtwork"/>) at
        /// the start of the sprite characters, which the body's draws name from character zero.
        /// </summary>
        internal void LoadShownInto(SnesVram vram)
        {
            Ensure.NotNull(vram);
            InstalledSamusBodyArtwork installed = self.Artwork ?? throw new InvalidOperationException(
                "Picturing Samus's body requires installed body artwork.");
            vram.LoadBytes(GameplayVramLayoutConstants.ObjectCharacters.Offset * sizeof(ushort),
                SamusBodySheetFixtures.CharactersOf(installed).BodyArtwork(self.Shown.Top, self.Shown.Bottom).Characters);
        }
    }
}
