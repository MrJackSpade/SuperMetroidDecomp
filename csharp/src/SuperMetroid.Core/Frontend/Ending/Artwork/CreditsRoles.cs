using static SuperMetroid.Core.Frontend.Ending.Artwork.CreditsPresentationDefinitions;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The credits' 67 staff roles in native order (<see cref="CreditsPresentationDefinitions"/>), each with its document key, glyph
/// style and preceding blank rows.
/// </summary>
public static class CreditsRoles
{
    /// <summary>Fixed native role order without duplicating the 67 role descriptors in storage.</summary>
    private static readonly CreditRoles Roles = new();
    /// <summary>Calculated read-only view of all 67 required credit roles in native order, with fixed one-/two-row styles and preceding blank-row counts; wording remains editable content.</summary>
    public static IReadOnlyList<CreditsLineDefinition> Lines => Roles;

    /// <summary>Calculates role identity, glyph style, and spacing from native line order.</summary>
    private sealed class CreditRoles : IReadOnlyList<CreditsLineDefinition>
    {
        /// <summary>The 67 staff roles present in the retail credits stream.</summary>
        public int Count => 67;
        /// <summary>Assigns headings a small font and section spacing, except for the paired sound-effects heading.</summary>
        public CreditsLineDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                string id = RoleId(index);
                bool heading = id.EndsWith("-heading", StringComparison.Ordinal);
                int blankRows = index == 0 ? InitialBlankRows : index == 17 ? 0
                    : heading ? SectionBlankRows : InterLineBlankRows;
                return new(id, heading ? CreditsLineStyle.Small : CreditsLineStyle.Large, blankRows);
            }
        }
        /// <summary>Yields all fixed role descriptors in displayed order.</summary>
        public IEnumerator<CreditsLineDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Adapts the ordered role sequence for non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

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
