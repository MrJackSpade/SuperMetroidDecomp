using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

/// <summary>Verification access to the VRAM gameplay shows, which the published frame no longer carries (#1403).</summary>
internal static class SuperMetroidRuntimeVramAccess
{
    /// <summary>Reads gameplay's VRAM as the room shows it.</summary>
    extension(SuperMetroidRuntime self)
    {
        /// <summary>
        /// A copy of gameplay's VRAM with its background character artwork composed over it (#1350): each binding shows over
        /// the words VRAM has not written since it was made, as the room's pictures resolve them.
        /// </summary>
        [PpuLayoutBoundary("exact hardware test: reads gameplay's VRAM image with its bound background artwork")]
        internal byte[] ComposedVram()
        {
            byte[] image = self.Vram.CaptureImage().ToArray();
            self.BgArtwork.ComposeOver(image);
            return image;
        }
    }
}
