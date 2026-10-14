namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The seven fixed bank-<c>$A6</c> Ceres Mode 7 transfer lists.</summary>
public enum CeresMode7TransferList : ushort
{
    /// <summary>$A6:F904, Ceres elevator landing-platform light frame.</summary>
    ElevatorLight = 0xf904,
    /// <summary>$A6:F90E, Ceres elevator landing-platform dark frame.</summary>
    ElevatorDark = 0xf90e,
    /// <summary>$A6:ACE2, baby-capsule tilemap frame zero.</summary>
    BabyFrame0 = 0xace2,
    /// <summary>$A6:ACF5, baby-capsule tilemap frame one (also frame three).</summary>
    BabyFrame1 = 0xacf5,
    /// <summary>$A6:AD08, baby-capsule tilemap frame two.</summary>
    BabyFrame2 = 0xad08,
    /// <summary>$A6:AD49, Ridley wing tilemap frame zero.</summary>
    WingFrame0 = 0xad49,
    /// <summary>$A6:AD80, Ridley wing tilemap frame one.</summary>
    WingFrame1 = 0xad80,
}
