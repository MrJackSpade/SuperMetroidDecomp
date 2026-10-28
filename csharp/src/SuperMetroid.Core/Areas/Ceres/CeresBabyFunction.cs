namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres Baby Metroid's behaviour while Ridley holds, drops or retrieves it.
/// Its values are arbitrary; <see cref="None"/> is the default of a workspace that was never initialized (#1415).
/// </summary>
public enum CeresBabyFunction
{
    /// <summary>Outside Ceres the Baby workspace is never initialized and holds no behaviour.</summary>
    None = 0,
    /// <summary><c>$A6:BE9C UpdateBabyMetroidPosition_CarriedInArms</c>: the Baby stays at Ridley's arm anchor.</summary>
    CarriedInArms = 1,
    /// <summary><c>$A6:BEB3 UpdateBabyMetroidPosition_CarriedInFeet</c>: the Baby follows Ridley's grasp point.</summary>
    CarriedInFeet,
    /// <summary><c>$A6:BECA DropBabyMetroid</c>: clears the fall state and starts falling.</summary>
    Drop,
    /// <summary><c>$A6:BEDC BabyMetroidDropped</c>: the Baby falls under gravity until it lands.</summary>
    Dropped,
    /// <summary><c>$A6:BF19 RTS_A6BF19</c>: the idle function after landing.</summary>
    Idle,
}
