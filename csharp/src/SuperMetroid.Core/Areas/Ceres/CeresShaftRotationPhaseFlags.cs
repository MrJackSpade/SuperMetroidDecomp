namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Bits of the Ceres elevator shaft's rotation phase in <c>RoomMainASMVar1</c>, stepped by room main <c>$89:ACD0</c>.</summary>
internal static class CeresShaftRotationPhaseFlags
{
    /// <summary>
    /// $8000, set while the sweep runs backwards. The low bits then count down from 68 to 1, and the phase never
    /// holds the bare flag.
    /// </summary>
    internal const ushort Reversing = 0x8000;
}
