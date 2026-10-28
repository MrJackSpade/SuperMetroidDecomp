using static SuperMetroid.Core.Rooms.Plms.RoomPlmDynamicCollectibleGraphicsDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Which graphics and palette offsets each dynamic collectible loads.</summary>
internal static class DynamicCollectibleGraphics
{
    /// <summary>The place of <paramref name="kind"/> in <see cref="RoomPlmDynamicCollectibleGraphicsDefinitions.Kinds"/>, or -1 when it has no dynamic graphics upload.</summary>
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
