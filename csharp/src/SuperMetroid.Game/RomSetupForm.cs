using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Game;

/// <summary>First-launch ROM selection and recoverable installation without blocking the UI thread.</summary>
internal sealed class RomSetupForm : Form
{
    /// <summary>Optional ROM path supplied on the command line or through the desktop ROM environment setting.</summary>
    private readonly string? sourcePath;
    /// <summary>Cancels an installation or repair operation when the form is closing.</summary>
    private readonly CancellationTokenSource stopping = new();
    /// <summary>Displays installer progress messages and the final setup status.</summary>
    private readonly Label status = new() { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
    /// <summary>Starts the file picker when setup needs a ROM from the user.</summary>
    private readonly Button choose = new() { Text = "Choose ROM…", AutoSize = true };
    /// <summary>Indicates that installation work is running while the UI remains responsive.</summary>
    private readonly ProgressBar progressBar = new() { Dock = DockStyle.Bottom, Height = 12, Style = ProgressBarStyle.Marquee };
    /// <summary>Tracks whether preparation is in progress so close requests can cancel it safely.</summary>
    private bool busy;
    /// <summary>Records that the user requested closure while preparation was still being canceled.</summary>
    private bool closeRequested;

    /// <summary>Installation opened or created after the selected ROM and required assets have been prepared.</summary>
    public GameInstallation? Installation { get; private set; }

    /// <summary>Creates the first-launch setup dialog, accepting at most one optional ROM path argument.</summary>
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

    /// <summary>Automatically opens or repairs the installed game, using the supplied ROM path when available.</summary>
    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await Prepare(string.IsNullOrWhiteSpace(sourcePath) ? null : sourcePath);
    }

    /// <summary>Runs installation or repair on a worker task and updates the dialog as progress is reported.</summary>
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
                ? GameAssetInstaller.OpenOrRepair(GameAssetInstaller.DesktopRoot, stopping.Token, progress)
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

    /// <summary>Cancels an active preparation and defers closing until its cleanup path has completed.</summary>
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

    /// <summary>Releases the cancellation source when the form is disposed.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) stopping.Dispose();
        base.Dispose(disposing);
    }
}
