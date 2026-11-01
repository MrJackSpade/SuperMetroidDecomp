namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Places the pause frame's button labels on its two button rows.
/// </summary>
internal static class PauseMenuButtonLabels
{
    /// <summary>MAP, EXIT and SAMUS labels recolored by the label palette routines.</summary>
    public static IEnumerable<PauseButtonLabelSpan> ButtonLabelSpans
    {
        get
        {
            foreach (PauseButtonLabel label in Enum.GetValues<PauseButtonLabel>())
            for (int row = 0; row < 2; row++)
                yield return ButtonLabelSpan(label, row);
        }
    }

    /// <summary>Projects a named two-row button label into the 32-column BG2 tilemap.</summary>
    /// <remarks>MAP/EXIT/SAMUS sit at columns5/12/22.
    /// Their lower halves are one32-word row below.
    /// The center EXIT label occupies four columns; the outside labels occupy five.</remarks>
    public static PauseButtonLabelSpan ButtonLabelSpan(PauseButtonLabel label, int row)
    {
        int column = label switch
        {
            PauseButtonLabel.Map => 5,
            PauseButtonLabel.Exit => 12,
            PauseButtonLabel.Samus => 22,
            _ => throw new ArgumentOutOfRangeException(nameof(label)),
        };
        if ((uint)row >= 2) throw new ArgumentOutOfRangeException(nameof(row));
        return new PauseButtonLabelSpan((25 + row) * 32 + column, label == PauseButtonLabel.Exit ? 4 : 5);
    }
}
