using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Cartridge definitions shared by the exposed, Chozo-orb, and shot-block
/// presentations of the 17 dynamically uploaded permanent-item kinds.
/// </summary>
internal static partial class RoomObjectDynamicCollectibleGraphicsDefinitions
{
    /// <summary>The permanent-item kinds whose artwork uses the dynamic PLM upload, in collectible-kind order: every kind after the four tanks.</summary>
    internal static readonly InWorldCollectibleKind[] Kinds =
    [
        InWorldCollectibleKind.Bombs, InWorldCollectibleKind.ChargeBeam, InWorldCollectibleKind.IceBeam,
        InWorldCollectibleKind.HiJumpBoots, InWorldCollectibleKind.SpeedBooster, InWorldCollectibleKind.WaveBeam,
        InWorldCollectibleKind.SpazerBeam, InWorldCollectibleKind.SpringBall, InWorldCollectibleKind.VariaSuit,
        InWorldCollectibleKind.GravitySuit, InWorldCollectibleKind.XrayScope, InWorldCollectibleKind.PlasmaBeam,
        InWorldCollectibleKind.GrappleBeam, InWorldCollectibleKind.SpaceJump, InWorldCollectibleKind.ScrewAttack,
        InWorldCollectibleKind.MorphBall, InWorldCollectibleKind.ReserveTank,
    ];

    /// <summary>
    /// The artwork blocks in their stored order (#2567): consecutive 256-byte blocks in native bank-$89 order, $8000 through
    /// $9000, so a block's place is its position here and never the order <see cref="ItemGraphic"/> declares its members.
    /// </summary>
    internal static readonly ItemGraphic[] StoredGraphics =
    [
        ItemGraphic.Bombs,
        ItemGraphic.GravitySuit,
        ItemGraphic.SpringBall,
        ItemGraphic.VariaSuit,
        ItemGraphic.HiJumpBoots,
        ItemGraphic.ScrewAttack,
        ItemGraphic.SpaceJump,
        ItemGraphic.MorphBall,
        ItemGraphic.GrappleBeam,
        ItemGraphic.XrayScope,
        ItemGraphic.SpeedBooster,
        ItemGraphic.ChargeBeam,
        ItemGraphic.IceBeam,
        ItemGraphic.WaveBeam,
        ItemGraphic.PlasmaBeam,
        ItemGraphic.Spazer,
        ItemGraphic.ReserveTank,
    ];

    /// <summary>The 256-byte artwork blocks of the dynamically uploaded items, stored as <see cref="StoredGraphics"/> lists them.</summary>
    internal enum ItemGraphic
    {
        /// <summary>$89:8000, ItemPLMGFX_Bombs: two-frame Bombs artwork block.</summary>
        Bombs,
        /// <summary>$89:8100, ItemPLMGFX_GravitySuit: two-frame GravitySuit artwork block.</summary>
        GravitySuit,
        /// <summary>$89:8200, ItemPLMGFX_SpringBall: two-frame SpringBall artwork block.</summary>
        SpringBall,
        /// <summary>$89:8300, ItemPLMGFX_VariaSuit: two-frame VariaSuit artwork block.</summary>
        VariaSuit,
        /// <summary>$89:8400, ItemPLMGFX_HiJumpBoots: two-frame HiJumpBoots artwork block.</summary>
        HiJumpBoots,
        /// <summary>$89:8500, ItemPLMGFX_ScrewAttack: two-frame ScrewAttack artwork block.</summary>
        ScrewAttack,
        /// <summary>$89:8600, ItemPLMGFX_SpaceJump: two-frame SpaceJump artwork block.</summary>
        SpaceJump,
        /// <summary>$89:8700, ItemPLMGFX_MorphBall: two-frame MorphBall artwork block.</summary>
        MorphBall,
        /// <summary>$89:8800, ItemPLMGFX_GrappleBeam: two-frame GrappleBeam artwork block.</summary>
        GrappleBeam,
        /// <summary>$89:8900, ItemPLMGFX_XrayScope: two-frame XrayScope artwork block.</summary>
        XrayScope,
        /// <summary>$89:8A00, ItemPLMGFX_SpeedBooster: two-frame SpeedBooster artwork block.</summary>
        SpeedBooster,
        /// <summary>$89:8B00, ItemPLMGFX_ChargeBeam: two-frame ChargeBeam artwork block.</summary>
        ChargeBeam,
        /// <summary>$89:8C00, ItemPLMGFX_IceBeam: two-frame IceBeam artwork block.</summary>
        IceBeam,
        /// <summary>$89:8D00, ItemPLMGFX_WaveBeam: two-frame WaveBeam artwork block.</summary>
        WaveBeam,
        /// <summary>$89:8E00, ItemPLMGFX_PlasmaBeam: two-frame PlasmaBeam artwork block.</summary>
        PlasmaBeam,
        /// <summary>$89:8F00, ItemPLMGFX_Spazer: two-frame Spazer artwork block.</summary>
        Spazer,
        /// <summary>$89:9000, ItemPLMGFX_ReserveTank: two-frame ReserveTank artwork block.</summary>
        ReserveTank,
    }
}
