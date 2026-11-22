using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Desktop.Diagnostics;
using SuperMetroid.Diagnostics.Configuration;

namespace SuperMetroid.Desktop.Window;

/// <summary>The normal reset-to-gameplay desktop shell for the translated game.</summary>
internal sealed class GameForm : Form
{
    /// <summary>Owns the playable surface and renderer for this desktop window.</summary>
    internal readonly PlayableGameControl gameControl;
    /// <summary>Prevents a second asynchronous renderer stop while close is pending.</summary>
    private bool closingRenderer;
    /// <summary>Allows the second close request after the renderer has stopped.</summary>
    internal bool rendererStopped;

    /// <summary>The normal ROM-independent host path after asset installation.</summary>
    /// <param name="installation">The installed content and player data directory.</param>
    /// <param name="hostOptions">Host options retained through in-window restarts.</param>
    /// <param name="driver">A recorded timeline that drives the game and its input, or null for live play.</param>
    /// <param name="errorReporter">Recoverable error reporter, or null to fail at the first frame error.</param>
    internal GameForm(
        GameInstallation installation,
        HostOptions hostOptions,
        IHostFrameDriver? driver = null,
        GitHubErrorReporter? errorReporter = null)
        : this(new PlayableGameControl(installation, hostOptions, driver, errorReporter))
    {
    }

    /// <summary>Hosts an existing playable control in the sized desktop shell.</summary>
    private GameForm(PlayableGameControl gameControl)
    {
        Text = "Super Metroid C#";
        StartPosition = FormStartPosition.CenterScreen;
        // This fits a crisp 3x 256x224 image plus the debugger toolbar/help text on an
        // ordinary 1080p desktop. RuntimeCanvas automatically chooses a smaller integer
        // scale if the user resizes the window.
        ClientSize = new Size(900, 760);
        this.gameControl = gameControl;
        Controls.Add(gameControl);
    }

    /// <summary>Starts the renderer once the desktop form becomes visible.</summary>
    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await gameControl.InitializeRendererAsync();
    }

    /// <summary>Defers window disposal until asynchronous renderer shutdown completes.</summary>
    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel || rendererStopped) return;
        e.Cancel = true;
        if (closingRenderer) return;
        closingRenderer = true;
        try { await gameControl.StopRendererAsync(); }
        catch (Exception error) { Console.Error.WriteLine($"Renderer shutdown failed: {error}"); }
        finally
        {
            rendererStopped = true;
            Close();
        }
    }
}
