using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The codec between a pose's typed definition and the bytes of its cartridge pose record, for tools that write native
/// traces and the checks that compare them with the cartridge's records (#1551). The game never
/// reads these bytes.
/// </summary>
[EncodingOwner("Samus pose record facing, movement and aim bytes")]
internal static class SamusPoseRecordCodecTooling
{
    /// <summary>
    /// The adjacent-code poses whose record movement byte lies beyond the retail movement dispatcher: the opcodes of
    /// the $91:BE11 X-ray HDMA routine.
    /// </summary>
    private static readonly FrozenDictionary<SamusPoseId, byte> IrregularMovementBytes = new Dictionary<SamusPoseId, byte>
    {
        [SamusPoseId.TallStillRightPose] = 0x8b,
        [SamusPoseId.ForwardStillCrouchHeightPose] = 0x38,
    }.ToFrozenDictionary();

    /// <summary>
    /// The adjacent-code poses whose record facing byte is neither forward, left nor right: the neighbouring code's
    /// bytes, which the game's facing tests read as facing neither way.
    /// </summary>
    private static readonly FrozenDictionary<SamusPoseId, byte> IrregularFacingBytes = new Dictionary<SamusPoseId, byte>
    {
        [SamusPoseId.ForwardStandingPose] = 0x29,
        [SamusPoseId.ForwardStillCrouchHeightPose] = 0x12,
    }.ToFrozenDictionary();

    /// <summary>The facing byte of a pose that faces neither way.</summary>
    private const byte ForwardFacingByte = 0x00;

    /// <summary>The facing byte of a pose that faces left.</summary>
    private const byte LeftFacingByte = 0x04;

    /// <summary>The facing byte of a pose that faces right.</summary>
    private const byte RightFacingByte = 0x08;


    /// <summary>Record byte one, the pose's movement type as the cartridge's movement dispatcher indexes it: the movement byte that names the pose's movement type.</summary>
    internal static byte MovementByte(SamusPoseId pose) =>
        IrregularMovementBytes.TryGetValue(pose, out byte irregular)
            ? irregular
            : SamusMovementTypeNativeWords.Byte(SamusPoseTraits.ReadMovement(pose));

    /// <summary>Record byte zero, the pose's facing as the cartridge stores it: zero forward, four left, eight right.</summary>
    internal static byte FacingByte(SamusPoseId pose) =>
        IrregularFacingBytes.TryGetValue(pose, out byte irregular)
            ? irregular
            : SamusPoseMetadata.ReadFacingDirection(pose) switch
            {
                SamusFacingDirection.ForwardOrSpecial => ForwardFacingByte,
                SamusFacingDirection.Left => LeftFacingByte,
                SamusFacingDirection.Right => RightFacingByte,
                var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusFacingDirection)} {undefined}."),
            };
}
