namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The eight palette selector bytes and 256 raw 4bpp character bytes consumed by
/// bank-$84 instruction <c>$8764</c> for one permanent-item kind.
/// </summary>
/// <param name="Kind">Permanent-item kind whose dynamic graphics are uploaded.</param>
/// <param name="GraphicsPointer">Bank-$89 address of the item's 256-byte graphics block.</param>
/// <param name="PaletteOffsets">Eight palette selectors, one for each 8x8 tile in the two-frame icon.</param>
/// <param name="Tiles">Raw 4bpp tile bytes copied by the dynamic graphics upload.</param>
public sealed record RoomPlmDynamicCollectibleGraphic(
    InWorldCollectibleKind Kind,
    ushort GraphicsPointer,
    ReadOnlyMemory<byte> PaletteOffsets,
    ReadOnlyMemory<byte> Tiles);

/// <summary>
/// Cartridge definitions shared by the exposed, Chozo-orb, and shot-block
/// presentations of the 17 dynamically uploaded permanent-item kinds.
/// </summary>
internal static partial class RoomPlmDynamicCollectibleGraphicsDefinitions
{
    /// <summary>Numeric kind value at which this contiguous dynamic-graphics catalog begins.</summary>
    internal const int FirstKind = (int)InWorldCollectibleKind.Bombs;

    /// <summary>Number of permanent-item kinds with graphics uploaded by the dynamic PLM path.</summary>
    internal const int GraphicCount = RoomPlmHeaders.PermanentCollectibleKindCount - FirstKind;

    /// <summary>Enumerates definitions for every kind handled by the dynamic permanent-item upload path.</summary>
    internal static IEnumerable<RoomPlmDynamicCollectibleGraphic> All
    {
        get
        {
            for (int kind = FirstKind; kind < FirstKind + GraphicCount; kind++)
                yield return Get((InWorldCollectibleKind)kind);
        }
    }

    /// <summary>Builds the bank-$89 artwork and per-tile palette selectors for one supported item kind.</summary>
    /// <param name="kind">Permanent-item kind whose upload data is requested.</param>
    /// <returns>The graphics pointer, eight palette selectors, and decoded tile bytes for the item.</returns>
    /// <exception cref="InvalidDataException">The kind is outside the contiguous dynamic graphics catalog.</exception>
    internal static RoomPlmDynamicCollectibleGraphic Get(InWorldCollectibleKind kind)
    {
        int index = (int)kind - FirstKind;
        if ((uint)index >= GraphicCount)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic graphics upload.");
        ItemGraphic graphic = GraphicFor(kind);
        byte[] palettes = new byte[8];
        for (int tile = 0; tile < palettes.Length; tile++)
            palettes[tile] = PaletteOffset(kind, tile);
        return new(kind, GraphicsPointer(kind), palettes, Convert.FromHexString(Tiles[(int)graphic]));
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

    /// <summary>ItemPLMGFX_Bombs begins at $89:8000; every graphic owns eight 32-byte tiles.</summary>
    internal static ushort GraphicsPointer(InWorldCollectibleKind kind) =>
        checked((ushort)(0x8000 + (int)GraphicFor(kind) * 0x100));

    /// <summary>Maps each supported item kind to its sequential bank-$89 artwork block.</summary>
    /// <param name="kind">Permanent-item kind whose graphics block is selected.</param>
    /// <returns>The catalog entry identifying the item's 256-byte artwork block.</returns>
    /// <exception cref="InvalidDataException">The kind has no dynamic graphics upload.</exception>
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
        _ => throw new InvalidDataException($"Permanent-item kind {kind} has no dynamic graphics upload."),
    };

    /// <summary>
    /// Eight upload palette selectors are two row-major 2x2 frames. Beam icons
    /// select their accent palette in the top-right tile of each frame; X-ray
    /// uses its top row with different palettes between the two frames.
    /// </summary>
    internal static byte PaletteOffset(InWorldCollectibleKind kind, int tile)
    {
        if ((uint)((int)kind - FirstKind) >= GraphicCount)
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
            _ => 0,
        };
    }
}
