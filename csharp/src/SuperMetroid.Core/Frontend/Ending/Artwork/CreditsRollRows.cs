using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The credits roll's rows (#1467): the picture of the staff line each row shows, laid out from the roles' order, styles
/// and spacing, or none for a blank row. The roll scrolls its rows one at a time, eight pixels apart.
/// </summary>
public static class CreditsRollRows
{
    /// <summary>Each row's picture name, or null for a blank row, from the first row of the roll.</summary>
    public static ImmutableArray<string?> Pictures { get; } = Layout();

    /// <summary>Lays out each role's blank rows and its one or two rows, then the trailing blank rows.</summary>
    /// <exception cref="InvalidDataException">The layout is not the roll's row count.</exception>
    private static ImmutableArray<string?> Layout()
    {
        var rows = ImmutableArray.CreateBuilder<string?>(CreditsRollDefinitions.ExpectedCompiledRows);
        IReadOnlyList<CreditsLineDefinition> lines = CreditsRoles.Lines;
        for (int line = 0; line < lines.Count; line++)
        {
            for (int blank = 0; blank < lines[line].BlankRowsBefore; blank++)
                rows.Add(null);
            rows.Add(EndingPictureNames.CreditsLine(line));
            if (lines[line].Style == CreditsLineStyle.Large)
                rows.Add(EndingPictureNames.CreditsLineBottom(line));
        }
        for (int blank = 0; blank < CreditsRollDefinitions.TrailingBlankRows; blank++)
            rows.Add(null);
        if (rows.Count != CreditsRollDefinitions.ExpectedCompiledRows)
            throw new InvalidDataException($"The credits roll lays out {rows.Count} rows, not {CreditsRollDefinitions.ExpectedCompiledRows}.");
        return rows.MoveToImmutable();
    }
}
