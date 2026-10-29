using System.Runtime.CompilerServices;
using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Enemies;
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

        /// <summary>
        /// A copy of the runtime's VRAM image with its background character artwork composed over it (#1350), each
        /// binding's bytes read from the artwork the runtime has installed (#1422).
        /// </summary>
        [WholeImageFixture("the runtime's composed VRAM image")]
        internal byte[] ComposedVram() => BgArtworkFixtures.Composed(self.Vram, self.BgArtwork, self.BackgroundArtwork());

        /// <summary>
        /// The bytes of the background artwork the runtime binds, from what it has installed: its room characters, HUD and
        /// Kraid backdrop, and the permanent items' cartridge characters. A name the runtime installs nothing for stays
        /// unregistered, and composing a binding of it fails.
        /// </summary>
        internal BgArtworkBytes BackgroundArtwork()
        {
            var artwork = new BgArtworkBytes();
            if (self.Artwork.RoomCharacterArt is { } characters)
                artwork.WithRoomCharacters(characters);
            if (self.Artwork.MapPresentation is { } presentation)
                artwork.WithHud(presentation.HudTiles);
            if ((self.BossArtwork.Artwork as BossSourceArtwork)?.KraidBackground is { } kraid)
                artwork.WithKraidBackground(kraid);
            artwork.WithCollectibles();
            return artwork;
        }
    }
}
