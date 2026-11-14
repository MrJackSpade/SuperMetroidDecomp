using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The credits roll's staff lines (#1467, #1601): each line's picture and where its top lies on the roll, in pixels, laid
/// out from the roles' order, styles and spacing; the roll ends the trailing space after its last line.
/// </summary>
public static class CreditsRollLines
{
    /// <summary>Each staff line, in roll order.</summary>
    public static ImmutableArray<CreditsRollLine> Lines { get; } = Layout();

    /// <summary>How far the roll has scrolled up when the credits finish, in pixels: its whole length and on past its end.</summary>
    public static int FinishingScroll => CreditsRollDefinitions.Length + CreditsRollDefinitions.ScrollPastEnd;

    /// <summary>The height of a line of <paramref name="style"/>, in pixels.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The style is undefined.</exception>
    public static int HeightOf(CreditsLineStyle style) => style switch
    {
        CreditsLineStyle.Small => CreditsRollDefinitions.SmallLineHeight,
        CreditsLineStyle.Large => CreditsRollDefinitions.LargeLineHeight,
        _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Undefined credits line style."),
    };

    /// <summary>Lays out each role's space and its line, then the trailing space.</summary>
    /// <exception cref="InvalidDataException">The layout is not the roll's length.</exception>
    private static ImmutableArray<CreditsRollLine> Layout()
    {
        IReadOnlyList<CreditsLineDefinition> roles = CreditsRoles.Lines;
        var lines = ImmutableArray.CreateBuilder<CreditsRollLine>(roles.Count);
        int top = 0;
        foreach (CreditsLineDefinition role in roles)
        {
            top += role.SpaceBefore;
            int height = HeightOf(role.Style);
            lines.Add(new CreditsRollLine(EndingPictureNames.CreditsLine(role.Role), top, height));
            top += height;
        }
        int length = top + CreditsRollDefinitions.TrailingSpace;
        if (length != CreditsRollDefinitions.Length)
            throw new InvalidDataException($"The credits roll lays out {length} pixels, not {CreditsRollDefinitions.Length}.");
        return lines.MoveToImmutable();
    }
}
