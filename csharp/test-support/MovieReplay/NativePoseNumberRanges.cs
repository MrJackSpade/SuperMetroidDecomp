using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The cartridge's own pose-number arithmetic, for references that mirror its draw code (#1517). The cartridge tests pose
/// families as unsigned ranges of pose numbers and indexes tables from a family's first pose; the game decides those
/// families by name, so only native references compare numbers.
/// </summary>
internal static class NativePoseNumberRanges
{
    /// <summary>Whether <paramref name="pose"/>'s number lies from <paramref name="first"/>'s through <paramref name="last"/>'s.</summary>
    internal static bool InRange(SamusPoseId pose, SamusPoseId first, SamusPoseId last) =>
        SamusPoseNumbering.Number(pose) >= SamusPoseNumbering.Number(first) && SamusPoseNumbering.Number(pose) <= SamusPoseNumbering.Number(last);

    /// <summary>Whether <paramref name="pose"/>'s number is below <paramref name="bound"/>'s, as the cartridge compares pose numbers.</summary>
    internal static bool Precedes(SamusPoseId pose, SamusPoseId bound) => SamusPoseNumbering.Number(pose) < SamusPoseNumbering.Number(bound);

    /// <summary>How many pose numbers <paramref name="pose"/> lies past <paramref name="first"/>, for tables indexed from a family's first pose.</summary>
    internal static int Offset(SamusPoseId pose, SamusPoseId first) => SamusPoseNumbering.Number(pose) - SamusPoseNumbering.Number(first);
}
