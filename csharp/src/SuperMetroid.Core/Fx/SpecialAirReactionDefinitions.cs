using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Area-dependent special-air dispatch expressed as named reaction cases, without stored tables.
/// The inside and collision dispatches select sixteen entries for each
/// of seven retail areas; each selected PLM header supplies its setup callback.
/// BTS callers clear bit seven before selection. Preserve distinct no-op header identities,
/// repeated quicksand-surface cases, and rejection of non-retail areas or indexes above fifteen.
/// These are engine dispatch rules, not editable room or presentation assets.
/// </summary>
public static class SpecialAirReactionDefinitions
{
    /// <summary>Each retail area owns sixteen authored BTS entries in both dispatches.</summary>
    public const int EntriesPerArea = 16;

    /// <summary>The empty PLM entry, whose setup does nothing.</summary>
    public static readonly SpecialAirReactionDefinition Nothing = new(PlmHeaderId.Nothing, SpecialAirReactionSetup.Nothing);

    /// <summary>
    /// The collision-reaction entry whose setup clears the
    /// carry.
    /// </summary>
    public static readonly SpecialAirReactionDefinition ClearCarry = new(PlmHeaderId.CollisionReactionClearCarry, SpecialAirReactionSetup.ClearCarry);

    /// <summary>Norfair inside-reaction no-op for BTS <c>$80</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing80 = new(PlmHeaderId.InsideReactionNothingFirst, SpecialAirReactionSetup.Nothing);

    /// <summary>Norfair inside-reaction no-op for BTS <c>$81</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing81 = new(PlmHeaderId.InsideReactionNothingSecond, SpecialAirReactionSetup.Nothing);

    /// <summary>Norfair inside-reaction no-op for BTS <c>$82</c>.</summary>
    public static readonly SpecialAirReactionDefinition NorfairInsideNothing82 = new(PlmHeaderId.InsideReactionNothingThird, SpecialAirReactionSetup.Nothing);

    /// <summary>
    /// The Brinstar floor plant's inside reaction and its
    /// setup.
    /// </summary>
    public static readonly SpecialAirReactionDefinition BrinstarFloorPlant = new(PlmHeaderId.InsideReactionBrinstarFloorPlant, SpecialAirReactionSetup.BrinstarFloorPlant);

    /// <summary>
    /// The Brinstar ceiling plant's inside reaction and its
    /// setup.
    /// </summary>
    public static readonly SpecialAirReactionDefinition BrinstarCeilingPlant = new(PlmHeaderId.InsideReactionBrinstarCeilingPlant, SpecialAirReactionSetup.BrinstarCeilingPlant);

    /// <summary>
    /// The Crateria inside reaction for BTS <c>$80</c> and its
    /// ice-physics setup.
    /// </summary>
    public static readonly SpecialAirReactionDefinition CrateriaIcePhysics = new(PlmHeaderId.InsideReactionCrateria80, SpecialAirReactionSetup.IcePhysics);

    /// <summary>Maridia quicksand-surface inside reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSurfaceInside = new(PlmHeaderId.InsideReactionQuicksandSurface, SpecialAirReactionSetup.QuicksandSurface);

    /// <summary>Maridia submerging-quicksand inside reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSubmergingInside = new(PlmHeaderId.InsideReactionSubmergingQuicksand, SpecialAirReactionSetup.SubmergingQuicksand);

    /// <summary>Maridia slow-sandfall inside reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSlowFallInside = new(PlmHeaderId.InsideReactionSandFallsSlow, SpecialAirReactionSetup.SandFallsSlow);

    /// <summary>Maridia fast-sandfall inside reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandFastFallInside = new(PlmHeaderId.InsideReactionSandFallsFast, SpecialAirReactionSetup.SandFallsFast);

    /// <summary>Maridia quicksand-surface collision reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSurfaceCollision = new(PlmHeaderId.CollisionReactionQuicksandSurface, SpecialAirReactionSetup.QuicksandSurfaceCollision);

    /// <summary>Maridia submerging-quicksand collision reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSubmergingCollision = new(PlmHeaderId.CollisionReactionSubmergingQuicksand, SpecialAirReactionSetup.SubmergingQuicksandCollision);

    /// <summary>Maridia slow-sandfall collision reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSlowFallCollision = new(PlmHeaderId.CollisionReactionSandFallsSlow, SpecialAirReactionSetup.SandFallsCollision);

    /// <summary>Maridia fast-sandfall collision reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandFastFallCollision = new(PlmHeaderId.CollisionReactionSandFallsFast, SpecialAirReactionSetup.SandFallsCollision);

    /// <summary>Brinstar slow respawning Speed Booster block.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSlowSpeedBlockRespawning = new(PlmHeaderId.SpeedBlockBrinstarSlowRespawning, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar slow permanent Speed Booster block.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSlowSpeedBlockPermanent = new(PlmHeaderId.SpeedBlockBrinstarSlowPermanent, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar Dachora respawning Speed Booster block.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarDachoraSpeedBlock = new(PlmHeaderId.SpeedBlockDachoraRespawning, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar permanent Speed Booster block.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSpeedBlockPermanent = new(PlmHeaderId.SpeedBlockPermanent, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Lower Norfair Chozo-hand collision trigger.</summary>
    public static readonly SpecialAirReactionDefinition LowerNorfairChozoHand = new(PlmHeaderId.CollisionLowerNorfairChozoHandCheck, SpecialAirReactionSetup.LowerNorfairChozoHand);

    /// <summary>Wrecked Ship Chozo-hand collision trigger.</summary>
    public static readonly SpecialAirReactionDefinition WreckedShipChozoHand = new(PlmHeaderId.CollisionWreckedShipChozoHandCheck, SpecialAirReactionSetup.WreckedShipChozoHand);
}
