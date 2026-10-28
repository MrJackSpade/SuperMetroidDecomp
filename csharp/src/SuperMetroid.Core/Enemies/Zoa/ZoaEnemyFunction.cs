namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The routines Zoa's AI dispatches.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum ZoaEnemyFunction
{
    /// <summary>$A3:B482, Function_Zoa_WaitForSamusToGetNear: remain hidden until Samus is fewer than 128 horizontal pixels away, then select the facing rise list.</summary>
    WaitForSamus = 1,
    /// <summary>$A3:B4A8, Function_Zoa_Rising: become visible and move upward 0.5 pixel per NTSC AI update while Samus is above; reaching her height selects horizontal launch.</summary>
    Rising,
    /// <summary>$A3:B4D6, Function_Zoa_Shooting: launch the enemy itself horizontally using instruction-controlled speeds; once its center leaves the camera square, hide and restore its spawn whole coordinates.</summary>
    Shooting,
}
