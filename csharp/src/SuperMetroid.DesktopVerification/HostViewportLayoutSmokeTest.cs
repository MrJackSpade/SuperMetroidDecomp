namespace SuperMetroid.Desktop;

/// <summary>Result of changing status text above a fixed gameplay viewport.</summary>
public readonly record struct HostViewportLayoutSmokeTestResult(
    Rectangle BeforeCanvasBounds,
    Rectangle AfterCanvasBounds,
    Size BeforeToolbarPreferredSize,
    Size AfterToolbarPreferredSize);

/// <summary>
/// Reproduces the one-column WinForms layout used by <see cref="PlayableGameControl"/>
/// without loading a ROM, opening a window, or starting the emulation timer.
/// </summary>
public static class HostViewportLayoutSmokeTest
{
    public static HostViewportLayoutSmokeTestResult Run()
    {
        using var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        toolStrip.Items.Add(new ToolStripButton("Restart"));
        toolStrip.Items.Add(new ToolStripButton("Pause"));
        toolStrip.Items.Add(new ToolStripButton("Step"));
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(new ToolStripLabel("State slot"));
        toolStrip.Items.Add(new ToolStripComboBox { AutoSize = false, Width = 48 });
        toolStrip.Items.Add(new ToolStripButton("Save State"));
        toolStrip.Items.Add(new ToolStripButton("Load State"));
        toolStrip.Items.Add(new ToolStripSeparator());
        ToolStripLabel status = HostToolbarLayout.CreateStatusLabel();
        status.Text = "state $08 MainGameplay | Gameplay | frame 100";
        toolStrip.Items.Add(status);

        using var canvas = new Panel { Dock = DockStyle.Fill };
        using var help = new Label { Dock = DockStyle.Bottom, Height = 58 };
        using var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
        };
        HostToolbarLayout.ConfigureGameplayColumn(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolStrip, 0, 0);
        layout.Controls.Add(canvas, 0, 1);
        using var host = new Panel { Size = new Size(900, 760) };
        host.Controls.Add(layout);
        host.Controls.Add(help);
        host.PerformLayout();
        layout.PerformLayout();
        Rectangle before = canvas.Bounds;
        Size beforeToolbarPreferredSize = toolStrip.PreferredSize;

        // This is the same kind of phase-length increase caused by a Start edge entering
        // the pause dispatcher. It must update text only; it must never resize the canvas
        // in which RuntimeCanvas horizontally centers the 4:3 presentation rectangle.
        status.Text =
            "state $0C PausingDarkening | Pausing: gameplay darken (15) | frame 123456 " +
            "| pad: USB Gamepad Controller (Vendor-defined name) | replay 123456/123456";
        toolStrip.PerformLayout();
        host.PerformLayout();
        layout.PerformLayout();
        Rectangle after = canvas.Bounds;
        Size afterToolbarPreferredSize = toolStrip.PreferredSize;
        if (afterToolbarPreferredSize.Width != beforeToolbarPreferredSize.Width)
        {
            throw new InvalidDataException(
                $"Status text changed toolbar preferred width " +
                $"{beforeToolbarPreferredSize.Width} -> {afterToolbarPreferredSize.Width}; " +
                "an auto-sized host can consequently recenter the gameplay image.");
        }
        if (after != before)
        {
            throw new InvalidDataException(
                $"Status transition moved/resized gameplay canvas {before} -> {after}.");
        }

        return new HostViewportLayoutSmokeTestResult(
            before,
            after,
            beforeToolbarPreferredSize,
            afterToolbarPreferredSize);
    }
}
