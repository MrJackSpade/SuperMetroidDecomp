namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres Baby Metroid's behaviour while Ridley holds, drops or retrieves it.
/// Its values are arbitrary; <see cref="None"/> is the default of a workspace that was never initialized (#1415).
/// </summary>
public enum CeresBabyAction
{
    /// <summary>Outside Ceres the Baby workspace is never initialized and holds no behaviour.</summary>
    None,
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
