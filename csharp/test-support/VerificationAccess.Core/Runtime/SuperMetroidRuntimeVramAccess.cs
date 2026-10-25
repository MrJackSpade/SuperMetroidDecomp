using System.Runtime.CompilerServices;
using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Verification access to a VRAM image of what gameplay shows (#1403, #1406): gameplay keeps no VRAM, so each runtime's
/// image is the test's own, which the runtime's background artwork composes over.
/// </summary>
internal static class SuperMetroidRuntimeVramAccess
{
    /// <summary>Each runtime's test-owned VRAM image.</summary>
    private static readonly ConditionalWeakTable<SuperMetroidRuntime, SnesVram> Images = new();

    /// <summary>Reads gameplay's shown characters as a VRAM image.</summary>
    extension(SuperMetroidRuntime self)
    {
        /// <summary>The test's VRAM image of the runtime, which nothing in gameplay writes.</summary>
        internal SnesVram Vram => Images.GetValue(self, _ => new SnesVram());

        /// <summary>A copy of the runtime's VRAM image with its background character artwork composed over it (#1350).</summary>
        [PpuLayoutBoundary("exact hardware test: reads gameplay's VRAM image with its bound background artwork")]
        internal byte[] ComposedVram() => BgArtworkFixtures.Composed(self.Vram, self.BgArtwork);
    }
}
