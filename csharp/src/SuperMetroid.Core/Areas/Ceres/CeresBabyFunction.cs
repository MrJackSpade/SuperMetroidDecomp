namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres Baby Metroid's behaviour while Ridley holds, drops or retrieves it.
/// Its values are arbitrary; <see cref="None"/> is the default of a workspace that was never initialized (#1415).
/// </summary>
public enum CeresBabyFunction
{
    /// <summary>Outside Ceres the Baby workspace is never initialized and holds no behaviour.</summary>
    None = 0,
    /// <summary>The Baby stays at Ridley's arm anchor.</summary>
    CarriedInArms = 1,
    /// <summary>The Baby follows Ridley's grasp point.</summary>
    CarriedInFeet,
    /// <summary>Clears the fall state and starts falling.</summary>
    Drop,
    /// <summary>The Baby falls under gravity until it lands.</summary>
    Dropped,
    /// <summary>The idle function after landing.</summary>
    Idle,
}
