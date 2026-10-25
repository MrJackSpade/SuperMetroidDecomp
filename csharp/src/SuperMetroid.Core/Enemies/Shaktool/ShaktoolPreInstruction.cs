namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The five routines Shaktool's segments run before their instructions.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum ShaktoolPreInstruction : ushort
{
    /// <summary><c>$AA:DCAA</c>, native <c>RTS_AADCAA</c>: suspends segment motion during the collision-response instruction sequence until its reset callback restores the segment functions.</summary>
    IdleAfterAttack = 1,
    /// <summary><c>$AA:DCAB</c>, native <c>RTS_AADCAB</c>: no-op function for the first, anchoring saw piece; this is not the central head's orientation routine.</summary>
    IdleHead,
    /// <summary><c>$AA:DCAC</c>, native <c>Function_Shaktool_ArmPiece_SetPosition_HandleCurling</c>: positions an arm relative to the preceding record, then advances its neighbor angle according to the curling flags.</summary>
    OrbitPreviousSegment,
    /// <summary><c>$AA:DCD7</c>, native <c>Function_Shaktool_Head</c>: performs the arm motion for the middle record and selects one of eight head animations from the adjacent segment angles and flip flag.</summary>
    OrbitAndOrientCenter,
    /// <summary><c>$AA:DD25</c>, native <c>Function_Shaktool_FinalPiece</c>: collision-tests the final saw's orbital movement, updates the chain's curl state, and reverses its ends when obstructed.</summary>
    DriveTailAndReverseAtWalls,
}
