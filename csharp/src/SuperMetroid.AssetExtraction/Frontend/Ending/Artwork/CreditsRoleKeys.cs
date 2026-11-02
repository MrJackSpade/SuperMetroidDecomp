using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The credits document's stable role keys (#1502): the case-sensitive key each ordered line of
/// <see cref="CreditsRoles.Lines"/> must carry. Only the import reads and writes the document; the game rolls the
/// compiled lines by style and spacing.
/// </summary>
public static class CreditsRoleKeys
{
    /// <summary>Maps a retail line ordinal to its stable editable-document role key.</summary>
    /// <param name="index">Line ordinal in roll order.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the credits' lines.</exception>
    public static string Of(int index) => (uint)index < (uint)CreditsRoles.Lines.Count
        ? RoleId(index) : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Maps a retail line ordinal to its stable editable-document role key.</summary>
    private static string RoleId(int index) => index switch
    {
        0 => "staff-heading",
        1 => "producer-heading",
        2 => "producer-name",
        3 => "director-heading",
        4 => "director-name",
        5 => "background-designers-heading",
        >= 6 and <= 8 => $"background-designer-{index - 5}",
        9 => "object-designers-heading",
        10 or 11 => $"object-designer-{index - 9}",
        12 => "samus-original-designer-heading",
        13 => "samus-original-designer-name",
        14 => "samus-designer-heading",
        15 => "samus-designer-name",
        16 => "sound-program-heading",
        17 => "sound-effects-heading",
        18 => "sound-programmer-name",
        19 => "music-composers-heading",
        20 or 21 => $"music-composer-{index - 19}",
        >= 22 and <= 37 => ProgrammingRole((index - 22) / 2) + (index % 2 == 0 ? "-heading" : "-name"),
        38 => "coordinators-heading",
        39 or 40 => $"coordinator-{index - 38}",
        41 => "printed-art-work-heading",
        >= 42 and <= 47 => $"printed-art-work-{index - 41}",
        48 => "special-thanks-heading",
        >= 49 and <= 63 => FormattableString.Invariant($"special-thanks-{index - 48:D2}"),
        64 => "special-thanks-r-and-d",
        65 => "general-manager-heading",
        _ => "general-manager-name",
    };

    /// <summary>Names the eight programming roles shared by their heading and name lines.</summary>
    private static string ProgrammingRole(int index) => index switch
    {
        0 => "program-director",
        1 => "system-coordinator",
        2 => "system-programmer",
        3 => "samus-programmer",
        4 => "event-programmer",
        5 => "enemy-programmer",
        6 => "map-programmer",
        _ => "assistant-programmer",
    };
}
