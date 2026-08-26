using SuperMetroid.Core.Rooms;

namespace SuperMetroid.RoomViewer;

/// <summary>
/// Small development host for stepping through room loading and composition. This is not a
/// level editor: it deliberately stays thin so a debugger enters game/data code immediately.
/// </summary>
internal sealed class RoomViewerForm : Form
{
    private readonly RoomCanvas roomCanvas;
    private readonly ToolStripLabel statusLabel;

    public RoomViewerForm(string rawAssetDirectory, string romPath)
    {
        Text = "Super Metroid C# decompilation viewer";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1280, 800);

        // This call is the useful F5 breakpoint boundary. From here the debugger walks the
        // original load chain: compression -> level stream -> block table -> tiles -> palette.
        RenderedRoom room = RoomRenderer.Render(rawAssetDirectory, RoomRenderer.LandingSite);
        Bitmap bitmap = CreateBitmap(room);
        roomCanvas = new RoomCanvas(bitmap);

        var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        toolStrip.Items.Add(new ToolStripLabel("Zoom:"));
        var zoomSelector = new ToolStripComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            AutoSize = false,
            Width = 80,
        };
        zoomSelector.Items.AddRange(["25%", "50%", "100%", "200%", "400%"]);
        zoomSelector.SelectedIndex = 2;
        zoomSelector.SelectedIndexChanged += (_, _) =>
        {
            // Integer and reciprocal-integer zoom levels keep source pixels on stable edges.
            float[] scales = [0.25f, 0.5f, 1f, 2f, 4f];
            roomCanvas.Zoom = scales[zoomSelector.SelectedIndex];
            UpdateStatus(room);
        };
        toolStrip.Items.Add(zoomSelector);
        toolStrip.Items.Add(new ToolStripSeparator());
        statusLabel = new ToolStripLabel();
        toolStrip.Items.Add(statusLabel);

        // AutoScroll supplies navigation over a room much larger than a 256x224 SNES viewport.
        // Keeping the canvas at its scaled logical size makes scrollbar coordinates meaningful.
        var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scrollPanel.Controls.Add(roomCanvas);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolStrip, 0, 0);
        layout.Controls.Add(scrollPanel, 0, 1);
        var roomTab = new TabPage("Landing Site") { Padding = new Padding(0) };
        roomTab.Controls.Add(layout);

        // Keep the static whole-room composition and dynamic 256x224 PPU preview separate.
        // The latter uses screen-relative OAM coordinates; overlaying it on the 2304x1280
        // room without a translated camera would imply a position the runtime does not know.
        var runtimeTab = new TabPage("Frame runtime") { Padding = new Padding(0) };
        runtimeTab.Controls.Add(new RuntimePreviewControl(romPath, room));

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(roomTab);
        tabs.TabPages.Add(runtimeTab);
        Controls.Add(tabs);

        UpdateStatus(room);
    }

    private void UpdateStatus(RenderedRoom room)
    {
        statusLabel.Text = $"{RoomRenderer.LandingSite.Name}  |  {room.Width}×{room.Height}px  |  {roomCanvas.Zoom:P0}";
    }

    private static Bitmap CreateBitmap(RenderedRoom room) => RgbaBitmap.Create(room.Width, room.Height, room.Pixels);
}
