namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Bank-$84 pre-instructions the Speed Booster escape PLM installs.</summary>
public enum SpeedBoosterEscapePreInstruction : ushort
{
    /// <summary>No pre-instruction installed.</summary>
    None = 0,

    /// <summary><c>$84:B7EF</c>: wait for Speed Booster, then start the lavaquake.</summary>
    WaitForSpeedBooster = 0xb7ef,

    /// <summary><c>$84:B82A</c>: wait until Samus has run far enough left.</summary>
    WaitForSamusLeft = 0xb82a,

    /// <summary><c>$84:B846</c>: advance lava stages as Samus continues left.</summary>
    AdvanceLava = 0xb846,
}
