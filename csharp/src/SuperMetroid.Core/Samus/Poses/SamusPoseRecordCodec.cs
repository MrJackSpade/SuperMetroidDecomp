using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The codec between a pose's typed facing and its cartridge pose-record facing byte (byte zero), which other actors
/// and transitions read back as the pose's direction. The pose history keeps typed movement types (#1473); this is the
/// one place a facing becomes its byte.
/// </summary>
/// <remarks>
/// The typed definitions give every pose a facing. Four records hold facing bytes that are neither left nor right (the
/// unused glitch-ball poses $21/$22 and the adjacent-code poses $FE/$FF); this codec carries those bytes so the records
/// stay exact.
/// </remarks>
[EncodingOwner("Samus pose record facing bytes")]
internal static class SamusPoseRecordCodec
{
    /// <summary>
    /// The poses whose record facing byte is neither forward, left nor right: the unused glitch-ball poses and the
    /// adjacent-code poses, whose bytes are the neighbouring data and code.
    /// </summary>
    private static readonly FrozenDictionary<SamusPoseId, byte> IrregularFacingBytes = new Dictionary<SamusPoseId, byte>
    {
        [SamusPoseId.UnusedPose21] = 0x01,
        [SamusPoseId.UnusedPose22] = 0x02,
        [SamusPoseId.AdjacentCodePoseFE] = 0x29,
        [SamusPoseId.AdjacentCodePoseFF] = 0x12,
    }.ToFrozenDictionary();

    /// <summary>Record byte zero, the pose's facing as the cartridge stores it.</summary>
    internal static byte FacingByte(SamusPoseId pose) =>
        IrregularFacingBytes.TryGetValue(pose, out byte irregular)
            ? irregular
            : FacingByte(SamusPoseDispatch.ReadFacing(pose));

    /// <summary>The record facing byte that stores a typed facing.</summary>
    private static byte FacingByte(SamusFacingDirection facing) => facing switch
    {
        SamusFacingDirection.ForwardOrSpecial => 0x00,
        SamusFacingDirection.Left => 0x04,
        SamusFacingDirection.Right => 0x08,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusFacingDirection)} {facing}."),
    };
}
