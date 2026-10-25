using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Development-tool reads of a pose record's movement byte, for traces and checks that compare cartridge bytes.</summary>
internal static class SamusPoseRecordCodecTooling
{
    /// <summary>
    /// The adjacent-code poses whose record movement byte lies beyond the retail movement dispatcher: the opcodes of
    /// the $91:BE11 X-ray HDMA routine.
    /// </summary>
    private static readonly FrozenDictionary<SamusPoseId, byte> IrregularMovementBytes = new Dictionary<SamusPoseId, byte>
    {
        [SamusPoseId.AdjacentCodePoseFD] = 0x8b,
        [SamusPoseId.AdjacentCodePoseFF] = 0x38,
    }.ToFrozenDictionary();

    /// <summary>Record byte one, the pose's movement type as the cartridge's movement dispatcher indexes it.</summary>
    internal static byte MovementByte(SamusPoseId pose) =>
        IrregularMovementBytes.TryGetValue(pose, out byte irregular)
            ? irregular
            : SamusPoseRecordCodec.MovementByte(SamusPoseDispatchDefinitions.ReadMovement(pose));
}
