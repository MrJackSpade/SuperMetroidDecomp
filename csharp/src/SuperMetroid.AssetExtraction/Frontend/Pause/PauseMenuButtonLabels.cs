using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>
/// Places the pause frame's button labels on its two button rows.
/// </summary>
internal static partial class PauseMenuButtonLabels
{
    /// <summary>MAP, EXIT and SAMUS labels recolored by the label palette routines.</summary>
    public static IEnumerable<PauseButtonLabelSpan> ButtonLabelSpans
    {
        get
        {
            foreach (PauseButtonLabel label in PauseSheetFrameOrder.ButtonLabels)
            for (int row = 0; row < PauseFrameDefinitions.ButtonRowCount; row++)
                yield return ButtonLabelSpan(label, row);
        }
    }

    /// <summary>Projects a named two-row button label into the 32-column BG2 tilemap.</summary>
    /// <remarks>MAP, EXIT and SAMUS sit side by side on the button row, each lower half one tilemap row below its upper
    /// half. The center EXIT label occupies four columns; the outside labels occupy five.</remarks>
    public static PauseButtonLabelSpan ButtonLabelSpan(PauseButtonLabel label, int row)
    {
        int column = label switch
        {
            PauseButtonLabel.Map => 5,
            PauseButtonLabel.Exit => 12,
            PauseButtonLabel.Samus => 22,
            _ => throw new ArgumentOutOfRangeException(nameof(label)),
        };
        if ((uint)row >= PauseFrameDefinitions.ButtonRowCount) throw new ArgumentOutOfRangeException(nameof(row));
        return new PauseButtonLabelSpan((PauseFrameDefinitions.ButtonRowsFirstRow + row) * PauseBackdropDefinitions.Columns + column, label == PauseButtonLabel.Exit ? 4 : 5);
    }
}
