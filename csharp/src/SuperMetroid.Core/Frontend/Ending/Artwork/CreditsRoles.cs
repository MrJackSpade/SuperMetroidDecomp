using static SuperMetroid.Core.Frontend.Ending.Artwork.CreditsRollDefinitions;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The credits' 67 staff roles in roll order (<see cref="CreditsRollDefinitions"/>), each with its glyph style and
/// the blank space before it.
/// </summary>
public static class CreditsRoles
{
    /// <summary>Fixed role order without duplicating the 67 role descriptors in storage.</summary>
    private static readonly CreditRoles Roles = new();
    /// <summary>Calculated read-only view of all 67 required credit roles in roll order, with fixed heading or name styles and the blank space before each; wording remains editable content.</summary>
    public static IReadOnlyList<CreditsLineDefinition> Lines => Roles;

    /// <summary>Calculates glyph style and spacing from line order.</summary>
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
                bool heading = IsHeading(index);
                int space = index == 0 ? InitialSpace : index == 17 ? 0
                    : heading ? SectionSpace : InterLineSpace;
                return new(heading ? CreditsLineStyle.Small : CreditsLineStyle.Large, space);
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

    /// <summary>
    /// Whether line <paramref name="index"/> is a section heading: the staff, producer, director, designer, sound, music,
    /// coordinator, printed-art-work, special-thanks and general-manager headings, and the heading line of each of the
    /// eight programming roles, which alternate heading and name.
    /// </summary>
    private static bool IsHeading(int index) =>
        index is 0 or 1 or 3 or 5 or 9 or 12 or 14 or 16 or 17 or 19 or 38 or 41 or 48 or 65
        || (index is >= 22 and <= 37 && index % 2 == 0);
}
