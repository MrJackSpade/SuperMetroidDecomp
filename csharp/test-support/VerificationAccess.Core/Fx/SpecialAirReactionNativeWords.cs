namespace SuperMetroid.Core.Fx;

/// <summary>
/// Verification's native view of the special-air reactions (#1630): the bank-$84 PLM header each named reaction's area
/// dispatch selects, and the setup routine that header names. The game selects its reactions by name and never sees
/// either word; a ROM oracle compares the cartridge's area tables through these.
/// </summary>
internal static class SpecialAirReactionNativeWords
{
    /// <summary>The PLM header the inside dispatch selects for each <see cref="SpecialAirInsideReaction"/>.</summary>
    internal static readonly IReadOnlyDictionary<SpecialAirInsideReaction, ushort> InsideHeaders = new Dictionary<SpecialAirInsideReaction, ushort>
    {
        [SpecialAirInsideReaction.Nothing] = 0xb62f,
        [SpecialAirInsideReaction.IcePhysics] = 0xb70f,
        [SpecialAirInsideReaction.BrinstarFloorPlant] = 0xb6cb,
        [SpecialAirInsideReaction.BrinstarCeilingPlant] = 0xb6cf,
        [SpecialAirInsideReaction.QuicksandSurface] = 0xb713,
        [SpecialAirInsideReaction.SubmergingQuicksand] = 0xb71f,
        [SpecialAirInsideReaction.SlowSandfall] = 0xb723,
        [SpecialAirInsideReaction.FastSandfall] = 0xb727,
    };

    /// <summary>The setup routine of each <see cref="SpecialAirInsideReaction"/>'s header.</summary>
    internal static readonly IReadOnlyDictionary<SpecialAirInsideReaction, ushort> InsideSetups = new Dictionary<SpecialAirInsideReaction, ushort>
    {
        [SpecialAirInsideReaction.Nothing] = 0xb3cf,
        [SpecialAirInsideReaction.IcePhysics] = 0xb3eb,
        [SpecialAirInsideReaction.BrinstarFloorPlant] = 0xb0dc,
        [SpecialAirInsideReaction.BrinstarCeilingPlant] = 0xb113,
        [SpecialAirInsideReaction.QuicksandSurface] = 0xb408,
        [SpecialAirInsideReaction.SubmergingQuicksand] = 0xb497,
        [SpecialAirInsideReaction.SlowSandfall] = 0xb4a8,
        [SpecialAirInsideReaction.FastSandfall] = 0xb4b6,
    };

    /// <summary>The PLM header the collision dispatch selects for each <see cref="SpecialAirCollisionReaction"/>.</summary>
    internal static readonly IReadOnlyDictionary<SpecialAirCollisionReaction, ushort> CollisionHeaders = new Dictionary<SpecialAirCollisionReaction, ushort>
    {
        [SpecialAirCollisionReaction.Nothing] = 0xb62f,
        [SpecialAirCollisionReaction.PassesThrough] = 0xb633,
        [SpecialAirCollisionReaction.BrinstarSlowRespawningSpeedBlock] = 0xd030,
        [SpecialAirCollisionReaction.BrinstarSlowPermanentSpeedBlock] = 0xd034,
        [SpecialAirCollisionReaction.DachoraRespawningSpeedBlock] = 0xd03c,
        [SpecialAirCollisionReaction.PermanentSpeedBlock] = 0xd040,
        [SpecialAirCollisionReaction.LowerNorfairChozoHand] = 0xd6da,
        [SpecialAirCollisionReaction.WreckedShipChozoHand] = 0xd6f2,
        [SpecialAirCollisionReaction.QuicksandSurface] = 0xb72b,
        [SpecialAirCollisionReaction.SubmergingQuicksand] = 0xb737,
        [SpecialAirCollisionReaction.SlowSandfall] = 0xb73b,
        [SpecialAirCollisionReaction.FastSandfall] = 0xb73f,
    };

    /// <summary>
    /// The setup routine of each <see cref="SpecialAirCollisionReaction"/>'s header: <c>$84:B3D0</c> clears the carry, so
    /// Samus passes; the four Speed Booster blocks share one setup, as do the two sandfalls.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpecialAirCollisionReaction, ushort> CollisionSetups = new Dictionary<SpecialAirCollisionReaction, ushort>
    {
        [SpecialAirCollisionReaction.Nothing] = 0xb3cf,
        [SpecialAirCollisionReaction.PassesThrough] = 0xb3d0,
        [SpecialAirCollisionReaction.BrinstarSlowRespawningSpeedBlock] = 0xcdea,
        [SpecialAirCollisionReaction.BrinstarSlowPermanentSpeedBlock] = 0xcdea,
        [SpecialAirCollisionReaction.DachoraRespawningSpeedBlock] = 0xcdea,
        [SpecialAirCollisionReaction.PermanentSpeedBlock] = 0xcdea,
        [SpecialAirCollisionReaction.LowerNorfairChozoHand] = 0xd18f,
        [SpecialAirCollisionReaction.WreckedShipChozoHand] = 0xd620,
        [SpecialAirCollisionReaction.QuicksandSurface] = 0xb4c4,
        [SpecialAirCollisionReaction.SubmergingQuicksand] = 0xb541,
        [SpecialAirCollisionReaction.SlowSandfall] = 0xb54f,
        [SpecialAirCollisionReaction.FastSandfall] = 0xb54f,
    };
}
