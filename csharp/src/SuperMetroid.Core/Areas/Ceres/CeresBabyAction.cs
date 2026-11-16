namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres Baby Metroid's behaviour while Ridley holds, drops or retrieves it. Its values are arbitrary; the Baby starts
/// in his arms (#2228).
/// </summary>
public enum CeresBabyAction
{
    /// <summary>The Baby stays at Ridley's arm anchor.</summary>
    CarriedInArms,
    /// <summary>The Baby follows Ridley's grasp point.</summary>
    CarriedInFeet,
    /// <summary>Clears the fall state and starts falling.</summary>
    Drop,
    /// <summary>The Baby falls under gravity until it lands.</summary>
    Dropped,
    /// <summary>The idle action after landing.</summary>
    Idle,
}
