namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Pre-instructions the Speed Booster escape PLM installs. The values are arbitrary (#1452); verification's view
/// names each member's routine word.
/// </summary>
public enum SpeedBoosterEscapeFrameBehavior
{
    /// <summary>No pre-instruction installed.</summary>
    None = 0,

    /// <summary>Wait for Speed Booster, then start the lavaquake.</summary>
    WaitForSpeedBooster = 1,

    /// <summary>Wait until Samus has run far enough left.</summary>
    WaitForSamusLeft = 2,

    /// <summary>Advance lava stages as Samus continues left.</summary>
    AdvanceLava = 3,
}
