namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Cartridge definitions shared by the exposed, Chozo-orb, and shot-block
/// presentations of the 17 dynamically uploaded permanent-item kinds.
/// </summary>
internal static partial class RoomPlmDynamicCollectibleGraphicsDefinitions
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

    /// <summary>Number of permanent-item kinds with a bank $89 graphics block.</summary>
    internal static int GraphicCount => Kinds.Length;

    /// <summary>The artwork blocks in their stored order.</summary>
    private static readonly ItemGraphic[] StoredGraphics = Enum.GetValues<ItemGraphic>();

    /// <summary>The place of <paramref name="kind"/> in <see cref="Kinds"/>, or -1 when it has no dynamic graphics upload.</summary>
    internal static int KindIndex(InWorldCollectibleKind kind) => Array.IndexOf(Kinds, kind);

    /// <summary>Enumerates the complete set of dynamic permanent-item graphics in collectible-kind order.</summary>
    internal static IEnumerable<RoomPlmDynamicCollectibleGraphic> All => Kinds.Select(Get);

    /// <summary>Combines a permanent item's eight palette selectors and tile bytes.</summary>
    internal static RoomPlmDynamicCollectibleGraphic Get(InWorldCollectibleKind kind)
    {
        if (KindIndex(kind) < 0)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic graphics upload.");
        byte[] palettes = new byte[8];
        for (int tile = 0; tile < palettes.Length; tile++)
            palettes[tile] = PaletteOffset(kind, tile);
        return new(kind, palettes, Convert.FromHexString(Tiles[GraphicIndex(kind)]));
    }

    /// <summary>Consecutive 256-byte artwork blocks in native bank-$89 order, $8000 through $9000.</summary>
    private enum ItemGraphic
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

    /// <summary>The place of <paramref name="kind"/>'s artwork block among the item graphics, in their stored order.</summary>
    internal static int GraphicIndex(InWorldCollectibleKind kind) => Array.IndexOf(StoredGraphics, GraphicFor(kind));

    /// <summary>Maps collectible identity to its distinct bank $89 tile block, independent of enum order.</summary>
    private static ItemGraphic GraphicFor(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.Bombs => ItemGraphic.Bombs,
        InWorldCollectibleKind.GravitySuit => ItemGraphic.GravitySuit,
        InWorldCollectibleKind.SpringBall => ItemGraphic.SpringBall,
        InWorldCollectibleKind.VariaSuit => ItemGraphic.VariaSuit,
        InWorldCollectibleKind.HiJumpBoots => ItemGraphic.HiJumpBoots,
        InWorldCollectibleKind.ScrewAttack => ItemGraphic.ScrewAttack,
        InWorldCollectibleKind.SpaceJump => ItemGraphic.SpaceJump,
        InWorldCollectibleKind.MorphBall => ItemGraphic.MorphBall,
        InWorldCollectibleKind.GrappleBeam => ItemGraphic.GrappleBeam,
        InWorldCollectibleKind.XrayScope => ItemGraphic.XrayScope,
        InWorldCollectibleKind.SpeedBooster => ItemGraphic.SpeedBooster,
        InWorldCollectibleKind.ChargeBeam => ItemGraphic.ChargeBeam,
        InWorldCollectibleKind.IceBeam => ItemGraphic.IceBeam,
        InWorldCollectibleKind.WaveBeam => ItemGraphic.WaveBeam,
        InWorldCollectibleKind.PlasmaBeam => ItemGraphic.PlasmaBeam,
        InWorldCollectibleKind.SpazerBeam => ItemGraphic.Spazer,
        InWorldCollectibleKind.ReserveTank => ItemGraphic.ReserveTank,
        InWorldCollectibleKind.EnergyTank or
            InWorldCollectibleKind.MissileTank or
            InWorldCollectibleKind.SuperMissileTank or
            InWorldCollectibleKind.PowerBombTank => throw new InvalidDataException($"Permanent-item kind {kind} has no dynamic graphics upload."),
        _ => throw new InvalidDataException($"Permanent-item kind {kind} has no dynamic graphics upload."),
    };

    /// <summary>
    /// Eight upload palette selectors are two row-major 2x2 frames. Beam icons
    /// select their accent palette in the top-right tile of each frame; X-ray
    /// uses its top row with different palettes between the two frames.
    /// </summary>
    internal static byte PaletteOffset(InWorldCollectibleKind kind, int tile)
    {
        if (KindIndex(kind) < 0)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic graphics upload.");
        if ((uint)tile >= 8)
            throw new ArgumentOutOfRangeException(nameof(tile));
        if (kind == InWorldCollectibleKind.XrayScope)
            return (byte)(tile % 4 < 2 ? (tile < 4 ? 1 : 3) : 0);
        if (tile % 4 != 1) return 0;
        return kind switch
        {
            InWorldCollectibleKind.IceBeam => 3,
            InWorldCollectibleKind.WaveBeam => 2,
            InWorldCollectibleKind.PlasmaBeam => 1,
            InWorldCollectibleKind.EnergyTank or InWorldCollectibleKind.MissileTank or
                InWorldCollectibleKind.SuperMissileTank or InWorldCollectibleKind.PowerBombTank or
                InWorldCollectibleKind.Bombs or InWorldCollectibleKind.ChargeBeam or
                InWorldCollectibleKind.HiJumpBoots or InWorldCollectibleKind.SpeedBooster or
                InWorldCollectibleKind.SpazerBeam or InWorldCollectibleKind.SpringBall or
                InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit or
                InWorldCollectibleKind.XrayScope or InWorldCollectibleKind.GrappleBeam or
                InWorldCollectibleKind.SpaceJump or InWorldCollectibleKind.ScrewAttack or
                InWorldCollectibleKind.MorphBall or InWorldCollectibleKind.ReserveTank => 0,
            _ => throw new InvalidOperationException($"Undefined InWorldCollectibleKind {kind}."),
        };
    }
}
