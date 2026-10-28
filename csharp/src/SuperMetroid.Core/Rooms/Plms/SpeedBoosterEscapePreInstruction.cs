namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Pre-instructions the Speed Booster escape PLM installs. The values are arbitrary (#1452); verification's native view
/// names each member's routine word.
/// </summary>
public enum SpeedBoosterEscapePreInstruction : ushort
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
