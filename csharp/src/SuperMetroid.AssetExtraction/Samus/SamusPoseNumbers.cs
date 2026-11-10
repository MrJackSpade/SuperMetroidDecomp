using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Each pose's number in the cartridge's pose tables (#1527): the import reads and lays out the pose tables by these
/// numbers, and installs what it reads keyed by each pose's name (#1595). The numbering itself, and the pose each number
/// names, is <see cref="SamusPoseNumbering"/>'s.
/// </summary>
internal static class SamusPoseNumbers
{
    /// <summary>Each pose's number: its position in <see cref="SamusPoseNumberDefinitions.ByNumber"/>.</summary>
    private static readonly FrozenDictionary<SamusPoseId, int> Numbers = Enumerable
        .Range(0, SamusPoseNumberDefinitions.ByNumber.Length)
        .Where(SamusPoseNumbering.NamesPose)
        .ToFrozenDictionary(SamusPoseNumbering.FromNumber, number => number);

    /// <summary>The pose's number, as the pose tables store it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is not a pose.</exception>
    internal static byte Number(SamusPoseId pose) => Numbers.TryGetValue(pose, out int number)
        ? checked((byte)number)
        : throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined pose.");

    /// <summary>Whether the pose has a record in the pose tables, rather than reading the code past them.</summary>
    internal static bool HasRecord(SamusPoseId pose) => Number(pose) < SamusPoseNumbering.RecordCount;

    /// <summary>
    /// Every pose with a record in the pose tables, in the cartridge's order: the poses each installed per-pose table
    /// holds an entry for. A record no pose names has no entry.
    /// </summary>
    internal static IReadOnlyList<SamusPoseId> RecordedPoses { get; } =
        [.. Enumerable.Range(0, SamusPoseNumbering.RecordCount).Where(SamusPoseNumbering.NamesPose).Select(SamusPoseNumbering.FromNumber)];

    /// <summary>Whether <paramref name="table"/> has an entry for exactly the poses with records.</summary>
    internal static bool CoversRecordedPoses<TValue>(IReadOnlyDictionary<SamusPoseId, TValue> table) =>
        table.Count == RecordedPoses.Count && RecordedPoses.All(table.ContainsKey);
}
