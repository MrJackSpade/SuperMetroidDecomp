using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Verification access: one actor-pass charge-flare step read as the OAM image its draws stand for.</summary>
internal static class ChargeFlareDrawAccess
{
    /// <summary>Advances and draws the charge flare as the actor pass does.</summary>
    extension(SamusProjectileSystem system)
    {
        /// <summary>
        /// Advances the flare, publishes the components it shows over the installed projectile page, and returns the
        /// OAM image those draws stand for.
        /// </summary>
        internal OamBuffer ChargeFlareOam(ISnesAddressSpace bus, SamusState samus, Position16<RoomPixels> layer1,
            ChargeFlarePlacementCatalog? placement, ChargeFlareSpriteCatalog? compositions, SamusMode7Transform? mode7Transform = null) =>
            SpriteFrameBuffer.DrawnOam(frame => system.DrawChargeFlare(bus, frame, samus, layer1,
                system.AdvanceChargeFlare(samus), mode7Transform,
                new ChargeFlareArtwork { Placement = placement, Compositions = compositions, Objects = InstalledFixtures.ProjectileArtwork() }));
    }
}
