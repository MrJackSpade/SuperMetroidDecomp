using SuperMetroid.Core;
namespace SuperMetroid.Desktop.Window;

/// <summary>Fixed geometry shared by the playable host and its layout regression.</summary>
internal static class HostToolbarLayout
{
    /// <summary>Fixed width that prevents a changing phase or timing label from recentering the gameplay canvas.</summary>
    /// <remarks>ToolStripLabel normally includes the complete text in ToolStrip.PreferredSize. The gameplay host places
    /// that strip above a horizontally centered RuntimeCanvas, so a longer phase name (for example, MainGameplay -&gt;
    /// PausingDarkening) used to widen the table-layout column and visibly shove the entire presented SNES picture to
    /// the right. Reserve one stable status region instead. The complete diagnostic remains available as the item's
    /// tooltip and on stdout; display text is clipped when it exceeds the box. The timing string is deliberately
    /// complete at the default 900-pixel host width. It still has a fixed width, so neither changing rates nor a longer
    /// worst-frame value can recenter the canvas as the old auto-sized diagnostic label did.</remarks>
    private const int StatusLabelWidth = 390;

    /// <summary>Creates a clipped status region so long diagnostics affect neither toolbar width nor canvas placement.</summary>
    public static ToolStripLabel CreateStatusLabel() => new()
    {
        AutoSize = false,
        Width = StatusLabelWidth,
        TextAlign = ContentAlignment.MiddleLeft,
        Overflow = ToolStripItemOverflow.Never,
    };

    /// <summary>
    /// Makes the single gameplay column a percentage of its host, never the preferred
    /// width of toolbar content. This is the second containment boundary: future buttons
    /// may enter ToolStrip overflow, but cannot alter the canvas coordinate system.
    /// </summary>
    public static void ConfigureGameplayColumn(TableLayoutPanel layout)
    {
        Ensure.NotNull(layout);
        layout.ColumnStyles.Clear();
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    }
}
