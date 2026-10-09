using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One fixed bank-$94 special-air dispatch result paired with the setup callback stored
/// by its bank-$84 PLM header.
/// </summary>
/// <param name="HeaderPointer">Bank-$84 PLM header selected by the special-air dispatch table.</param>
/// <param name="SetupPointer">Bank-$84 setup routine invoked by that PLM header.</param>
public readonly record struct SpecialAirReactionDefinition(
    ushort HeaderPointer,
    ushort SetupPointer);

/// <summary>
/// Area-dependent special-air dispatch expressed as named reaction cases, without stored tables.
/// Bank-$94 pointers at $9B06 (inside) and $92D9 (collision) select sixteen entries for each
/// of seven retail areas; each selected bank-$84 header supplies its setup callback.
/// BTS callers clear bit seven before selection. Preserve distinct no-op header identities,
/// repeated quicksand-surface cases, and rejection of non-retail areas or indexes above fifteen.
/// These are engine dispatch rules, not editable room or presentation assets.
/// </summary>
public static class SpecialAirReactionDefinitions
{
    /// <summary>Each retail area owns sixteen authored BTS entries in both native tables.</summary>
    public const int EntriesPerArea = 16;

    /// <summary><c>$84:B62F PLMEntries_nothing</c>, whose setup is <c>$84:B3CF RTS</c>.</summary>
    public static readonly SpecialAirReactionDefinition Nothing = new(0xb62f, 0xb3cf);

    /// <summary>
    /// <c>$84:B633 PLMEntries_collisionReactionClearCarry</c>, whose setup is
    /// <c>$84:B3D0 Setup_ClearCarry</c>.
    /// </summary>
    public static readonly SpecialAirReactionDefinition ClearCarry = new(0xb633, 0xb3d0);

    /// <summary><c>$84:B653</c>, Norfair inside-reaction no-op for BTS <c>$80</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing80 = new(0xb653, 0xb3cf);

    /// <summary><c>$84:B657</c>, Norfair inside-reaction no-op for BTS <c>$81</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing81 = new(0xb657, 0xb3cf);

    /// <summary><c>$84:B65B</c>, Norfair inside-reaction no-op for BTS <c>$82</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing82 = new(0xb65b, 0xb3cf);

    /// <summary>
    /// <c>$84:B6CB PLMEntries_insideReactionBrinstarFloorPlant</c> and setup
    /// <c>$84:B0DC Setup_BrinstarFloorPlant</c>.
    /// </summary>
    public static readonly SpecialAirReactionDefinition BrinstarFloorPlant = new(0xb6cb, 0xb0dc);

    /// <summary>
    /// <c>$84:B6CF PLMEntries_insideReactionBrinstarCeilingPlant</c> and setup
    /// <c>$84:B113 Setup_BrinstarCeilingPlant</c>.
    /// </summary>
    public static readonly SpecialAirReactionDefinition BrinstarCeilingPlant = new(0xb6cf, 0xb113);

    /// <summary>
    /// <c>$84:B70F PLMEntries_insideReactionCrateria80</c> and setup
    /// <c>$84:B3EB Setup_IcePhysics</c>.
    /// </summary>
    public static readonly SpecialAirReactionDefinition CrateriaIcePhysics = new(0xb70f, 0xb3eb);

    /// <summary>Maridia quicksand-surface inside reaction at <c>$84:B713/$B408</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSurfaceInside = new(0xb713, 0xb408);

    /// <summary>Maridia submerging-quicksand inside reaction at <c>$84:B71F/$B497</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSubmergingInside = new(0xb71f, 0xb497);

    /// <summary>Maridia slow-sandfall inside reaction at <c>$84:B723/$B4A8</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSlowFallInside = new(0xb723, 0xb4a8);

    /// <summary>Maridia fast-sandfall inside reaction at <c>$84:B727/$B4B6</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandFastFallInside = new(0xb727, 0xb4b6);

    /// <summary>Maridia quicksand-surface collision reaction at <c>$84:B72B/$B4C4</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSurfaceCollision = new(0xb72b, 0xb4c4);

    /// <summary>Maridia submerging-quicksand collision reaction at <c>$84:B737/$B541</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSubmergingCollision = new(0xb737, 0xb541);

    /// <summary>Maridia slow-sandfall collision reaction at <c>$84:B73B/$B54F</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSlowFallCollision = new(0xb73b, 0xb54f);

    /// <summary>Maridia fast-sandfall collision reaction at <c>$84:B73F/$B54F</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandFastFallCollision = new(0xb73f, 0xb54f);

    /// <summary>Brinstar slow respawning Speed Booster block at <c>$84:D030/$CDEA</c>.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSlowSpeedBlockRespawning = new(0xd030, 0xcdea);

