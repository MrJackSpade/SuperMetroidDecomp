using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Each pose's number in the cartridge's pose tables (#1527): the import reads and lays out the pose tables by these
/// numbers. The numbering itself, and the pose each number names, is <see cref="SamusPoseNumbering"/>'s.
/// </summary>
internal static class SamusPoseNumbers
{
    /// <summary>Each pose's number: its position in <see cref="SamusPoseNumberDefinitions.ByNumber"/>.</summary>
    private static readonly FrozenDictionary<SamusPoseId, byte> Numbers = Enumerable
        .Range(0, SamusPoseNumberDefinitions.ByNumber.Length)
        .Where(SamusPoseNumbering.NamesPose)
        .ToFrozenDictionary(SamusPoseNumbering.FromNumber, number => checked((byte)number));

    /// <summary>The pose's number.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is not a pose.</exception>
    internal static byte Number(SamusPoseId pose) => Numbers.TryGetValue(pose, out byte number)
        ? number
        : throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined pose.");

    /// <summary>Whether the pose has a record in the pose tables, rather than reading the code past them.</summary>
    internal static bool HasRecord(SamusPoseId pose) => Number(pose) < SamusPoseNumbering.RecordCount;
}
