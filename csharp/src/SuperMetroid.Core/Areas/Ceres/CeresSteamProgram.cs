namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres steam vents' typed programs (#641), one per bank-$A6 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="CeresSteamAnimations"/>.
/// </summary>
internal enum CeresSteamProgram
{
    /// <summary><c>InstList_CeresSteam_Up_0</c> at $A6:F04D.</summary>
    Up,
    /// <summary>The loop at $A6:F059.</summary>
    UpLoop,
    /// <summary>The loop at $A6:F061.</summary>
    UpLoop2,
    /// <summary><c>InstList_CeresSteam_Left_0</c> at $A6:F081.</summary>
    Left,
    /// <summary>The loop at $A6:F08D.</summary>
    LeftLoop,
    /// <summary>The loop at $A6:F095.</summary>
    LeftLoop2,
    /// <summary><c>InstList_CeresSteam_Down_0</c> at $A6:F0B5.</summary>
    Down,
    /// <summary>The loop at $A6:F0C1.</summary>
    DownLoop,
    /// <summary>The loop at $A6:F0C9.</summary>
    DownLoop2,
    /// <summary><c>InstList_CeresSteam_Right_0</c> at $A6:F0E9.</summary>
    Right,
    /// <summary>The loop at $A6:F0F5.</summary>
    RightLoop,
    /// <summary>The loop at $A6:F0FD.</summary>
    RightLoop2,
}