    /// <summary>Brinstar slow permanent Speed Booster block at <c>$84:D034/$CDEA</c>.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSlowSpeedBlockPermanent = new(0xd034, 0xcdea);

    /// <summary>Brinstar Dachora respawning Speed Booster block at <c>$84:D03C/$CDEA</c>.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarDachoraSpeedBlock = new(0xd03c, 0xcdea);

    /// <summary>Brinstar permanent Speed Booster block at <c>$84:D040/$CDEA</c>.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSpeedBlockPermanent = new(0xd040, 0xcdea);

    /// <summary>Lower Norfair Chozo-hand collision trigger at <c>$84:D6DA/$D18F</c>.</summary>
    public static readonly SpecialAirReactionDefinition LowerNorfairChozoHand = new(0xd6da, 0xd18f);

    /// <summary>Wrecked Ship Chozo-hand collision trigger at <c>$84:D6F2/$D620</c>.</summary>
    public static readonly SpecialAirReactionDefinition WreckedShipChozoHand = new(0xd6f2, 0xd620);

    /// <summary>Resolves the native inside-body header/setup for retail areas and indexes 0..15.</summary>
    public static SpecialAirReactionDefinition ResolveInside(AreaId area, byte areaReactionIndex)
    {
        Validate(area, areaReactionIndex, "inside-body");
        return (area, areaReactionIndex) switch
        {
            (AreaId.Crateria, 0) => CrateriaIcePhysics,
            (AreaId.Brinstar, 0) => BrinstarFloorPlant,
            (AreaId.Brinstar, 1) => BrinstarCeilingPlant,
            (AreaId.Norfair, 0) => NorfairInsideNothing80,
            (AreaId.Norfair, 1) => NorfairInsideNothing81,
            (AreaId.Norfair, 2) => NorfairInsideNothing82,
            (AreaId.Maridia, 0) => QuicksandSurfaceInside,
            (AreaId.Maridia, 1) => QuicksandSurfaceInside,
            (AreaId.Maridia, 2) => QuicksandSurfaceInside,
            (AreaId.Maridia, 3) => QuicksandSubmergingInside,
            (AreaId.Maridia, 4) => QuicksandSlowFallInside,
            (AreaId.Maridia, 5) => QuicksandFastFallInside,
            _ => Nothing,
        };
    }

    /// <summary>Resolves the native movement-collision header/setup for retail areas and indexes 0..15.</summary>
    public static SpecialAirReactionDefinition ResolveCollision(AreaId area, byte areaReactionIndex)
    {
        Validate(area, areaReactionIndex, "movement-collision");
        return (area, areaReactionIndex) switch
        {
            (AreaId.Brinstar, 0) => ClearCarry,
            (AreaId.Brinstar, 1) => ClearCarry,
            (AreaId.Brinstar, 2) => BrinstarSlowSpeedBlockRespawning,
            (AreaId.Brinstar, 3) => BrinstarSlowSpeedBlockPermanent,
            (AreaId.Brinstar, 4) => BrinstarDachoraSpeedBlock,
            (AreaId.Brinstar, 5) => BrinstarSpeedBlockPermanent,
            (AreaId.Norfair, 3) => LowerNorfairChozoHand,
            (AreaId.WreckedShip, 0) => WreckedShipChozoHand,
            (AreaId.Maridia, 0) => QuicksandSurfaceCollision,
            (AreaId.Maridia, 1) => QuicksandSurfaceCollision,
            (AreaId.Maridia, 2) => QuicksandSurfaceCollision,
            (AreaId.Maridia, 3) => QuicksandSubmergingCollision,
            (AreaId.Maridia, 4) => QuicksandSlowFallCollision,
            (AreaId.Maridia, 5) => QuicksandFastFallCollision,
            _ => Nothing,
        };
    }

    /// <summary>Rejects non-retail areas and BTS indexes outside the sixteen entries authored for each area.</summary>
    /// <param name="area">Area whose identity must map to a retail area index.</param>
    /// <param name="areaReactionIndex">Zero-based special-air reaction index selected from the area's table.</param>
    /// <param name="context">Dispatch context included in an invalid-index error message.</param>
    /// <exception cref="ArgumentOutOfRangeException">The reaction index is at least <see cref="EntriesPerArea"/>.</exception>
    private static void Validate(AreaId area, byte areaReactionIndex, string context)
    {
        _ = AreaIds.ToIndex(area);
        if (areaReactionIndex >= EntriesPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(areaReactionIndex),
                areaReactionIndex,
                $"Special-air {context} BTS indexes are authored only from zero through fifteen.");
        }
    }
}
