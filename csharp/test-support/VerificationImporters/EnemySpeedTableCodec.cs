using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec of the cartridge's shared enemy speed tables (#1472): eight bytes per row, the positive speed in the
/// first four and its negation in the last four. The linear table stores each speed whole word first, the quadratic
/// table fraction word first. The game holds typed rows; this is the one place their layout is known, for checks.
/// </summary>
[EncodingOwner("cartridge enemy speed tables")]
internal static class EnemySpeedTableCodec
{
    /// <summary>The linear table in the enemy bank.</summary>
    public const int LinearTable = 0xa08187;

    /// <summary>The Ripper bank's copy of the linear table.</summary>
    public const int LinearRipperCopy = 0xa28187;

    /// <summary>The quadratic table in the enemy bank.</summary>
    public const int QuadraticTable = 0xa0838f;

    /// <summary>The enemy-projectile copy of the quadratic table.</summary>
    public const int QuadraticProjectileCopy = 0xa0cbc7;

    /// <summary>The bytes of one row.</summary>
    private const int RowBytes = 8;

    /// <summary>Where a row's negative speed starts.</summary>
    private const int NegativeOffset = 4;

    /// <summary>The word after a speed's first word.</summary>
    private const int SecondWordOffset = 2;

    /// <summary>Linear row <paramref name="row"/> of the table at <paramref name="table"/>.</summary>
    public static EnemyLinearSpeedRow LinearRow(IImportCartridgeSource cartridge, int table, int row)
    {
        int start = table + row * RowBytes;
        return new EnemyLinearSpeedRow(LinearSpeed(cartridge, start), LinearSpeed(cartridge, start + NegativeOffset));
    }

    /// <summary>Quadratic row <paramref name="row"/> of the table at <paramref name="table"/>.</summary>
    public static EnemyQuadraticSpeedRow QuadraticRow(IImportCartridgeSource cartridge, int table, int row)
    {
        int start = table + row * RowBytes;
        return new EnemyQuadraticSpeedRow(QuadraticSpeed(cartridge, start), QuadraticSpeed(cartridge, start + NegativeOffset));
    }

    /// <summary>The linear row whose speeds start <paramref name="byteOffset"/> bytes into the table, as native enemy state records it.</summary>
    /// <exception cref="InvalidDataException">The offset does not start a row.</exception>
    public static EnemyLinearSpeedRow LinearRowAtOffset(int byteOffset) => byteOffset % RowBytes == 0
        ? EnemyLinearSpeeds.Row(byteOffset / RowBytes)
        : throw new InvalidDataException($"Linear speed-table offset {byteOffset} does not start a row.");

    /// <summary>
    /// The coarse speed of quadratic row <paramref name="row"/>: the word one byte into the speed, the fraction's high
    /// byte under the whole pixels' low byte, which the cartridge reads for jump heights and wing orbits.
    /// </summary>
    public static int QuadraticCoarseSpeed(IImportCartridgeSource cartridge, int row, EnemySpeedSign sign) =>
        cartridge.ReadCartridgeWord(QuadraticTable + row * RowBytes + (sign == EnemySpeedSign.Negative ? NegativeOffset : 0) + 1);

    /// <summary>A linear speed: the signed whole word, then the fraction word.</summary>
    private static SignedSixteenSixteen LinearSpeed(IImportCartridgeSource cartridge, int address) =>
        SignedSixteenSixteen.FromParts(unchecked((short)cartridge.ReadCartridgeWord(address)),
            cartridge.ReadCartridgeWord(address + SecondWordOffset));

    /// <summary>A quadratic speed: the fraction word, then the whole word.</summary>
    private static int QuadraticSpeed(IImportCartridgeSource cartridge, int address) =>
        unchecked((int)SixteenSixteen.Compose(cartridge.ReadCartridgeWord(address + SecondWordOffset),
            cartridge.ReadCartridgeWord(address)));
}
