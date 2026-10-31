namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The cartridge's pose numbering: pose <c>n</c> is the <c>n</c>th record of the 253-record pose tables, and the
/// three numbers past them read adjacent code. Core keeps the numbers only where the cartridge's arithmetic on them
/// is gameplay-observable or an installed table is laid out by them: pose families tested as ranges of numbers, the
/// HUD's posture admission over-reading its table by number, and the installed Samus artwork holding one entry per
/// numbered pose. The animation programs name their transition poses by identity (#1473). This is the one place a
/// pose becomes its number or a number its pose.
/// </summary>
[EncodingOwner("pose numbers of the installed Samus pose tables")]
internal static class SamusPoseNumbering
{
    /// <summary>The number of poses with records in the cartridge's pose tables.</summary>
    internal const int RecordCount = 253;

    /// <summary>The pose's number.</summary>
    internal static byte Number(SamusPoseId pose) => (byte)pose;

    /// <summary>The pose a number names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is outside a byte.</exception>
    internal static SamusPoseId FromNumber(int number) =>
        (uint)number <= byte.MaxValue
            ? (SamusPoseId)number
            : throw new ArgumentOutOfRangeException(nameof(number), number, "A pose number is one byte.");

    /// <summary>
    /// Whether <paramref name="pose"/>'s number lies from <paramref name="first"/>'s through <paramref name="last"/>'s
    /// (#1452): the cartridge tests pose families such as the landing or shinespark poses as unsigned ranges of pose
    /// numbers.
    /// </summary>
    internal static bool InRange(SamusPoseId pose, SamusPoseId first, SamusPoseId last) =>
        Number(pose) >= Number(first) && Number(pose) <= Number(last);

    /// <summary>Whether <paramref name="pose"/>'s number is below <paramref name="bound"/>'s, as the cartridge compares pose numbers.</summary>
    internal static bool Precedes(SamusPoseId pose, SamusPoseId bound) => Number(pose) < Number(bound);

    /// <summary>How many pose numbers <paramref name="pose"/> lies past <paramref name="first"/>, for tables indexed from a family's first pose.</summary>
    internal static int Offset(SamusPoseId pose, SamusPoseId first) => Number(pose) - Number(first);

    /// <summary>Whether the pose has a record in the pose tables, rather than reading the code past them.</summary>
    internal static bool HasRecord(SamusPoseId pose) => Number(pose) < RecordCount;
}
