using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Reads a cartridge pose record's facing and aim bytes as the typed pose definitions name them, for checks that compare
/// the game's typed pose reads with the cartridge's records (#1551). The game never reads these bytes.
/// </summary>
internal static class SamusPoseRecordDecoding
{
    /// <summary>The aim byte of a pose that disables shooting.</summary>
    private const int ShootingDisabledByte = 0xff;

    /// <summary>The bits of an aim byte the charge flare's placement tables are indexed by.</summary>
    private const int FlareSelectionBits = 0x0f;

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
