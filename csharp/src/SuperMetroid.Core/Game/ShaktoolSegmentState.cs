using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

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

/// <summary>
/// Typed view over one of Shaktool's seven consecutive enemy records. The properties are
/// deliberately backed by native variables A-F instead of duplicated host state, so a C#
/// debugger shows exactly the words that the 65816 implementation would mutate.
/// </summary>
public sealed class ShaktoolSegmentState
{
    private readonly RoomEnemySlot _slot;

    internal ShaktoolSegmentState(RoomEnemySlot slot) => _slot = slot;
    /// <summary>Native <c>facingAngle</c> at <c>$0FA8 + enemy index</c>, stored in variable A: the chain's target heading, also advanced during straightening and used by attack movement instructions; one wrapped 16-bit turn is <c>$10000</c>.</summary>
    public ushort TargetAngle
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }
    /// <summary>Native <c>neighborAngle</c> at <c>$0FAA + enemy index</c>, stored in variable B: the angle around the immediately preceding record, in wrapped 16-bit turns; its high byte indexes the 256-angle displacement table.</summary>
    public ushort OrbitAngle
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }
    /// <summary>Native <c>neighborAngleDelta</c> at <c>$0FAC + enemy index</c>, stored in variable C: the unsigned orbital increment per gameplay update in 1/65536-turn units; movement flags determine its sign, and straightening substitutes <c>$0100</c>.</summary>
    public ushort AngularVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }
    /// <summary>Native role-dependent word at <c>$0FAE + enemy index</c>, stored in variable D: the central head uses its low byte for eight orientation buckets and bit 15 for flipping; the final saw instead accumulates angular increments to detect full curling at <c>$F000</c>.</summary>
    public ushort OrientationAndAcceleration
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }
    /// <summary>Native <c>primaryPieceEnemyIndex</c> at <c>$0FB0 + enemy index</c>, stored in variable E: the byte-offset enemy index of the first record in the seven-piece chain, whose consecutive records are <c>$40</c> bytes apart.</summary>
    public ushort OwnerNativeIndex
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }
    /// <summary>Native <c>function</c> at <c>$0FB2 + enemy index</c>, stored in variable F: the bank-$AA segment motion callback run by Shaktool's main AI; collision-response instructions temporarily replace it with the no-op attack function.</summary>
    public ShaktoolPreInstruction PreInstruction
    {
        get => (ShaktoolPreInstruction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

[Flags]
internal enum ShaktoolMotionFlags : ushort
{
    ReversedOnce = 0x2000,
    Straightening = 0x4000,
    Clockwise = 0x8000,
}
