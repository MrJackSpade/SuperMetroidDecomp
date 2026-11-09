namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where the Speed Booster escape's lavaquake is in its course (<see cref="SpeedBoosterEscapeObject"/>).</summary>
internal enum SpeedBoosterEscapePhase
{
    /// <summary>Spawned: it starts watching on its next update.</summary>
    Starting,
    /// <summary>Waiting for Samus to have the Speed Booster, which starts the lavaquake.</summary>
    WaitingForSpeedBooster,
    /// <summary>Waiting for Samus to run far enough left, which starts the lava moving.</summary>
    WaitingForSamusLeft,
    /// <summary>Raising the lava rise by rise as Samus escapes left, then marking the escape.</summary>
    AdvancingLava,
}
