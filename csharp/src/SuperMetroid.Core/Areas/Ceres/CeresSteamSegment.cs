namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' animations (#641, #1826), for each way a vent blows: its dormant wait before its first puff, its
/// rest between puffs and its puff. A resting vent puffs next and a puffing vent rests next, for good. Their steps are
/// <see cref="CeresSteamAnimations"/>.
/// </summary>
internal enum CeresSteamSegment
{
    /// <summary>An upward vent, hidden, counting down to its first puff.</summary>
    UpDormant,
    /// <summary>An upward vent hidden for 64 updates between puffs, then shown.</summary>
    UpResting,
    /// <summary>An upward vent's puff of steam: its seven pictures, three updates each.</summary>
    UpPuffing,
    /// <summary>A leftward vent, hidden, counting down to its first puff.</summary>
    LeftDormant,
    /// <summary>A leftward vent hidden for 64 updates between puffs, then shown.</summary>
    LeftResting,
    /// <summary>A leftward vent's puff of steam: its seven pictures, three updates each.</summary>
    LeftPuffing,
    /// <summary>A downward vent, hidden, counting down to its first puff.</summary>
    DownDormant,
    /// <summary>A downward vent hidden for 64 updates between puffs, then shown.</summary>
    DownResting,
    /// <summary>A downward vent's puff of steam: its seven pictures, three updates each.</summary>
    DownPuffing,
    /// <summary>A rightward vent, hidden, counting down to its first puff.</summary>
    RightDormant,
    /// <summary>A rightward vent hidden for 64 updates between puffs, then shown.</summary>
    RightResting,
    /// <summary>A rightward vent's puff of steam: its seven pictures, three updates each.</summary>
    RightPuffing,
}
