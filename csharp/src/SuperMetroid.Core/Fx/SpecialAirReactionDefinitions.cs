using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Area-dependent special-air dispatch expressed as named reaction cases, without stored tables.
/// The inside and collision dispatches select sixteen entries for each
/// of seven retail areas; each selected room-object header supplies its setup callback.
/// BTS callers clear bit seven before selection. Every entry that does nothing is the one no-op reaction;
/// repeated quicksand-surface cases and the rejection of non-retail areas or indexes above fifteen are preserved.
/// These are engine dispatch rules, not editable room or presentation assets.
/// </summary>
public static class SpecialAirReactionDefinitions
{
    /// <summary>Each retail area owns sixteen authored BTS entries in both dispatches.</summary>
    public const int EntriesPerArea = 16;

    /// <summary>The empty room-object entry, whose setup does nothing.</summary>
    public static readonly SpecialAirReactionDefinition Nothing = new(RoomObjectHeaderId.Nothing, SpecialAirReactionSetup.Nothing);

    /// <summary>The collision-reaction entry through which Samus passes, as through air.</summary>
    public static readonly SpecialAirReactionDefinition ClearCarry = new(RoomObjectHeaderId.CollisionReactionPassThrough, SpecialAirReactionSetup.ClearCarry);

    /// <summary>
    /// The Brinstar floor plant's inside reaction and its
    /// setup.
    /// </summary>
    public static readonly SpecialAirReactionDefinition BrinstarFloorPlant = new(RoomObjectHeaderId.InsideReactionBrinstarFloorPlant, SpecialAirReactionSetup.BrinstarFloorPlant);

    /// <summary>
    /// The Brinstar ceiling plant's inside reaction and its
    /// setup.
    /// </summary>
    public static readonly SpecialAirReactionDefinition BrinstarCeilingPlant = new(RoomObjectHeaderId.InsideReactionBrinstarCeilingPlant, SpecialAirReactionSetup.BrinstarCeilingPlant);

    /// <summary>
    /// The Crateria inside reaction for the first special air block and its
    /// ice-physics setup.
    /// </summary>
    public static readonly SpecialAirReactionDefinition CrateriaIcePhysics = new(RoomObjectHeaderId.InsideReactionCrateriaIcePhysics, SpecialAirReactionSetup.IcePhysics);

    /// <summary>Maridia quicksand-surface inside reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSurfaceInside = new(RoomObjectHeaderId.InsideReactionQuicksandSurface, SpecialAirReactionSetup.QuicksandSurface);

    /// <summary>Maridia submerging-quicksand inside reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSubmergingInside = new(RoomObjectHeaderId.InsideReactionSubmergingQuicksand, SpecialAirReactionSetup.SubmergingQuicksand);

    /// <summary>Maridia slow-sandfall inside reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSlowFallInside = new(RoomObjectHeaderId.InsideReactionSandFallsSlow, SpecialAirReactionSetup.SandFallsSlow);

    /// <summary>Maridia fast-sandfall inside reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandFastFallInside = new(RoomObjectHeaderId.InsideReactionSandFallsFast, SpecialAirReactionSetup.SandFallsFast);

    /// <summary>Maridia quicksand-surface collision reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSurfaceCollision = new(RoomObjectHeaderId.CollisionReactionQuicksandSurface, SpecialAirReactionSetup.QuicksandSurfaceCollision);

    /// <summary>Maridia submerging-quicksand collision reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSubmergingCollision = new(RoomObjectHeaderId.CollisionReactionSubmergingQuicksand, SpecialAirReactionSetup.SubmergingQuicksandCollision);

    /// <summary>Maridia slow-sandfall collision reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandSlowFallCollision = new(RoomObjectHeaderId.CollisionReactionSandFallsSlow, SpecialAirReactionSetup.SandFallsCollision);

    /// <summary>Maridia fast-sandfall collision reaction.</summary>
    public static readonly SpecialAirReactionDefinition QuicksandFastFallCollision = new(RoomObjectHeaderId.CollisionReactionSandFallsFast, SpecialAirReactionSetup.SandFallsCollision);

    /// <summary>Brinstar slow respawning Speed Booster block.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSlowSpeedBlockRespawning = new(RoomObjectHeaderId.SpeedBlockBrinstarSlowRespawning, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar slow permanent Speed Booster block.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSlowSpeedBlockPermanent = new(RoomObjectHeaderId.SpeedBlockBrinstarSlowPermanent, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar Dachora respawning Speed Booster block.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarDachoraSpeedBlock = new(RoomObjectHeaderId.SpeedBlockDachoraRespawning, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Brinstar permanent Speed Booster block.</summary>
    public static readonly SpecialAirReactionDefinition BrinstarSpeedBlockPermanent = new(RoomObjectHeaderId.SpeedBlockPermanent, SpecialAirReactionSetup.SpeedBlock);

    /// <summary>Lower Norfair Chozo-hand collision trigger.</summary>
    public static readonly SpecialAirReactionDefinition LowerNorfairChozoHand = new(RoomObjectHeaderId.CollisionLowerNorfairChozoHandCheck, SpecialAirReactionSetup.LowerNorfairChozoHand);

    /// <summary>Wrecked Ship Chozo-hand collision trigger.</summary>
    public static readonly SpecialAirReactionDefinition WreckedShipChozoHand = new(RoomObjectHeaderId.CollisionWreckedShipChozoHandCheck, SpecialAirReactionSetup.WreckedShipChozoHand);
}
