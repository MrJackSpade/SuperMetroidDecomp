using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Game;

/// <summary>First-launch ROM selection and recoverable installation without blocking the UI thread.</summary>
internal sealed class RomSetupForm : Form
{
    private readonly string? sourcePath;
    private readonly CancellationTokenSource stopping = new();
    private readonly Label status = new() { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Button choose = new() { Text = "Choose ROM…", AutoSize = true };
    private readonly ProgressBar progressBar = new() { Dock = DockStyle.Bottom, Height = 12, Style = ProgressBarStyle.Marquee };
    private bool busy;
    private bool closeRequested;
    public GameInstallation? Installation { get; private set; }

    public RomSetupForm(string[] arguments)
    {
        if (arguments.Length > 1) throw new ArgumentException("Supply at most one ROM path.");
        sourcePath = arguments.Length == 1 ? arguments[0] : Environment.GetEnvironmentVariable("SUPERMETROID_ROM");
        Text = "Super Metroid — Game setup";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(540, 235);
        MinimumSize = new Size(500, 250);
        Padding = new Padding(20);
        var introduction = new Label
        {
            Dock = DockStyle.Top, Height = 68,
            Text = "Choose your own Super Metroid ROM to get started.\r\n" +
                   "Supported: Japan/USA NTSC v1.0 (.smc or .sfc).\r\n" +
                   "A copy and the required audio will be saved in your local app data.",
        };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Cancel", AutoSize = true };
        cancel.Click += (_, _) => Close();
        choose.Click += async (_, _) =>
        {
            using var picker = new OpenFileDialog
            {
                Title = "Choose your Super Metroid ROM",
                Filter = "SNES ROMs (*.smc;*.sfc)|*.smc;*.sfc|All files (*.*)|*.*",
                CheckFileExists = true,
            };
            if (picker.ShowDialog(this) == DialogResult.OK) await Prepare(picker.FileName);
        };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(choose);
        Controls.Add(status);
        Controls.Add(progressBar);
        Controls.Add(buttons);
        Controls.Add(introduction);
        AcceptButton = choose;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await Prepare(string.IsNullOrWhiteSpace(sourcePath) ? null : sourcePath);
    }

    private async Task Prepare(string? source)
    {
        busy = true;
        choose.Enabled = false;
        progressBar.Visible = true;
        status.Text = "Checking installed game files…";
        var progress = new Progress<string>(message => { if (!IsDisposed) status.Text = message; });
        try
        {
            Installation = await Task.Run(() => source is null
                ? GameAssetInstaller.EnsureInstalled(GameAssetInstaller.DesktopRoot, stopping.Token, progress)
                : GameAssetInstaller.Install(source, GameAssetInstaller.DesktopRoot, stopping.Token, progress));
            status.Text = Installation is null ? "Select a ROM to continue. Your original file will be kept." : "Ready to play.";
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { status.Text = error.Message; }
        finally
        {
            busy = false;
            choose.Enabled = true;
            progressBar.Visible = false;
        }
        if (closeRequested) { DialogResult = DialogResult.Cancel; Close(); }
        else if (Installation is not null) { DialogResult = DialogResult.OK; Close(); }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (busy)
        {
            closeRequested = true;
            stopping.Cancel();
            e.Cancel = true;
        }
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) stopping.Dispose();
        base.Dispose(disposing);
    }
}
