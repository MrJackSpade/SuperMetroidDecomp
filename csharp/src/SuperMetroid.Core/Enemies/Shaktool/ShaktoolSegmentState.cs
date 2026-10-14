namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Typed view over one of Shaktool's seven consecutive enemy records. The properties are
/// deliberately backed by native variables A-F instead of duplicated host state, so a C#
/// debugger shows exactly the words that the 65816 implementation would mutate.
/// </summary>
public sealed class ShaktoolSegmentState
{
    /// <summary>Physical enemy record whose variables A through F carry the segment's native state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view over an existing Shaktool enemy record without copying its variables.</summary>
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
