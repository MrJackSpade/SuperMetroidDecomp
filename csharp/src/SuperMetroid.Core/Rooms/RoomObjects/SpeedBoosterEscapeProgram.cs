namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster lavaquake's three PLM phases (#1296), each ending in the sleep its pre-instruction ends.</summary>
internal enum SpeedBoosterEscapeProgram
{
    /// <summary>Waits until Samus has the Speed Booster.</summary>
    WaitForSpeedBooster,
    /// <summary>Waits until Samus runs left far enough to start the lava.</summary>
    WaitForSamusLeft,
    /// <summary>Raises the lava stage by stage until Samus escapes.</summary>
    AdvanceLava,
}
