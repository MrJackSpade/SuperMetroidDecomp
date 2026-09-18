using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One fixed bank-$94 special-air dispatch result paired with the setup callback stored
/// by its bank-$84 PLM header.
/// </summary>
public readonly record struct SpecialAirReactionDefinition(
    ushort HeaderPointer,
    ushort SetupPointer);

/// <summary>
/// Compiled area-dependent special-air dispatch tables used by inside-body and movement
/// collision probes. These are engine rules, not editable room or presentation assets.
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

    /// <summary>Resolves one authored inside-body reaction for a retail area and BTS index.</summary>
    public static SpecialAirReactionDefinition ResolveInside(AreaId area, byte areaReactionIndex) =>
        Resolve(inside, area, areaReactionIndex, "inside-body");

    /// <summary>Resolves one authored movement-collision reaction for a retail area and BTS index.</summary>
    public static SpecialAirReactionDefinition ResolveCollision(AreaId area, byte areaReactionIndex) =>
        Resolve(collision, area, areaReactionIndex, "movement-collision");

    private static SpecialAirReactionDefinition Resolve(
        SpecialAirReactionDefinition[] table,
        AreaId area,
        byte areaReactionIndex,
        string context)
    {
        int areaIndex = AreaIds.ToIndex(area);
        if (areaReactionIndex >= EntriesPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(areaReactionIndex),
                areaReactionIndex,
                $"Special-air {context} BTS indexes are authored only from zero through fifteen.");
        }

        return table[areaIndex * EntriesPerArea + areaReactionIndex];
    }

    private static SpecialAirReactionDefinition[] CreateDefaultTable()
    {
        var table = new SpecialAirReactionDefinition[AreaIds.RetailCount * EntriesPerArea];
        Array.Fill(table, Nothing);
        return table;
    }

    private static void Set(
        SpecialAirReactionDefinition[] table,
        AreaId area,
        int index,
        SpecialAirReactionDefinition definition) =>
        table[AreaIds.ToIndex(area) * EntriesPerArea + index] = definition;

    private static SpecialAirReactionDefinition[] BuildInside()
    {
        SpecialAirReactionDefinition[] table = CreateDefaultTable();
        Set(table, AreaId.Crateria, 0, CrateriaIcePhysics);
        Set(table, AreaId.Brinstar, 0, BrinstarFloorPlant);
        Set(table, AreaId.Brinstar, 1, BrinstarCeilingPlant);
        Set(table, AreaId.Norfair, 0, NorfairInsideNothing80);
        Set(table, AreaId.Norfair, 1, NorfairInsideNothing81);
        Set(table, AreaId.Norfair, 2, NorfairInsideNothing82);
        Set(table, AreaId.Maridia, 0, QuicksandSurfaceInside);
        Set(table, AreaId.Maridia, 1, QuicksandSurfaceInside);
        Set(table, AreaId.Maridia, 2, QuicksandSurfaceInside);
        Set(table, AreaId.Maridia, 3, QuicksandSubmergingInside);
        Set(table, AreaId.Maridia, 4, QuicksandSlowFallInside);
        Set(table, AreaId.Maridia, 5, QuicksandFastFallInside);
        return table;
    }

    private static SpecialAirReactionDefinition[] BuildCollision()
    {
        SpecialAirReactionDefinition[] table = CreateDefaultTable();
        Set(table, AreaId.Brinstar, 0, ClearCarry);
        Set(table, AreaId.Brinstar, 1, ClearCarry);
        Set(table, AreaId.Brinstar, 2, BrinstarSlowSpeedBlockRespawning);
        Set(table, AreaId.Brinstar, 3, BrinstarSlowSpeedBlockPermanent);
        Set(table, AreaId.Brinstar, 4, BrinstarDachoraSpeedBlock);
        Set(table, AreaId.Brinstar, 5, BrinstarSpeedBlockPermanent);
        Set(table, AreaId.Norfair, 3, LowerNorfairChozoHand);
        Set(table, AreaId.WreckedShip, 0, WreckedShipChozoHand);
        Set(table, AreaId.Maridia, 0, QuicksandSurfaceCollision);
        Set(table, AreaId.Maridia, 1, QuicksandSurfaceCollision);
        Set(table, AreaId.Maridia, 2, QuicksandSurfaceCollision);
        Set(table, AreaId.Maridia, 3, QuicksandSubmergingCollision);
        Set(table, AreaId.Maridia, 4, QuicksandSlowFallCollision);
        Set(table, AreaId.Maridia, 5, QuicksandFastFallCollision);
        return table;
    }

    private static readonly SpecialAirReactionDefinition[] inside = BuildInside();
    private static readonly SpecialAirReactionDefinition[] collision = BuildCollision();
}
