using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Verification access to what the last accepted NMI latched of BG3 (#1466): the room effect's plane and the HUD's
/// images, laid out as the cartridge's tilemap words.
/// </summary>
internal static class SuperMetroidRuntimeHudAccess
{
    /// <summary>The latched BG3.</summary>
    extension(SuperMetroidRuntime self)
    {
        /// <summary>The room effect's BG3 plane the last accepted NMI latched.</summary>
        /// <exception cref="InvalidOperationException">No NMI has latched a room's backgrounds yet.</exception>
        internal FxOverlayPlane AcceptedFxOverlayPlane =>
            self.Display.Latched.FxOverlay ?? throw new InvalidOperationException("No accepted NMI has latched a room's BG3 plane.");

        /// <summary>
        /// The installed HUD artwork the runtime's HUD layout was read with: the tiles the import pictures the HUD's images
        /// from, which the runtime never holds (#1616).
        /// </summary>
        /// <exception cref="InvalidOperationException">No map presentation is bound, or it is not a fixture's.</exception>
        internal GameplayHudArtwork InstalledHudArtwork =>
            SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportOf(self.Artwork.MapPresentation
                ?? throw new InvalidOperationException("No map presentation is bound.")).GameplayHud;

        /// <summary>
        /// The HUD's four rows the last accepted NMI latched, as the installed images' tiles lay them out, row by row, as
        /// the cartridge's packed tilemap words.
        /// </summary>
        /// <exception cref="InvalidOperationException">No HUD layout is bound.</exception>
        internal ushort[] AcceptedHudWords => self.Hud.Presentation is null
            ? throw new InvalidOperationException("No HUD layout is bound.")
            : BackgroundTileWordBytes.Words(self.InstalledHudArtwork.Rows(self.AcceptedHud));
    }
}
