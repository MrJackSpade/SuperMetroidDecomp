namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Bank-$AA function pointers stored in Shaktool's native variable F. Keeping the raw
/// addresses visible makes debugger watches line up with the ROM while giving each of the
/// five actually-used routines a domain name.
/// </summary>
public enum ShaktoolPreInstruction : ushort
{
    /// <summary><c>$AA:DCAA</c>, native <c>RTS_AADCAA</c>: suspends segment motion during the collision-response instruction sequence until its reset callback restores the segment functions.</summary>
    IdleAfterAttack = 0xdcaa,
    /// <summary><c>$AA:DCAB</c>, native <c>RTS_AADCAB</c>: no-op function for the first, anchoring saw piece; this is not the central head's orientation routine.</summary>
    IdleHead = 0xdcab,
    /// <summary><c>$AA:DCAC</c>, native <c>Function_Shaktool_ArmPiece_SetPosition_HandleCurling</c>: positions an arm relative to the preceding record, then advances its neighbor angle according to the curling flags.</summary>
    OrbitPreviousSegment = 0xdcac,
    /// <summary><c>$AA:DCD7</c>, native <c>Function_Shaktool_Head</c>: performs the arm motion for the middle record and selects one of eight head animations from the adjacent segment angles and flip flag.</summary>
    OrbitAndOrientCenter = 0xdcd7,
    /// <summary><c>$AA:DD25</c>, native <c>Function_Shaktool_FinalPiece</c>: collision-tests the final saw's orbital movement, updates the chain's curl state, and reverses its ends when obstructed.</summary>
    DriveTailAndReverseAtWalls = 0xdd25,
}
