using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The codec between a pose's typed definition and the bytes of its cartridge pose record, for tools that write native
/// traces and for checks that compare the game's typed pose reads with the cartridge's records (#1551). The game never
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
        [SamusPoseId.AdjacentCodePoseFD] = 0x8b,
        [SamusPoseId.AdjacentCodePoseFF] = 0x38,
    }.ToFrozenDictionary();

    /// <summary>
    /// The adjacent-code poses whose record facing byte is neither forward, left nor right: the neighbouring code's
    /// bytes, which the game's facing tests read as facing neither way.
    /// </summary>
    private static readonly FrozenDictionary<SamusPoseId, byte> IrregularFacingBytes = new Dictionary<SamusPoseId, byte>
    {
        [SamusPoseId.AdjacentCodePoseFE] = 0x29,
        [SamusPoseId.AdjacentCodePoseFF] = 0x12,
    }.ToFrozenDictionary();

    /// <summary>The aim byte of a pose that disables shooting.</summary>
    private const int ShootingDisabledByte = 0xff;

    /// <summary>The bits of an aim byte the charge flare's placement tables are indexed by.</summary>
    private const int FlareSelectionBits = 0x0f;

    /// <summary>Record byte one, the pose's movement type as the cartridge's movement dispatcher indexes it: the movement byte that names the pose's movement type.</summary>
    internal static byte MovementByte(SamusPoseId pose) =>
        IrregularMovementBytes.TryGetValue(pose, out byte irregular)
            ? irregular
            : SamusMovementTypeNativeWords.Byte(SamusPoseDispatch.ReadMovement(pose));

    /// <summary>Record byte zero, the pose's facing as the cartridge stores it: zero forward, four left, eight right.</summary>
    internal static byte FacingByte(SamusPoseId pose) =>
        IrregularFacingBytes.TryGetValue(pose, out byte irregular)
            ? irregular
            : SamusPoseMetadata.ReadFacingDirection(pose) switch
            {
                SamusFacingDirection.ForwardOrSpecial => 0x00,
                SamusFacingDirection.Left => 0x04,
                SamusFacingDirection.Right => 0x08,
                var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusFacingDirection)} {undefined}."),
            };

    /// <summary>
    /// The facing the game's facing tests read from record byte zero: four is left, eight is right, and every other
    /// byte faces neither way.
    /// </summary>
    internal static SamusFacingDirection DecodeFacing(int facingByte) => facingByte switch
    {
        0x04 => SamusFacingDirection.Left,
        0x08 => SamusFacingDirection.Right,
        _ => SamusFacingDirection.ForwardOrSpecial,
    };

    /// <summary>
    /// The aim the game's readers take from record byte three: an aim index fires in its direction; the disabled byte
    /// fires nothing and draws no flare; any other byte, the three turning restrictions among them, fires nothing and
    /// places a held charge's flare as its low four bits select, a muzzle or a turning placement.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The byte's low four bits select past the turning placements.</exception>
    internal static SamusPoseAim DecodeAim(int aimByte) =>
        aimByte == ShootingDisabledByte ? SamusPoseAim.ShootingDisabled
        : DirectionOfAimByte(aimByte) is { } direction ? SamusPoseAim.Aiming(direction)
        : SamusPoseAim.Unaimed(DecodeFlare(aimByte & FlareSelectionBits));

    /// <summary>
    /// The direction an aim byte names when it is one of the ten aim indices, or null for any other byte: a restriction,
    /// the disabled byte or code read past the authored poses. Native transition-shot words carry the same byte.
    /// </summary>
    internal static SamusProjectileDirection? DirectionOfAimByte(int aimByte) =>
        aimByte is >= 0 and < SamusProjectileDirectionCodec.Count ? SamusProjectileDirectionCodec.FromAimIndex(aimByte) : null;

    /// <summary>The flare placement an aim byte's low four bits select.</summary>
    private static SamusFlareAim DecodeFlare(int selection) =>
        DirectionOfAimByte(selection) is { } muzzle ? SamusFlareAim.AtMuzzle(muzzle)
        : selection - SamusProjectileDirectionCodec.Count is >= 0 and < 3
            ? SamusFlareAim.WhileTurning(TurningAim(selection - SamusProjectileDirectionCodec.Count))
            : throw new ArgumentOutOfRangeException(nameof(selection), selection, "No flare placement follows the turning placements.");

    /// <summary>The turning aim at <paramref name="index"/> in placement order: up, neutral, down.</summary>
    private static SamusTurningAim TurningAim(int index) => index switch
    {
        0 => SamusTurningAim.Up,
        1 => SamusTurningAim.Neutral,
        2 => SamusTurningAim.Down,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Three turning aims."),
    };
}
