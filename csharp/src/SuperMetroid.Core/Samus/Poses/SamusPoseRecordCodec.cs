using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The codec between a pose's typed facing and movement and the bytes of its cartridge pose-definition record
/// ($91:B629, eight bytes per pose): facing byte zero and movement byte one, and the packed direction/movement word
/// the pose history keeps (WRAM $0A22/$0A26). The history word is gameplay-observable state (the wall-jump probe
/// reads its movement byte back, and other actors read the facing byte), so Core keeps the bytes, here and nowhere
/// else.
/// </summary>
/// <remarks>
/// The typed definitions give every pose a facing and a movement type. Four records hold facing bytes that are
/// neither left nor right (the unused glitch-ball poses $21/$22 and the adjacent-code poses $FE/$FF) and two hold
/// movement bytes beyond the retail dispatcher (the adjacent-code poses $FD/$FF, which read the $91:BE11 X-ray HDMA
/// routine's opcodes); this codec carries those bytes so the records stay exact.
/// </remarks>
[EncodingOwner("Samus pose record facing and movement bytes")]
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
            : FacingByte(SamusPoseDispatchDefinitions.ReadFacing(pose));

    /// <summary>The bits of a history word's facing byte, below its movement byte.</summary>
    private const int FacingBits = 8;

    /// <summary>The record facing byte that stores a typed facing.</summary>
    private static byte FacingByte(SamusFacingDirection facing) => facing switch
    {
        SamusFacingDirection.ForwardOrSpecial => 0x00,
        SamusFacingDirection.Left => 0x04,
        SamusFacingDirection.Right => 0x08,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusFacingDirection)} {facing}."),
    };

    /// <summary>
    /// The packed direction/movement word a committed transition records in the pose history: the facing byte, then
    /// the movement byte. A pose whose movement byte lies beyond the dispatcher fails, as reading its movement does.
    /// </summary>
    internal static ushort HistoryWord(SamusPoseId pose) =>
        (ushort)(FacingByte(pose) | MovementByte(SamusPoseDispatchDefinitions.ReadMovement(pose)) << FacingBits);

    /// <summary>The movement type a history word records, or null when its movement byte names none.</summary>
    internal static SamusMovementType? HistoryMovement(ushort word)
    {
        byte movement = unchecked((byte)(word >> FacingBits));
        return movement <= MovementByte(SamusMovementType.Special) ? (SamusMovementType)movement : null;
    }

    /// <summary>A movement type's dispatcher index: the movement types are declared in dispatcher order.</summary>
    internal static byte MovementByte(SamusMovementType movement) => (byte)movement;
}
