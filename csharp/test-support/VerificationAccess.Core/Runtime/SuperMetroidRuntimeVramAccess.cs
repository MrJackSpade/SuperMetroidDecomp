using System.Runtime.CompilerServices;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Verification access to a VRAM image of what gameplay shows (#1403, #1406, #1466): gameplay keeps no VRAM and binds no
/// characters, so each runtime's image is the test's own, which the bindings the cartridge would hold for what the runtime
/// shows compose over.
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
        /// The background character bindings the cartridge would hold for what the runtime shows (#1350, #1466): the HUD's
        /// characters once its artwork is installed, the loaded room's tileset, and the collectibles and Kraid's backdrop
        /// the room shows over it.
        /// </summary>
        internal BgArtworkBindings BgArtwork
        {
            get
            {
                BgArtworkBytes artwork = self.BackgroundArtwork();
                var bindings = new BgArtworkBindings();
                if (self.Artwork.MapPresentation is not null)
                    bindings.Bind(BgArtworkIdentity.Asset(InstalledArtworkId.StandardHudTiles), BgCharacterSet.Layer3, 0,
                        BgCharacterSet.Layer3.CharactersIn(SuperMetroid.AssetExtraction.Hud.HudTileAtlasFormat.CharacterByteCount));
                if (self.Room.Scope is { } scope)
                {
                    RoomAppearanceBindings.BindLoad(bindings, scope.Assets.Tileset, artwork);
                    RoomAppearanceBindings.BindShown(bindings, scope.Appearance, artwork);
                }
                return bindings;
            }
        }

        /// <summary>
        /// A copy of the runtime's VRAM image with its background character artwork composed over it (#1350), each
        /// binding's bytes read from the artwork the runtime has installed (#1422).
        /// </summary>
        [WholeImageFixture("the runtime's composed VRAM image")]
        internal byte[] ComposedVram() => BgArtworkFixtures.Composed(self.Vram, self.BgArtwork, self.BackgroundArtwork());

        /// <summary>
        /// The bytes of the background artwork the runtime shows, from what it has installed: its room characters, HUD and
        /// Kraid backdrop, and the permanent items' cartridge characters. A name the runtime installs nothing for stays
        /// unregistered, and composing a binding of it fails.
        /// </summary>
        internal BgArtworkBytes BackgroundArtwork()
        {
            var artwork = new BgArtworkBytes();
            artwork.WithRoomCharacters(self.Artwork.RoomCharacterArt);
            if (self.Artwork.MapPresentation is SuperMetroid.Core.Frontend.Map.Artwork.AreaMapPresentation presentation)
                artwork.WithHud(SuperMetroid.AssetExtraction.Frontend.Map.MapCharacterArtwork.Of(presentation).Hud);
            if ((self.BossArtwork.Artwork as BossSourceArtwork)?.KraidBackground is { } kraid)
                artwork.WithKraidBackground(kraid);
            artwork.WithCollectibles();
            return artwork;
        }
    }
}
