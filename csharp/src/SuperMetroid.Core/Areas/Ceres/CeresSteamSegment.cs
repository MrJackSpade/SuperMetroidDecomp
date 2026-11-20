namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' animations (#641, #1826), for each way a vent blows: its dormant wait before its first puff, its
/// rest between puffs and its puff. A resting vent puffs next and a puffing vent rests next, for good. Their steps are
/// <see cref="CeresSteamAnimations"/>.
/// </summary>
internal enum CeresSteamSegment
{
    /// <summary>An upward vent hiding before its first puff, which its owner then keeps hidden for its drawn wait.</summary>
    UpDormant,
    /// <summary>An upward vent hidden for its rest between puffs (<see cref="CeresSteamAnimations.RestUpdates"/>), then shown.</summary>
    UpResting,
    /// <summary>An upward vent's puff of steam: its pictures, each for <see cref="CeresSteamAnimations.PuffPictureUpdates"/>.</summary>
    UpPuffing,
    /// <summary>A leftward vent hiding before its first puff, which its owner then keeps hidden for its drawn wait.</summary>
    LeftDormant,
    /// <summary>A leftward vent hidden for its rest between puffs (<see cref="CeresSteamAnimations.RestUpdates"/>), then shown.</summary>
    LeftResting,
    /// <summary>A leftward vent's puff of steam: its pictures, each for <see cref="CeresSteamAnimations.PuffPictureUpdates"/>.</summary>
    LeftPuffing,
    /// <summary>A downward vent hiding before its first puff, which its owner then keeps hidden for its drawn wait.</summary>
    DownDormant,
    /// <summary>A downward vent hidden for its rest between puffs (<see cref="CeresSteamAnimations.RestUpdates"/>), then shown.</summary>
    DownResting,
    /// <summary>A downward vent's puff of steam: its pictures, each for <see cref="CeresSteamAnimations.PuffPictureUpdates"/>.</summary>
    DownPuffing,
    /// <summary>A rightward vent hiding before its first puff, which its owner then keeps hidden for its drawn wait.</summary>
    RightDormant,
    /// <summary>A rightward vent hidden for its rest between puffs (<see cref="CeresSteamAnimations.RestUpdates"/>), then shown.</summary>
    RightResting,
    /// <summary>A rightward vent's puff of steam: its pictures, each for <see cref="CeresSteamAnimations.PuffPictureUpdates"/>.</summary>
    RightPuffing,
}
