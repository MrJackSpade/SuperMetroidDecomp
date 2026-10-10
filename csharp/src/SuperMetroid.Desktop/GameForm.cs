using SuperMetroid.Desktop;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Game;

/// <summary>The normal reset-to-gameplay desktop shell for the translated game.</summary>
internal sealed class GameForm : Form
{
    /// <summary>Gameplay control that owns renderer startup and shutdown for this window.</summary>
    private readonly PlayableGameControl gameControl;

    /// <summary>Guards against starting renderer shutdown more than once during close events.</summary>
    private bool closingRenderer;

    /// <summary>Records that renderer shutdown completed so the follow-up close can proceed.</summary>
    private bool rendererStopped;

    /// <summary>The normal ROM-independent host path after asset installation.</summary>
    /// <param name="installation">Installed assets and content used to create the playable game.</param>
    /// <param name="gameOptions">Runtime options for the new gameplay session.</param>
    /// <param name="replay">Optional controller recording to play after initialization.</param>
    /// <param name="errorReporter">Optional reporter for recoverable GitHub-host errors.</param>
    public GameForm(
        GameInstallation installation,
        SuperMetroidGameOptions gameOptions,
        ControllerInputRecording? replay = null,
        GitHubErrorReporter? errorReporter = null)
        : this(new PlayableGameControl(installation, gameOptions, replay, errorReporter))
    {
    }

    /// <summary>Initializes the game window around an already constructed gameplay control.</summary>
    /// <param name="gameControl">Gameplay control whose renderer and session are hosted by the window.</param>
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

    /// <summary>Initializes the renderer after the form becomes visible.</summary>
    /// <param name="e">Event data supplied by the WinForms shown notification.</param>
    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await gameControl.InitializeRendererAsync();
    }

    /// <summary>Stops the renderer asynchronously before allowing the window to close.</summary>
    /// <param name="e">Closing event data whose cancellation flag defers closure until shutdown finishes.</param>
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
