using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

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
    public static readonly SpecialAirReactionDefinition Nothing = new(PlmHeaderId.Nothing, SpecialAirReactionSetup.Nothing);

    /// <summary>
    /// <c>$84:B633 PLMEntries_collisionReactionClearCarry</c>, whose setup is
    /// <c>$84:B3D0 Setup_ClearCarry</c>.
    /// </summary>
    public static readonly SpecialAirReactionDefinition ClearCarry = new(PlmHeaderId.CollisionReactionClearCarry, SpecialAirReactionSetup.ClearCarry);

    /// <summary><c>$84:B653</c>, Norfair inside-reaction no-op for BTS <c>$80</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing80 = new(PlmHeaderId.InsideReactionNothingFirst, SpecialAirReactionSetup.Nothing);

    /// <summary><c>$84:B657</c>, Norfair inside-reaction no-op for BTS <c>$81</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing81 = new(PlmHeaderId.InsideReactionNothingSecond, SpecialAirReactionSetup.Nothing);

    /// <summary><c>$84:B65B</c>, Norfair inside-reaction no-op for BTS <c>$82</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing82 = new(PlmHeaderId.InsideReactionNothingThird, SpecialAirReactionSetup.Nothing);

    /// <summary>
    /// <c>$84:B6CB PLMEntries_insideReactionBrinstarFloorPlant</c> and setup
    /// <c>$84:B0DC Setup_BrinstarFloorPlant</c>.
    /// </summary>
    public static readonly SpecialAirReactionDefinition BrinstarFloorPlant = new(PlmHeaderId.InsideReactionBrinstarFloorPlant, SpecialAirReactionSetup.BrinstarFloorPlant);

    /// <summary>
    /// <c>$84:B6CF PLMEntries_insideReactionBrinstarCeilingPlant</c> and setup
    /// <c>$84:B113 Setup_BrinstarCeilingPlant</c>.
    /// </summary>
    public static readonly SpecialAirReactionDefinition BrinstarCeilingPlant = new(PlmHeaderId.InsideReactionBrinstarCeilingPlant, SpecialAirReactionSetup.BrinstarCeilingPlant);

    /// <summary>
    /// <c>$84:B70F PLMEntries_insideReactionCrateria80</c> and setup
    /// <c>$84:B3EB Setup_IcePhysics</c>.
    /// </summary>
    public static readonly SpecialAirReactionDefinition CrateriaIcePhysics = new(PlmHeaderId.InsideReactionCrateria80, SpecialAirReactionSetup.IcePhysics);

    /// <summary>Maridia quicksand-surface inside reaction at <c>$84:B713/$B408</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSurfaceInside = new(PlmHeaderId.InsideReactionQuicksandSurface, SpecialAirReactionSetup.QuicksandSurface);

    /// <summary>Maridia submerging-quicksand inside reaction at <c>$84:B71F/$B497</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSubmergingInside = new(PlmHeaderId.InsideReactionSubmergingQuicksand, SpecialAirReactionSetup.SubmergingQuicksand);

    /// <summary>Maridia slow-sandfall inside reaction at <c>$84:B723/$B4A8</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSlowFallInside = new(PlmHeaderId.InsideReactionSandFallsSlow, SpecialAirReactionSetup.SandFallsSlow);

    /// <summary>Maridia fast-sandfall inside reaction at <c>$84:B727/$B4B6</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandFastFallInside = new(PlmHeaderId.InsideReactionSandFallsFast, SpecialAirReactionSetup.SandFallsFast);

    /// <summary>Maridia quicksand-surface collision reaction at <c>$84:B72B/$B4C4</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSurfaceCollision = new(PlmHeaderId.CollisionReactionQuicksandSurface, SpecialAirReactionSetup.QuicksandSurfaceCollision);

    /// <summary>Maridia submerging-quicksand collision reaction at <c>$84:B737/$B541</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSubmergingCollision = new(PlmHeaderId.CollisionReactionSubmergingQuicksand, SpecialAirReactionSetup.SubmergingQuicksandCollision);

    /// <summary>Maridia slow-sandfall collision reaction at <c>$84:B73B/$B54F</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSlowFallCollision = new(PlmHeaderId.CollisionReactionSandFallsSlow, SpecialAirReactionSetup.SandFallsCollision);

    /// <summary>Maridia fast-sandfall collision reaction at <c>$84:B73F/$B54F</c>.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandFastFallCollision = new(PlmHeaderId.CollisionReactionSandFallsFast, SpecialAirReactionSetup.SandFallsCollision);

    /// <summary>Brinstar slow respawning Speed Booster block at <c>$84:D030/$CDEA</c>.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSlowSpeedBlockRespawning = new(PlmHeaderId.SpeedBlockBrinstarSlowRespawning, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar slow permanent Speed Booster block at <c>$84:D034/$CDEA</c>.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSlowSpeedBlockPermanent = new(PlmHeaderId.SpeedBlockBrinstarSlowPermanent, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar Dachora respawning Speed Booster block at <c>$84:D03C/$CDEA</c>.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarDachoraSpeedBlock = new(PlmHeaderId.SpeedBlockDachoraRespawning, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar permanent Speed Booster block at <c>$84:D040/$CDEA</c>.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSpeedBlockPermanent = new(PlmHeaderId.SpeedBlockPermanent, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Lower Norfair Chozo-hand collision trigger at <c>$84:D6DA/$D18F</c>.</summary>
    public static readonly SpecialAirReactionDefinition LowerNorfairChozoHand = new(PlmHeaderId.CollisionLowerNorfairChozoHandCheck, SpecialAirReactionSetup.LowerNorfairChozoHand);

    /// <summary>Wrecked Ship Chozo-hand collision trigger at <c>$84:D6F2/$D620</c>.</summary>
    public static readonly SpecialAirReactionDefinition WreckedShipChozoHand = new(PlmHeaderId.CollisionWreckedShipChozoHandCheck, SpecialAirReactionSetup.WreckedShipChozoHand);
}
