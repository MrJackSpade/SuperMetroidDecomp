namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Places the pause frame's button labels on its two button rows.
/// </summary>
internal static class PauseMenuButtonLabels
{
    /// <summary>MAP, EXIT and SAMUS labels recolored by $82:A628-$A84C.</summary>
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
    /// <remarks>$82:A633/A66F/A6AB select MAP/EXIT/SAMUS at columns5/12/22.
    /// Their lower halves are one32-word row below, as at A651/A68D/A6C9.
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
