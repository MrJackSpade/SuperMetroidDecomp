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
        internal Layer3Plane AcceptedLayer3Plane =>
            self.Display.Latched.Layer3 ?? throw new InvalidOperationException("No accepted NMI has latched a room's BG3 plane.");

        /// <summary>
        /// The HUD's four rows the last accepted NMI latched, as the installed images' tiles lay them out, row by row, as
        /// the cartridge's packed tilemap words.
        /// </summary>
        /// <exception cref="InvalidOperationException">No HUD artwork is bound.</exception>
        internal ushort[] AcceptedHudWords => BackgroundTileWordBytes.Words(GameplayHudArtwork.Of(self.Hud.Presentation
            ?? throw new InvalidOperationException("No HUD artwork is bound.")).Rows(self.AcceptedHud));
    }
}
