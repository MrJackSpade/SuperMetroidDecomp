namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' animations (#641, #1826): a vent's dormant wait before its first puff, its rest between puffs
/// and its puff. A resting vent puffs next and a puffing vent rests next, for good. The way the vent blows
/// (<see cref="CeresSteamWay"/>) chooses the pictures each plays; their steps are <see cref="CeresSteamAnimations"/>.
/// </summary>
internal enum CeresSteamSegment
{
    /// <summary>A vent hiding before its first puff, which its owner then keeps hidden for its drawn wait.</summary>
    Dormant,
    /// <summary>A vent hidden for its rest between puffs (<see cref="CeresSteamAnimations.RestUpdates"/>), then shown.</summary>
    Resting,
    /// <summary>A vent's puff of steam: its way's pictures, each for <see cref="CeresSteamAnimations.PuffPictureUpdates"/>.</summary>
    Puffing,
}
